using Infrastructure.ExternalAuth.Models;

namespace Infrastructure.ExternalAuth.Interfaces
{
    public interface IExternalAuthService
    {
        Task<ExternallyAuthenticatedPersonModel> CheckAuthorizationCodeAsync(
            string code,
            CancellationToken cancellationToken
        );
    }
}
