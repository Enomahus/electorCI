using Application.Common.Enums;

namespace Infrastructure.ExternalAuth.Models
{
    public class ExternallyAuthenticatedPersonModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public required AuthProvider AuthProvider { get; set; }
    }
}
