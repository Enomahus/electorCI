using Application.Common.Enums;
using Application.Features.Common.GridData;
using Application.Models;
using Infrastructure.Persistence.SQLServer.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Pcea.Core.Net.Authorization.Application.Attributes;
using Tools.Logging;

namespace Application.Features.Districts.GetDistricts
{
    [WithPermission(nameof(AppPermission.GetDistricts))]
    public class GetDistrictsQuery : GridDataQuery, IRequest<Result<GridDataResponse<GetDistrictsResponse>>>
    {
    }

    public class GetDistrictsQueryValidator : GridDataQueryValidator<GetDistrictsQuery>
    {
        public GetDistrictsQueryValidator() { }
    }

    public class GetDistrictsQueryHandler(
        ReadOnlyDbContext context,
        TimeProvider timeProvider
    ) : IRequestHandler<GetDistrictsQuery, Result<GridDataResponse<GetDistrictsResponse>>>
    {
        public async Task<Result<GridDataResponse<GetDistrictsResponse>>> Handle(
            GetDistrictsQuery request,
            CancellationToken cancellationToken
        )
        {
            using var activity = ActivitySourceLog.CQRS.Start();
            var dateNow = timeProvider.GetUtcNow();

            var districts = await context
                .Districts.AsNoTracking()
                .AsSplitQuery()
                .Include(r => r.Subconstituency)
                .ThenInclude(d => d.Subconstituency)
                .ThenInclude(sp => sp.Subconstituency)
                .ThenInclude(m => m.Subconstituency)
                .ThenInclude(vs => vs.Subconstituency)
                .Include(p => p.PollingStations)
                .Where(r => r.ParentId == null)
                .ToListAsync(cancellationToken);

            var rows = districts.Select(d => GetDistrictsResponse.From(d, dateNow)).AsQueryable();

            var result = rows.ApplyGrid(request);

            return Result<GridDataResponse<GetDistrictsResponse>>.From(result);
        }
    }
}
