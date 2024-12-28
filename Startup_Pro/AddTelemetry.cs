using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace InfraLib.Startup_Pro
{
    public static class AddTelemetry
    {
        public static void Builder(WebApplicationBuilder builder, string serviceName, bool isdevenv = false)
        {
            _ = builder.Logging.AddOpenTelemetry(options =>
            {
                options.IncludeScopes = true;
                options.IncludeFormattedMessage = true;
                _ = options
                    .SetResourceBuilder(
                        ResourceBuilder.CreateDefault()
                            .AddService(serviceName))
                    .AddConsoleExporter();
            });

            _ = builder.Services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(serviceName))
                .WithTracing(tracing =>
                {
                    if (!isdevenv)
                    {
                        _ = tracing.SetSampler<AlwaysOnSampler>();
                    }
                    _ = tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation(o => o.SetDbStatementForText = true);
                })
                .WithMetrics(metrics =>
                {
                    _ = metrics
                    .AddRuntimeInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddMeter(
                        "Microsoft.AspNetCore.Hosting",
                        "Microsoft.AspNetCore.Server.Kestrel",
                        "System.Net.Http",
                        builder.Environment.ApplicationName
                        )
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation();
                });

            _ = AddOpenTelemetryExporter(builder);
        }

        private static IHostApplicationBuilder AddOpenTelemetryExporter(this IHostApplicationBuilder builder)
        {
            //builder.Services.Configure<OpenTelemetryLoggerOptions>(loging => loging.AddOtlpExporter());

            _ = builder.Services.ConfigureOpenTelemetryLoggerProvider(loging => loging.AddOtlpExporter());

            _ = builder.Services.ConfigureOpenTelemetryMeterProvider(mterics => mterics.AddOtlpExporter());

            _ = builder.Services.ConfigureOpenTelemetryTracerProvider(tracing => tracing.AddOtlpExporter());

            _ = builder.Services.AddOpenTelemetry().WithMetrics(mterics => mterics.AddPrometheusExporter());

            return builder;
        }

        public static void App(WebApplication app)
        {
            _ = app.MapPrometheusScrapingEndpoint();
        }
    }
}
