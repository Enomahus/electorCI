using Application.Features.Common.Citizen;
using Application.Models.Errors;
using FluentValidation;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.RegistrationRequests.Common
{
    public class RegistrationRequestValidationBase<T_Command> : AbstractValidator<T_Command>
        where T_Command : RegistrationRequestCommandBase
    {
        protected readonly ReadOnlyDbContext _context;
        protected readonly TimeProvider _timeProvider;

        public RegistrationRequestValidationBase(ReadOnlyDbContext context, TimeProvider timeProvider)
        {
            _context = context;
            _timeProvider = timeProvider;

            RuleFor(v => v.RegistrationRequest)
                .NotNull()
                .WithMessage(ValidationErrorCode.Required.ToString());

            When(
                r => r.RegistrationRequest != null,
                () =>
                {
                    RuleFor(r => r.RegistrationRequest!.DistrictId)
                        .NotNull()
                        .WithMessage(ValidationErrorCode.Required.ToString())
                        .DependentRules(() =>
                        {
                            RuleFor(r => r.RegistrationRequest!.DistrictId)
                                .MustAsync(
                                    (districtId, token) =>
                                    {
                                        return CheckDistrictMustExistAsync(districtId!.Value, token);
                                    }
                                )
                                .WithMessage(ValidationErrorCode.DistrictMustExist.ToString());
                        });

                    // Validation imbriquée du citoyen
                    When(
                        r => r.RegistrationRequest!.Citizen != null,
                        () =>
                            RuleFor(r => r.RegistrationRequest!.Citizen!)
                                .SetValidator(new CitizenValidatorBase<CitizenModel>(_context, _timeProvider))
                    );
                }
            );

            RuleFor(r => r.ResidenceCertificate)
                .NotNull()
                .WithMessage(ValidationErrorCode.Required.ToString());
            RuleFor(r => r.IdentityDocument).NotNull().WithMessage(ValidationErrorCode.Required.ToString());
            RuleFor(r => r.Photo).NotNull().WithMessage(ValidationErrorCode.Required.ToString());
        }

        protected virtual Task<bool> CheckDistrictMustExistAsync(
            long districtId,
            CancellationToken cancellationToken
        )
        {
            return _context.Districts.AnyAsync(d => d.Id == districtId, cancellationToken);
        }
    }
}
