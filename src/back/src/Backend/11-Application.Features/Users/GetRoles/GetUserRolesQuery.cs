using Application.Models;
using FluentValidation;
using Infrastructure.Persistence.SQLServer.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tools.Logging;

namespace Application.Features.Users.GetRoles
{
    public class GetUserRolesQuery : IRequest<Result<List<RoleModel>>> { }

    public class GetUserRolesQueryValidator : AbstractValidator<GetUserRolesQuery> { }

    public class GetUserRolesQueryHandler(ReadOnlyDbContext context)
        : IRequestHandler<GetUserRolesQuery, Result<List<RoleModel>>>
    {
        public async Task<Result<List<RoleModel>>> Handle(
            GetUserRolesQuery request,
            CancellationToken cancellationToken
        )
        {
            using var activity = ActivitySourceLog.CQRS.Start();

            var rolesDao = await context
                .Roles.Select(r => new RoleModel() { Id = r.Id, Name = r.Name })
                .ToListAsync(cancellationToken);

            return Result<List<RoleModel>>.From(rolesDao);
        }
    }
}
