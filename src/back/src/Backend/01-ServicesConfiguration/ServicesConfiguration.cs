using Application;
using Application.Features;
using Infrastructure;
using Infrastructure.ExternalAuth;
using Infrastructure.Persistence;
using Infrastructure.Persistence.File;
using Infrastructure.Persistence.SQLServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tools;

namespace ServicesConfiguration
{
    public static class ServicesConfiguration
    {
        public static IServiceCollection ConfigureAllServices(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services.AddMediator();
            services.AddToolsServices(configuration);
            services.AddInfrastructureServices(configuration);
            services.AddInfrastructurePersistenceServices(configuration);
            services.AddInfrastructureSQLServerServices(configuration);
            services.AddInfrastructureIdentityServices(configuration);
            services.AddInfrastructureFileServices(configuration);
            services.AddInfrastructureExternalAuthServices(configuration);
            services.AddApplicationServices();
            services.AddApplicationFeaturesServices();

            return services;
        }
    }
}
