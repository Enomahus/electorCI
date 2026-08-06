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
                    //string userRoleName = GetUserRoleName(command.RoleName!);
                    await MapToDaoAsync(command, userDao, skipAdminFields: true, cancellationToken);

                    var defaultRoleId = await _context
                        .Roles.Where(r => r.Name == AppConstants.ElectorRole)
                        //.Roles.Where(r => r.Name == userRoleName)
                        .Select(r => r.Id)
                        .FirstAsync(cancellationToken);
                    userDao.UserRoles.Clear();
                    userDao.UserRoles.Add(new UserRoleDao() { RoleId = defaultRoleId });

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
