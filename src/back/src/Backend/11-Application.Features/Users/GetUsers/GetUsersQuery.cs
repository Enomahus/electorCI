using Application.Common.Enums;
using Application.Common.Interfaces.Services;
using Application.Features.Common.GridData;
using Application.Models;
using Infrastructure.Persistence.SQLServer.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Pcea.Core.Net.Authorization.Application.Attributes;
using Tools.Logging;

namespace Application.Features.Users.GetUsers
{
    [WithPermission(nameof(AppPermission.GetUsers))]
    public class GetUsersQuery
        : GridDataQuery,
            IRequest<Result<GridDataResponse<GetUsersResponse>>> { }

    public class GetUsersQueryValidator : GridDataQueryValidator<GetUsersQuery> { }

    public class GetUsersQueryHandler(
        ReadOnlyDbContext context,
        ICurrentUserService currentUserService,
        TimeProvider timeProvider
    ) : IRequestHandler<GetUsersQuery, Result<GridDataResponse<GetUsersResponse>>>
    {
        public async Task<Result<GridDataResponse<GetUsersResponse>>> Handle(
            GetUsersQuery request,
            CancellationToken cancellationToken
        )
        {
            using var activity = ActivitySourceLog.CQRS.Start();

            var now = timeProvider.GetUtcNow();
            var currentUserId = currentUserService.UserId;

            var users = await context
                .Users.Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.UserDistricts)
                    .ThenInclude(ud => ud.District)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var rows = users
                .Select(u => new GetUsersResponse
                {
                    Id = u.Id,
                    Title = u.Civility,
                    LastName = u.LastName,
                    FirstName = u.FirstName,
                    Email = u.Email,
                    Phone = u.PhoneNumber,
                    District = u.UserDistricts.FirstOrDefault()?.District?.Wording,
                    IsActive = u.DisabledDate is null || u.DisabledDate > now,
                    CanBeDeleted = u.Id != currentUserId,
                    CanBeToggled = u.Id != currentUserId,
                    CreatedAt = u.CreatedAt,
                    Roles = [.. u.UserRoles.Select(ur => ur.Role.Name ?? "")],
                    AuthProvider = u.AuthProvider,
                })
                .AsQueryable();

            var result = rows.ApplyGrid(request);

            return Result<GridDataResponse<GetUsersResponse>>.From(result);
        }
    }
}
