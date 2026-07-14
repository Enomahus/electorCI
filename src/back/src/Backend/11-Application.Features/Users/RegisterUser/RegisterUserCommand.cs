using Application.Features.Districts.Common;
using Application.Features.Users.Common;
using Application.Models;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Tools.Configuration;
using Tools.Constants;
using Tools.Logging;
using Tools.Serialization;

namespace Application.Features.Users.RegisterUser
{
    public class RegisterUserCommand : UserModel, IRequest<Result<Guid>>
    {
        [SensitiveData]
        public string? Password { get; set; }
    }

    public class RegisterUserCommandValidator : UserCommandValidatorBase<RegisterUserCommand>
    {
        public RegisterUserCommandValidator(ReadOnlyDbContext context) : base(context)
        {
        }
    }

    public class RegisterUserCommandHandler(
    WritableDbContext context,
    UserManager<UserDao> userManager,
    IOptions<AppConfiguration> config,
    TimeProvider timeProvider,
    DistrictService districtService
)
    : UserCommandHandlerBase(context, userManager, config, timeProvider, districtService),
        IRequestHandler<RegisterUserCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
        {
            using var activity = ActivitySourceLog.CQRS.Start();

            var userDao = new UserDao();
            var now = _timeProvider.GetUtcNow();

            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteInTransactionAsync(
                async () =>
                {
                    await MapToDaoAsync(command, userDao, skipAdminFields: true, cancellationToken);

                    var defaultRoleId = await _context
                        .Roles.Where(r => r.Name == AppConstants.ElectorRole)
                        .Select(r => r.Id)
                        .FirstAsync(cancellationToken);
                    userDao.UserRoles.Clear();
                    userDao.UserRoles.Add(new UserRoleDao() { RoleId = defaultRoleId });

                    await _userManager.CreateAsync(userDao);
                },
                () => Task.FromResult(true)
            );

            activity.AddParameter(userDao, u => u.Id);

            return Result<Guid>.From(userDao.Id);

        }
    }
}
