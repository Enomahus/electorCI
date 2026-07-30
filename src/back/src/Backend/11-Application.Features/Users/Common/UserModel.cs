using Application.Common.Enum;
using Application.Common.Enums;
using Application.Features.Common.District;
using Infrastructure.Persistence.Entities;

namespace Application.Features.Users.Common
{
    public class UserModel
    {
        public PersonTitle Title { get; set; }
        public string UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public List<Guid> Roles { get; set; } = [];
        public string EmployeeNumber { get; set; }
        public bool IsActive { get; set; }

        public long? DistrictId { get; set; }

        public DistrictModel? NewDistrict { get; set; }
        public AuthProvider? AuthProvider { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ActivationDate { get; set; }

        public static UserModel FromDao(UserDao dao, DateTimeOffset now)
        {
            var model = new UserModel();
            MapDaoToModel(dao, model, now);
            return model;
        }

        public static void MapDaoToModel(UserDao dao, UserModel model, DateTimeOffset now)
        {
            model.Title = dao.Civility;
            model.UserName = dao.UserName ?? "";
            model.FirstName = dao.FirstName;
            model.LastName = dao.LastName;
            model.Email = dao.Email;
            model.Phone = dao.PhoneNumber;
            model.IsActive = dao.DisabledDate is null || dao.DisabledDate > now;
            model.Roles = [.. dao.UserRoles.Select(ur => ur.RoleId)];
            model.EmployeeNumber = dao.EmployeeNumber;
            model.DistrictId = dao.UserDistricts.FirstOrDefault()?.DistrictId;
            model.CreatedAt = dao.CreatedAt;
            model.AuthProvider = dao.AuthProvider;
        }
    }
}
