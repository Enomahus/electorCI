using Application.Common.Enums;
using Infrastructure.Persistence.Entities;

namespace Application.Features.Common.Citizen
{
    public class BasicCitizenModel
    {
        public Gender Gender { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTimeOffset BirthDate { get; set; }
        public string? BirthPlace { get; set; }
        public string? Nationality { get; set; }

        public CitizenDao ToDao()
        {
            return new()
            {
                Gender = Gender,
                FirstName = FirstName,
                LastName = LastName,
                BirthDate = BirthDate,
                BirthPlace = BirthPlace,
                Nationality = Nationality,
            };
        }
    }
}
