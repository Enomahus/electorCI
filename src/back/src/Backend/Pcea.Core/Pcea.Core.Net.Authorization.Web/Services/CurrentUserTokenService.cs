using Microsoft.AspNetCore.Http;
using Pcea.Core.Net.Authorization.Application.Interfaces.Services;
using Pcea.Core.Net.Authorization.Persistence.Interfaces.Services;
using Pcea.Core.Net.Authorization.Web.Interfaces.Services;

namespace Pcea.Core.Net.Authorization.Web.Services
{
    public abstract class CurrentUserTokenService<T_RoleId, T_EntityId>(
        IHttpContextAccessor httpContextAccessor,
        ITokenRoleClaimBuilder<T_EntityId> tokenRoleClaimBuilder,
        IPermissionsService<T_RoleId> permissionsService
    ) : ICurrentUserPermissionsProvider, ICurrentUserEntityPermissionsProvider<T_EntityId>
    {
        protected IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public virtual async Task<IEnumerable<string>> GetCurrentUserPermissionsAsync(
            CancellationToken cancellationToken = default
        )
        {
            var tokenRoles = await GetTokenRolesAsync(cancellationToken);
            if (tokenRoles is null)
            {
                return [];
            }

            var rolesPermissions = await GetRolePermissionsAsync(
                tokenRoles.UserRoles,
                cancellationToken
            );
            return rolesPermissions;
        }

        public abstract Task<bool> IsCurrentUserAuthenticatedAsync(
            CancellationToken cancellationToken = default
        );

        public virtual async Task<IEnumerable<string>> GetCurrentUserPermissionsOnEntityAsync(
            T_EntityId entityId,
            CancellationToken cancellationToken = default
        )
        {
            var tokenRoles = await GetTokenRolesAsync(cancellationToken);
            if (
                tokenRoles is null
                || !tokenRoles.UserEntitiesRoles.TryGetValue(entityId, out var userEntityRoles)
                || !userEntityRoles.Any()
            )
            {
                return [];
            }
            var userRolesPermissions = await GetRolePermissionsAsync(
                tokenRoles.UserRoles,
                cancellationToken
            );
            if (!userEntityRoles.Any())
            {
                // If user has no roles on entity, then use its global roles
                return userRolesPermissions;
            }
            var entityRolesPermissions = await GetRolePermissionsAsync(
                userEntityRoles,
                cancellationToken
            );
            // Returns roles which are both in user roles and entity roles
            // If some roles are in entity roles but not in user roles, then they won't be returned
            return userRolesPermissions.Intersect(entityRolesPermissions);
        }

        protected virtual async Task<IRoleToken<T_EntityId>?> GetTokenRolesAsync(
            CancellationToken cancellationToken
        )
        {
            var claims = _httpContextAccessor.HttpContext?.User?.Claims ?? [];
            if (!claims.Any())
            {
                return null;
            }
            return await tokenRoleClaimBuilder.ParseRolesClaimsAsync(claims, cancellationToken);
        }

        protected virtual async Task<IEnumerable<string>> GetRolePermissionsAsync(
            IEnumerable<string> rolesIdStr,
            CancellationToken cancellationToken
        )
        {
            var tokenRolesId = rolesIdStr.Select(ParseRoleId);
            var rolesPermissions = await permissionsService.GetRolePermissionsAsync(
                tokenRolesId,
                cancellationToken
            );
            return rolesPermissions.Select(p => p.Code);
        }

        protected abstract T_RoleId ParseRoleId(string roleIdStr);
    }
}
