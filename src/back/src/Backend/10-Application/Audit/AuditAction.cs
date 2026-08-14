namespace Application.Audit
{
    public enum AuditAction
    {
        RegistrationRequestValidated,
        RegistrationRequestCreated,
        RegistrationRequestUpdated,
        RegistrationRequestStatusUpdated,

        DistrictCreated,
        DistrictUpdated,
        DistrictDeleted,

        UserCreated,
        UserUpdated,
        UserDeleted,
        SuccesfullyAuthenticated,

        ForgotPasswordMailSent,
        PasswordResetMailSent,
    }
}
