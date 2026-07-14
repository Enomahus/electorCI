using Application.Common.Enums;
using Application.Models.Errors;
using FluentValidation;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.PollingStation.Common
{
    public class PollingStationValidatorBase<T> : AbstractValidator<T> where T : PollingStationModel
    {
        protected readonly ReadOnlyDbContext _context;

        public PollingStationValidatorBase(ReadOnlyDbContext context)
        {
            _context = context;

            RuleFor(x => x.Wording)
            .NotEmpty()
            .WithMessage(ValidationErrorCode.Required.ToString())
            .MaximumLength(100)
            .WithMessage(ValidationErrorCode.MaxLength.ToString());

            RuleFor(x => x.DistrictId)
                .NotEmpty()
                .WithMessage(ValidationErrorCode.Required.ToString())
                .DependentRules(() =>
                {
                    RuleFor(x => x)
                        .MustAsync(DistrictMustBeVotingLocationAsync)
                        .WithMessage(ValidationErrorCode.InvalidLevel.ToString());
                });
        }

        private async Task<bool> DistrictMustBeVotingLocationAsync(
            T model,
            CancellationToken cancellationToken
        )
        {
            if (model.DistrictId <= 0)
                return false;

            var constituency = await _context.Districts.FirstOrDefaultAsync(
                x => x.Id == model.DistrictId,
                cancellationToken
            );

            if (constituency is null)
                return false;

            // Vérifier que la circonscription est de niveau VotingLocation
            return constituency.Level == ElectoralDistrictLevel.VotingLocation;
        }
    }
}
