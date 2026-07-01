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
            services.AddToolsServices(configuration);
            return services;
        }
    }
}
