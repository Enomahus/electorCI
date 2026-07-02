using Pcea.Core.Net.Authorization.Persistence.Interfaces.Domain;

namespace Pcea.Core.Net.Authorization.Persistence.Interfaces.Services
{
    /// <summary>
    /// Service to get permissions from role
    /// Usually, the implementation is a cache service
    /// </summary>
    /// <typeparam name="T_RoleId"></typeparam>
    public interface IPermissionsService<T_RoleId>
    {
        public Task<IEnumerable<IPermission>> GetRolePermissionsAsync(
            IEnumerable<T_RoleId> rolesId,
            CancellationToken token
        );
    }
}
