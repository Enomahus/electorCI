using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tools.Logging;

namespace Web.Common
{
    public static class ConfigureTelemetryServices
    {
        public static WebApplicationBuilder AddTelemetryServices(
            this WebApplicationBuilder builder,
            ILogger logger
        )
        {
            var metricEnable = builder.Configuration.GetValue<bool>("Metric:Enable");
            var endpoint = builder.Configuration.GetValue<string>("Metric:TracerEndpoint");
            var appInsightConnectionString = builder.Configuration.GetValue<string>(
                "APPLICATIONINSIGHTS_CONNECTION_STRING"
            );

            // Application Insights (Azure Monitor exporter) requires a connection string.
            // Only enable it when configured (prod); skip it locally to avoid a startup crash.
            if (!string.IsNullOrEmpty(appInsightConnectionString))
            {
                builder.Services.AddApplicationInsightsTelemetry();
            }

            if (metricEnable && !string.IsNullOrEmpty(endpoint))
            {
                logger.LogInformation($"[Metric] Enabled");

                var resourceAttributes = new Dictionary<string, object>
                {
                    { "service.namespace", "ElectorCI" },
                    { "span.kind", "SERVER" },
                };

                builder.Logging.AddOpenTelemetry(logging =>
                {
                    logging.AddOtlpExporter(opt =>
                    {
                        opt.Endpoint = new Uri(endpoint);
                        opt.Protocol = OtlpExportProtocol.Grpc;
                    });
                });

                var otBuilder = builder
                    .Services.AddOpenTelemetry()
                    .ConfigureResource(resourceBuilder =>
                        resourceBuilder.AddService("App").AddAttributes(resourceAttributes)
                    )
                    .WithTracing(tracing =>
                        tracing
                            .AddAspNetCoreInstrumentation()
                            .AddHttpClientInstrumentation()
                            .AddOtlpExporter(o =>
                            {
                                o.Endpoint = new System.Uri(endpoint);
                                o.Protocol = OtlpExportProtocol.Grpc;
                            })
                    )
                    .WithMetrics(builder =>
                        builder
                            .AddAspNetCoreInstrumentation()
                            .AddHttpClientInstrumentation()
                            .AddMeter("Microsoft.AspNetCore.Hosting")
                            .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                            .AddOtlpExporter(opt =>
                            {
                                opt.Endpoint = new System.Uri(endpoint);
                                opt.Protocol = OtlpExportProtocol.Grpc;
                            })
                    );

                if (!string.IsNullOrEmpty(appInsightConnectionString))
                {
                    otBuilder.UseAzureMonitorExporter();
                }

                AddTracerProvider(
                        "REPOSITORY",
                        endpoint,
                        appInsightConnectionString,
                        resourceAttributes,
                        ActivitySourceLog.REPOSITORY.Name
                    )
                    .Build();
                AddTracerProvider(
                        "CONTROLLER",
                        endpoint,
                        appInsightConnectionString,
                        resourceAttributes,
                        ActivitySourceLog.CONTROLLER.Name
                    )
                    .Build();
                AddTracerProvider(
                        "INFRASTRUCTURE",
                        endpoint,
                        appInsightConnectionString,
                        resourceAttributes,
                        ActivitySourceLog.INFRASTRUCTURE.Name
                    )
                    .Build();
                AddTracerProvider(
                        "TOOLS",
                        endpoint,
                        appInsightConnectionString,
                        resourceAttributes,
                        ActivitySourceLog.TOOLS.Name
                    )
                    .Build();
                AddTracerProvider(
                        "CQRS",
                        endpoint,
                        appInsightConnectionString,
                        resourceAttributes,
                        ActivitySourceLog.CQRS.Name
                    )
                    .Build();
                AddTracerProvider(
                        "WEB",
                        endpoint,
                        appInsightConnectionString,
                        resourceAttributes,
                        ActivitySourceLog.WEB.Name
                    )
                    .Build();

                AddTracerProvider(
                        "EF",
                        endpoint,
                        appInsightConnectionString,
                        resourceAttributes,
                        ActivitySourceLog.EF.Name
                    )
                    .AddEntityFrameworkCoreInstrumentation(
                        Infrastructure
                            .Persistence
                            .SQLServer
                            .ConfigureTelemetryServices
                            .AddEntityFrameworkCoreTelemetry
                    )
                    .Build();

                logger.LogInformation($"[Metric] start");
            }
            else
            {
                logger.LogInformation($"[Metric] Disabled");
            }

            return builder;
        }

        private static TracerProviderBuilder AddTracerProvider(
            string name,
            string endpoint,
            string appInsightConnectionString,
            Dictionary<string, object> resourceAttributes,
            params string[] sources
        )
        {
            var resourceBuilder = ResourceBuilder
                .CreateDefault()
                .AddService(name)
                .AddAttributes(resourceAttributes);

            var builder = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(resourceBuilder)
                .AddSource(sources)
                .AddOtlpExporter(opt =>
                {
                    opt.Endpoint = new System.Uri(endpoint);
                    opt.Protocol = OtlpExportProtocol.Grpc;
                });

            if (!string.IsNullOrEmpty(appInsightConnectionString))
            {
                builder = builder.AddAzureMonitorTraceExporter(opt =>
                {
                    opt.ConnectionString = appInsightConnectionString;
                });
            }

            return builder;
        }
    }
}
