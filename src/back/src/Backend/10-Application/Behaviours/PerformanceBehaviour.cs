using Application.Models;
using MediatR;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;
using Tools.Logging;

namespace Application.Behaviours
{
    [ExcludeFromCodeCoverage]
    public class PerformanceBehaviour<TRequest, TResponse>() : Behavior<TRequest,TResponse>
        where TRequest : notnull
    {
        private readonly Stopwatch _timer = new();

        protected override async Task<TResponse> HandleRequest(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
        {
            using var activity = ActivitySourceLog.CQRS.Start();

            _timer.Start();

            try
            {
                TResponse response = await next();

                _timer.Stop();

                var elapsedMilliseconds = _timer.ElapsedMilliseconds;

                var result = response as Result;

                result?.SetDuration(elapsedMilliseconds);

                return response;
            }
            catch (Exception ex)
            {
                ExceptionDispatchInfo? exception = ExceptionDispatchInfo.Capture(ex);
                exception.Throw();
            }
            return default;
        }
    }
}
