using Application.Common.Enums;
using Application.Common.Interfaces.Services;
using Application.Features.Common.Citizen;
using Application.Features.Common.GridData;
using Application.Features.RegistrationRequests.Common;
using Application.Models;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.EntityFrameworkCore;
using Pcea.Core.Net.Authorization.Application.Attributes;
using Tools.Logging;

namespace Application.Features.RegistrationRequests.GetRegistrationRequests
{
    [WithPermission([nameof(AppPermission.GetRegistrationRequests)])]
    public class GetRegistrationRequestsQuery
        : GetRegistrationRequestsBase<GetRegistrationRequestsResponse> { }

    public class GetRegistrationRequestsQueryValidator
        : GetRegistrationRequestsQueryValidatorBase<
            GetRegistrationRequestsQuery,
            GetRegistrationRequestsResponse
        > { }

    public class GetRegistrationRequestsQueryHandler(
        ReadOnlyDbContext context,
        ICurrentUserService currentUserService
    ) : GetRegistrationRequestsHandlerBase<GetRegistrationRequestsQuery, GetRegistrationRequestsResponse>()
    {
        public override async Task<Result<GridDataResponse<GetRegistrationRequestsResponse>>> Handle(
            GetRegistrationRequestsQuery query,
            CancellationToken cancellationToken
        )
        {
            using var activity = ActivitySourceLog.CQRS.Start();

            var registrationRequests = await context
                .RegistrationRequests.AsNoTracking()
                .AsSplitQuery()
                .Where(r => r.AuthorId == currentUserService.UserId)
                .Include(r => r.District)
                .Include(r => r.Author)
                .Include(r => r.Citizen)
                    .ThenInclude(c => c.Father)
                .Include(r => r.Citizen)
                    .ThenInclude(c => c.Mother)
                .ToListAsync(cancellationToken);

            var rows = registrationRequests.Select(MapToResponse).AsQueryable();

            var result = rows.ApplyGrid(query);

            return Result<GridDataResponse<GetRegistrationRequestsResponse>>.From(result);
        }

        protected override GetRegistrationRequestsResponse MapToResponse(
            RegistrationRequestDao registrationRequest
        ) =>
            new()
            {
                Id = registrationRequest.Id,
                RequestReference = registrationRequest.Reference,
                RequestDate = registrationRequest.SubmissionDate,
                RequestType = registrationRequest.RequestType,
                Status = registrationRequest.Status,
                DistrictId = registrationRequest.DistrictId,
                DistrictName = registrationRequest.District.Wording,
                Comment = registrationRequest.ReasonForRejection ?? string.Empty,
                Citizen = CitizenModel.FromDao(registrationRequest.Citizen),
                CanBeDeleted = ComputeCanBeDeleted(registrationRequest),
                CreatedAt = registrationRequest.SubmissionDate,
            };
    }
}
