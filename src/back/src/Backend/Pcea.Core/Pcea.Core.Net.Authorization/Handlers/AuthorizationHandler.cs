using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using Pcea.Core.Net.Authorization.Interfaces.Handlers;
using Pcea.Core.Net.Authorization.Models;

namespace Pcea.Core.Net.Authorization.Handlers
{
    public class AuthorizationHandler : IAuthorizationHandler
    {
        public IEnumerable<string> RequiredPermissionsCodes { get; set; } = [];
        public IEnumerable<string> PermissionsCodes { get; set; } = [];

        public virtual Task BuildAsync(
            IEnumerable<string> requiredPermissionsCodes,
            IEnumerable<string> permissionsCodes,
            CancellationToken cancellationToken = default
        )
        {
            RequiredPermissionsCodes = requiredPermissionsCodes;
            PermissionsCodes = permissionsCodes;
            return Task.CompletedTask;
        }

        public virtual Task<AuthorizationResult> HandleAsync(
            CancellationToken cancellationToken = default
        )
        {
            ThrowNullParameterExceptionIfNeeded(RequiredPermissionsCodes);
            ThrowNullParameterExceptionIfNeeded(PermissionsCodes);

            // Check permissions
            var result = new AuthorizationResult() { IsAuthorized = true };
            var missingPermission = RequiredPermissionsCodes.FirstOrDefault(c =>
                !PermissionsCodes.Contains(c)
            );
            if (missingPermission == default)
            {
                result.AdditionalData.Add(AuthorizationResult.PERMISSION_FOUND_CODE, null);
            }
            else
            {
                result.IsAuthorized = false;
                result.AdditionalData.Add(
                    AuthorizationResult.MISSING_PERMISSION_CODE,
                    missingPermission
                );
            }
            return Task.FromResult(result);
        }

        private static void ThrowNullParameterExceptionIfNeeded<T>(
            [NotNull] T? parameter,
            [CallerArgumentExpression(nameof(parameter))] string argumentName = ""
        )
        {
            if (parameter is null)
            {
                throw new InvalidOperationException(
                    $"{argumentName} is null. Have you called {nameof(BuildAsync)} before calling {nameof(HandleAsync)}?"
                );
            }
        }
    }
}
