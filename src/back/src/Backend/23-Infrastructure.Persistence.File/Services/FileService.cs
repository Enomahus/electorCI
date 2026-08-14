using Application.Common.Interfaces.Services;
using Application.Exceptions;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.File.Configurations;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Persistence.File.Services
{
    public class FileService(
        BlobServiceClient blobServiceClient,
        IOptions<StorageConfiguration> storageConfig,
        WritableDbContext context
    ) : IFileService
    {
        public async Task<Guid> UploadFileAsync(
            Stream stream,
            string fileName,
            string contentType,
            CancellationToken cancellationToken,
            Guid? existingDocumentId = null
        )
        {
            var guid = Guid.Empty;
            var strategy = context.Database.CreateExecutionStrategy();
            await strategy.ExecuteInTransactionAsync(
                async () =>
                {
                    guid = await UploadFileNoTransactionAsync(
                        stream,
                        fileName,
                        contentType,
                        cancellationToken,
                        existingDocumentId
                    );
                },
                () => Task.FromResult(true)
            );

            return guid;
        }

        public async Task<Guid> UploadFileNoTransactionAsync(
            Stream stream,
            string fileName,
            string contentType,
            CancellationToken token,
            Guid? existingDocumentId = null
        )
        {
            DocumentDao document;
            if (existingDocumentId is null)
            {
                document = new DocumentDao()
                {
                    FileName = fileName,
                    ContentType = contentType,
                    FileSize = stream.Length,
                };
                context.Add(document);
            }
            else
            {
                document =
                    await context.Documents.FirstOrDefaultAsync(x => x.Id == existingDocumentId, token)
                    ?? throw new NotFoundException(nameof(DocumentDao), existingDocumentId);

                document.FileName = fileName;
                document.ContentType = contentType;
                document.FileSize = stream.Length;
            }

            await context.SaveChangesAsync(token);
            try
            {
                BlobClient blobClient = await GetBlobClientAsync(document.Id, token);
                await blobClient.UploadAsync(stream, true, token);
            }
            catch (Exception ex)
            {
                throw new StorageException("Error while uploading file to storage.", ex);
            }
            return document.Id;
        }

        public async Task<Stream> GetFileDownloadStreamAsync(
            Guid documentId,
            CancellationToken cancellationToken
        )
        {
            var blobClient = await GetBlobClientAsync(documentId, cancellationToken);

            Response<BlobDownloadStreamingResult>? result;
            try
            {
                result = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                throw new StorageException("Error while creating file stream from storage.", ex);
            }
            if (!result.HasValue)
            {
                throw new StorageException("No file found in storage.");
            }
            return result.Value.Content;
        }

        public async Task DeleteFileByIdAsync(Guid documentId, CancellationToken cancellationToken)
        {
            var document =
                await context.Documents.FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken)
                ?? throw new NotFoundException(nameof(DocumentDao), documentId);
            await DeleteFileAsync(document, cancellationToken);
            await context.SaveChangesAsync(cancellationToken: default);
        }

        private async Task DeleteFileAsync(DocumentDao document, CancellationToken cancellationToken)
        {
            BlobClient blobClient = await GetBlobClientAsync(document.Id, cancellationToken);

            try
            {
                await blobClient.DeleteAsync(cancellationToken: default);
            }
            catch (RequestFailedException ex)
            {
                if (ex.Status != 404)
                {
                    throw new StorageException("Error while deleting file", ex);
                }
            }
            context.Documents.Remove(document);
        }

        public async Task DeleteFilesByIdsAsync(
            IEnumerable<Guid> documentIds,
            CancellationToken cancellationToken
        )
        {
            var documents = await context
                .Documents.Where(d => documentIds.Contains(d.Id))
                .ToListAsync(cancellationToken);

            foreach (var document in documents)
            {
                await DeleteFileAsync(document, cancellationToken: default);
            }

            //await context.Documents
            //    .Where(d => documentIds.Contains(d.Id))
            //    .ExecuteDeleteAsync(cancellationToken);

            await context.SaveChangesAsync(cancellationToken: default);
        }

        private async Task<BlobClient> GetBlobClientAsync(
            Guid documentId,
            CancellationToken cancellationToken
        )
        {
            var containerClient = blobServiceClient.GetBlobContainerClient(
                storageConfig.Value.BlobContainerName
            );
            await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
            var blobClient = containerClient.GetBlobClient(documentId.ToString());
            return blobClient;
        }
    }
}
