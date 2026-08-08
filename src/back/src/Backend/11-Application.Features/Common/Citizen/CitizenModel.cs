using Application.Common.Enums;
using Application.Features.Common.Elector;
using Infrastructure.Persistence.Entities;

namespace Application.Features.Common.Citizen
{
    public class CitizenModel
    {
        public Gender Gender { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTimeOffset BirthDate { get; set; }
        public string? BirthPlace { get; set; }
        public MaritalStatus MaritalStatus { get; set; }
        public string? MarriedName { get; set; }
        public string? Nationality { get; set; }
        public string? Profession { get; set; }
        public string? Email { get; set; }
        public string? PhysicalAddress { get; set; }
        public string? PostalAddress { get; set; }
        public Guid? FatherId { get; set; }
        public CitizenModel? Father { get; set; }
        public Guid? MotherId { get; set; }
        public CitizenModel? Mother { get; set; }
        public BasicCitizenModel? NewFather { get; set; }
        public BasicCitizenModel? NewMather { get; set; }
        public ElectorModel? Elector { get; set; }

        public static CitizenModel FromDao(CitizenDao dao)
        {
            return new CitizenModel()
            {
                Gender = dao.Gender,
                FirstName = dao.FirstName,
                LastName = dao.LastName,
                BirthDate = dao.BirthDate,
                BirthPlace = dao.BirthPlace,
                MaritalStatus = dao.MaritalStatus,
                MarriedName = dao.MarriedName,
                Profession = dao.Profession,
                Email = dao.Email,
                Nationality = dao.Nationality,
                PhysicalAddress = dao.PhysicalAddress,
                PostalAddress = dao.PostalAddress,
                FatherId = dao.FatherId,
                Father = dao.Father != null ? FromDao(dao.Father) : null,
                MotherId = dao.MotherId,
                Mother = dao.Mother != null ? FromDao(dao.Mother) : null,
            };
        }

        public CitizenDao ToDao()
        {
            return new CitizenDao()
            {
                Gender = Gender,
                FirstName = FirstName,
                LastName = LastName,
                BirthDate = BirthDate,
                BirthPlace = BirthPlace,
                MaritalStatus = MaritalStatus,
                MarriedName = MarriedName,
                Profession = Profession,
                Email = Email,
                Nationality = Nationality,
                PhysicalAddress = PhysicalAddress,
                PostalAddress = PostalAddress,
                FatherId = FatherId,
                Father = Father?.ToDao(),
                MotherId = MotherId,
                Mother = Mother?.ToDao(),
            };
        }
    }
}
