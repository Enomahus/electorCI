using Application.Common.Interfaces.Services;
using Application.Exceptions;
using Application.Features.Districts.Common;
using Application.Features.Users.Common;
using Application.Models;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tools.Logging;

namespace Application.Features.Users.UpdateCurrentUser
{
    public class UpdateCurrentUserCommand : UserModel, IRequest<Result<Guid>>
    {
    }

    public class UpdateCurrentUserCommandValidator : UserCommandValidatorBase<UpdateCurrentUserCommand>
    {
        protected readonly ICurrentUserService _currentUserService;

        public UpdateCurrentUserCommandValidator(
            ReadOnlyDbContext context,
            ICurrentUserService currentUserService
        )
            : base(context, validateRoles: false)
        {
            _currentUserService = currentUserService;
        }

        protected override Task<bool> BeUniqueEmailAsync(
            UpdateCurrentUserCommand command,
            string email,
            CancellationToken cancellationToken
        )
        {
            return _context.Users.AllAsync(
                u => u.Id == _currentUserService.UserId || (u.Email != email && u.UserName != email),
                cancellationToken
            );
        }
    }

    public class UpdateCurrentUserCommandHandler(
        WritableDbContext context,
        UserManager<UserDao> userManager,
        TimeProvider timeProvider,
        DistrictService districtService,
        ICurrentUserService currentUserService
    )
    : UserCommandHandlerBase(context, userManager,timeProvider, districtService),
        IRequestHandler<UpdateCurrentUserCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(
            UpdateCurrentUserCommand command,
            CancellationToken cancellationToken
        )
        {
            using var activity = ActivitySourceLog.CQRS.Start();

            var userDao =
                await _context
                    .Users.Include(u => u.UserRoles)
                    .Include(u => u.UserDistricts)
                    .Where(u => u.Id == currentUserService.UserId)
                    .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException(nameof(UserDao), command.Email);

            var strategy = _context.Database.CreateExecutionStrategy();
            var sendAccountConfirmation = userDao.Email != command.Email;
            await strategy.ExecuteInTransactionAsync(
                async () =>
                {
                    await MapToDaoAsync(command, userDao, skipAdminFields: true, cancellationToken);

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
