using Application.Features.Common.District;
using Infrastructure.Persistence.Entities;

namespace Application.Features.Districts.GetDistrict
{
    public class GetDistrictResponse : DistrictModel
    {
        public required long Id { get; set; }
        public string? Code { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public static GetDistrictResponse FromDao(DistrictDao dao, DateTimeOffset dateNow)
        {
            return new GetDistrictResponse
            {
                Id = dao.Id,
                Code = dao.Code,
                Wording = dao.Wording,
                Level = dao.Level,
                CreatedAt = dao.CreatedAt,
                IsActive = dao.DisabledDate == null || dao.DisabledDate > dateNow
            };
        }
    }
}
