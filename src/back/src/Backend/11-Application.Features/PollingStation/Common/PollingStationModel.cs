using Infrastructure.Persistence.Entities;

namespace Application.Features.PollingStation.Common
{
    public class PollingStationModel
    {
        public string Wording { get; set; } = string.Empty;
        public long DistrictId { get; set; }
        public bool IsActive { get; set; }

        public static PollingStationModel FromDao(PollingStationDao dao, DateTimeOffset dateNow)
        {
            return new PollingStationModel
            {
                Wording = dao.Wording,
                DistrictId = dao.DistrictId,
                IsActive = dao.DisabledDate is null || dao.DisabledDate > dateNow,
            };
        }

        public PollingStationDao ToDao()
        {
            return new PollingStationDao()
            {
                Wording = Wording!,
                DistrictId = DistrictId,
            };
        }
    }
}
