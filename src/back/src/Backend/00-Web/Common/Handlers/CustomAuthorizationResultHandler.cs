using System.Text.Json;
using Application.Exceptions.Auth;
using Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Tools.Exceptions.Errors;

namespace Web.Common.Handlers
{
    public class CustomAuthorizationResultHandler(JsonSerializerOptions serializerOptions)
        : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler _defaultHandler =
            new AuthorizationMiddlewareResultHandler();

        public async Task HandleAsync(
            RequestDelegate next,
            HttpContext context,
            AuthorizationPolicy policy,
            PolicyAuthorizationResult authorizeResult
        )
        {
            if (authorizeResult.Forbidden)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                var ex = new UserAccessException();
                await context.Response.WriteAsJsonAsync(Result<Error>.From(), serializerOptions);
            }
            else
            {
                await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
            }
        }
    }
}
