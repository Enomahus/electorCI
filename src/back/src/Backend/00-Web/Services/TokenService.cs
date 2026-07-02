using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Exceptions.Auth;
using Application.Interfaces.Services;
using Application.Models.Auth;
using Infrastructure.Configurations;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Pcea.Core.Net.Authorization.Web.Interfaces.Services;
using Tools.Exceptions;
using Tools.Helpers;

namespace Web.Services
{
    public class TokenService(
        IOptions<TokenConfiguration> tokenConfig,
        WritableDbContext context,
        TimeProvider timeProvider,
        ITokenRoleClaimBuilder<long> tokenRoleClaimBuilder
    ) : ITokenService
    {
        private async Task<string> CreateTokenAsync(
            UserDao user,
            IEnumerable<string> roles,
            IDictionary<long, IEnumerable<string>> userEntitiesRoles,
            CancellationToken cancellationToken = default
        )
        {
            var signingCredentials = GetSigningCredentials();
            var claims = await GetClaimsAsync(user, roles, userEntitiesRoles, cancellationToken);
            var tokenOptions = GenerateTokenOptions(signingCredentials, claims);
            return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        }

        private SigningCredentials GetSigningCredentials()
        {
            var secretStr =
                tokenConfig.Value.Secret
                ?? throw new ConfigurationMissingException(
                    "Missing configuration : JwtConfig.Secret"
                );
            var key = Encoding.UTF8.GetBytes(secretStr);
            var secret = new SymmetricSecurityKey(key);
            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }

        private JwtSecurityToken GenerateTokenOptions(
            SigningCredentials signingCredentials,
            List<Claim> claims
        )
        {
            var tokenOptions = new JwtSecurityToken(
                issuer: tokenConfig.Value.ValidIssuer,
                claims: claims,
                expires: timeProvider
                    .GetUtcNow()
                    .DateTime.AddMinutes(tokenConfig.Value.AccessTokenExpirationMinutes),
                signingCredentials: signingCredentials
            );
            return tokenOptions;
        }

        private async Task<List<Claim>> GetClaimsAsync(
            UserDao user,
            IEnumerable<string> roles,
            IDictionary<long, IEnumerable<string>> userEntitiesRoles,
            CancellationToken cancellationToken = default
        )
        {
            var rolesClaims = await tokenRoleClaimBuilder.BuildRolesClaimsAsync(
                roles,
                userEntitiesRoles,
                cancellationToken
            );

            List<Claim> claims =
            [
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Name, user.UserName!),
                new(JwtRegisteredClaimNames.Email, user.Email!),
                new("lastName", user.LastName ?? ""),
                new("firstName", user.FirstName ?? ""),
                .. rolesClaims,
            ];
            return claims;
        }

        public void TrackRefreshTokensToClean()
        {
            var now = timeProvider.GetUtcNow();
            var expiredTokens = context.RefreshTokens.Where(t => t.Expiry < now);
            context.RefreshTokens.RemoveRange(expiredTokens);
        }

        public async Task<string> CreateRefreshTokenAsync(
            Guid userId,
            CancellationToken cancellationToken = default
        )
        {
            var newToken = new RefreshTokenDao()
            {
                UserId = userId != Guid.Empty ? userId : null,
                Expiry = DateTimeOffset.Now.AddMinutes(
                    tokenConfig.Value.RefreshTokenExpirationMinutes
                ),
            };
            context.RefreshTokens.Add(newToken);
            TrackRefreshTokensToClean();
            await context.SaveChangesAsync(cancellationToken);

            return Base64Helper.GuidToBase64(newToken.Id);
        }

        public async Task<Tokens> CreateTokensAsync(
            UserDao user,
            IEnumerable<string> roles,
            IDictionary<long, IEnumerable<string>> userEntitiesRoles,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                string accessToken = await CreateTokenAsync(
                    user,
                    roles,
                    userEntitiesRoles,
                    cancellationToken
                );
                string refreshToken = await CreateRefreshTokenAsync(user.Id, cancellationToken);

                return new Tokens(accessToken, refreshToken);
            }
            catch (Exception ex)
            {
                throw new UserAuthenticationException(user.UserName ?? "", ex);
            }
        }
    }
}
