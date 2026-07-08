using Infrastructure.ExternalAuth.Configuration;
using Infrastructure.ExternalAuth.Interfaces;
using Infrastructure.ExternalAuth.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.ExternalAuth
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddInfrastructureExternalAuthServices(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services.AddKeyedScoped<IExternalAuthService, GoogleAuthService>(
                ExternalAuthServiceKeys.GoogleAuthService
            );
            services.AddKeyedScoped<IExternalAuthService, MicrosoftAuthService>(
                ExternalAuthServiceKeys.MicrosoftAuthService
            );

            services.Configure<ExternalAuthConfiguration>(
                ExternalAuthServiceKeys.GoogleConfiguration,
                configuration.GetSection("GoogleAuth")
            );
            services.Configure<ExternalAuthConfiguration>(
                ExternalAuthServiceKeys.MicrosoftConfiguration,
                configuration.GetSection("MicrosoftAuth")
            );

            return services;
        }
    }
}
