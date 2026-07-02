using System.Security.Claims;
using Application.Common.Enums;
using Application.Common.Interfaces.Services;
using Microsoft.AspNetCore.Authentication;
using Pcea.Core.Net.Authorization.Application.Interfaces.Services;
using Pcea.Core.Net.Authorization.Persistence.Interfaces.Services;
using Pcea.Core.Net.Authorization.Web.Interfaces.Services;
using Pcea.Core.Net.Authorization.Web.Services;

namespace Web.Common.Authorization
{
    public class CurrentUserTokenService(
        IHttpContextAccessor httpContextAccessor,
        ITokenRoleClaimBuilder<long> tokenRoleClaimBuilder,
        IPermissionsService<Guid> permissionsService
    )
        : CurrentUserTokenService<Guid, long>(
            httpContextAccessor,
            tokenRoleClaimBuilder,
            permissionsService
        ),
            ICurrentUserService,
            ICurrentUserPermissionsProvider,
            ICurrentUserEntityPermissionsProvider<long>
    {
        public Guid? UserId =>
            Guid.TryParse(
                _httpContextAccessor
                    .HttpContext?.User.Claims.FirstOrDefault(c =>
                        c.Type == ClaimTypes.NameIdentifier
                    )
                    ?.Value
                    ?? "",
                out var parsedId
            )
                ? parsedId
                : null;

        public string? UserEmail =>
            _httpContextAccessor
                .HttpContext?.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)
                ?.Value;

        public string? ClientIp =>
            _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "";

        public string? LanguageCode =>
            _httpContextAccessor
                .HttpContext?.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Locality)
                ?.Value;

        public Task<string?> GetTokenAsync(CancellationToken token = default)
        {
            return _httpContextAccessor.HttpContext?.GetTokenAsync("access_token")
                ?? Task.FromResult<string?>(null);
        }

        public override Task<bool> IsCurrentUserAuthenticatedAsync(
            CancellationToken cancellationToken = default
        )
        {
            return Task.FromResult(UserId.HasValue);
        }

        public override async Task<IEnumerable<string>> GetCurrentUserPermissionsOnEntityAsync(
            long entityId,
            CancellationToken cancellationToken = default
        )
        {
            var tokenRoles = await GetTokenRolesAsync(cancellationToken);
            if (tokenRoles is null)
            {
                return [];
            }

            var userRolePermissions = await GetRolePermissionsAsync(
                tokenRoles.UserRoles,
                cancellationToken
            );
            // Check for global admin permissions
            var userAdminPermissions = userRolePermissions.Where(r =>
                r == nameof(AppPermission.SuperAdmin)
            );
            if (!tokenRoles.UserEntitiesRoles.ContainsKey(entityId))
            {
                return userAdminPermissions;
            }

            if (
                !tokenRoles.UserEntitiesRoles.TryGetValue(entityId, out var userEntityRoles)
                || userEntityRoles.All(string.IsNullOrEmpty)
            )
            {
                // If user has no roles on entity, then use its global roles
                return userRolePermissions;
            }
            var entityRoles = await GetRolePermissionsAsync(userEntityRoles, cancellationToken);
            // Returns roles which are both in user roles and entity roles
            // If some roles are in entity roles but not in user roles, then they won't be returned
            return userRolePermissions
                .Intersect(entityRoles)
                .Union(userAdminPermissions)
                .Distinct();
        }

        protected override Guid ParseRoleId(string roleIdStr)
        {
            return Guid.Parse(roleIdStr);
        }
    }
}
