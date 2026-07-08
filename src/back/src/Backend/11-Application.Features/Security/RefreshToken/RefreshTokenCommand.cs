using System;
using System.Collections.Generic;
using System.Text;
using Application.Exceptions.Auth;
using Application.Features.Security.Common;
using Application.Models;
using Application.Models.Errors;
using FluentValidation;
using Infrastructure.Persistence.SQLServer.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tools.Helpers;
using Tools.Logging;

namespace Application.Features.Security.RefreshToken
{
    public class RefreshTokenCommand : IRequest<Result<TokenResponse>>
    {
        public string? UserName { get; set; }
        public string? RefreshToken { get; set; }
    }

    public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(v => v.UserName)
                .NotEmpty()
                .WithMessage(ValidationErrorCode.Required.ToString());

            RuleFor(v => v.RefreshToken)
                .NotEmpty()
                .WithMessage(ValidationErrorCode.Required.ToString())
                .DependentRules(() =>
                {
                    RuleFor(v => v.RefreshToken)
                        .Must(t => Base64Helper.IsBase64String(t!))
                        .WithMessage(ValidationErrorCode.Base64Format.ToString());
                });
        }
    }

    public class RefreshTokenCommandHandler(
        WritableDbContext context,
        ITokenHelper tokenHelper,
        TimeProvider timeProvider
    ) : IRequestHandler<RefreshTokenCommand, Result<TokenResponse>>
    {
        public async Task<Result<TokenResponse>> Handle(
            RefreshTokenCommand request,
            CancellationToken cancellationToken
        )
        {
            using var activity = ActivitySourceLog.CQRS.Start();

            var now = timeProvider.GetUtcNow();

            Guid? tokenId = Base64Helper.GetGuidFromBase64(request.RefreshToken!);

            var tokenQuery = context.RefreshTokens.Where(rt =>
                rt.Id == tokenId && rt.User != null && rt.User.UserName == request.UserName
            );

            var user =
                await tokenHelper.GetUserForAuthenticationAsync(request.UserName!)
                ?? throw new UserAuthenticationException(request.UserName!);

            var tokenDb = await tokenQuery.SingleOrDefaultAsync(cancellationToken);

            if (tokenDb is null || tokenDb.Expiry < now)
            {
                throw new UserAuthenticationException(request.UserName!);
            }

            context.RefreshTokens.Remove(tokenDb);

            var model = await tokenHelper.GenerateTokenAsync(user, cancellationToken);
            return Result<TokenResponse>.From(model);
        }
    }
}
