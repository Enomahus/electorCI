using System.Diagnostics.CodeAnalysis;
using MediatR;

namespace Pcea.Core.Net.Authorization.Application.Behaviors
{
    [ExcludeFromCodeCoverage]
    public abstract class Behavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        protected readonly string _name;

        protected Behavior()
        {
            _name = GetType().Name;
        }

        public Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken
        )
        {
            return HandleRequest(request, next, cancellationToken);
        }

        protected abstract Task<TResponse> HandleRequest(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken
        );
    }
}
