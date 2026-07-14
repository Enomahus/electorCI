using Application.Common.Enums;
using Application.Models.Errors;
using FluentValidation;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Common.District
{
    public class DistrictValidatorBase<T> : AbstractValidator<T>
        where T : DistrictModel 
    {
        protected readonly ReadOnlyDbContext _context;
        public DistrictValidatorBase(ReadOnlyDbContext context)
        {
            _context = context;

            RuleFor(v => v.Wording)
                .NotEmpty()
                .WithMessage(ValidationErrorCode.Required.ToString())
                .MaximumLength(50)
                .WithMessage(ValidationErrorCode.MaxLength.ToString())
                .MustAsync(BeUniqueNameInParentAsync)
                .WithMessage(ValidationErrorCode.AlreadyExists.ToString());

            When(
                x => x.Level == ElectoralDistrictLevel.Region,
                () =>
                {
                    RuleFor(x => x.ParentId)
                        .Must(parentId => !parentId.HasValue)
                        .WithMessage(ValidationErrorCode.InvalidParent.ToString());
                }
            );

            When(
                x => x.Level != ElectoralDistrictLevel.Region,
                () =>
                {
                    RuleFor(x => x.ParentId)
                        .NotNull()
                        .WithMessage(ValidationErrorCode.DistrictMustHaveParent.ToString())
                        .DependentRules(() =>
                        {
                            RuleFor(x => x)
                                .MustAsync(ParentHasCorrectLevelAsync)
                                .WithMessage(ValidationErrorCode.InvalidLevel.ToString());
                        });
                }
            );
        }


        private async Task<bool> BeUniqueNameInParentAsync(
            T command,
            string name,
            CancellationToken cancellationToken
        )
        {
            return !await _context.Districts.AnyAsync(
                x => x.Wording == name && x.ParentId == command.ParentId,
                cancellationToken
            );
        }

        private async Task<bool> ParentHasCorrectLevelAsync(
            T command,
            CancellationToken cancellationToken
        )
        {
            if (command.ParentId is null)
                return false;

            var parent = await _context.Districts.FirstOrDefaultAsync(
                x => x.Id == command.ParentId.Value,
                cancellationToken
            );
            if (parent is null)
                return false;

            var expectedParentLevel = (ElectoralDistrictLevel)((int)command.Level - 1);
            return parent.Level == expectedParentLevel;
        }
    }
}
