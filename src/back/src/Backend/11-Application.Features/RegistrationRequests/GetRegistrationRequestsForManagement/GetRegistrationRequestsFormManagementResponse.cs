using Application.Features.RegistrationRequests.Common;

namespace Application.Features.RegistrationRequests.GetRegistrationRequestsForManagement
{
    public record GetRegistrationRequestsFormManagementResponse : GetRegistrationRequestsResponseModel
    {
        public required string AuthorName { get; set; }
    }
}
