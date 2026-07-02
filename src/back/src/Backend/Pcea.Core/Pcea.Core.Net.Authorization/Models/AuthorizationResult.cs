using System;
using System.Collections.Generic;
using System.Text;

namespace Pcea.Core.Net.Authorization.Models
{
    public class AuthorizationResult
    {
        public static readonly string PERMISSION_FOUND_CODE = "PermissionFound";

        public static readonly string MISSING_PERMISSION_CODE = "MissingPermission";
        public bool IsAuthorized { get; set; }

        /// <summary>
        /// Dictionary of errors. Key is error kind, value is explanation
        /// <example>
        /// For found permissions, value is the permission found
        /// {"PermissionFound", "ReadEntity"}
        /// </example>
        /// </summary>
        /// <value></value>
        public IDictionary<string, object?> AdditionalData { get; set; } =
            new Dictionary<string, object?>();
    }
}
