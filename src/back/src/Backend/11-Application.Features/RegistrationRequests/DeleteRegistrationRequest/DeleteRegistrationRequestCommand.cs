using Application.Common.Enums;
using Application.Common.Interfaces.Services;
using Application.Features.RegistrationRequests.Common;
using Application.Models.Errors;
using FluentValidation;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.EntityFrameworkCore;
using Pcea.Core.Net.Authorization.Application.Attributes;

namespace Application.Features.RegistrationRequests.DeleteRegistrationRequest
{
    [WithPermission([nameof(AppPermission.DeleteRegistrationRequest)])]
    public class DeleteRegistrationRequestCommand : DeleteRegistrationRequestCommandBase
    {
        public DeleteRegistrationRequestCommand(Guid id)
        {
            Id = id;
        }
    }

    public class DeleteRegistrationRequestCommandValidator(
        ReadOnlyDbContext context,
        ICurrentUserService currentUserService
    ) : DeleteRegistrationRequestCommandValidatorBase<DeleteRegistrationRequestCommand>(context)
    {
        private readonly ICurrentUserService _currentUserService = currentUserService;

        protected override void AddCustomRules()
        {
            RuleFor(cr => cr.Id)
                .MustAsync(CheckRegistrationRequestUserAsync)
                .WithMessage(ValidationErrorCode.RegistrationRequestMustBeOwnedByUser.ToString())
                .DependentRules(() =>
                {
                    RuleFor(cr => cr.Id)
                        .MustAsync(CheckRegistrationRequestStatusAsync)
                        .WithMessage(
                            ValidationErrorCode.RegistrationRequestMustBeDraftOrToBeProcessed.ToString()
                        );
                });
        }

        private async Task<bool> CheckRegistrationRequestUserAsync(
            Guid registrationRequestId,
            CancellationToken cancellationToken
        )
        {
            var registrationRequest = await _context.RegistrationRequests.SingleAsync(
                cr => cr.Id == registrationRequestId,
                cancellationToken
            );

            return registrationRequest.AuthorId == _currentUserService.UserId;
        }

        private async Task<bool> CheckRegistrationRequestStatusAsync(
            Guid registrationRequestId,
            CancellationToken cancellationToken
        )
        {
            var registrationRequest = await _context.RegistrationRequests.SingleAsync(
                cr => cr.Id == registrationRequestId,
                cancellationToken
            );

            return registrationRequest.Status == RegistrationStatus.ToBeProcessed
                || registrationRequest.Status == RegistrationStatus.Draft;
        }
    }

    public class DeleteRegistrationRequestCommandHandler(WritableDbContext context)
        : DeleteRegistrationRequestCommandHandlerBase<DeleteRegistrationRequestCommand>(context) { }
}
