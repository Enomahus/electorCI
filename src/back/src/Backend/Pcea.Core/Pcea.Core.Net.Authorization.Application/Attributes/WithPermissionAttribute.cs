using System;
using System.Collections.Generic;
using System.Text;

namespace Pcea.Core.Net.Authorization.Application.Attributes
{
    /// <summary>
    /// Specifies that the class this attribute is applied to requires authorization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class WithPermissionAttribute : Attribute, IPermissionsHolder
    {
        /// <summary>
        /// Permissions codes required
        /// </summary>
        /// <value></value>
        public IEnumerable<string> Permissions { get; } = [];

        public WithPermissionAttribute() { }

        public WithPermissionAttribute(params string[] permissions)
        {
            Permissions = permissions;
        }
    }
}
