using Tools.Exceptions;
using Tools.Exceptions.Errors;

namespace Application.Exceptions.Auth;

public class ResetTokenException : AppException
{
    public ResetTokenException(Exception? innerException = null)
        : base(
            ErrorCode.Validation,
            ErrorKind.Validation,
            $"Reset token is invalid",
            innerException
        ) { }
}
