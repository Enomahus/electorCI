using Application.Common.Enums;
using Application.Models;
using Application.Models.Errors;
using FluentValidation;
using Infrastructure.Persistence.SQLServer.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tools.Logging;

namespace Application.Features.Citizens.SearchCitizen
{
    public class SearchCitizenQuery : IRequest<Result<List<SearchCitizenResponse>>>
    {
        public string? SearchTerm { get; set; }
        public Guid? Id { get; set; }
        public Gender Gender { get; set; }
    }

    public class SearchCitizenQueryValidator : AbstractValidator<SearchCitizenQuery>
    {
        public SearchCitizenQueryValidator()
        {
            RuleFor(v => v.Gender).IsInEnum();

            When(
                s => s.Id is null,
                () =>
                {
                    RuleFor(v => v.SearchTerm)
                        .NotEmpty()
                        .WithMessage(ValidationErrorCode.Required.ToString())
                        .DependentRules(() =>
                        {
                            RuleFor(v => v.SearchTerm)
                                .MinimumLength(3)
                                .WithMessage(ValidationErrorCode.MinLength.ToString());
                        });
                }
            );
        }
    }

    public class SearchCitizenQueryHandler(ReadOnlyDbContext context)
        : IRequestHandler<SearchCitizenQuery, Result<List<SearchCitizenResponse>>>
    {
        public async Task<Result<List<SearchCitizenResponse>>> Handle(
            SearchCitizenQuery request,
            CancellationToken cancellationToken
        )
        {
            using var activity = ActivitySourceLog
                .CQRS.Start()
                .AddParameter(request, r => r.SearchTerm)
                .AddParameter(request, r => r.Id);

            var citizens = await context
                .Citizens.Where(c =>
                    request.SearchTerm != null
                        && c.Gender == request.Gender
                        && (
                            c.FirstName.Contains(request.SearchTerm)
                            || c.LastName.Contains(request.SearchTerm)
                            || c.BirthPlace.Contains(request.SearchTerm)
                        )
                    || c.Id == request.Id
                )
                .Select(c => new SearchCitizenResponse()
                {
                    Id = c.Id,
                    Gender = c.Gender,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    BirthDate = c.BirthDate,
                    BirthPlace = c.BirthPlace,
                    Nationality = c.Nationality,
                })
                .ToListAsync(cancellationToken);

            return Result<List<SearchCitizenResponse>>.From(citizens);
        }
    }
}
