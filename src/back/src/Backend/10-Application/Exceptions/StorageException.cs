using System.Diagnostics.CodeAnalysis;
using Tools.Exceptions;
using Tools.Exceptions.Errors;

namespace Application.Exceptions;

[ExcludeFromCodeCoverage]
public class StorageException : AppException
{
    public StorageException(string? message, Exception? innerException = null)
        : base(ErrorCode.Storage, ErrorKind.Technical, message, innerException) { }
}
