using Application.Features.Document.Common;
using Application.Models;
using Application.Models.Errors;
using FluentValidation;
using Infrastructure.Persistence.SQLServer.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tools.Logging;

namespace Application.Features.Document.GetDocumentsInfos
{
    public class GetDocumentsInfosQuery : IRequest<Result<GetDocumentsInfosResponse>>
    {
        public List<Guid>? DocumentIds { get; set; }
    }

    public class GetDocumentsInfosQueryValidator : AbstractValidator<GetDocumentsInfosQuery>
    {
        public GetDocumentsInfosQueryValidator()
        {
            RuleFor(c => c.DocumentIds).NotEmpty().WithMessage(ValidationErrorCode.Required.ToString());
        }
    }

    public class GetDocumentsInfosQueryHandler(ReadOnlyDbContext context)
        : IRequestHandler<GetDocumentsInfosQuery, Result<GetDocumentsInfosResponse>>
    {
        public async Task<Result<GetDocumentsInfosResponse>> Handle(
            GetDocumentsInfosQuery request,
            CancellationToken cancellationToken
        )
        {
            using var activity = ActivitySourceLog.CQRS.Start().AddParameters(request, r => r.DocumentIds!);

            var documents = await context
                .Documents.Where(d => request.DocumentIds!.Contains(d.Id))
                .ToListAsync(cancellationToken);

            var response = new GetDocumentsInfosResponse()
            {
                DocumentsInfos = [.. documents.Select(DocumentInfoModel.FromDao)],
            };

            return Result<GetDocumentsInfosResponse>.From(response);
        }
    }
}
