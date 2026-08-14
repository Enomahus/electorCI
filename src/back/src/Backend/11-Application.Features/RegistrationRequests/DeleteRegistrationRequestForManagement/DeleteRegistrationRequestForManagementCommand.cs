using Application.Common.Enums;
using Application.Common.Interfaces.Services;
using Application.Features.RegistrationRequests.Common;
using Infrastructure.Persistence.SQLServer.Contexts;
using Pcea.Core.Net.Authorization.Application.Attributes;

namespace Application.Features.RegistrationRequests.DeleteRegistrationRequestForManagement
{
    [WithPermission([nameof(AppPermission.DeleteRegistrationRequestForManagement)])]
    public class DeleteRegistrationRequestForManagementCommand : DeleteRegistrationRequestCommandBase
    {
        public DeleteRegistrationRequestForManagementCommand(Guid id)
        {
            Id = id;
        }
    }

    public class DeleteRegistrationRequestForManagementCommandValidator(
        ReadOnlyDbContext context,
        ICurrentUserService currentUserService
    ) : DeleteRegistrationRequestCommandValidatorBase<DeleteRegistrationRequestForManagementCommand>(context)
    {
        private readonly ICurrentUserService _currentUserService = currentUserService;

        protected override void AddCustomRules() { }
    }

    public class DeleteRegistrationRequestForManagementCommandHandler(WritableDbContext context)
        : DeleteRegistrationRequestCommandHandlerBase<DeleteRegistrationRequestForManagementCommand>(
            context
        ) { }
}
