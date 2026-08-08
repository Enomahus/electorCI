using Application.Features.RegistrationRequests.Common;

namespace Application.Features.RegistrationRequests.GetRegistrationRequestsForAdmin
{
    public record GetRegistrationRequestsFormAdminResponse : GetRegistrationRequestsResponseModel
    {
        public required string AuthorName { get; set; }
    }
}
