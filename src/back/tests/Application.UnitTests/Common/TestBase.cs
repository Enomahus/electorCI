using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Application.Common.Interfaces.Services;
using Application.Features;
using Application.Interfaces.Services;
using Azure.Storage.Blobs;
using Infrastructure.Configurations;
using Infrastructure.Persistence.Configurations;
using Infrastructure.Persistence.SQLServer;
using Infrastructure.Persistence.SQLServer.Seeders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Pcea.Core.Net.Authorization;
using Pcea.Core.Net.Authorization.Application.Interfaces.Services;
using Pcea.Core.Net.Authorization.Handlers;
using Pcea.Core.Net.Authorization.Interfaces.Handlers;
using Pcea.Core.Net.Authorization.Models;
using Pcea.Core.Net.Authorization.Persistence;
using Pcea.Core.Net.Authorization.Web.Interfaces.Services;
using Tools.Configuration;
using Web.Services;

namespace Application.UnitTests.Common
{
    public class TestBase
    {
        protected TestBase() { }

        public static IServiceCollection CreateServiceCollection(
            Action<TimeProvider>? setupDateService = null,
            Action<BlobClient>? setupBlobClient = null,
            Action<IFileService>? setupFileService = null,
            bool mockAuthorization = true
        )
        {
            var currentUserServiceSub = Substitute.For<ICurrentUserService>();
            var tokenRoleClaimBuilderSub = Substitute.For<ITokenRoleClaimBuilder<long>>();
            var currentUserPermissionsProviderSub =
                Substitute.For<ICurrentUserPermissionsProvider>();
            var currentUserEntityPermissionsProviderSub = Substitute.For<
                ICurrentUserEntityPermissionsProvider<long>
            >();

            var timeProviderSub = Substitute.For<TimeProvider>();
            BlobServiceClient blobServiceSub = ConfigureBlobServiceSubstitute(setupBlobClient);
            timeProviderSub
                .GetUtcNow()
                .Returns(new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero));
            var fileServiceSub = setupFileService != null ? Substitute.For<IFileService>() : null;

            setupDateService?.Invoke(timeProviderSub);
            setupFileService?.Invoke(fileServiceSub!);

            var configuration = new ConfigurationBuilder().Build();

            var services = new ServiceCollection();
            services
                .AddApplicationServices()
                .AddMediator()
                .AddDatabase(timeProviderSub)
                .AddInfrastructureIdentityServices(configuration)
                .AddPceaCoreNetAuthorization()
                .AddPceaCoreNetAuthorizationPersistence()
                .AddSingleton(currentUserServiceSub)
                .AddSingleton(tokenRoleClaimBuilderSub)
                .AddSingleton(currentUserPermissionsProviderSub)
                .AddSingleton(currentUserEntityPermissionsProviderSub)
                .AddSingleton(timeProviderSub)
                .AddSingleton(blobServiceSub)
                .AddScoped<ITokenService, TokenService>()
                //.AddScoped<ITokenHelper, TokenHelper>()
                .AddScoped<IAuthorizationHandler, AuthorizationHandler>()
                //.AddScoped<CriteriaService>()
                //.AddScoped<PanelReferenceService>()
                //.AddScoped<StakeholderService>()
                //.AddKeyedSingleton(ExternalAuthServiceKeys.GoogleAuthService, externalAuthSub)
                //.AddKeyedSingleton(ExternalAuthServiceKeys.MicrosoftAuthService, externalAuthSub)
                .Configure<TokenConfiguration>(c =>
                    c.Secret = "DEV_SECRET_JWT_KEY_VERY_LONG_FOR_SECURITY"
                )
                .Configure<AppConfiguration>(c =>
                {
                    c.AppUrl = "http://localhost:44026";
                })
                .Configure<DataConfiguration>(c =>
                {
                    c.Seed = true;
                    c.DefaultUserPassword = "Secret1";
                });

            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentUICulture = CultureInfo.CurrentUICulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.CurrentCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.CurrentUICulture;

            var serviceProvider = services.BuildServiceProvider();
            var seeder = ActivatorUtilities.CreateInstance<DataSeeder>(serviceProvider);
            seeder.SeedDataAsync().Wait();

            if (mockAuthorization)
            {
                var substitute = Substitute.For<IAuthorizationHandler>();
                substitute
                    .HandleAsync()
                    .Returns(Task.FromResult(new AuthorizationResult() { IsAuthorized = true }));
                var descriptor = new ServiceDescriptor(
                    typeof(IAuthorizationHandler),
                    p => substitute,
                    ServiceLifetime.Transient
                );
                services.Replace(descriptor);
                currentUserPermissionsProviderSub
                    .IsCurrentUserAuthenticatedAsync()
                    .Returns(Task.FromResult(true));
            }

            return services;
        }
    }
}
