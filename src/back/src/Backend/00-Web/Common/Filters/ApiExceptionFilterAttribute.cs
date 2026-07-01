using Application.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Tools.Exceptions;
using Tools.Exceptions.Errors;

namespace Web.Common.Filters
{
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Method,
        AllowMultiple = true,
        Inherited = true
    )]
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly Dictionary<Type, Action<ExceptionContext>> _exceptionHandlers;

        public ApiExceptionFilterAttribute()
        {
            // Register known exception types and handlers.
            _exceptionHandlers = new Dictionary<Type, Action<ExceptionContext>>
            {
                { typeof(AppException), HandleDomainException },
            };
        }

        public override void OnException(ExceptionContext context)
        {
            HandleException(context);

            base.OnException(context);
        }

        private void HandleException(ExceptionContext context)
        {
            Type type = context.Exception.GetType();
            foreach (var handler in _exceptionHandlers)
            {
                if (handler.Key.IsAssignableFrom(type))
                {
                    handler.Value.Invoke(context);
                    return;
                }
            }

            if (!context.ExceptionHandled)
            {
                var result = Result<Error>.From(Error.ObfuscatedError);
                context.Result = new ObjectResult(result)
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                };
            }
        }

        private void HandleDomainException(ExceptionContext context)
        {
            var exception = (AppException)context.Exception;

            var result = Result<Error>.From(exception.ToError());

            if (result?.Data?.Code == ErrorCode.NotFound)
            {
                context.Result = new NotFoundObjectResult(result);
            }

            switch (result?.Data?.Kind)
            {
                case ErrorKind.Authentication:
                    context.Result = new UnauthorizedObjectResult(result);
                    break;
                case ErrorKind.AccessRights:
                    context.Result = new ObjectResult(result)
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                    };
                    break;
                case ErrorKind.Validation:
                case ErrorKind.DomainRule:
                case ErrorKind.RequestData:
                    context.Result = new BadRequestObjectResult(result);
                    break;
                case ErrorKind.None:
                case ErrorKind.Technical:
                default:
                    context.Result = new ObjectResult(Result<Error>.From(Error.ObfuscatedError))
                    {
                        StatusCode = StatusCodes.Status500InternalServerError,
                    };
                    break;
            }

            context.ExceptionHandled = false;
        }
    }
}
