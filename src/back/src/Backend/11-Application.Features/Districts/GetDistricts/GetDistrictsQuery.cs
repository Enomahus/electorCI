using Application.Common.Enums;
using Application.Models;
using FluentValidation;
using Infrastructure.Persistence.SQLServer.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Pcea.Core.Net.Authorization.Application.Attributes;
using Tools.Logging;

namespace Application.Features.Districts.GetDistricts
{
    [WithPermission(nameof(AppPermission.GetDistricts))]
    public class GetDistrictsQuery : IRequest<Result<IEnumerable<GetDistrictsResponse>>> { }

    public class GetDistrictsQueryValidator : AbstractValidator<GetDistrictsQuery>
    {
        public GetDistrictsQueryValidator() { }
    }

    public class GetDistrictsQueryHandler(ReadOnlyDbContext context, TimeProvider timeProvider)
        : IRequestHandler<GetDistrictsQuery, Result<IEnumerable<GetDistrictsResponse>>>
    {
        public async Task<Result<IEnumerable<GetDistrictsResponse>>> Handle(
            GetDistrictsQuery request,
            CancellationToken cancellationToken
        )
        {
            using var activity = ActivitySourceLog.CQRS.Start();
            var dateNow = timeProvider.GetUtcNow();

            var rootsDistricts = await context
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

            var rows = rootsDistricts.Select(d => GetDistrictsResponse.From(d, dateNow));

            return Result<IEnumerable<GetDistrictsResponse>>.From(rows);
        }
    }
}
