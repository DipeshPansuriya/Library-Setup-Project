// Ignore Spelling: Serilog

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System.Data;

namespace InfraLib.Startup_Pro
{
    public static class AddSerilog
    {
        public static void Builder(
            WebApplicationBuilder builder,
            List<LogWriteTo> logWriteTargets,
            string applicationname)
        {
            _ = builder.Services
                .AddHttpLogging(
                    logging =>
                    {
                        logging.LoggingFields = HttpLoggingFields.All;
                        logging.RequestBodyLogLimit = int.MaxValue;
                        logging.ResponseBodyLogLimit = int.MaxValue;
                        logging.MediaTypeOptions.AddText("application/javascript");
                        logging.CombineLogs = true;
                    });

            LoggerConfiguration loggerConfig = new LoggerConfiguration()
                .MinimumLevel
                .Error()
                .MinimumLevel
                .Override(applicationname, LogEventLevel.Debug)
                .Enrich
                .WithProcessId()
                .Enrich
                .WithEnvironmentName()
                .Enrich
                .WithMachineName()
                .Enrich
                .WithClientIp()
                .Enrich
                .WithCorrelationId()
                .Enrich
                .WithProcessName()
                .Enrich
                .FromLogContext();

            if (logWriteTargets.Contains(LogWriteTo.File))
            {
                _ = loggerConfig.WriteTo
                    .File(
                        $@"{AppSettings.ServicesWWWRoot}\Logs\Error\{DateTime.Now:yyyy-MM-dd}log.txt",
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: "{NewLine}--------------------Log Start --------------------" +
                            "{NewLine}{Timestamp:yyyy-MM-dd HH:mm} {NewLine}" +
                            "{NewLine}<{MachineName}> {NewLine} {Message} Error Level: [{Level}] {NewLine}" +
                            "--------------------Source Context --------------------" +
                            "{NewLine}({SourceContext}) {NewLine}" +
                            "--------------------Message--------------------" +
                            "{NewLine}{Message}{NewLine}" +
                            "--------------------Exception--------------------" +
                            "{NewLine}{Exception}{NewLine}" +
                            "--------------------Log End--------------------{NewLine}");
            }

            if (logWriteTargets.Contains(LogWriteTo.SQL))
            {
                _ = loggerConfig.WriteTo
                    .MSSqlServer(
                        connectionString: AppSettings.dbSettings.MastersLogDb,
                        sinkOptions: new MSSqlServerSinkOptions
                        {
                            TableName = "LogsTable",
                            AutoCreateSqlTable = true
                        },
                        columnOptions: new ColumnOptions
                        {
                            AdditionalColumns =
                                [
                                        new("LogLevel", SqlDbType.NVarChar),
                                        new("EventId", SqlDbType.BigInt),
                                        new("SourceContext", SqlDbType.NVarChar),
                                        new("CorrelationId", SqlDbType.NVarChar),
                                        new("ProcessName", SqlDbType.NVarChar),
                                        new("ProcessId", SqlDbType.BigInt),
                                        new("ThreadId", SqlDbType.BigInt),
                                        new("MachineName", SqlDbType.NVarChar),
                                        new("ClientIp", SqlDbType.NVarChar),
                                        new("EnvironmentName", SqlDbType.NVarChar)
                                    ],
                        });
            }

            if (logWriteTargets.Contains(LogWriteTo.SEQ) &&
                !string.IsNullOrEmpty(AppSettings.SeqURL))
            {
                _ = loggerConfig.WriteTo.Seq(AppSettings.SeqURL);
            }

            if (logWriteTargets.Contains(LogWriteTo.Console))
            {
                _ = loggerConfig.WriteTo
                    .Console(
                        outputTemplate: "{NewLine}--------------------Log Start --------------------" +
                            "{NewLine}{Timestamp:yyyy-MM-dd HH:mm} {NewLine}" +
                            "{NewLine}<{MachineName}> {NewLine} {Message} Error Level: [{Level}] {NewLine}" +
                            "--------------------Source Context --------------------" +
                            "{NewLine}({SourceContext}) {NewLine}" +
                            "--------------------Message--------------------" +
                            "{NewLine}{Message}{NewLine}" +
                            "--------------------Exception--------------------" +
                            "{NewLine}{Exception}{NewLine}" +
                            "--------------------Log End--------------------{NewLine}");
            }

            Log.Logger = loggerConfig.CreateLogger();

            _ = builder.Host.UseSerilog();
        }

        public static void App(WebApplication app)
        {
            _ = app.UseHttpLogging();
            _ = app.UseSerilogRequestLogging();
        }
    }

    public enum LogWriteTo
    {
        SQL,
        File,
        Console,
        SEQ
    }
}