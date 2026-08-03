using Application.Common.Enum;
using Application.Common.Enums;

namespace Application.Features.Users.GetUsers
{
    public class GetUsersResponse
    {
        public required Guid Id { get; init; }
        public PersonTitle Title { get; init; }
        public required string LastName { get; init; }
        public required string FirstName { get; init; }
        public required string? Email { get; init; }
        public required string? Phone { get; init; }
        public required string? District { get; init; }
        public required bool IsActive { get; init; }
        public required bool IsAdmin { get; init; }

        public required bool CanBeDeleted { get; set; }
        public required bool CanBeToggled { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ActivationDate { get; set; }
        public IEnumerable<string>? Roles { get; set; }
        public AuthProvider? AuthProvider { get; set; }
    }
}
