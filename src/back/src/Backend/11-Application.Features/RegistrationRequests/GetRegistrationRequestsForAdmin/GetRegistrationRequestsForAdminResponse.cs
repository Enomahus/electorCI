using Application.Features.RegistrationRequests.Common;

namespace Application.Features.RegistrationRequests.GetRegistrationRequestsForAdmin
{
    public record GetRegistrationRequestsForAdminResponse : GetRegistrationRequestsResponseModel
    {
        public required string AuthorName { get; set; }
    }
}
