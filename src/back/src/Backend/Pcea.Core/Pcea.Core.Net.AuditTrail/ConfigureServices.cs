using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Pcea.Core.Net.AuditTrail.Behaviours;
using Pcea.Core.Net.AuditTrail.Interfaces;

namespace Pcea.Core.Net.AuditTrail
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddPceaCoreNetAuditTrailServices<TAuditService>(
            this IServiceCollection services
        )
            where TAuditService : class, IAuditService
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditBehaviour<,>));
            services.AddScoped<IAuditService, TAuditService>();

            return services;
        }
    }
}
