using Application.Common.Interfaces.Services;
using Azure.Storage.Blobs;
using Infrastructure.Persistence.File.Configurations;
using Infrastructure.Persistence.File.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Infrastructure.Persistence.File
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddInfrastructureFileServices(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            //var connectionString = configuration.GetConnectionString("Storage");
            services.Configure<StorageConfiguration>(configuration.GetSection("StorageConfig"));

            services.AddSingleton(sp =>
            {
                var storageConfig = sp.GetRequiredService<IOptions<StorageConfiguration>>().Value;
                return new BlobServiceClient(storageConfig.ConnectionString);
            });
            services.AddScoped<IFileService, FileService>();

            return services;
        }
    }
}
