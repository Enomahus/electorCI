using Application.Common.Enums;
using Application.Features.Common.District;
using Application.Features.Users.Common;
using Infrastructure.Persistence.Entities;

namespace Application.Features.Users.GetCurrentUser
{
    public class GetCurrentUserResponse : UserModel
    {
        public Guid Id { get; private set; }
        public List<AppPermission> Permissions { get; set; } = [];
        public DistrictModel? CurrentUserDistrict { get; set; }

        public static GetCurrentUserResponse FromDao(
            UserDao userDao,
            List<AppPermission> permissions,
            DateTimeOffset now
        )
        {
            var response = new GetCurrentUserResponse()
            {
                Id = userDao.Id,
                Permissions = permissions,
                CurrentUserDistrict = userDao
                    .UserDistricts.Select(us => us.District)
                    .Where(s => s.DisabledDate == null || s.DisabledDate > now)
                    .Select(dao => DistrictModel.FromDao(dao, now))
                    .FirstOrDefault(),
            };
            UserModel.MapDaoToModel(userDao, response, now);

            return response;
        }
    }
}
