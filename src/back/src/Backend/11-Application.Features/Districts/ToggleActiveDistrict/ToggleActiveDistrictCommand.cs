using Application.Common.Enums;
using Application.Models;
using Application.Models.Errors;
using FluentValidation;
using Infrastructure.Persistence.SQLServer.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Pcea.Core.Net.Authorization.Application.Attributes;
using Tools.Logging;

namespace Application.Features.Districts.ToggleActiveDistrict
{
    [WithPermission(nameof(AppPermission.UpdateDistrict))]
    public class ToggleActiveDistrictCommand(long id) : IRequest<Result>
    {
        public long Id { get; set; } = id;
    }

    public class ToggleActiveDistrictCommandValidator : AbstractValidator<ToggleActiveDistrictCommand>
    {
        private readonly ReadOnlyDbContext _context;

        public ToggleActiveDistrictCommandValidator(ReadOnlyDbContext context)
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
                                _context.Districts.AnyAsync(ps => ps.Id == stationId, token)
                        )
                        .WithMessage(ValidationErrorCode.DistrictMustExist.ToString());
                });
        }
    }


    public class ToggleActiveDistrictCommandHandler(
        WritableDbContext context,
        TimeProvider timeProvider
    ) : IRequestHandler<ToggleActiveDistrictCommand, Result>
    {
        public async Task<Result> Handle(ToggleActiveDistrictCommand cmd, CancellationToken cancellationToken)
        {
            using var activity = ActivitySourceLog.CQRS.Start().AddParameter(cmd, ps => ps.Id);

            var dateNow = timeProvider.GetUtcNow();

            var district = await context.Districts.FirstAsync(d => d.Id == cmd.Id, cancellationToken);

            DateTimeOffset? disableDate = district.DisabledDate.HasValue ? null : dateNow;

            district.DisabledDate = disableDate;

            context.Districts.Update(district);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Default();
        }
    }
}
