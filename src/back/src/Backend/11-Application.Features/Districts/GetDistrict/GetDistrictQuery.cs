using Application.Common.Enums;
using Application.Exceptions;
using Application.Models;
using Application.Models.Errors;
using FluentValidation;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Pcea.Core.Net.Authorization.Application.Attributes;
using Tools.Logging;

namespace Application.Features.Districts.GetDistrict
{
    [WithPermission(nameof(AppPermission.GetDistrict))]
    public class GetDistrictQuery(long id) : IRequest<Result<GetDistrictResponse>>
    {
        public long Id { get; set; } = id;
    }

    public class GetDistrictQueryValidator : AbstractValidator<GetDistrictQuery>
    {
        public GetDistrictQueryValidator()
        {
            RuleFor(v => v.Id).NotEmpty().WithMessage(ValidationErrorCode.Required.ToString());
        }
    }

    public class GetDistrictQueryHandler(ReadOnlyDbContext context, TimeProvider timeProvider) 
        : IRequestHandler<GetDistrictQuery, Result<GetDistrictResponse>>
    {        
        public async Task<Result<GetDistrictResponse>> Handle(GetDistrictQuery request, CancellationToken cancellationToken)
        {
            using var activity = ActivitySourceLog.CQRS.Start().AddParameter(request, r => r.Id);
            var dateNow = timeProvider.GetUtcNow();

            var district = await context.Districts.AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(nameof(DistrictDao), request.Id);
            
            var response = GetDistrictResponse.FromDao(district, dateNow);
            return Result<GetDistrictResponse>.From(response);
        }
    }
}
