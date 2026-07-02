namespace Tools.Exceptions.Errors;

public enum ErrorKind
{
    None = 0,
    Authentication,
    AccessRights,
    Validation,
    Technical,
    RequestData,
    DomainRule
}
