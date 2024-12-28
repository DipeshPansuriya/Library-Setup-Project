using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InfraLib.Startup_Pro
{
    public static class AddAppSetting
    {
        public static void SetAppSettings(WebApplicationBuilder builder)
        {
            DBSettings dbconfig = new();
            builder.Configuration.Bind("DBSettings", dbconfig);

            HangfireSettings hangfireconfig = new();
            builder.Configuration.Bind("HangfireSettings", hangfireconfig);

            AppSettings.dbSettings = dbconfig;

            SqlConnectionStringBuilder masterdb = new(AppSettings.dbSettings.MastersDb);
            AppSettings.dbSettings.MastersDb = masterdb.InitialCatalog;

            SqlConnectionStringBuilder masterlogdb = new(AppSettings.dbSettings.MastersLogDb);
            AppSettings.dbSettings.MastersLogDbName = masterlogdb.InitialCatalog;

            SqlConnectionStringBuilder masterhangfiredb = new(AppSettings.dbSettings.MastersHangfireDb);
            AppSettings.dbSettings.MastersLogDbName = masterhangfiredb.InitialCatalog;

            AppSettings.hangfireSettings = hangfireconfig;

            AppSettings.EnvironmentName = builder.Environment.EnvironmentName.ToLower().Contains("dev")
                ? "dev"
                : builder.Environment.EnvironmentName.ToLower();

            AppSettings.ServicesWWWRoot = string.IsNullOrWhiteSpace(builder.Environment.WebRootPath)
                ? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")
                : builder.Environment.WebRootPath;

            AppSettings.ApplicationName = builder.Environment.ApplicationName;

            AppSettings.AllowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
        }

        public static void SetServicesURL(WebApplication app)
        {
            _ = app.Lifetime.ApplicationStarted
                .Register(
                    () =>
                    {
                        Console.WriteLine("Application has started!");

                        LogApplicationUrls(app);
                    });
        }

        private static void LogApplicationUrls(WebApplication app)
        {
            IServer server = app.Services.GetRequiredService<IServer>();
            IServerAddressesFeature addresses = server.Features.Get<IServerAddressesFeature>();

            if (addresses != null)
            {
                Console.WriteLine("Application URLs:");
                foreach (string address in addresses.Addresses)
                {
                    Console.WriteLine(address);
                    AppSettings.ServicesAPIURL = address;
                }
            }
        }
    }
}