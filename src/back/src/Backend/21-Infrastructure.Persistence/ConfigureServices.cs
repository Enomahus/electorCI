using Infrastructure.Persistence.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pcea.Core.Net.Authorization.Persistence;

namespace Infrastructure.Persistence
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddInfrastructurePersistenceServices(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services.Configure<DataConfiguration>(configuration.GetSection("DataConfig"));
            services.AddPceaCoreNetAuthorizationPersistence();

            return services;
        }
    }
}
