using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Pcea.Core.Net.Authorization.Web.Interfaces.Services;

namespace Pcea.Core.Net.Authorization.Web.Services
{
    public abstract class TokenRoleClaimBuilder<T_EntityId> : ITokenRoleClaimBuilder<T_EntityId>
        where T_EntityId : notnull
    {
        public Task<IEnumerable<Claim>> BuildRolesClaimsAsync(
            IEnumerable<string> userRoles,
            IDictionary<T_EntityId, IEnumerable<string>> userEntitiesRoles,
            CancellationToken cancellationToken = default
        )
        {
            return Task.FromResult(
                userRoles
                    .Select(p => new Claim(ITokenRoleClaimBuilderBase.ROLE_CLAIM_TYPE, p))
                    .Concat(
                        userEntitiesRoles.Select(
                            (p) =>
                                new Claim(
                                    ITokenRoleClaimBuilderBase.ENTITY_ROLE_CLAIM_TYPE,
                                    GetEntityRoleToClaimValue(p.Key, p.Value)
                                )
                        )
                    )
            );
        }

        public Task<IRoleToken<T_EntityId>> ParseRolesClaimsAsync(
            IEnumerable<Claim> claims,
            CancellationToken cancellationToken = default
        )
        {
            var result = new RoleToken<T_EntityId>
            {
                UserRoles = claims
                    .Where(c => c.Type == ITokenRoleClaimBuilderBase.ROLE_CLAIM_TYPE)
                    .Select(c => c.Value),
                UserEntitiesRoles = claims
                    .Where(c => c.Type == ITokenRoleClaimBuilderBase.ENTITY_ROLE_CLAIM_TYPE)
                    .Select(c => GetEntityRoleFromClaimValue(c.Value))
                    .ToDictionary((values) => values.entityId, (values) => values.roles),
            };
            return Task.FromResult<IRoleToken<T_EntityId>>(result);
        }

        protected virtual string GetEntityRoleToClaimValue(
            T_EntityId entityId,
            IEnumerable<string> roles
        )
        {
            return $"{entityId}_{string.Join(';', roles)}";
        }

        protected virtual (
            T_EntityId entityId,
            IEnumerable<string> roles
        ) GetEntityRoleFromClaimValue(string claimValue)
        {
            var keySplitted = claimValue.Split('_', 2);
            var entityId = keySplitted[0];
            var roles = keySplitted[1].Split(';', StringSplitOptions.RemoveEmptyEntries);
            return (ParseEntityId(entityId), roles);
        }

        protected abstract T_EntityId ParseEntityId(string entityId);
    }
}
