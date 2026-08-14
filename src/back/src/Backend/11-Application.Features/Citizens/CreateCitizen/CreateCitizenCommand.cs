using Application.Common.Enums;
using Application.Features.Common.Citizen;
using Application.Models;
using Infrastructure.Persistence.SQLServer.Contexts;
using MediatR;
using Pcea.Core.Net.Authorization.Application.Attributes;
using Tools.Logging;

namespace Application.Features.Citizens.CreateCitizen
{
    [WithPermission(nameof(AppPermission.CreateCitizen))]
    public class CreateCitizenCommand : CitizenModel, IRequest<Result<Guid>> { }

    public class CreateCitizenCommandValidator(ReadOnlyDbContext context, TimeProvider timeProvider)
        : CitizenValidatorBase<CreateCitizenCommand>(context, timeProvider) { }

    public class CreateCitizenCommandHandler(WritableDbContext context)
        : IRequestHandler<CreateCitizenCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(
            CreateCitizenCommand command,
            CancellationToken cancellationToken
        )
        {
            using var activity = ActivitySourceLog.CQRS.Start();

            var citizenDao = command.ToDao();

            await context.Citizens.AddAsync(citizenDao, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.From(citizenDao.Id);
        }
    }
}
