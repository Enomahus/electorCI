using Application.Common.Enums;
using Application.Common.Interfaces.Services;
using Application.Exceptions;
using Application.Exceptions.Auth;
using Application.Models;
using Application.Models.Errors;
using FluentValidation;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Pcea.Core.Net.Authorization.Application.Attributes;
using Tools.Logging;

namespace Application.Features.RegistrationRequests.GetRegistrationRequest
{
    [WithPermission(nameof(AppPermission.GetRegistrationRequest))]
    public class GetRegistrationRequestQuery : IRequest<Result<GetRegistrationRequestResponse>>
    {
        public Guid Id { get; set; }

        public GetRegistrationRequestQuery(Guid id)
        {
            Id = id;
        }
    }

    public class GetRegistrationRequestQueryValidator : AbstractValidator<GetRegistrationRequestQuery>
    {
        public GetRegistrationRequestQueryValidator()
        {
            RuleFor(v => v.Id).NotEmpty().WithMessage(ValidationErrorCode.Required.ToString());
        }
    }

    public class GetRegistrationRequestQueryHandler(
        ReadOnlyDbContext context,
        ICurrentUserService currentUserService,
        TimeProvider timeProvider
    ) : IRequestHandler<GetRegistrationRequestQuery, Result<GetRegistrationRequestResponse>>
    {
        public async Task<Result<GetRegistrationRequestResponse>> Handle(
            GetRegistrationRequestQuery query,
            CancellationToken cancellationToken
        )
        {
            using var activity = ActivitySourceLog.CQRS.Start();

            var dateNow = timeProvider.GetUtcNow();
            var currentUserId = currentUserService.UserId;

            var currentUser =
                await context
                    .Users.AsNoTracking()
                    .Include(u => u.UserDistricts)
                        .ThenInclude(ud => ud.District)
                    .FirstOrDefaultAsync(u => u.Id == currentUserId, cancellationToken)
                ?? throw new UserAccessException();

            var registrationRequest =
                await context
                    .RegistrationRequests.AsNoTracking()
                    .Include(r => r.District)
                    .Include(r => r.Citizen)
                        .ThenInclude(c => c.Father)
                    .Include(r => r.Citizen)
                        .ThenInclude(c => c.Mother)
                    .Include(r => r.RegistrationRequestDocuments)
                    .FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken)
                ?? throw new NotFoundException(nameof(RegistrationRequestDao), query.Id);

            var response = GetRegistrationRequestResponse.From(registrationRequest, currentUser, dateNow);

            return Result<GetRegistrationRequestResponse>.From(response);
        }
    }
}
