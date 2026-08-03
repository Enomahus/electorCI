using Application.Common.Enums;
using Application.Features.Common.GridData;
using Application.Models;
using Infrastructure.Persistence.SQLServer.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Pcea.Core.Net.Authorization.Application.Attributes;
using Tools.Logging;

namespace Application.Features.PollingStation.GetPollingStations
{
    [WithPermission([nameof(AppPermission.GetPollingStations)])]
    public class GetPollingStationsQuery
        : GridDataQuery,
            IRequest<Result<GridDataResponse<GetPollingStationsResponse>>> { }

    public class GetPollingStationsQueryValidator
        : GridDataQueryValidator<GetPollingStationsQuery> { }

    public class GetPollingStationsQueryHandler(
        ReadOnlyDbContext context,
        TimeProvider timeProvider
    )
        : IRequestHandler<
            GetPollingStationsQuery,
            Result<GridDataResponse<GetPollingStationsResponse>>
        >
    {
        public async Task<Result<GridDataResponse<GetPollingStationsResponse>>> Handle(
            GetPollingStationsQuery request,
            CancellationToken cancellationToken
        )
        {
            using var activity = ActivitySourceLog.CQRS.Start();

            var now = timeProvider.GetUtcNow();

            var pollingStations = await context
                .PollingStations.AsNoTracking()
                .AsSplitQuery()
                .Include(ps => ps.District)
                    .ThenInclude(c => c.Parent)
                        .ThenInclude(c => c.Parent)
                            .ThenInclude(c => c.Parent)
                                .ThenInclude(c => c.Parent)
                .ToListAsync(cancellationToken);

            var rows = pollingStations
                .Select(ps => new GetPollingStationsResponse(
                    (long?)ps.District.Parent.Parent.Parent.Parent.Id,
                    ps.District.Parent.Parent.Parent.Parent.Code,
                    ps.District.Parent.Parent.Parent.Parent.Wording,
                    (long?)ps.District.Parent.Parent.Parent.Id,
                    ps.District.Parent.Parent.Parent.Code,
                    ps.District.Parent.Parent.Parent.Wording,
                    (long?)ps.District.Parent.Parent.Id,
                    ps.District.Parent.Parent.Code,
                    ps.District.Parent.Parent.Wording,
                    (long?)ps.District.Parent.Id,
                    ps.District.Parent.Code,
                    ps.District.Parent.Wording,
                    (long?)ps.District.Id,
                    ps.District.Code,
                    ps.District.Wording,
                    ps.Id,
                    ps.StationNumber,
                    !ps.DisabledDate.HasValue || ps.DisabledDate >= now,
                    ps.DisabledDate
                ))
                .AsQueryable();

            var result = rows.ApplyGrid(request);

            return Result<GridDataResponse<GetPollingStationsResponse>>.From(result);
        }
    }
}
