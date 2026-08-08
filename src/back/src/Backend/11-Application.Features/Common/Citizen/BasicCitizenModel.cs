using Application.Common.Enums;

namespace Application.Features.Common.Citizen
{
    public class BasicCitizenModel
    {
        public Gender Gender { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTimeOffset? BirthDate { get; set; }
        public string? BirthPlace { get; set; }
        public string? Nationality { get; set; }
        public MaritalStatus? MaritalStatus { get; set; }
    }
}
