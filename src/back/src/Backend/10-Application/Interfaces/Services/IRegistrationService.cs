using Infrastructure.Persistence.SQLServer.Contexts;

namespace Application.Interfaces.Services
{
    public interface IRegistrationService
    {
        Task<string> GenerateRequestReferenceAsync(
            WritableDbContext context,
            TimeProvider timeProvider,
            CancellationToken cancellationToken
        );
        Task<string> GenerateElectorNumberAsync(
            WritableDbContext context,
            long pollingStationId,
            CancellationToken cancellationToken
        );
    }
}
