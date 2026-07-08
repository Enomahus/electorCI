using System.Net;
using Hangfire;
using Infrastructure.Persistence.SQLServer;
using Microsoft.AspNetCore.Diagnostics;
using ServicesConfiguration;
using Web.Common;
using Web.Common.Authorization;

public static class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        Action<ILoggingBuilder> configureLogger = loggingBuilder =>
        {
            loggingBuilder.AddConsole();
        };

        builder.Services.AddLogging(configureLogger);
        var logger = LoggerFactory.Create(configureLogger).CreateLogger($"{nameof(Program)}");

        builder.Configuration.AddJsonFile($"appsettings.json");
        builder.Configuration.AddJsonFile(
            $"appsettings.{builder.Environment.EnvironmentName}.json",
            true
        );

        builder.Configuration.AddEnvironmentVariables();

        builder.Services.AddWebServices(builder.Configuration);
        builder.Services.ConfigureAllServices(builder.Configuration);

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddHangfire(config =>
            config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(builder.Configuration.GetConnectionString("AppDb"))
        );
        builder.Services.AddHangfireServer();

        builder.Services.AddHealthChecks();

        builder.AddTelemetryServices(logger);

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.AddServerHeader = false;
        });

        builder.Services.AddControllers();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        logger.LogInformation("Environment: {EnvironmentName}", app.Environment.EnvironmentName);

        app.UseHangfireDashboard(
            options: new DashboardOptions { Authorization = [new HangfireAuthorizationFilter()] }
        );

        await app.Services.UseInfrastructureSQLServerServicesAsync(app.Environment.EnvironmentName);

        await app.UseWebServicesAsync();

        app.Use(
            async (context, next) =>
            {
                context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
                context.Response.Headers.Append(
                    "Referrer-Policy",
                    "strict-origin-when-cross-origin"
                );
                context.Response.Headers.Append(
                    "Permissions-Policy",
                    "accelerometer=(), autoplay=(self), camera=(self), display-capture=(), fullscreen=(self), geolocation=(self), gyroscope=(), microphone=(), payment=(), usb=()"
                );
                await next();
            }
        );

        app.UseExceptionHandler(c =>
            c.Run(async context =>
            {
                var exception = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;
                if (exception is HttpRequestException httpRequestException)
                {
                    context.Response.StatusCode = (int)(
                        httpRequestException.StatusCode ?? HttpStatusCode.InternalServerError
                    );
                    await context.Response.WriteAsync(exception.Message);
                }
            })
        );

        app.UseHealthChecks("/health");
        app.UseCors();

        app.MapControllers();

        await app.RunAsync();
    }
}
