using Application.Common.Enums;
using Application.Models;
using Application.Models.Errors;
using FluentValidation;
using Infrastructure.Persistence.SQLServer.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Pcea.Core.Net.Authorization.Application.Attributes;
using Tools.Logging;

namespace Application.Features.PollingStation.DeletePollingStation
{
    [WithPermission(nameof(AppPermission.DeletePollingStation))]
    public class DeletePollingStationCommand(long id) : IRequest<Result>
    {
        public long Id { get; set; } = id;
    }
    public class DeletePollingStationCommandValidator : AbstractValidator<DeletePollingStationCommand>
    {
        private readonly ReadOnlyDbContext _context;

        public DeletePollingStationCommandValidator(ReadOnlyDbContext context)
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
                        .WithMessage(ValidationErrorCode.PollingStationMustExist.ToString())
                        .DependentRules(() =>
                        {
                            RuleFor(s => s.Id)
                                .MustAsync(CheckPollingStationLinksAsync)
                                .WithMessage(ValidationErrorCode.PollingStationLinked.ToString());
                        });
                });
        }

        private Task<bool> CheckPollingStationLinksAsync(long stationId, CancellationToken cancellationToken)
        {
            return _context.PollingStations.AnyAsync(
                s => s.Id == stationId && s.Electors.Count == 0,
                cancellationToken
            );
        }
    }

    public class DeletePollingStationCommandHandler(WritableDbContext context)
        : IRequestHandler<DeletePollingStationCommand, Result>
    {
        public async Task<Result> Handle(
            DeletePollingStationCommand command,
            CancellationToken cancellationToken
        )
        {
            using var activity = ActivitySourceLog.CQRS.Start().AddParameter(command, r => r.Id);

            var pollingStation = await context.PollingStations.FirstAsync(
                ps => ps.Id == command.Id,
                cancellationToken
            );

            context.PollingStations.Remove(pollingStation);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Default();
        }
    }
}
