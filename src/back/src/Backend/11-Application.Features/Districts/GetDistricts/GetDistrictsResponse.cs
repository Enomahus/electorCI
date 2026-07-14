using Application.Common.Enums;
using Application.Features.PollingStation.Common;
using Infrastructure.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Districts.GetDistricts
{
    public class GetDistrictsResponse
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string Wording { get; set; }
        public ElectoralDistrictLevel Level { get; set; }
        public long? ParentId { get; set; }
        public ICollection<GetDistrictsResponse> Children { get; set; } = [];
        public ICollection<PollingStationModel> PollingStations { get; set; } = [];


        public static GetDistrictsResponse From(DistrictDao dao, DateTimeOffset now)
        {
            return new GetDistrictsResponse()
            {
                Code = dao.Code,
                Id = dao.Id,
                Level = dao.Level,
                Wording = dao.Wording,
                ParentId = dao.ParentId,
                Children = dao.Subconstituency?.Select(x => From(x, now)).ToList() ?? [],
                PollingStations =
                    dao.PollingStations?.Select(ps => PollingStationModel.FromDao(ps, now)).ToList() ?? [],
            };
        }
    }
}
