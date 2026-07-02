using System;
using System.Collections.Generic;
using System.Text;
using Pcea.Core.Net.Authorization.Models;

namespace Pcea.Core.Net.Authorization.Interfaces.Handlers
{
    /// <summary>
    /// Service to handle authorization decision
    /// </summary>
    public interface IAuthorizationHandler
    {
        /// <summary>
        /// Build the handler by providing everything necessary for it to handle authorization
        /// </summary>
        /// <param name="requiredPermissionsCodes">List of required permissions</param>
        /// <param name="permissionsCodes">List of permissions to take into account for the decision making</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task BuildAsync(
            IEnumerable<string> requiredPermissionsCodes,
            IEnumerable<string> permissionsCodes,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Authorization logic
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>Authorization result with <see cref="AuthorizationResult.IsAuthorized"/> being the authorization decision</returns>
        public Task<AuthorizationResult> HandleAsync(CancellationToken cancellationToken = default);
    }
}
