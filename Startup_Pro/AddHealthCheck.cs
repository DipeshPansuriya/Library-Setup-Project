using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace InfraLib.Startup_Pro
{
    public static class AddHealthCheck
    {
        public static void Builder(WebApplicationBuilder builder, List<(string connectionString, string name)> sqlHealthChecks = null, bool hangfire = false)
        {
            IHealthChecksBuilder healthChecksBuilder = builder.Services.AddHealthChecks();

            if (sqlHealthChecks != null)
            {
                foreach ((string connectionString, string name) in sqlHealthChecks)
                {
                    _ = healthChecksBuilder.AddSqlServer(
                        connectionString,
                        healthQuery: "SELECT 1;",
                        name: name);
                }
            }

            if (hangfire)
            {
                _ = healthChecksBuilder.AddHangfire(
                    setup => setup.MaximumJobsFailed = 5,
                    name: "Hangfire",
                    failureStatus: HealthStatus.Unhealthy);
            }

            AddDefaultHealthChecks(builder, healthChecksBuilder);
        }

        public static void Builder(WebApplicationBuilder builder, bool hangfire = false)
        {
            Builder(builder, null, hangfire);
        }

        private static void AddDefaultHealthChecks(WebApplicationBuilder builder, IHealthChecksBuilder healthChecksBuilder)
        {
            _ = healthChecksBuilder
                  .AddProcessAllocatedMemoryHealthCheck(
                    maximumMegabytesAllocated: 500,
                    name: "Memory",
                    failureStatus: HealthStatus.Degraded,
                    tags: new[] { "server", "memory" });

            _ = builder.Services
                .AddHealthChecksUI(setupSettings: setup => setup.AddHealthCheckEndpoint("Health Checks", "/health"))
                .AddInMemoryStorage();
        }

        public static void App(WebApplication app)
        {
            _ = app.UseEndpoints(
                endpoints =>
                {
                    _ = endpoints.MapHealthChecks(
                        "/health",
                        new HealthCheckOptions
                        {
                            Predicate = _ => true, // Checks all registered checks
                            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                        }).DisableHttpMetrics().DisableRateLimiting();

                    _ = endpoints.MapHealthChecksUI(
                        options =>
                        {
                            options.UIPath = "/health-ui"; // Path for the UI
                            options.ApiPath = "/health-ui-api";  // Path for the UI's API calls
                        }).DisableHttpMetrics().DisableRateLimiting();
                });
        }
    }
}
