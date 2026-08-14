using Application.Common.Interfaces.Services;
using Application.Exceptions;
using Application.Models;
using Application.Models.Errors;
using FluentValidation;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tools.Logging;

namespace Application.Features.Document.DocumentDownload
{
    public class DocumentDownloadQuery : IRequest<Result<DocumentDownloadResponse>>
    {
        public Guid DocumentId { get; set; }
    }

    public class DocumentDownloadQueryValidator : AbstractValidator<DocumentDownloadQuery>
    {
        public DocumentDownloadQueryValidator()
        {
            RuleFor(c => c.DocumentId).NotNull().WithMessage(ValidationErrorCode.Required.ToString());
        }
    }

    public class DocumentDownloadQueryHandler(ReadOnlyDbContext context, IFileService fileService)
        : IRequestHandler<DocumentDownloadQuery, Result<DocumentDownloadResponse>>
    {
        public async Task<Result<DocumentDownloadResponse>> Handle(
            DocumentDownloadQuery request,
            CancellationToken cancellationToken
        )
        {
            using var activity = ActivitySourceLog.CQRS.Start().AddParameter(request, r => r.DocumentId);

            var document =
                await context.Documents.FirstOrDefaultAsync(
                    d => d.Id == request.DocumentId,
                    cancellationToken
                ) ?? throw new NotFoundException(nameof(DocumentDao), request.DocumentId);

            var fileContent = await fileService.GetFileDownloadStreamAsync(
                request.DocumentId,
                cancellationToken
            );

            var response = new DocumentDownloadResponse()
            {
                FileName = document.FileName,
                ContentType = document.ContentType,
                Content = fileContent,
            };

            return Result<DocumentDownloadResponse>.From(response);
        }
    }
}
