using Application.Common.Enums;
using Infrastructure.Persistence.Entities;

namespace Application.Features.Common.Elector
{
    public class ElectorModel
    {
        public DateTimeOffset? RegistrationDate { get; set; }
        public ElectorStatus Status { get; set; }
        public Guid CitizenId { get; set; }
        public long PollingStationId { get; set; }

        public ElectorDao ToDao(long? pollingStationId, Guid? citizenId)
        {
            return new ElectorDao
            {
                RegistrationDate = RegistrationDate!.Value,
                Status = Status,
                PollingStationId = pollingStationId!.Value,
                Id = citizenId!.Value,
            };
        }
    }
}
