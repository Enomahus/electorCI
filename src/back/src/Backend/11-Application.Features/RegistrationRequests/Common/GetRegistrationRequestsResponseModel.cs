using Application.Common.Enums;
using Application.Features.Common.Citizen;

namespace Application.Features.RegistrationRequests.Common
{
    public record GetRegistrationRequestsResponseModel
    {
        public required Guid Id { get; set; }
        public required string RequestReference { get; set; }
        public required DateTimeOffset RequestDate { get; set; }
        public required RegistrationRequestType RequestType { get; set; }
        public required RegistrationStatus Status { get; set; }
        public required long DistrictId { get; set; }
        public required string DistrictName { get; set; }
        public required string Comment { get; set; }
        public required CitizenModel Citizen { get; set; }
        public required bool CanBeDeleted { get; set; }
        public required DateTimeOffset CreatedAt { get; set; }
    }
}
