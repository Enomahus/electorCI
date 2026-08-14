using Application.Common.Enums;
using Application.Common.Interfaces.Services;
using Application.Exceptions;
using Application.Exceptions.Auth;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Pcea.Core.Net.Authorization.Application.Interfaces.Services;

namespace Application.Features.RegistrationRequests.Common
{
    public class RegistrationRequestService(
        WritableDbContext context,
        ICurrentUserService currentUserService,
        ICurrentUserPermissionsProvider currentUserPermissions,
        IFileService fileService
    )
    {
        public async Task<RegistrationRequestDao> PrepareDaoForUpdate(
            RegistrationRequestCommandBase command,
            CancellationToken cancellationToken
        )
        {
            var registrationRequest =
                await context
                    .RegistrationRequests.AsNoTracking()
                    .AsSplitQuery()
                    .Include(r => r.RegistrationRequestDocuments)
                        .ThenInclude(r => r.Document)
                    .Include(r => r.Citizen)
                        .ThenInclude(c => c.Father)
                    .Include(r => r.Citizen)
                        .ThenInclude(c => c.Mother)
                    .Include(r => r.District)
                    .FirstOrDefaultAsync(r => r.Id == command.RegistrationRequest!.Id, cancellationToken)
                ?? throw new NotFoundException(
                    nameof(RegistrationRequestDao),
                    command.RegistrationRequest!.Id
                );

            var currentUserId = currentUserService.UserId;
            var currentUser =
                await context
                    .Users.Include(u => u.UserDistricts)
                        .ThenInclude(c => c.District)
                    .FirstOrDefaultAsync(u => u.Id == currentUserId, cancellationToken)
                ?? throw new NotFoundException(nameof(UserDao), currentUserId);

            var permissions = await currentUserPermissions.GetCurrentUserPermissionsAsync(cancellationToken);
            var userIsSuperAdmin = permissions.Contains(AppPermission.SuperAdmin.ToString());
            long districtId = command.RegistrationRequest!.DistrictId!.Value;

            if (
                registrationRequest.Status == RegistrationStatus.Approved
                || registrationRequest.Status == RegistrationStatus.Rejected
            )
            {
                throw new Exception("This request has already been processed and cannot be changed");
            }

            if (!userIsSuperAdmin && registrationRequest.AuthorId != currentUser.Id)
            {
                throw new UserAccessException();
            }

            await UpdateRegistrationRequestDocuments(command, registrationRequest, cancellationToken);

            command.RegistrationRequest!.ToDao(districtId);

            registrationRequest.LastUpdaterId = currentUser.Id;

            return registrationRequest;
        }

        private async Task UpdateRegistrationRequestDocuments(
            RegistrationRequestCommandBase command,
            RegistrationRequestDao registrationRequestDao,
            CancellationToken cancellationToken
        )
        {
            //Remove old documents
            await RemoveObsoleteDocuments(
                [.. command.RegistrationRequest?.IdentityDocumentIds ?? []],
                RegistrationRequestDocumentType.IdentityDocumentOrNationalCertificate,
                registrationRequestDao,
                cancellationToken
            );

            await RemoveObsoleteDocuments(
                [.. command.RegistrationRequest?.PhotoIds ?? []],
                RegistrationRequestDocumentType.PassportPhoto,
                registrationRequestDao,
                cancellationToken
            );

            await RemoveObsoleteDocuments(
                [.. command.RegistrationRequest?.ResidenceCertificateIds ?? []],
                RegistrationRequestDocumentType.ResidenceCertificate,
                registrationRequestDao,
                cancellationToken
            );

            //Upload new documents

            await UploadRegistrationRequestDocumentsByTypeAsync(
                [.. command.IdentityDocument],
                RegistrationRequestDocumentType.IdentityDocumentOrNationalCertificate,
                registrationRequestDao,
                cancellationToken
            );

            await UploadRegistrationRequestDocumentsByTypeAsync(
                [.. command.Photo],
                RegistrationRequestDocumentType.PassportPhoto,
                registrationRequestDao,
                cancellationToken
            );

            await UploadRegistrationRequestDocumentsByTypeAsync(
                [.. command.ResidenceCertificate],
                RegistrationRequestDocumentType.ResidenceCertificate,
                registrationRequestDao,
                cancellationToken
            );
        }

        protected async Task RemoveObsoleteDocuments(
            List<Guid> newAattachmentsIds,
            RegistrationRequestDocumentType docType,
            RegistrationRequestDao? existingRegistrationRequest,
            CancellationToken cancellationToken = default
        )
        {
            if (existingRegistrationRequest == null)
                return;

            var obsoleteDocuments = existingRegistrationRequest
                .RegistrationRequestDocuments.Where(d =>
                    d.RegistrationRequestDocumentType == docType
                    && !newAattachmentsIds.Contains(d.Document.Id)
                )
                .ToList();

            await fileService.DeleteFilesByIdsAsync(
                [.. obsoleteDocuments.Select(d => d.Document.Id)],
                cancellationToken
            );
        }

        public async Task UploadRegistrationRequestDocumentsByTypeAsync(
            List<IFormFile> attachments,
            RegistrationRequestDocumentType docType,
            RegistrationRequestDao existingRegistrationRequest,
            CancellationToken cancellationToken = default
        )
        {
            var existingDocumentsForType =
                existingRegistrationRequest
                    .RegistrationRequestDocuments.Where(d => d.RegistrationRequestDocumentType == docType)
                    .ToList()
                ?? [];

            var newDocuments = new List<RegistrationRequestDocumentDao>();

            foreach (var attachment in attachments)
            {
                var existingDocument = existingDocumentsForType.FirstOrDefault(doc =>
                    string.Equals(
                        doc.Document.FileName,
                        attachment.FileName,
                        StringComparison.OrdinalIgnoreCase
                    )
                );
                Guid? existingDocumentId = existingDocument?.DocumentId;

                using var stream = attachment.OpenReadStream();

                var documentId = await fileService.UploadFileNoTransactionAsync(
                    stream,
                    attachment.FileName,
                    attachment.ContentType,
                    cancellationToken
                );

                if (existingDocument is null)
                {
                    var newDocument = new RegistrationRequestDocumentDao
                    {
                        RegistrationRequestId = existingRegistrationRequest.Id,
                        RegistrationRequestDocumentType = docType,
                        DocumentId = documentId,
                    };
                    newDocuments.Add(newDocument);
                }
            }

            context.RegistrationRequestDocuments.AddRange(newDocuments);

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
