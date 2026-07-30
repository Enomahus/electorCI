using Application.Exceptions;
using Application.Models;
using Application.Models.Errors;
using FluentValidation;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text;
using Tools.Logging;

namespace Application.Features.PollingStation.GetPollingStation
{
    public class GetPollingStationQuery : IRequest<Result<GetPollingStationResponse>>
    {
        public long Id { get; set; }
    }
    public class GetPollingStationQueryValidator : AbstractValidator<GetPollingStationQuery>
    {
        public GetPollingStationQueryValidator()
        {
            RuleFor(v => v.Id).NotEmpty().WithMessage(ValidationErrorCode.Required.ToString());
        }
    }

    public class GetPollingStationByIdQueryHandler(ReadOnlyDbContext context, TimeProvider timeProvider)
    : IRequestHandler<GetPollingStationQuery, Result<GetPollingStationResponse>>
    {
        public async Task<Result<GetPollingStationResponse>> Handle(
            GetPollingStationQuery query,
            CancellationToken cancellationToken
        )
        {
            using var activity = ActivitySourceLog.CQRS.Start();

            var dateNow = timeProvider.GetUtcNow();

            var dao =
                await context
                    .PollingStations.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken)
                ?? throw new NotFoundException(nameof(PollingStationDao), query.Id);

            var model = GetPollingStationResponse.Fromdao(dao, dateNow);

            return Result<GetPollingStationResponse>.From(model);
        }
    }
}
