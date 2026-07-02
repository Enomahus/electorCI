using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Features
{
    [ExcludeFromCodeCoverage]
    public static class ConfigureServices
    {
        public static IServiceCollection AddMediator(this IServiceCollection services)
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(executingAssembly));
            services.AddValidatorsFromAssembly(executingAssembly);

            return services;
        }

        public static IServiceCollection AddApplicationFeaturesServices(
            this IServiceCollection services
        )
        {
            //services.AddScoped<IAddressService, AddressService>();
            //services.AddScoped<IScaleVersionQueryService, ScaleVersionQueryService>();
            //services.AddScoped<CriteriaService>();
            //services.AddScoped<TranslationService>();
            //services.AddScoped<CertificateRequestService>();
            //services.AddScoped<StakeholderService>();
            //services.AddScoped<IConformityStatusService, ConformityStatusService>();
            //services.AddScoped<ITokenHelper, TokenHelper>();
            //services.AddScoped<PanelReferenceCompleteFormValidatorsService>();
            //services.AddScoped<PanelReferenceCompleteSideEffectsService>();
            //services.AddScoped<PanelReferenceService>();

            return services;
        }

        //public static void UseApplicationFeaturesServices(
        //    this IServiceProvider serviceProvider,
        //    IConfiguration configuration
        //)
        //{
        //    serviceProvider.AddRecurringJob<NotifyEndOfConformityJob>(
        //        configuration.GetValue<string>("Jobs:NotifyEndOfConformityCron") ?? Cron.Daily(8)
        //    );
        //}
    }
}
