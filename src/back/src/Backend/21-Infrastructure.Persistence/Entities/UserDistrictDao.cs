using System.ComponentModel.DataAnnotations.Schema;
using Infrastructure.Persistence.Common;

namespace Infrastructure.Persistence.Entities
{
    public class UserDistrictDao : EntityBaseDao<Guid>
    {
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public UserDao User { get; set; }
        public long DistrictId { get; set; }

        [ForeignKey(nameof(DistrictId))]
        public DistrictDao District { get; set; }
        public virtual ICollection<RoleDao> SpecificRoles { get; set; } = [];
    }
}
