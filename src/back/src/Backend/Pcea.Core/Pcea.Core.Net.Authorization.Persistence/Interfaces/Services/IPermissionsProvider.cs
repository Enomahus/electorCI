using Pcea.Core.Net.Authorization.Persistence.Interfaces.Domain;

namespace Pcea.Core.Net.Authorization.Persistence.Interfaces.Services
{
    /// <summary>
    /// Permission persistence service
    /// </summary>
    /// <typeparam name="T_Id"></typeparam>
    public interface IPermissionsProvider<T_Id>
    {
        /// <summary>
        /// Get Role permissions. Ideally, this should use a cache
        /// </summary>
        /// <param name="rolesId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<IEnumerable<IPermission>> FetchRolePermissionsAsync(
            IEnumerable<T_Id> rolesId,
            CancellationToken token = default
        );
    }
}
