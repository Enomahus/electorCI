using Application.Common.Interfaces.Services;
using Application.Exceptions;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.File.Configurations;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Persistence.File.Services
{
    public class LocalFileService(
        IOptions<StorageConfiguration> storageConfig,
        WritableDbContext context
    ) : IFileService
    {
        private readonly string _rootPath = Path.GetFullPath(storageConfig.Value.LocalStoragePath);

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
                Directory.CreateDirectory(_rootPath);
                await using var fileStream = System.IO.File.Create(GetFilePath(document.Id));
                await stream.CopyToAsync(fileStream, token);
            }
            catch (Exception ex)
            {
                throw new StorageException("Error while uploading file to storage.", ex);
            }
            return document.Id;
        }

        public Task<Stream> GetFileDownloadStreamAsync(
            Guid documentId,
            CancellationToken cancellationToken
        )
        {
            var filePath = GetFilePath(documentId);
            if (!System.IO.File.Exists(filePath))
            {
                throw new StorageException("No file found in storage.");
            }

            try
            {
                return Task.FromResult<Stream>(System.IO.File.OpenRead(filePath));
            }
            catch (Exception ex)
            {
                throw new StorageException("Error while creating file stream from storage.", ex);
            }
        }

        public async Task DeleteFileByIdAsync(Guid documentId, CancellationToken cancellationToken)
        {
            var document =
                await context.Documents.FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken)
                ?? throw new NotFoundException(nameof(DocumentDao), documentId);
            DeleteFile(document);
            context.Documents.Remove(document);
            await context.SaveChangesAsync(cancellationToken);
        }

        private void DeleteFile(DocumentDao document)
        {
            try
            {
                System.IO.File.Delete(GetFilePath(document.Id));
            }
            catch (Exception ex)
            {
                throw new StorageException("Error while deleting file", ex);
            }
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
                DeleteFile(document);
                context.Documents.Remove(document);
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        private string GetFilePath(Guid documentId) => Path.Combine(_rootPath, documentId.ToString());
    }
}
