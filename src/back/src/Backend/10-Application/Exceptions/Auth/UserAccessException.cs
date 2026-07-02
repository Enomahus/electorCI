using Tools.Exceptions;
using Tools.Exceptions.Errors;

namespace Application.Exceptions.Auth;

public class UserAccessException : AppException
{
    public UserAccessException(Exception? innerException = null)
        : base(
            ErrorCode.AccessRights,
            ErrorKind.AccessRights,
            $"Insufficient rights for user",
            innerException
        ) { }
}
