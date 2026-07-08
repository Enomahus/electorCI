using Infrastructure.Persistence.Entities;

namespace Application.Features.Security.Common
{
    public interface ITokenHelper
    {
        Task<TokenResponse> GenerateTokenAsync(UserDao user, CancellationToken cancellationToken);
        Task<UserDao?> GetUserForAuthenticationAsync(string userName);
    }
}
