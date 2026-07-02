using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Infrastructure.Persistence.Common;

namespace Infrastructure.Persistence.Entities
{
    public class PollingStationDao : EntityBaseDao<long>, ITimestampedEntity
    {
        [Required]
        public string StationNumber { get; set; } // Bureau No: 04

        [Required]
        public string Wording { get; set; } // Lieu de vote: LYON
        public DateTimeOffset? DisabledDate { get; set; }

        // Relation vers la circonscription (ex: Lieu de vote)
        public long DistrictId { get; set; }

        [ForeignKey(nameof(DistrictId))]
        public virtual DistrictDao District { get; set; } = null!;

        public virtual ICollection<ElectorDao> Electors { get; set; } = [];
        public DateTimeOffset ModifiedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
