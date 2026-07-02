using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pcea.Core.Net.Authorization.Handlers;
using Pcea.Core.Net.Authorization.Interfaces.Handlers;

namespace Pcea.Core.Net.Authorization
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddPceaCoreNetAuthorization(
            this IServiceCollection services
        )
        {
            services.TryAddScoped<IAuthorizationHandler, AuthorizationHandler>();

            return services;
        }
    }
}
