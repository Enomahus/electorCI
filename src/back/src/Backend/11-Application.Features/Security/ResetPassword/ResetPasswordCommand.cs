using Application.Exceptions;
using Application.Exceptions.Auth;
using Application.Models;
using Application.Models.Errors;
using FluentValidation;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tools.Logging;
using Tools.Serialization;

namespace Application.Features.Security.ResetPassword
{
    public class ResetPasswordCommand : IRequest<Result>
    {
        public string? UserEmail { get; set; }

        [SensitiveData]
        public string? ResetToken { get; set; }

        [SensitiveData]
        public string? Password { get; set; }
    }

    public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordCommandValidator(ReadOnlyDbContext context)
        {
            RuleFor(c => c.UserEmail)
                .NotEmpty()
                .WithMessage(ValidationErrorCode.Required.ToString())
                .EmailAddress()
                .WithMessage(ValidationErrorCode.InvalidEmail.ToString())
                .DependentRules(() =>
                {
                    RuleFor(c => c.UserEmail)
                        .MustAsync(
                            (email, token) => context.Users.AnyAsync(u => u.Email == email, token)
                        )
                        .WithMessage(ValidationErrorCode.UserMustExist.ToString());
                });

            RuleFor(c => c.ResetToken)
                .NotEmpty()
                .WithMessage(ValidationErrorCode.Required.ToString());

            RuleFor(c => c.Password)
                .NotEmpty()
                .WithMessage(ValidationErrorCode.Required.ToString())
                .MinimumLength(8)
                .WithMessage(ValidationErrorCode.InvalidPassword.ToString())
                .MaximumLength(24)
                .WithMessage(ValidationErrorCode.InvalidPassword.ToString())
                .Matches(@"[A-Z]+")
                .WithMessage(ValidationErrorCode.InvalidPassword.ToString())
                .Matches(@"[a-z]+")
                .WithMessage(ValidationErrorCode.InvalidPassword.ToString())
                .Matches(@"[0-9]+")
                .WithMessage(ValidationErrorCode.InvalidPassword.ToString());
        }
    }

    public class ResetPasswordCommandHandler(
        UserManager<UserDao> userManager,
        WritableDbContext context
    ) : IRequestHandler<ResetPasswordCommand, Result>
    {
        public async Task<Result> Handle(
            ResetPasswordCommand request,
            CancellationToken cancellationToken
        )
        {
            using var activity = ActivitySourceLog
                .CQRS.Start()
                .AddParameter(request, r => r.UserEmail);

            var user =
                await context.Users.FirstOrDefaultAsync(
                    u => u.Email == request.UserEmail,
                    cancellationToken
                ) ?? throw new NotFoundException(nameof(UserDao), request.UserEmail);

            IdentityResult result;

            try
            {
                result = await userManager.ResetPasswordAsync(
                    user,
                    request.ResetToken!,
                    request.Password!
                );
            }
            catch (Exception e)
            {
                throw new ResetTokenException(e);
            }

            if (!result.Succeeded)
            {
                throw new ResetTokenException();
            }

            return Result.Default();
        }
    }
}
