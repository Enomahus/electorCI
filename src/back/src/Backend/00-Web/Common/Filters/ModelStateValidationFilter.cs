using Application.Exceptions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Web.Common.Filters
{
    /// <summary>
    /// Guards against invalid or malformed request bodies before the action runs.
    /// <see cref="Microsoft.AspNetCore.Mvc.ApiBehaviorOptions.SuppressModelStateInvalidFilter"/>
    /// is enabled so FluentValidation drives the standard error shape, but that also disables
    /// ASP.NET Core's built-in guard against binding failures (e.g. a malformed body binding
    /// [FromBody] parameters to null). Without this filter, a null command reaches the action
    /// and crashes with an ArgumentNullException when it is sent to MediatR.
    /// </summary>
    public class ModelStateValidationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
             if (context.ModelState.IsValid)
            {
                return;
            }

            var failures = context
                .ModelState.Where(entry => entry.Value?.Errors.Count > 0)
                .SelectMany(entry =>
                    entry.Value!.Errors.Select(error =>
                    {
                        var failure = new ValidationFailure(entry.Key, error.ErrorMessage)
                        {
                            FormattedMessagePlaceholderValues = new Dictionary<string, object>
                            {
                                ["PropertyName"] = entry.Key,
                            },
                        };
                        return failure;
                    })
                )
                .ToList();

            throw new ValidationException(failures);
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
