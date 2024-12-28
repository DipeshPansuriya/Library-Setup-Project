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
        public static AppSettings SetAppSettings(WebApplicationBuilder builder)
        {
            AppSettings appSettings = new();
            DBSettings dbconfig = new();
            builder.Configuration.Bind("DBSettings", dbconfig);

            HangfireSettings hangfireconfig = new();
            builder.Configuration.Bind("HangfireSettings", hangfireconfig);

            appSettings.dbSettings = dbconfig;

            SqlConnectionStringBuilder masterdb = new(appSettings.dbSettings.MastersDb);
            appSettings.dbSettings.MastersDb = masterdb.InitialCatalog;

            SqlConnectionStringBuilder masterlogdb = new(appSettings.dbSettings.MastersLogDb);
            appSettings.dbSettings.MastersLogDbName = masterlogdb.InitialCatalog;

            SqlConnectionStringBuilder masterhangfiredb = new(appSettings.dbSettings.MastersHangfireDb);
            appSettings.dbSettings.MastersLogDbName = masterhangfiredb.InitialCatalog;

            appSettings.hangfireSettings = hangfireconfig;

            appSettings.EnvironmentName = builder.Environment.EnvironmentName.ToLower().Contains("dev")
                ? "dev"
                : builder.Environment.EnvironmentName.ToLower();

            appSettings.ServicesWWWRoot = string.IsNullOrWhiteSpace(builder.Environment.WebRootPath)
                ? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")
                : builder.Environment.WebRootPath;

            appSettings.ApplicationName = builder.Environment.ApplicationName;

            appSettings.AllowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
            return appSettings;
        }

        public static string SetServicesURL(WebApplication app)
        {
            string url = string.Empty;
            _ = app.Lifetime.ApplicationStarted
                .Register(
                    () =>
                    {
                        Console.WriteLine("Application has started!");

                        url = LogApplicationUrls(app);
                    });
            return url;
        }

        private static string LogApplicationUrls(WebApplication app)
        {
            IServer server = app.Services.GetRequiredService<IServer>();
            IServerAddressesFeature addresses = server.Features.Get<IServerAddressesFeature>();
            string url = string.Empty;
            if (addresses != null)
            {
                Console.WriteLine("Application URLs:");

                foreach (string address in addresses.Addresses)
                {
                    Console.WriteLine(address);
                    url = address;
                }
            }
            return url;
        }
    }
}