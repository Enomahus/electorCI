using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using Pcea.Core.Net.Authorization.Application.Attributes;
using Pcea.Core.Net.Authorization.Application.Exceptions;
using Pcea.Core.Net.Authorization.Application.Interfaces.Services;
using Pcea.Core.Net.Authorization.Interfaces.Handlers;

namespace Pcea.Core.Net.Authorization.Application.Behaviors
{
    public abstract class EntityAuthorizationBehavior<TRequest, TResponse, T_EntityId>(
        ICurrentUserEntityPermissionsProvider<T_EntityId> permissionsProvider,
        IAuthorizationHandler handler
    ) : Behavior<TRequest, TResponse>
        where TRequest : notnull
    {
        protected readonly ICurrentUserEntityPermissionsProvider<T_EntityId> _permissionsProvider =
            permissionsProvider;
        protected readonly IAuthorizationHandler _authorizationHandler = handler;

        protected override async Task<TResponse> HandleRequest(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken
        )
        {
            var requiredPermissionsHolders = await GetRequiredPermissionsHoldersAsync(
                request,
                cancellationToken
            );

            if (!requiredPermissionsHolders.Any())
            {
                return await next();
            }

            if (!await _permissionsProvider.IsCurrentUserAuthenticatedAsync(cancellationToken))
            {
                throw new UserAccessException(
                    null,
                    new Dictionary<string, object?>() { { "No user authenticated", null } }
                );
            }
            var entityId = await GetRequestEntityIdAsync(
                request,
                requiredPermissionsHolders,
                cancellationToken
            );
            var permissionCodes = await _permissionsProvider.GetCurrentUserPermissionsOnEntityAsync(
                entityId,
                cancellationToken
            );

            var requiredPermissionsCodes = requiredPermissionsHolders
                .SelectMany(a => a.Permissions)
                .Distinct();
            await _authorizationHandler.BuildAsync(
                requiredPermissionsCodes,
                permissionCodes,
                cancellationToken
            );
            var result = await _authorizationHandler.HandleAsync(cancellationToken);
            if (!result.IsAuthorized)
            {
                result.AdditionalData.Add("Entity Id", entityId);
                throw new UserAccessException(null, result.AdditionalData);
            }

            // User is authorized / authorization not required
            return await next();
        }

        protected abstract Task<T_EntityId> GetRequestEntityIdAsync(
            TRequest request,
            IEnumerable<IPermissionsHolder> permissionAttributes,
            CancellationToken cancellationToken
        );

        protected abstract Task<IEnumerable<IPermissionsHolder>> GetRequiredPermissionsHoldersAsync(
            TRequest request,
            CancellationToken cancellationToken
        );
    }
}
