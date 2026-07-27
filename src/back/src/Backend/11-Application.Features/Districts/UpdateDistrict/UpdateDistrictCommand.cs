using Application.Common.Enums;
using Application.Exceptions;
using Application.Features.Common.District;
using Application.Models;
using Application.Models.Errors;
using FluentValidation;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Pcea.Core.Net.Authorization.Application.Attributes;

namespace Application.Features.Districts.UpdateDistrict
{
    [WithPermission(nameof(AppPermission.UpdateDistrict))]
    public class UpdateDistrictCommand : DistrictModel, IRequest<Result<long>>
    {
        public long? Id { get; set; }
    }

    public class UpdateDistrictCommandValidator : DistrictValidatorBase<UpdateDistrictCommand>
    {
        public UpdateDistrictCommandValidator(ReadOnlyDbContext context)
            : base(context)
        {
            RuleFor(v => v.Id).NotEmpty().WithMessage(ValidationErrorCode.Required.ToString());
        }
    }

    public class UpdateDistrictCommandHandler(WritableDbContext context)
        : IRequestHandler<UpdateDistrictCommand, Result<long>>
    {
        public async Task<Result<long>> Handle(
            UpdateDistrictCommand command,
            CancellationToken cancellationToken
        )
        {
            var entity =
                await context.Districts.FirstOrDefaultAsync(
                    x => x.Id == command.Id,
                    cancellationToken
                ) ?? throw new NotFoundException(nameof(DistrictDao), command.Id);

            entity.Wording = command.Wording;
            entity.Level = command.Level;
            entity.ParentId = command.ParentId;

            await context.SaveChangesAsync(cancellationToken);

            return Result<long>.From(entity.Id);
        }
    }
}
