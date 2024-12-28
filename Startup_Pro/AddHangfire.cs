using Hangfire;
using Hangfire.Dashboard.BasicAuthorization;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;

namespace InfraLib.Startup_Pro
{
    public static class AddHangFire
    {
        public static void Builder(WebApplicationBuilder builder)
        {
            _ = builder.Services
                .AddHangfire(
                    configuration => configuration
                        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                        .UseSimpleAssemblyNameTypeSerializer()
                        .UseRecommendedSerializerSettings()
                        .UseSqlServerStorage(
                            AppSettings.dbSettings.MastersHangfireDb,
                            new SqlServerStorageOptions
                            {
                                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                                QueuePollInterval = TimeSpan.Zero,
                                UseRecommendedIsolationLevel = true,
                                DisableGlobalLocks = true,
                                SchemaName = $"{AppSettings.ApplicationName}_Hangfire"
                            }));

            _ = builder.Services.AddHangfireServer();
        }

        public static void App(WebApplication app)
        {
            ConfigureHangfireServices(app);
        }

        private static void ConfigureHangfireServices(WebApplication app)
        {
            _ = app.UseHangfireDashboard(
                $"/{AppSettings.ApplicationName}_hangfire",
                new DashboardOptions
                {
                    Authorization =
                        new[]
                            {
                                new BasicAuthAuthorizationFilter(
                                new BasicAuthAuthorizationFilterOptions
                                    {
                                        RequireSsl = false,
                                        SslRedirect = false,
                                        LoginCaseSensitive = false,
                                        Users =
                                            new []
                                                        {
                                                            new BasicAuthAuthorizationUser
                                                            {
                                                                Login = AppSettings.hangfireSettings.Accessid,
                                                                PasswordClear =
                                                                    AppSettings.hangfireSettings.Accesspwd
                                                            }
                                                        }
                                    })
                            }
                });

            AppSettings.hangfireSettings.ServicesHangfireURL = $"{AppSettings.ServicesAPIURL}/{AppSettings.ApplicationName}_hangfire";
        }
    }
}