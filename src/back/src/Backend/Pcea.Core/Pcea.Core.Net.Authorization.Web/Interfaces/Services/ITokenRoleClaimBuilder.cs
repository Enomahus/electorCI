using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Pcea.Core.Net.Authorization.Web.Interfaces.Services
{
    public interface IRoleToken<T_EntityId>
    {
        public IEnumerable<string> UserRoles { get; set; }
        public IDictionary<T_EntityId, IEnumerable<string>> UserEntitiesRoles { get; set; }
    }

    public class RoleToken<T_EntityId> : IRoleToken<T_EntityId>
        where T_EntityId : notnull
    {
        public IEnumerable<string> UserRoles { get; set; } = [];
        public IDictionary<T_EntityId, IEnumerable<string>> UserEntitiesRoles { get; set; } =
            new Dictionary<T_EntityId, IEnumerable<string>>();
    }

    public interface ITokenRoleClaimBuilderBase
    {
        public static readonly string ENTITY_ROLE_CLAIM_TYPE = "EntityRole";
        public static readonly string ROLE_CLAIM_TYPE = "Role";
    }

    public interface ITokenRoleClaimBuilder<T_EntityId> : ITokenRoleClaimBuilderBase
    {
        public Task<IEnumerable<Claim>> BuildRolesClaimsAsync(
            IEnumerable<string> userRoles,
            IDictionary<T_EntityId, IEnumerable<string>> userEntitiesRoles,
            CancellationToken cancellationToken = default
        );

        public Task<IRoleToken<T_EntityId>> ParseRolesClaimsAsync(
            IEnumerable<Claim> claims,
            CancellationToken cancellationToken = default
        );
    }
}
