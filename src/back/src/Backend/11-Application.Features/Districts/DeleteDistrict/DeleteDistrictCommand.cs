using Application.Common.Enums;
using Application.Exceptions;
using Application.Models;
using Application.Models.Errors;
using FluentValidation;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Pcea.Core.Net.Authorization.Application.Attributes;
using Tools.Logging;

namespace Application.Features.Districts.DeleteDistrict
{
    [WithPermission(nameof(AppPermission.DeleteDistrict))]
    public class DeleteDistrictCommand(long id) : IRequest<Result>
    {
        public long Id { get; set; } = id;
    }

    public class DeleteDistrictCommandValidator : AbstractValidator<DeleteDistrictCommand>
    {
        private readonly ReadOnlyDbContext _context;

        public DeleteDistrictCommandValidator(ReadOnlyDbContext context)
        {
            _context = context;

            RuleFor(c => c.Id)
                .NotEmpty()
                .WithMessage(ValidationErrorCode.Required.ToString())
                .DependentRules(() =>
                {
                    RuleFor(c => c.Id)
                        .MustAsync(
                            (pollingStationId, token) =>
                                _context.PollingStations.AnyAsync(
                                    ps => ps.Id == pollingStationId,
                                    token
                                )
                        )
                        .WithMessage(ValidationErrorCode.DistrictMustExist.ToString());
                });
        }
    }

    public class DeleteConstituencyCommandHandler(WritableDbContext context)
        : IRequestHandler<DeleteDistrictCommand, Result>
    {
        public async Task<Result> Handle(
            DeleteDistrictCommand command,
            CancellationToken cancellationToken
        )
        {
            using var activity = ActivitySourceLog.CQRS.Start().AddParameter(command, c => c.Id);

            var districtDao =
                await context.Districts.FirstOrDefaultAsync(
                    c => c.Id == command.Id,
                    cancellationToken
                ) ?? throw new NotFoundException(nameof(DistrictDao), command.Id);

            context.Districts.Remove(districtDao);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Default();
        }
    }
}
