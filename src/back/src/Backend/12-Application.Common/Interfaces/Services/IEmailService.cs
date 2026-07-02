namespace Application.Common.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendCreatePasswordEmailAsync(string activateLink, string toEmail);
        Task SendEndOfConformityEmailAsync(
            string certificateRequestLink,
            string createNewCertificateRequestLink,
            string? toEmail,
            IEnumerable<string> bcc
        );
        Task SendEndOfConformitySoonEmailAsync(
            string certificateRequestLink,
            string? toEmail,
            IEnumerable<string> bcc
        );
        Task SendEndOfVersionEmailAsync(
            string certificateRequestLink,
            string createNewCertificateRequestLink,
            string? toEmail,
            IEnumerable<string> bcc
        );
        Task SendForgotPasswordEmailAsync(string resetLink, string toEmail);
    }
}
