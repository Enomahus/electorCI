using Application.Common.Enums;
using Application.Exceptions;
using Application.Features.Users.Common;
using Application.Models;
using Application.Models.Errors;
using FluentValidation;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Pcea.Core.Net.Authorization.Application.Attributes;
using Tools.Logging;

namespace Application.Features.Users.GetUser;

[WithPermission(nameof(AppPermission.GetUser))]
public class GetUserQuery : IRequest<Result<UserModel>>
{
    public Guid? UserId { get; set; }
}

public class GetUserQueryValidator : AbstractValidator<GetUserQuery>
{
    public GetUserQueryValidator()
    {
        RuleFor(v => v.UserId).NotEmpty().WithMessage(ValidationErrorCode.Required.ToString());
    }
}

public class GetUserQueryHandler(ReadOnlyDbContext context, TimeProvider timeProvider)
: IRequestHandler<GetUserQuery, Result<UserModel>>
{
    public async Task<Result<UserModel>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        using var activity = ActivitySourceLog.CQRS.Start().AddParameter(request, r => r.UserId);

        var user =
            await context
                .Users.Include(s => s.UserDistricts)
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(UserDao), request.UserId);

        return Result<UserModel>.From(UserModel.FromDao(user, timeProvider.GetUtcNow()));
    }
}
