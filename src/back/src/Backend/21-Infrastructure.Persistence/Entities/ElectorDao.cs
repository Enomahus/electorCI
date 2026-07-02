using System.ComponentModel.DataAnnotations;
using Application.Common.Enums;
using Infrastructure.Persistence.Common;

namespace Infrastructure.Persistence.Entities
{
    public class ElectorDao : EntityBaseDao<Guid>, ITimestampedEntity
    {
        [MaxLength(50)]
        public string VoterRegistrationNumber { get; set; } // V 0034 6601 11

        public DateTimeOffset RegistrationDate { get; set; }
        public ElectorStatus Status { get; set; } = ElectorStatus.Active;

        public virtual CitizenDao Citizen { get; set; } = null!;

        public DateTimeOffset ModifiedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public long PollingStationId { get; set; }
        public virtual PollingStationDao PollingStation { get; set; } = null!;
    }
}
