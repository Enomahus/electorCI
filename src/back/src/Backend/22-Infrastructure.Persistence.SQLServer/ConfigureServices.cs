using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using Infrastructure.Persistence.SQLServer.Providers;
using Infrastructure.Persistence.SQLServer.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pcea.Core.Net.Authorization.Persistence.Interfaces.Services;

namespace Infrastructure.Persistence.SQLServer
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddInfrastructureSQLServerServices(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            var connectionString = configuration.GetConnectionString("AppDb");

            if (!string.IsNullOrEmpty(connectionString))
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseSqlServer(
                        connectionString,
                        b =>
                        {
                            b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                            b.EnableRetryOnFailure();
                            b.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                        }
                    );
                });

                services.AddDbContext<ReadOnlyDbContext>(options =>
                {
                    options.UseSqlServer(
                        connectionString,
                        b =>
                        {
                            b.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                        }
                    );
                });

                services.AddDbContext<WritableDbContext>(options =>
                {
                    options.UseSqlServer(
                        connectionString,
                        b =>
                        {
                            b.EnableRetryOnFailure();
                            b.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                        }
                    );
                });
            }

            services.AddScoped<IPermissionsProvider<Guid>, PermissionProvider>();

            return services;
        }

        public static IServiceCollection AddInfrastructureIdentityServices(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services.AddDataProtection();

            services.Configure<DataProtectionTokenProviderOptions>(
                configuration.GetSection("DataProtectionTokenProviderOptions")
            );

            services
                .AddIdentityCore<UserDao>(options =>
                {
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireDigit = true;
                    options.Password.RequireNonAlphanumeric = false;

                    options.Tokens.PasswordResetTokenProvider = "DataProtectorTokenProvider";
                })
                .AddTokenProvider<DataProtectorTokenProvider<UserDao>>("DataProtectorTokenProvider")
                .AddRoles<RoleDao>()
                .AddEntityFrameworkStores<WritableDbContext>();

            return services;
        }

        public static async Task UseInfrastructureSQLServerServicesAsync(
            this IServiceProvider serviceProvider,
            string environment
        )
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();

            if (environment == "NSwag")
            {
                return;
            }

            using var context = services.GetRequiredService<ApplicationDbContext>();
            try
            {
                await context.Database.MigrateAsync();

                var seeder = ActivatorUtilities.CreateInstance<DataSeeder>(services);
                await seeder.SeedDataAsync();

                var testSeeder = ActivatorUtilities.CreateInstance<TestDataSeeder>(services);
                await testSeeder.SeedDataAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while migrating database.");
            }
        }
    }
}
