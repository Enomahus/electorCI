using Application.Interfaces.Services;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Security.Common
{
    public class TokenHelper(ITokenService tokenService, WritableDbContext context) : ITokenHelper
    {
        public async Task<TokenResponse> GenerateTokenAsync(
            UserDao user,
            CancellationToken cancellationToken
        )
        {
            var userRolesId = user.UserRoles.Select(ur => ur.Role.Id.ToString()).ToList();
            var userEntitiesRoles = user.UserDistricts.ToDictionary(
                ud => ud.DistrictId,
                ud => ud.SpecificRoles.Select(r => r.Id.ToString())
            );

            var tokens = await tokenService.CreateTokensAsync(
                user,
                userRolesId,
                userEntitiesRoles,
                cancellationToken
            );
            var model = new TokenResponse(tokens.AccessToken, tokens.RefreshToken);
            return model;
        }

        public async Task<UserDao?> GetUserForAuthenticationAsync(string userName)
        {
            return await context
                .Users.Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.Actions)
                            .ThenInclude(a => a.Permissions)
                .Include(u => u.UserDistricts)
                    .ThenInclude(ue => ue.SpecificRoles)
                        .ThenInclude(r => r.Actions)
                            .ThenInclude(a => a.Permissions)
                .Where(u => u.UserName == userName)
                .FirstOrDefaultAsync();
        }
    }
}
