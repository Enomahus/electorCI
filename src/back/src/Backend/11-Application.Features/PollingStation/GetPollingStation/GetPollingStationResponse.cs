using Application.Features.PollingStation.Common;
using Infrastructure.Persistence.Entities;

namespace Application.Features.PollingStation.GetPollingStation
{
    public class GetPollingStationResponse : PollingStationModel
    {
        public required long Id { get; set; }
        public string StationNumber { get; set; } = string.Empty;

        public static GetPollingStationResponse Fromdao(PollingStationDao dao, DateTimeOffset dateNow)
        {
            return new GetPollingStationResponse
            {
                Id = dao.Id,
                StationNumber = dao.StationNumber,
                Wording = dao.Wording,
                DistrictId = dao.DistrictId,
                IsActive = dao.DisabledDate is null || dao.DisabledDate > dateNow,
            };
        }
    }
}
