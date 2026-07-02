using System.Reflection;
using MediatR;
using Pcea.Core.Net.Authorization.Application.Attributes;
using Pcea.Core.Net.Authorization.Application.Exceptions;
using Pcea.Core.Net.Authorization.Application.Interfaces.Services;
using Pcea.Core.Net.Authorization.Interfaces.Handlers;

namespace Pcea.Core.Net.Authorization.Application.Behaviors
{
    // <summary>
    /// MediatR behavior to handle authorization for global permissions check
    /// <seealso cref="WithPermissionAttribute"/>
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public class AuthorizationBehavior<TRequest, TResponse>(
        ICurrentUserPermissionsProvider currentUserPermissionsProvider,
        IAuthorizationHandler handler
    ) : Behavior<TRequest, TResponse>
        where TRequest : notnull
    {
        protected override async Task<TResponse> HandleRequest(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken
        )
        {
            var withPermissionAttributes = request
                .GetType()
                .GetCustomAttributes<WithPermissionAttribute>();

            if (!withPermissionAttributes.Any())
            {
                return await next();
            }
            if (
                !await currentUserPermissionsProvider.IsCurrentUserAuthenticatedAsync(
                    cancellationToken
                )
            )
            {
                throw new UserAccessException(
                    null,
                    new Dictionary<string, object?>() { { "No user authenticated", null } }
                );
            }

            // Check permissions
            var permissionCodes =
                await currentUserPermissionsProvider.GetCurrentUserPermissionsAsync(
                    cancellationToken
                );
            var requiredPermissionsCodes = withPermissionAttributes
                .SelectMany(a => a.Permissions)
                .Distinct();
            await handler.BuildAsync(requiredPermissionsCodes, permissionCodes, cancellationToken);
            var result = await handler.HandleAsync(cancellationToken);
            if (!result.IsAuthorized)
            {
                throw new UserAccessException(null, result.AdditionalData);
            }

            // User is authorized / authorization not required
            return await next();
        }
    }
}
