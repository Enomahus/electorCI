using Application.Common.Enums;
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
using Pcea.Core.Net.Authorization.Application.Attributes;
using Tools.Logging;
using Tools.Serialization;

namespace Application.Features.Users.CreateUser
{
    [WithPermission(nameof(AppPermission.CreateUser))]
    public class CreateUserCommand : UserModel, IRequest<Result<Guid>>
    {
        [SensitiveData]
        public string? Password { get; set; }
    }

    public class CreateUserCommandValidator
    : UserCommandValidatorBase<CreateUserCommand>
    { 
        public CreateUserCommandValidator(ReadOnlyDbContext context) : base(context)
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
        }
    }

    public class CreateUserCommandHandler(
        WritableDbContext context,
        UserManager<UserDao> userManager,
        TimeProvider timeProvider,
        DistrictService districtService
    ) : UserCommandHandlerBase(context, userManager, timeProvider, districtService),
        IRequestHandler<CreateUserCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
        {
            using var activity = ActivitySourceLog.CQRS.Start();

            var userDao = new UserDao();

            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteInTransactionAsync(
                async () =>
                {
                    await MapToDaoAsync(command, userDao, cancellationToken: cancellationToken);

                    var result = await _userManager.CreateAsync(userDao, command.Password!);
                    EnsureIdentitySucceeded(result);
                },
                () => Task.FromResult(true)
            );

            activity.AddParameter(userDao, u => u.Id);

            return Result<Guid>.From(userDao.Id);
        }
    }
}
