using Application.Common.Enums;
using Application.Models;
using Application.Models.Errors;
using FluentValidation;
using Infrastructure.Persistence.SQLServer.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Pcea.Core.Net.Authorization.Application.Attributes;
using Tools.Logging;

namespace Application.Features.PollingStation.ToggleActivatePollingStation
{
    [WithPermission(nameof(AppPermission.UpdatePollingStation))]
    public class ToggleActivatePollingStationCommand(long id) : IRequest<Result>
    {
        public long Id { get; set; } = id;
    }

    public class ToggleActivatePollingStationCommandValidator
        : AbstractValidator<ToggleActivatePollingStationCommand>
    {
        private readonly ReadOnlyDbContext _context;

        public ToggleActivatePollingStationCommandValidator(ReadOnlyDbContext context)
        {
            _context = context;

            RuleFor(v => v.Id)
                .NotEmpty()
                .WithMessage(ValidationErrorCode.Required.ToString())
                .DependentRules(() =>
                {
                    RuleFor(ps => ps.Id)
                        .MustAsync(
                            (stationId, token) =>
                                _context.PollingStations.AnyAsync(ps => ps.Id == stationId, token)
                        )
                        .WithMessage(ValidationErrorCode.PollingStationMustExist.ToString());
                });
        }
    }

    public class ToogleActivatePollingStationCommandHandler(
        WritableDbContext context,
        TimeProvider timeProvider
    ) : IRequestHandler<ToggleActivatePollingStationCommand, Result>
    {
        public async Task<Result> Handle(ToggleActivatePollingStationCommand command, CancellationToken cancellationToken)
        {
            using var activity = ActivitySourceLog.CQRS.Start().AddParameter(command, ps => ps.Id);

            var dateNow = timeProvider.GetUtcNow();

            var pollingStationDao = await context.PollingStations.FirstAsync(
                ps => ps.Id == command.Id,
                cancellationToken
            );

            DateTimeOffset? disableDate = pollingStationDao.DisabledDate.HasValue ? null : dateNow;

            pollingStationDao.DisabledDate = disableDate;

            context.PollingStations.Update(pollingStationDao);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Default();

        }
    }



}
