using Application.Features.Districts.Common;
using Application.Features.Users.Common;
using Application.Models;
using Application.Models.Errors;
using FluentValidation;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tools.Constants;
using Tools.Logging;
using Tools.Serialization;

namespace Application.Features.Users.RegisterUser
{
    public class RegisterUserCommand : UserModel, IRequest<Result<Guid>>
    {
        [SensitiveData]
        public string? Password { get; set; }
        //public string? RoleName { get; set; }
    }

    public class RegisterUserCommandValidator : UserCommandValidatorBase<RegisterUserCommand>
    {
        public RegisterUserCommandValidator(ReadOnlyDbContext context)
            : base(context, validateRoles: false)
        {
            RuleFor(v => v.Password)
                .NotEmpty()
                .WithMessage(ValidationErrorCode.Required.ToString())
                .MinimumLength(8)
                .WithMessage(ValidationErrorCode.MinLength.ToString())
                .Matches(@"[A-Z]+")
                .WithMessage(ValidationErrorCode.InvalidPassword.ToString())
                .Matches(@"[a-z]+")
                .WithMessage(ValidationErrorCode.InvalidPassword.ToString())
                .Matches(@"[0-9]+")
                .WithMessage(ValidationErrorCode.InvalidPassword.ToString());
            //RuleFor(v => v.RoleName)
            //    .NotEmpty()
            //    .WithMessage(ValidationErrorCode.Required.ToString());
        }
    }

    public class RegisterUserCommandHandler(
        WritableDbContext context,
        UserManager<UserDao> userManager,
        TimeProvider timeProvider,
        DistrictService districtService
    )
        : UserCommandHandlerBase(context, userManager, timeProvider, districtService),
            IRequestHandler<RegisterUserCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(
            RegisterUserCommand command,
            CancellationToken cancellationToken
        )
        {
            using var activity = ActivitySourceLog.CQRS.Start();

            var userDao = new UserDao();
            var now = _timeProvider.GetUtcNow();

            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteInTransactionAsync(
                async () =>
                {
                    await MapToDaoAsync(command, userDao, skipAdminFields: true, cancellationToken);

                    userDao.UserRoles.Clear();
                    foreach (var role in command.Roles)
                    {
                        userDao.UserRoles.Add(new UserRoleDao() { RoleId = role });
                    }

                    var result = await _userManager.CreateAsync(userDao, command.Password!);
                    EnsureIdentitySucceeded(result);
                },
                () => Task.FromResult(true)
            );

            activity.AddParameter(userDao, u => u.Id);

            return Result<Guid>.From(userDao.Id);
        }

        private static string GetUserRoleName(string roleName)
        {
            return roleName switch
            {
                "supervisor" => AppConstants.SuperAdminRole,
                "agent" => AppConstants.OrganismAgentRole,
                _ => AppConstants.OrganismAgentRole,
            };
        }
    }
}
