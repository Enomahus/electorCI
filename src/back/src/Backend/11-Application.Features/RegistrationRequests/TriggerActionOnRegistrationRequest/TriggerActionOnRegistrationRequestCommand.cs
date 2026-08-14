using Application.Common.Enums;
using Application.Common.Interfaces.Services;
using Application.Exceptions;
using Application.Interfaces.Services;
using Application.Models;
using Application.Models.Errors;
using FluentValidation;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Pcea.Core.Net.Authorization.Application.Attributes;
using Tools.Constants;
using Tools.Logging;

namespace Application.Features.RegistrationRequests.TriggerActionOnRegistrationRequest
{
    [WithPermission(nameof(AppPermission.TriggerActionOnRegistrationRequest))]
    public class TriggerActionOnRegistrationRequestCommand : IRequest<Result<Guid>>
    {
        public Guid RequestId { get; set; }
        public RegistrationStatus NewStatus { get; set; }
        public string? Comment { get; set; }
    }

    public class TriggerActionOnRegistrationRequestCommandValidator
        : AbstractValidator<TriggerActionOnRegistrationRequestCommand>
    {
        public TriggerActionOnRegistrationRequestCommandValidator()
        {
            RuleFor(v => v.RequestId).NotEmpty().WithMessage(ValidationErrorCode.Required.ToString());
            RuleFor(v => v.NewStatus).IsInEnum();
            RuleFor(v => v.Comment)
                .NotEmpty()
                .When(v => v.NewStatus == RegistrationStatus.Rejected)
                .WithMessage(ValidationErrorCode.Required.ToString())
                .MaximumLength(500);
        }
    }

    public class TriggerActionOnRegistrationRequestCommandHandler(
        WritableDbContext context,
        TimeProvider timeProvider,
        ICurrentUserService currentUserService,
        IRegistrationService registrationService
    ) : IRequestHandler<TriggerActionOnRegistrationRequestCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(
            TriggerActionOnRegistrationRequestCommand command,
            CancellationToken cancellationToken
        )
        {
            using var activity = ActivitySourceLog.CQRS.Start().AddParameter(command, r => r.RequestId);

            var dateNow = timeProvider.GetUtcNow();
            var currentUserId = currentUserService.UserId;

            var registrationRequest =
                await context
                    .RegistrationRequests.AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == command.RequestId, cancellationToken)
                ?? throw new NotFoundException(nameof(RegistrationRequestDao), command.RequestId);

            if (command.NewStatus == RegistrationStatus.Rejected)
            {
                registrationRequest.Status = RegistrationStatus.Rejected;
                registrationRequest.ReasonForRejection = command.Comment;
            }
            else if (command.NewStatus == RegistrationStatus.Approved)
            {
                await ProcessApprovalAsync(registrationRequest, dateNow, cancellationToken);
                registrationRequest.Status = RegistrationStatus.Approved;
            }

            registrationRequest.LastUpdaterId = currentUserId;

            context.RegistrationRequests.Update(registrationRequest);
            await context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.From(registrationRequest.Id);
        }

        private async Task ProcessApprovalAsync(
            RegistrationRequestDao dao,
            DateTimeOffset dateNow,
            CancellationToken cancellationToken
        )
        {
            var alreadyElector = await context
                .Electors.AsNoTracking()
                .AnyAsync(e => e.Id == dao.CitizenId && e.Status == ElectorStatus.Active, cancellationToken);

            if (alreadyElector)
                throw new Exception("The citizen is already registered as an elector.");

            var pollingStation = await GetOrCreateAvailablePollingStationAsync(
                dao.DistrictId,
                cancellationToken
            );

            string voterNumber = await registrationService.GenerateElectorNumberAsync(
                context,
                pollingStation.Id,
                cancellationToken
            );

            // Création du profil Électeur
            var elector = new ElectorDao
            {
                Id = dao.CitizenId, // Relation 1:1 avec Citizen
                VoterRegistrationNumber = voterNumber,
                RegistrationDate = dateNow,
                Status = ElectorStatus.Active,
                PollingStationId = pollingStation.Id,
                CreatedAt = dateNow,
                ModifiedAt = dateNow,
            };
            context.Electors.Add(elector);
        }

        private async Task<PollingStationDao> GetOrCreateAvailablePollingStationAsync(
            long districtId,
            CancellationToken cancellationToken
        )
        {
            const int maxElectorsPerStation = AppConstants.MAX_ELECTORS_PER_STATION;
            var now = timeProvider.GetUtcNow();

            var district =
                await context
                    .Districts.Include(ps => ps.PollingStations)
                        .ThenInclude(pse => pse.Electors)
                    .FirstOrDefaultAsync(c => c.Id == districtId, cancellationToken)
                ?? throw new NotFoundException(nameof(DistrictDao), districtId);

            if (district.Level != ElectoralDistrictLevel.VotingLocation)
                throw new InvalidOperationException("The specified district is not a voting location.");

            var availableStation = district.PollingStations.FirstOrDefault(ps =>
                ps.Electors.Count < maxElectorsPerStation
            );

            if (availableStation is not null)
                return availableStation;

            int nextStationNumber =
                district.PollingStations.Count == 0
                    ? 1
                    : district
                        .PollingStations.Select(ps => int.TryParse(ps.StationNumber, out var n) ? n : 0)
                        .Max() + 1;

            //: district.PollingStations.Max(ps => int.Parse(ps.StationNumber)) + 1;

            // Création d'un nouveau bureau de vote si aucun n'existe pour le district
            var newPollingStation = new PollingStationDao
            {
                StationNumber = nextStationNumber.ToString("D2"),
                DistrictId = districtId,
                Wording = $"Bureau de vote - {districtId}",
                CreatedAt = now,
                ModifiedAt = now,
            };
            context.PollingStations.Add(newPollingStation);
            await context.SaveChangesAsync(cancellationToken);

            return newPollingStation;
        }
    }
}
