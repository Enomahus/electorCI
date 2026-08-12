using Application.Features.Common.Citizen;
using Infrastructure.Persistence.SQLServer.Contexts;

namespace Application.Features.RegistrationRequests.Common
{
    public class CitizenService(WritableDbContext context)
    {
        public async Task<Guid> CreateNewCitizenAsync(
            BasicCitizenModel model,
            CancellationToken cancellationToken
        )
        {
            var citizen = model.ToDao();
            await context.Citizens.AddAsync(citizen, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return citizen.Id;
        }
    }
}
