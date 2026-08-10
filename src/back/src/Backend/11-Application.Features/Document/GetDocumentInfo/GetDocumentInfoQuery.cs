using Application.Exceptions;
using Application.Features.Document.Common;
using Application.Models;
using Application.Models.Errors;
using FluentValidation;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tools.Logging;

namespace Application.Features.Document.GetDocumentInfo
{
    public class GetDocumentInfoQuery : IRequest<Result<GetDocumentInfoResponse>>
    {
        public Guid? DocumentId { get; set; }
    }

    public class GetDocumentInfoQueryValidator : AbstractValidator<GetDocumentInfoQuery>
    {
        public GetDocumentInfoQueryValidator()
        {
            RuleFor(c => c.DocumentId).NotNull().WithMessage(ValidationErrorCode.Required.ToString());
        }
    }

    public class GetDocumentInfoQueryHandler(ReadOnlyDbContext context)
        : IRequestHandler<GetDocumentInfoQuery, Result<GetDocumentInfoResponse>>
    {
        public async Task<Result<GetDocumentInfoResponse>> Handle(
            GetDocumentInfoQuery request,
            CancellationToken cancellationToken
        )
        {
            using var activity = ActivitySourceLog.CQRS.Start().AddParameter(request, r => r.DocumentId);

            var document =
                await context.Documents.FirstOrDefaultAsync(
                    d => d.Id == request.DocumentId,
                    cancellationToken
                ) ?? throw new NotFoundException(nameof(DocumentDao), request.DocumentId);

            var response = DocumentInfoModel.FromDao(document);
            return Result<GetDocumentInfoResponse>.From(new() { DocumentInfo = response });
        }
    }
}
