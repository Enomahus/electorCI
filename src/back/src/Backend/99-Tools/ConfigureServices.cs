using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tools.Configuration;

namespace Tools
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddToolsServices(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            var configSection = configuration.GetSection("AppConfig");
            services.Configure<AppConfiguration>(configSection);

            return services;
        }
    }
}
