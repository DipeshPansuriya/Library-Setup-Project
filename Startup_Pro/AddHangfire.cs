using Hangfire;
using Hangfire.Dashboard.BasicAuthorization;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;

namespace InfraLib.Startup_Pro
{
    public static class AddHangFire
    {
        public static void Builder(WebApplicationBuilder builder, string dbname)
        {
            _ = builder.Services
                .AddHangfire(
                    configuration => configuration
                        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                        .UseSimpleAssemblyNameTypeSerializer()
                        .UseRecommendedSerializerSettings()
                        .UseSqlServerStorage(
                            dbname,
                            new SqlServerStorageOptions
                            {
                                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                                QueuePollInterval = TimeSpan.Zero,
                                UseRecommendedIsolationLevel = true,
                                DisableGlobalLocks = true,
                                SchemaName = $"{builder.Environment.ApplicationName}_Hangfire"
                            }));

            _ = builder.Services.AddHangfireServer();
        }

        public static string App(WebApplication app, string AccessUserId, string AccessPassword, string apiurl)
        {
            return ConfigureHangfireServices(app, AccessUserId, AccessPassword, apiurl);
        }

        private static string ConfigureHangfireServices(WebApplication app, string AccessUserId, string AccessPassword, string apiurl)
        {
            _ = app.UseHangfireDashboard(
                $"/{app.Environment.ApplicationName}_hangfire",
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
                                                                Login = AccessUserId,
                                                                PasswordClear = AccessPassword                                                            }
                                                        }
                                    })
                            }
                });

            return $"{apiurl}/{app.Environment.ApplicationName}_hangfire";
        }
    }
}