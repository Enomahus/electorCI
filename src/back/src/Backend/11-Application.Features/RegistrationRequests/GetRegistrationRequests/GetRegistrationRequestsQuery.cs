using Application.Common.Enums;
using Application.Features.RegistrationRequests.Common;
using Pcea.Core.Net.Authorization.Application.Attributes;

namespace Application.Features.RegistrationRequests.GetRegistrationRequests
{
    [WithPermission([nameof(AppPermission.GetRegistrationRequests)])]
    public class GetRegistrationRequestsQuery
        : GetRegistrationRequestsBase<GetRegistrationRequestsResponse> { }
}
