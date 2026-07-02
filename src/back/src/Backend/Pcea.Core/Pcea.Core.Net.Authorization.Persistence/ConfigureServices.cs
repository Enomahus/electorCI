using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pcea.Core.Net.Authorization.Persistence.Interfaces.Services;
using Pcea.Core.Net.Authorization.Persistence.Services;

namespace Pcea.Core.Net.Authorization.Persistence
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddPceaCoreNetAuthorizationPersistence(
            this IServiceCollection services
        )
        {
            services.TryAddScoped(typeof(IPermissionsService<>), typeof(PermissionsService<>));
            return services;
        }
    }
}
