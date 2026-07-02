using System.Diagnostics.CodeAnalysis;
using Tools.Exceptions.Errors;

namespace Tools.Exceptions;

[ExcludeFromCodeCoverage]
public class AppException : Exception
{
    public ErrorCode Code { get; }
    public ErrorKind Kind { get; }
    public Dictionary<string, string> AdditionalData { get; protected set; }
    public Dictionary<string, string> Values { get; protected set; }

    protected AppException(
        ErrorCode code,
        ErrorKind kind,
        string? message,
        Dictionary<string, string>? additionalData = null,
        Dictionary<string, string>? values = null
    )
        : base(message)
    {
        Code = code;
        Kind = kind;
        AdditionalData = additionalData ?? [];
        Values = values ?? [];
    }

    protected AppException(
        ErrorCode code,
        ErrorKind kind,
        string? message,
        Exception? innerException,
        Dictionary<string, string>? additionalData = null,
        Dictionary<string, string>? values = null
    )
        : base(message, innerException)
    {
        Code = code;
        Kind = kind;
        AdditionalData = additionalData ?? [];
        Values = values ?? [];
    }

    public Error ToError()
    {
        return new Error(Code, Message, Kind, AdditionalData, Values);
    }

}
