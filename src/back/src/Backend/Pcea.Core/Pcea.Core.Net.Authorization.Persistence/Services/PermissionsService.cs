using Pcea.Core.Net.Authorization.Persistence.Interfaces.Domain;
using Pcea.Core.Net.Authorization.Persistence.Interfaces.Services;

namespace Pcea.Core.Net.Authorization.Persistence.Services
{
    public class PermissionsService<T_IdRole>(IPermissionsProvider<T_IdRole> permissionsProvider)
        : IPermissionsService<T_IdRole>
    {
        public virtual async Task<IEnumerable<IPermission>> GetRolePermissionsAsync(
            IEnumerable<T_IdRole> rolesId,
            CancellationToken token
        )
        {
            var rolePermissionsName = await permissionsProvider.FetchRolePermissionsAsync(
                rolesId,
                token
            );
            return rolePermissionsName;
        }
    }
}
