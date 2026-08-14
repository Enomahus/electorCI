using Application.Common.Enums;
using Application.Features.RegistrationRequests.Common;
using Infrastructure.Persistence.SQLServer.Contexts;
using Pcea.Core.Net.Authorization.Application.Attributes;

namespace Application.Features.RegistrationRequests.DeleteRegistrationRequestForAdmin
{
    [WithPermission([nameof(AppPermission.DeleteRegistrationRequestForAdmin)])]
    public class DeleteRegistrationRequestForAdminCommand : DeleteRegistrationRequestCommandBase
    {
        public DeleteRegistrationRequestForAdminCommand(Guid id)
        {
            Id = id;
        }
    }

    public class DeleteRegistrationRequestForAdminCommandValidator(ReadOnlyDbContext context)
        : DeleteRegistrationRequestCommandValidatorBase<DeleteRegistrationRequestForAdminCommand>(context)
    {
        protected override void AddCustomRules() { }
    }

    public class DeleteRegistrationRequestForAdminCommandHandler(WritableDbContext context)
        : DeleteRegistrationRequestCommandHandlerBase<DeleteRegistrationRequestForAdminCommand>(context) { }
}
