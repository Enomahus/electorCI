using Application.Audit;
using Application.Common.Enums;
using Application.Common.Interfaces.Services;
using Application.Features.RegistrationRequests.Common;
using Application.Interfaces.Services;
using Application.Models;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Pcea.Core.Net.AuditTrail.Attributes;
using Pcea.Core.Net.Authorization.Application.Attributes;
using Tools.Logging;

namespace Application.Features.RegistrationRequests.CreateRegistrationRequest;

[AuditParameters(
    Category = nameof(AuditCategory.RegistrationRequest),
    Action = nameof(AuditAction.RegistrationRequestCreated)
)]
[WithPermission(nameof(AppPermission.CreateRegistrationRequest))]
public class CreateRegistrationRequestCommand : RegistrationRequestCommandBase, IRequest<Result<Guid>> { }

public class CreateRegistrationRequestCommandValidator
    : RegistrationRequestValidationBase<CreateRegistrationRequestCommand>
{
    public CreateRegistrationRequestCommandValidator(ReadOnlyDbContext context, TimeProvider timeProvider)
        : base(context, timeProvider) { }
}

public class CreateRegistrationRequestCommandHandler(
    ICurrentUserService currentUserService,
    WritableDbContext context,
    TimeProvider timeProvider,
    IRegistrationService registrationService,
    RegistrationRequestService requestService
) : IRequestHandler<CreateRegistrationRequestCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateRegistrationRequestCommand command,
        CancellationToken cancellationToken
    )
    {
        using var activity = ActivitySourceLog.CQRS.Start();

        var dateNow = timeProvider.GetUtcNow();
        var districtId = command.RegistrationRequest!.DistrictId;

        var currentUserId = currentUserService.UserId;

        RegistrationRequestDao? registrationRequest = null;
        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteInTransactionAsync(
            async () =>
            {
                registrationRequest = command.RegistrationRequest.ToDao(districtId!.Value);
                registrationRequest.Reference = await registrationService.GenerateRequestReferenceAsync(
                    context,
                    timeProvider,
                    cancellationToken
                );

                registrationRequest.SubmissionDate = dateNow;
                registrationRequest.Status = RegistrationStatus.ToBeProcessed;
                registrationRequest.AuthorId = currentUserId!.Value;
                registrationRequest.LastUpdaterId = currentUserId;

                await context.RegistrationRequests.AddAsync(registrationRequest, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
                activity.AddParameter(registrationRequest, r => r.Id);

                //Upload documents
                await requestService.UploadRegistrationRequestDocumentsByTypeAsync(
                    [.. command.IdentityDocument],
                    RegistrationRequestDocumentType.IdentityDocumentOrNationalCertificate,
                    registrationRequest,
                    cancellationToken
                );

                await requestService.UploadRegistrationRequestDocumentsByTypeAsync(
                    [.. command.Photo],
                    RegistrationRequestDocumentType.PassportPhoto,
                    registrationRequest,
                    cancellationToken
                );

                await requestService.UploadRegistrationRequestDocumentsByTypeAsync(
                    [.. command.ResidenceCertificate],
                    RegistrationRequestDocumentType.ResidenceCertificate,
                    registrationRequest,
                    cancellationToken
                );
            },
            () => Task.FromResult(true)
        );

        return AuditResult<Guid>.From(registrationRequest!.Reference, registrationRequest.Id);
    }
}
