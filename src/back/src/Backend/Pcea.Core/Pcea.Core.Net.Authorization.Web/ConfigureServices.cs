using Microsoft.Extensions.DependencyInjection;
using Pcea.Core.Net.Authorization.Application;
using Pcea.Core.Net.Authorization.Persistence;

namespace Pcea.Core.Net.Authorization.Web
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddApolloCoreNetAuthorizationWeb(
            this IServiceCollection services
        )
        {
            services.AddPceaCoreNetAuthorizationApplication();
            services.AddPceaCoreNetAuthorizationPersistence();
            return services;
        }
    }
}
