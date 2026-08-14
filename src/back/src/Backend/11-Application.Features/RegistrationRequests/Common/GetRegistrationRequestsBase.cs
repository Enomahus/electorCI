using Application.Common.Enums;
using Application.Features.Common.GridData;
using Application.Models;
using Infrastructure.Persistence.Entities;
using MediatR;

namespace Application.Features.RegistrationRequests.Common
{
    public abstract class GetRegistrationRequestsBase<T_Response>
        : GridDataQuery,
            IRequest<Result<GridDataResponse<T_Response>>>
        where T_Response : GetRegistrationRequestsResponseModel { }

    public abstract class GetRegistrationRequestsQueryValidatorBase<T_Query, T_Response>
        : GridDataQueryValidator<T_Query>
        where T_Query : GetRegistrationRequestsBase<T_Response>
        where T_Response : GetRegistrationRequestsResponseModel { }

    public abstract class GetRegistrationRequestsHandlerBase<T_Query, T_Response>()
        : IRequestHandler<T_Query, Result<GridDataResponse<T_Response>>>
        where T_Query : GetRegistrationRequestsBase<T_Response>
        where T_Response : GetRegistrationRequestsResponseModel
    {
        public abstract Task<Result<GridDataResponse<T_Response>>> Handle(
            T_Query request,
            CancellationToken cancellationToken
        );

        protected abstract T_Response MapToResponse(RegistrationRequestDao registrationRequest);

        protected static bool ComputeCanBeDeleted(RegistrationRequestDao registrationRequest) =>
            registrationRequest.Status is RegistrationStatus.Draft or RegistrationStatus.ToBeProcessed;
    }
}
