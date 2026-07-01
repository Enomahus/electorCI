using System.Diagnostics.CodeAnalysis;
using Tools.Exceptions.Errors;

namespace Tools.Exceptions;

[ExcludeFromCodeCoverage]
public class ConfigurationMissingException : AppException
{
    public ConfigurationMissingException(string message)
        : base(ErrorCode.ConfigurationMissing, ErrorKind.Technical, message) { }
}
