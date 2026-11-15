using Application.Common.Interfaces.Services;
using Infrastructure.Persistence.File.Configurations;
using Infrastructure.Persistence.File.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence.File
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddInfrastructureFileServices(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services.Configure<StorageConfiguration>(configuration.GetSection("StorageConfig"));
            services.AddScoped<IFileService, LocalFileService>();

            return services;
        }
    }
}
