using Application.Common.Interfaces.Services;
using Application.Models;
using MediatR;
using Tools.Logging;

namespace Application.Behaviours
{
    public class CurrentUserBehaviour<TRequest, TResponse>(ICurrentUserService currentUserService)
        : Behavior<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : IResult
    {
        protected override async Task<TResponse> HandleRequest(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken
        )
        {
            using var activity = ActivitySourceLog
                .CQRS.Start()
                .AddParameter(currentUserService, c => c.ClientIp);

            if (currentUserService.UserId is not null)
            {
                activity.AddParameter(currentUserService, c => c.UserId);
            }
            return await next();
        }
    }
}
