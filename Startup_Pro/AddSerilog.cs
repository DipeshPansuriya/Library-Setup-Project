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
            string dbname = null, string sequrl = null, string filepath = null, bool isconsole = true)
        {
            _ = builder.Services
                .AddHttpLogging(logging =>
                {
                    logging.LoggingFields = HttpLoggingFields.All;
                    logging.RequestBodyLogLimit = int.MaxValue;
                    logging.ResponseBodyLogLimit = int.MaxValue;
                    logging.MediaTypeOptions.AddText("application/javascript");
                    logging.CombineLogs = true;
                });

            LoggerConfiguration loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Error()
                .MinimumLevel.Override(builder.Environment.ApplicationName, LogEventLevel.Debug)
                .Enrich.WithProcessId()
                .Enrich.WithEnvironmentName()
                .Enrich.WithMachineName()
                .Enrich.WithClientIp()
                .Enrich.WithCorrelationId()
                .Enrich.WithProcessName()
                .Enrich.FromLogContext();

            AddFileLogging(loggerConfig, filepath);
            AddSqlLogging(loggerConfig, dbname);
            AddSeqLogging(loggerConfig, sequrl);
            AddConsoleLogging(loggerConfig, isconsole);

            Log.Logger = loggerConfig.CreateLogger();
            _ = builder.Host.UseSerilog();
        }

        public static void Builder(WebApplicationBuilder builder, string filepath)
        {
            Builder(builder, null, null, filepath, true);
        }

        public static void Builder(WebApplicationBuilder builder, string dbname, string sequrl)
        {
            Builder(builder, dbname, sequrl, null, true);
        }

        public static void Builder(WebApplicationBuilder builder, bool isconsole)
        {
            Builder(builder, null, null, null, isconsole);
        }

        private static void AddFileLogging(LoggerConfiguration loggerConfig, string filepath)
        {
            if (!string.IsNullOrEmpty(filepath))
            {
                _ = loggerConfig.WriteTo.File(
                    $@"{filepath}\Logs\Error\{DateTime.Now:yyyy-MM-dd}log.txt",
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
        }

        private static void AddSqlLogging(LoggerConfiguration loggerConfig, string dbname)
        {
            if (!string.IsNullOrEmpty(dbname))
            {
                _ = loggerConfig.WriteTo.MSSqlServer(
                    connectionString: dbname,
                    sinkOptions: new MSSqlServerSinkOptions
                    {
                        TableName = "LogsTable",
                        AutoCreateSqlTable = true
                    },
                    columnOptions: new ColumnOptions
                    {
                        AdditionalColumns =
                        {
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
                        },
                    });
            }
        }

        private static void AddSeqLogging(LoggerConfiguration loggerConfig, string sequrl)
        {
            if (!string.IsNullOrEmpty(sequrl))
            {
                _ = loggerConfig.WriteTo.Seq(sequrl);
            }
        }

        private static void AddConsoleLogging(LoggerConfiguration loggerConfig, bool isconsole)
        {
            if (isconsole)
            {
                _ = loggerConfig.WriteTo.Console(
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
        }

        public static void App(WebApplication app)
        {
            _ = app.UseHttpLogging();
            _ = app.UseSerilogRequestLogging();
        }
    }
}

