using Application.Common.Enums;
using Application.Models;
using FluentValidation;
using Infrastructure.Persistence.SQLServer.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Pcea.Core.Net.Authorization.Application.Attributes;
using Tools.Logging;

namespace Application.Features.Citizens.GetCitizens
{
    [WithPermission(nameof(AppPermission.GetCitizens))]
    public class GetCitizensQuery : IRequest<Result<List<GetCitizensResponse>>> { }

    public class GetCitizensQueryValidator : AbstractValidator<GetCitizensQuery>
    {
        public GetCitizensQueryValidator() { }
    }

    public class GetCitizensQueryHandler(ReadOnlyDbContext context)
        : IRequestHandler<GetCitizensQuery, Result<List<GetCitizensResponse>>>
    {
        public async Task<Result<List<GetCitizensResponse>>> Handle(
            GetCitizensQuery request,
            CancellationToken cancellationToken
        )
        {
            using var activity = ActivitySourceLog.CQRS.Start();

            var citizens = await context
                .Citizens.AsNoTracking()
                .Include(c => c.Father)
                .Include(c => c.Mother)
                .ToListAsync(cancellationToken);

            return Result<List<GetCitizensResponse>>.From([.. citizens.Select(GetCitizensResponse.From)]);
        }
    }
}
