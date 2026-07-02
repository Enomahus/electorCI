using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Behaviours
{
    public abstract class Behavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest: notnull
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
