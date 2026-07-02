using System.Diagnostics.CodeAnalysis;
using FluentValidation.Results;
using Tools.Exceptions;
using Tools.Exceptions.Errors;

namespace Application.Exceptions;

[ExcludeFromCodeCoverage]
public class ValidationException : AppException
{
    public ValidationException()
        : base(
            ErrorCode.Validation,
            ErrorKind.Validation,
            "One or more validation failures have occurred."
        ) { }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        AdditionalData = failures
            .GroupBy(
                e =>
                    e.FormattedMessagePlaceholderValues["PropertyName"]?.ToString()
                    ?? e.PropertyName,
                e => e.ErrorMessage
            )
            .ToDictionary(
                failureGroup => failureGroup.Key,
                failureGroup => string.Join(" ", failureGroup)
            );
    }
}
