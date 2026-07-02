using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Persistence.Entities
{
    public class UserRoleDao : IdentityUserRole<Guid>
    {
        public virtual UserDao User { get; set; }
        public virtual RoleDao Role { get; set; }
    }
}
