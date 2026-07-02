using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Common.Enums;
using Infrastructure.Persistence.Common;

namespace Infrastructure.Persistence.Entities
{
    public class DistrictDao : EntityBaseDao<long>, ITimestampedEntity
    {
        [Required]
        public string Code { get; set; }

        [Required, MaxLength(50)]
        public string Wording { get; set; }

        [Required]
        public ElectoralDistrictLevel Level { get; set; }
        public DateTimeOffset? DisabledDate { get; set; }

        public DateTimeOffset ModifiedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        // Auto-référence : le parent de cette zone
        public long? ParentId { get; set; }

        [ForeignKey(nameof(ParentId))]
        public virtual DistrictDao Parent { get; set; }

        // Liste des zones enfants (ex: la France a plusieurs villes)
        public virtual ICollection<DistrictDao> Subconstituency { get; set; } = [];

        // Liste des bureaux de vote rattachés à cette zone précise (souvent le dernier niveau)
        public virtual ICollection<PollingStationDao> PollingStations { get; set; } = [];

        public virtual ICollection<RegistrationRequestDao> RegistrationRequests { get; set; } = [];
        public virtual ICollection<UserDistrictDao> UserDistricts { get; set; } = [];
    }
}
