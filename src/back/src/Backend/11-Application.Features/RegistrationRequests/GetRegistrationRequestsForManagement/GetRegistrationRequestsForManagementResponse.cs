using Application.Features.RegistrationRequests.Common;

namespace Application.Features.RegistrationRequests.GetRegistrationRequestsForManagement
{
    public record GetRegistrationRequestsForManagementResponse : GetRegistrationRequestsResponseModel
    {
        public required string AuthorName { get; set; }
    }
}
