using Application.Exceptions.Auth;
using MediatR;
using Pcea.Core.Net.Authorization.Application.Interfaces.Services;
using Pcea.Core.Net.Authorization.Interfaces.Handlers;

namespace Application.Behaviours
{
    public class AuthorizationBehavior<TRequest, TResponse>(
        ICurrentUserPermissionsProvider currentUserPermissionsProvider,
        IAuthorizationHandler handler
    )
        : Pcea.Core.Net.Authorization.Application.Behaviors.AuthorizationBehavior<
            TRequest,
            TResponse
        >(currentUserPermissionsProvider, handler)
        where TRequest : notnull
    {
        protected override async Task<TResponse> HandleRequest(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken
        )
        {
            try
            {
                return await base.HandleRequest(request, next, cancellationToken);
            }
            catch (Pcea.Core.Net.Authorization.Application.Exceptions.UserAccessException ex)
            {
                throw new UserAccessException(ex);
            }
        }
    }
}
