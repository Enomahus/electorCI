namespace Pcea.Core.Net.Authorization.Application.Attributes
{
    /// <summary>
    /// Specifies the class this attribute is applied to requires entity authorization.
    /// This attribute should be used on classes which implements <see cref="Requests.IEntityAuthorizedRequest{T_Id}"/>
    /// which should hold the id of the entity to check for permissions
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class EntityPermissionAttribute : Attribute, IPermissionsHolder
    {
        /// <summary>
        /// A user which should have at least one of those permissions on the entity with
        /// permission to be able to access the feature.
        /// </summary>
        /// <value></value>
        public IEnumerable<string> Permissions { get; } = [];

        public EntityPermissionAttribute() { }

        public EntityPermissionAttribute(string[] permissions)
        {
            Permissions = permissions;
        }
    }
}
