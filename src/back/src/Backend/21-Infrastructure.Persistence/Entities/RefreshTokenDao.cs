using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Infrastructure.Persistence.Common;

namespace Infrastructure.Persistence.Entities
{
    public class RefreshTokenDao : EntityBaseDao<Guid>
    {
        [Required]
        public required DateTimeOffset Expiry { get; set; }

        public Guid? UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public UserDao User { get; set; }
    }
}
