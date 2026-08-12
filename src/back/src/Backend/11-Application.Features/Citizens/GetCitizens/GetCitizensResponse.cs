using Application.Features.Common.Citizen;
using Infrastructure.Persistence.Entities;

namespace Application.Features.Citizens.GetCitizens
{
    public class GetCitizensResponse : CitizenModel
    {
        public Guid Id { get; set; }

        public static GetCitizensResponse From(CitizenDao dao)
        {
            return new()
            {
                Id = dao.Id,
                Gender = dao.Gender,
                FirstName = dao.FirstName,
                LastName = dao.LastName,
                BirthDate = dao.BirthDate,
                BirthPlace = dao.BirthPlace,
                MaritalStatus = dao.MaritalStatus,
                MarriedName = dao.MarriedName,
                Nationality = dao.Nationality,
                Profession = dao.Profession,
                Email = dao.Email,
                PhysicalAddress = dao.PhysicalAddress,
                PostalAddress = dao.PostalAddress,
                FatherId = dao.FatherId,
                Father = dao.Father != null ? CitizenModel.FromDao(dao.Father) : null,
                Mother = dao.Mother != null ? CitizenModel.FromDao(dao.Mother) : null,
                MotherId = dao.MotherId,
            };
        }
    }
}
