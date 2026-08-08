using Application.Features.Common.GridData;
using Application.Models;
using MediatR;
using Tools.Logging;

namespace Application.Features.RegistrationRequests.Common
{
    public class GetRegistrationRequestsBase<T_Model>
        : GridDataQuery,
            IRequest<Result<GridDataResponse<T_Model>>>
        where T_Model : GetRegistrationRequestsResponseModel { }

    public abstract class GetRegistrationRequestsHandlerBase<T_Query, T_Response>()
        : IRequestHandler<T_Query, Result<GridDataResponse<T_Response>>>
        where T_Query : GetRegistrationRequestsBase<T_Response>
        where T_Response : GetRegistrationRequestsResponseModel
    {
        public Task<Result<GridDataResponse<T_Response>>> Handle(
            T_Query request,
            CancellationToken cancellationToken
        )
        {
            using var activity = ActivitySourceLog.CQRS.Start();
        }
    }
}
