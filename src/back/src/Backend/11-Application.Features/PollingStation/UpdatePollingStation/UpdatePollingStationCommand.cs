using Application.Common.Enums;
using Application.Exceptions;
using Application.Features.PollingStation.Common;
using Application.Models;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Pcea.Core.Net.Authorization.Application.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using Tools.Logging;

namespace Application.Features.PollingStation.UpdatePollingStation
{
    [WithPermission(nameof(AppPermission.UpdatePollingStation))]
    public class UpdatePollingStationCommand : PollingStationModel, IRequest<Result<long>>
    {
        public long Id { get; set; }
    }
    public class UpdatePollingStationCommandValidator : PollingStationValidatorBase<UpdatePollingStationCommand>
    {
        public UpdatePollingStationCommandValidator(ReadOnlyDbContext context) : base(context)
        {
        }
    }

    public class UpdatePollingStationCommandHandler(WritableDbContext context)
    : IRequestHandler<UpdatePollingStationCommand, Result<long>>
    {
        public async Task<Result<long>> Handle(
            UpdatePollingStationCommand command,
            CancellationToken cancellationToken
        )
        {
            using var activity = ActivitySourceLog.CQRS.Start();

            var existingEntity = await context.PollingStations
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken)
                ?? throw new NotFoundException(nameof(PollingStationDao), command.Id);

            existingEntity.Wording = command.Wording!;
            existingEntity.DistrictId = command.DistrictId;

            context.PollingStations.Update(existingEntity);
            await context.SaveChangesAsync(cancellationToken);

            return Result<long>.From(existingEntity.Id);
        }
    }
}
