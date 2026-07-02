using System.Diagnostics.CodeAnalysis;
using Tools.Exceptions;
using Tools.Exceptions.Errors;

namespace Application.Exceptions;

[ExcludeFromCodeCoverage]
public class NotFoundException : AppException
{
    public NotFoundException(string name, object? key)
        : base(
            ErrorCode.NotFound,
            ErrorKind.RequestData,
            $"Entity {name}({key}) was not found.",
            null,
            new Dictionary<string, string> { { "type", name }, { "id", key?.ToString() ?? "" } }
        ) { }
}
