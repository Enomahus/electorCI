using System;
using System.Collections.Generic;
using System.Text;

namespace Pcea.Core.Net.Authorization.Application.Interfaces.Services
{
    /// <summary>
    /// Service to provide current user permissions on en entity
    /// </summary>
    /// <typeparam name="T_EntityId"></typeparam>
    public interface ICurrentUserEntityPermissionsProvider<T_EntityId>
    {
        // <summary>
        /// Get current user permissions on entity
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<IEnumerable<string>> GetCurrentUserPermissionsOnEntityAsync(
            T_EntityId entityId,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Is there a current user
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<bool> IsCurrentUserAuthenticatedAsync(
            CancellationToken cancellationToken = default
        );
    }
}
