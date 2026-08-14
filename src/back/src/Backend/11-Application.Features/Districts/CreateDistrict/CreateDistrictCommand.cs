using Application.Common.Enums;
using Application.Exceptions;
using Application.Features.Common.District;
using Application.Models;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Pcea.Core.Net.Authorization.Application.Attributes;
using Tools.Logging;

namespace Application.Features.Districts.CreateDistrict
{
    [WithPermission(nameof(AppPermission.CreateDistrict))]
    public class CreateDistrictCommand : DistrictModel, IRequest<Result<long>> { }

    public class CreateDistrictCommandValidator(ReadOnlyDbContext context)
        : DistrictValidatorBase<CreateDistrictCommand>(context) { }

    public class CreateDistrictCommandHandler(WritableDbContext context)
        : IRequestHandler<CreateDistrictCommand, Result<long>>
    {
        public async Task<Result<long>> Handle(
            CreateDistrictCommand command,
            CancellationToken cancellationToken
        )
        {
            using var activity = ActivitySourceLog.CQRS.Start();

            // Générer le code de la circonscription
            var code = await GenerateDistrictCode(command, cancellationToken);

            var newEntity = new DistrictDao
            {
                Code = code,
                Wording = command.Wording,
                Level = command.Level,
                ParentId = command.ParentId,
            };

            await context.Districts.AddAsync(newEntity, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Result<long>.From(newEntity.Id);
        }

        private async Task<string> GenerateDistrictCode(
            CreateDistrictCommand command,
            CancellationToken cancellationToken
        )
        {
            string parentIdPrefix = string.Empty;

            if (command.ParentId.HasValue)
            {
                var parent =
                    await context
                        .Districts.AsNoTracking()
                        .Where(d => d.Id == command.ParentId)
                        .Select(c => c.Code)
                        .FirstOrDefaultAsync(cancellationToken)
                    ?? throw new NotFoundException(nameof(DistrictDao), command.ParentId.Value);

                parentIdPrefix = parent;
            }

            var maxExistingCode = await context
                .Districts.Where(d => d.Level == command.Level && d.ParentId == command.ParentId)
                .Select(c => c.Code)
                .MaxAsync(cancellationToken);

            int nextSequence = 1;

            if (!string.IsNullOrEmpty(maxExistingCode))
            {
                string lastSequenceStr = maxExistingCode.Substring(maxExistingCode.Length - 3);

                if (int.TryParse(lastSequenceStr, out int maxSequence))
                {
                    nextSequence = maxSequence + 1;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Le code existant '{maxExistingCode}' a un format invalide."
                    );
                }
            }

            return $"{parentIdPrefix}{nextSequence:D3}";
        }
    }
}
