using Application.Common.Enums;
using Application.Exceptions;
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

namespace Application.Features.Users.UpdateUser
{
    [WithPermission(nameof(AppPermission.UpdateUser))]
    public class UpdateUserCommand : UserModel, IRequest<Result<Guid>>
    {
        public Guid UserId { get; set; }
    }

    public class UpdateUserCommandValidator : UserCommandValidatorBase<UpdateUserCommand>
    {
        public UpdateUserCommandValidator(ReadOnlyDbContext context)
            : base(context)
        {
            RuleFor(v => v.UserId).NotEmpty().WithMessage(ValidationErrorCode.Required.ToString());
        }

        protected override Task<bool> BeUniqueEmailAsync(
            UpdateUserCommand command,
            string email,
            CancellationToken cancellationToken
        )
        {
            return _context.Users.AllAsync(
                u => command.UserId == u.Id || u.UserName != email && u.Email != email,
                cancellationToken
            );
        }
    }

    public class UpdateUserCommandHandler(
        WritableDbContext context,
        UserManager<UserDao> userManager,
        TimeProvider timeProvider,
        DistrictService districtService
    )
    : UserCommandHandlerBase(context, userManager, timeProvider, districtService),
        IRequestHandler<UpdateUserCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
        {
            using var activity = ActivitySourceLog.CQRS.Start();

            var userDao =
                await _context
                    .Users.Include(u => u.UserRoles)
                    .Include(u => u.UserDistricts)
                    .Where(u => u.Id == command.UserId)
                    .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException(nameof(UserDao), command.UserId);

            var strategy = _context.Database.CreateExecutionStrategy();
            var sendAccountConfirmation = userDao.Email != command.Email;
            await strategy.ExecuteInTransactionAsync(
                async () =>
                {
                    await MapToDaoAsync(command, userDao, cancellationToken: cancellationToken);
                    
                    await _context.SaveChangesAsync(cancellationToken);
                },
                () => Task.FromResult(true)
            );
            //if (sendAccountConfirmation)
            //{
            //    await SendCreatePasswordEmailAsync(userDao);
            //}

            return Result<Guid>.From(userDao.Id);
        }

    }
}
