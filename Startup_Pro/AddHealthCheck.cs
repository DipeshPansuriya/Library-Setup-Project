using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace InfraLib.Startup_Pro
{
    public static class AddHealthCheck
    {
        public static void Builder(WebApplicationBuilder builder)
        {
            _ = builder.Services
                .AddHealthChecks()
                .AddSqlServer(
                    @"Data Source=DIPESH-PANSURIY\SQLEXPRESS; Initial Catalog=masterdb; User Id=dbsa; Password=India_123456;MultipleActiveResultSets=True; Connection Timeout=300; TrustServerCertificate=true;Trusted_Connection=True",
                    healthQuery: "SELECT 1;",
                    name: "Masters Database")
                .AddSqlServer(
                    @"Data Source=DIPESH-PANSURIY\SQLEXPRESS; Initial Catalog=masterlogdb; User Id=dbsa; Password=India_123456;MultipleActiveResultSets=True; Connection Timeout=300; TrustServerCertificate=true;Trusted_Connection=True",
                    healthQuery: "SELECT 1;",
                    name: "Master Log Database")
                .AddSqlServer(
                    @"Data Source=DIPESH-PANSURIY\SQLEXPRESS; Initial Catalog=masterhangfiredb; User Id=dbsa; Password=India_123456;MultipleActiveResultSets=True; Connection Timeout=300; TrustServerCertificate=true;Trusted_Connection=True",
                    healthQuery: "SELECT 1;",
                    name: "Master Hangfire Database")
                .AddHangfire(
                    setup => setup.MaximumJobsFailed = 5,
                    name: "Hangfire",
                    failureStatus: HealthStatus.Unhealthy)
                .AddProcessAllocatedMemoryHealthCheck(
                    maximumMegabytesAllocated: 500,
                    name: "Memory",
                    failureStatus: HealthStatus.Degraded,
                    tags: new[] { "server", "memory" });


            //.AddPrivateMemoryHealthCheck(
            //    maximumMegabytesThreshold: 1000, // setting the threshold correctly in MB
            //    name: "Private Memory",
            //    failureStatus: HealthStatus.Degraded,
            //    tags: new[] { "server", "memory" });
            //.AddUrlGroup(
            //    new Uri("https://yourapi.com/health"),
            //    name: "API",
            //    failureStatus: HealthStatus.Unhealthy,
            //    tags: new[] { "external", "api" });

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