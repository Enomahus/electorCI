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
        CitizenMustExist,
        InvalidParent,
        DistrictMustHaveParent,
        InvalidLevel,
        PollingStationMustExist,
        PollingStationLinked,
        DistrictLinked,
        MustBeAdult,
        MarriedNameRequired,
        RegistrationRequestMustExist,
        RegistrationRequestMustBeOwnedByUser,
        RegistrationRequestMustBeDraftOrToBeProcessed,
    }
}
