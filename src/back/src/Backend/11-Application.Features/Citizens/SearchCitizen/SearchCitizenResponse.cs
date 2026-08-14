using Application.Common.Enums;

namespace Application.Features.Citizens.SearchCitizen
{
    public class SearchCitizenResponse
    {
        public required Guid Id { get; set; }
        public required Gender Gender { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required DateTimeOffset BirthDate { get; set; }
        public required string BirthPlace { get; set; }
        public required string Nationality { get; set; }
    }
}
