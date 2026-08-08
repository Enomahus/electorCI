using Application.Common.Enums;
using Application.Features.Common.Citizen;
using Application.Features.Common.GridData;
using Application.Features.RegistrationRequests.Common;
using Application.Features.RegistrationRequests.GetRegistrationRequests;
using Application.Models;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.EntityFrameworkCore;
using Pcea.Core.Net.Authorization.Application.Attributes;
using Tools.Logging;

namespace Application.Features.RegistrationRequests.GetRegistrationRequestsForAdmin
{
    [WithPermission([nameof(AppPermission.GetRegistrationRequestForAdmin)])]
    public class GetRegistrationRequestsForAdminQuery
        : GetRegistrationRequestsBase<GetRegistrationRequestsForAdminResponse> { }

    public class GetRegistrationRequestsForAdminQueryValidator
        : GetRegistrationRequestsQueryValidatorBase<
            GetRegistrationRequestsForAdminQuery,
            GetRegistrationRequestsForAdminResponse
        > { }

    public class GetRegistrationRequestsForAdminQueryHandler(ReadOnlyDbContext context)
        : GetRegistrationRequestsHandlerBase<
            GetRegistrationRequestsForAdminQuery,
            GetRegistrationRequestsForAdminResponse
        >()
    {
        public override async Task<Result<GridDataResponse<GetRegistrationRequestsForAdminResponse>>> Handle(
            GetRegistrationRequestsForAdminQuery request,
            CancellationToken cancellationToken
        )
        {
            using var activity = ActivitySourceLog.CQRS.Start();

            var registrationRequests = await context
                .RegistrationRequests.AsNoTracking()
                .AsSplitQuery()
                .Include(r => r.District)
                .Include(r => r.Author)
                .Include(r => r.Citizen)
                    .ThenInclude(c => c.Father)
                .Include(r => r.Citizen)
                    .ThenInclude(c => c.Mother)
                .ToListAsync(cancellationToken);

            var rows = registrationRequests.Select(MapToResponse).AsQueryable();

            var result = rows.ApplyGrid(request);

            return Result<GridDataResponse<GetRegistrationRequestsForAdminResponse>>.From(result);
        }

        protected override GetRegistrationRequestsForAdminResponse MapToResponse(
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
                CanBeDeleted = true,
                CreatedAt = registrationRequest.SubmissionDate,
                AuthorName = $"{registrationRequest.Author.FirstName} {registrationRequest.Author.LastName}",
            };
    }
}
