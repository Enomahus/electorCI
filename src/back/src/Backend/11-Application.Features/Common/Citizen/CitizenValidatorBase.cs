using Application.Common.Enums;
using Application.Models.Errors;
using FluentValidation;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Common.Citizen;

public static class CitizenValidatorBaseExtensions
{
    public static IRuleBuilderOptions<T, string> IsRequiredName<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        int maxLength = 50
    )
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage(ValidationErrorCode.Required.ToString())
            .MaximumLength(maxLength)
            .WithMessage(ValidationErrorCode.MaxLength.ToString());
    }

    public static IRuleBuilderOptions<T, DateTimeOffset> MustBeAdult<T>(
        this IRuleBuilder<T, DateTimeOffset> ruleBuilder,
        TimeProvider timeProvider
    )
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage(ValidationErrorCode.Required.ToString())
            .Must(date => date <= timeProvider.GetUtcNow().AddYears(-18))
            .WithMessage(ValidationErrorCode.MustBeAdult.ToString());
    }
}

public class CitizenValidatorBase<T> : AbstractValidator<T>
    where T : CitizenModel
{
    protected readonly ReadOnlyDbContext _context;
    protected readonly TimeProvider _timeProvider;

    public CitizenValidatorBase(ReadOnlyDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;

        ApplyBaseRules();

        RuleFor(c => c.MaritalStatus).IsInEnum();

        When(
            c => c.MaritalStatus == MaritalStatus.Married,
            () =>
            {
                RuleFor(c => c.MarriedName)
                    .NotEmpty()
                    .WithMessage(ValidationErrorCode.MarriedNameRequired.ToString());
            }
        );

        RuleFor(c => c.PhysicalAddress).NotEmpty().WithMessage(ValidationErrorCode.Required.ToString());

        RuleFor(c => c.PostalAddress).NotEmpty().WithMessage(ValidationErrorCode.Required.ToString());

        When(
            c => c.NewFather is null,
            () =>
            {
                RuleFor(c => c.FatherId)
                    .NotEmpty()
                    .WithMessage(ValidationErrorCode.Required.ToString())
                    .DependentRules(() =>
                    {
                        RuleFor(c => c.FatherId)
                            .MustAsync(CitizenMustExistsAsync)
                            .WithMessage(ValidationErrorCode.CitizenMustExist.ToString());
                    });
            }
        );

        When(
            c => c.FatherId is null,
            () =>
            {
                RuleFor(c => c.NewFather)
                    .NotNull()
                    .WithMessage(ValidationErrorCode.Required.ToString())
                    .DependentRules(() =>
                    {
                        RuleFor(c => c.NewFather!)
                            .SetValidator(new BasicCitizenModelValidator(_timeProvider));
                    });
            }
        );

        When(
            c => c.NewMother is null,
            () =>
            {
                RuleFor(c => c.MotherId)
                    .NotEmpty()
                    .WithMessage(ValidationErrorCode.Required.ToString())
                    .DependentRules(() =>
                    {
                        RuleFor(c => c.MotherId)
                            .MustAsync(CitizenMustExistsAsync)
                            .WithMessage(ValidationErrorCode.CitizenMustExist.ToString());
                    });
            }
        );

        When(
            c => c.MotherId is null,
            () =>
            {
                RuleFor(c => c.NewMother)
                    .NotNull()
                    .WithMessage(ValidationErrorCode.Required.ToString())
                    .DependentRules(() =>
                    {
                        RuleFor(c => c.NewMother!)
                            .SetValidator(new BasicCitizenModelValidator(_timeProvider));
                    });
            }
        );
    }

    private Task<bool> CitizenMustExistsAsync(Guid? citizenId, CancellationToken token)
    {
        return _context.Citizens.AnyAsync(c => c.Id == citizenId, token);
    }

    protected void ApplyBaseRules()
    {
        RuleFor(v => v.Gender).NotNull().WithMessage(ValidationErrorCode.Required.ToString());
        RuleFor(v => v.FirstName!).IsRequiredName(100);
        RuleFor(v => v.LastName!).IsRequiredName();
        RuleFor(v => v.BirthDate).MustBeAdult(_timeProvider);
        RuleFor(v => v.BirthPlace).NotEmpty().WithMessage(ValidationErrorCode.Required.ToString());
        RuleFor(v => v.Nationality!).IsRequiredName();
    }
}

public class BasicCitizenModelValidator : AbstractValidator<BasicCitizenModel>
{
    public BasicCitizenModelValidator(TimeProvider timeProvider)
    {
        var now = timeProvider.GetUtcNow();
        RuleFor(v => v.Gender).NotNull().WithMessage(ValidationErrorCode.Required.ToString());

        RuleFor(v => v.FirstName!).IsRequiredName(100);

        RuleFor(v => v.LastName!).IsRequiredName();

        RuleFor(x => x.BirthDate)
            .NotNull()
            .WithMessage(ValidationErrorCode.Required.ToString())
            .Must(date => IsAdult(date, now))
            .WithMessage(ValidationErrorCode.MustBeAdult.ToString());

        RuleFor(v => v.BirthPlace).NotEmpty().WithMessage(ValidationErrorCode.Required.ToString());
        RuleFor(v => v.Nationality!).IsRequiredName();
    }

    private static bool IsAdult(DateTimeOffset birthDate, DateTimeOffset now)
    {
        var age = now.Year - birthDate.Year;
        if (birthDate.Date > now.AddYears(-age))
            age--;

        return age >= 18;
    }
}
