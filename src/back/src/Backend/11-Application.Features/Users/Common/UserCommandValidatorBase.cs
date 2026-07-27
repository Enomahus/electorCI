using Application.Features.Common.District;
using Application.Models.Errors;
using FluentValidation;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.EntityFrameworkCore;
using Tools.Constants;

namespace Application.Features.Users.Common
{
    public class UserCommandValidatorBase<T> : AbstractValidator<T>
        where T : UserModel
    {
        protected readonly ReadOnlyDbContext _context;

        public UserCommandValidatorBase(ReadOnlyDbContext context, bool validateRoles = true)
        {
            _context = context;

            RuleFor(v => v.FirstName)
                .NotEmpty()
                .WithMessage(ValidationErrorCode.Required.ToString())
                .MaximumLength(50)
                .WithMessage(ValidationErrorCode.MaxLength.ToString());

            RuleFor(v => v.LastName)
                .NotEmpty()
                .WithMessage(ValidationErrorCode.Required.ToString())
                .MaximumLength(50)
                .WithMessage(ValidationErrorCode.MaxLength.ToString());

            RuleFor(u => u.Phone)
                .NotEmpty()
                .WithMessage(ValidationErrorCode.Required.ToString())
                .MaximumLength(50)
                .WithMessage(ValidationErrorCode.MaxLength.ToString());

            RuleFor(v => v.Email)
                .NotEmpty()
                .WithMessage(ValidationErrorCode.Required.ToString())
                .DependentRules(() =>
                {
                    RuleFor(v => v.Email)
                        .EmailAddress()
                        .WithMessage(ValidationErrorCode.InvalidEmail.ToString());
                    RuleFor(v => v.Email!)
                        .MustAsync(BeUniqueEmailAsync)
                        .WithMessage(ValidationErrorCode.Unique.ToString());
                });

            WhenAsync(
                async (model, token) =>
                    await _context.Roles.AnyAsync(
                        r =>
                            model.Roles.Contains(r.Id)
                            && (
                                r.Name == AppConstants.SuperAdminRole
                                || r.Name == AppConstants.OrganismAgentRole
                            ),
                        token
                    ),
                () =>
                {
                    RuleFor(v => v.EmployeeNumber)
                        .NotEmpty()
                        .WithMessage(ValidationErrorCode.Required.ToString());
                }
            );

            When(
                v => v.NewDistrict is null,
                () =>
                {
                    RuleFor(v => v.DistrictId)
                        .NotEmpty()
                        .WithMessage(ValidationErrorCode.Required.ToString())
                        .DependentRules(() =>
                        {
                            RuleFor(v => v.DistrictId)
                                .MustAsync(DistrictExistsAsync)
                                .WithMessage(ValidationErrorCode.DistrictMustExist.ToString());
                        });
                }
            );

            When(
                v => v.DistrictId is null,
                () =>
                {
                    RuleFor(v => v.NewDistrict)
                        .NotEmpty()
                        .WithMessage(ValidationErrorCode.Required.ToString())
                        .DependentRules(() =>
                        {
                            RuleFor(v => v.NewDistrict!)
                                .SetValidator(new DistrictValidatorBase<DistrictModel>(context));
                        });
                }
            );

            if (validateRoles)
            {
                RuleFor(v => v.Roles)
                    .NotEmpty()
                    .WithMessage(ValidationErrorCode.Required.ToString())
                    .DependentRules(() =>
                    {
                        RuleFor(v => v.Roles)
                            .MustAsync(RolesExistAsync)
                            .WithMessage(ValidationErrorCode.RoleMustExist.ToString());
                    });
            }
        }

        protected virtual Task<bool> BeUniqueEmailAsync(
            T command,
            string email,
            CancellationToken token
        )
        {
            return _context.Users.AllAsync(u => u.UserName != email && u.Email != email, token);
        }

        private Task<bool> DistrictExistsAsync(
            long? stakeholderId,
            CancellationToken cancellationToken
        )
        {
            return _context.Districts.AnyAsync(s => s.Id == stakeholderId, cancellationToken);
        }

        private async Task<bool> RolesExistAsync(
            List<Guid> rolesList,
            CancellationToken cancellationToken
        )
        {
            var existingRolesCount = await _context
                .Roles.Where(r => rolesList.Contains(r.Id))
                .CountAsync(cancellationToken);
            return existingRolesCount == rolesList.Count;
        }
    }
}
