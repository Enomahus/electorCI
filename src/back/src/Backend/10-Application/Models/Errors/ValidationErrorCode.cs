using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Models.Errors
{
    public enum ValidationErrorCode
    {
        Required = 0,
        MinLength,
        MaxLength,
        Unique,
        AlreadyExists,
        Base64Format,
        InvalidPassword,
        PositiveNumber,
        InvalidEmail,
        UserMustExist,
        UserLinked,
        UserCannotBeCurrentUser,
        RoleMustExist,
        DistrictMustExist,
        InvalidParent,
        DistrictMustHaveParent,
        InvalidLevel
    }
}
