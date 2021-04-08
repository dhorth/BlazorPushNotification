using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.IO;

namespace BlazorNotifySample
{
    public class Program
    {
        public static readonly string Namespace = typeof(Program).Namespace;
        public static readonly string AppName = "BlazorNotifySample";

        public static int Main(string[] args)
        {
            var configuration = GetConfiguration();
            Log.Logger = CreateSerilogLogger(configuration, AppName);

            try
            {
                Log.Information("Configuring web host ({ApplicationContext})...", AppName);
                var host = BuildWebHost(configuration, args);

                Log.Information("Starting web host ({ApplicationContext})...", AppName);
                host.Run();

                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", AppName);
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }


        private static IWebHost BuildWebHost(IConfiguration configuration, string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .CaptureStartupErrors(false)
                .UseStartup<Startup>()
                .UseConfiguration(configuration)
                .UseSerilog()
                .Build();

        public static IConfiguration GetConfiguration()
        {
            var env = "Production";
#if DEBUG
            env = "Development";
#endif

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.Shared.{env}.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            return builder.Build();
        }
        public static ILogger CreateSerilogLogger(IConfiguration configuration, string appName)
        {

            var SeqServerUrl = configuration.GetSection("Seq")["SeqServerUrl"];
            var LogstashgUrl = configuration.GetSection("Seq")["LogstashgUrl"];
            var logFile = configuration.GetSection("Seq")["LogFileName"];

            var logger = new LoggerConfiguration()
                .Enrich.WithProperty("ApplicationContext", appName)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .WriteTo.Console()
                //.WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
                .ReadFrom.Configuration(configuration);

            if (!string.IsNullOrWhiteSpace(SeqServerUrl))
                logger.WriteTo.Seq(SeqServerUrl);

            if (!string.IsNullOrWhiteSpace(LogstashgUrl))
                logger.WriteTo.Http(LogstashgUrl);

            if (!string.IsNullOrWhiteSpace(logFile))
            {
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), appName);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                path = Path.Combine(path, logFile);
                Console.WriteLine($"Serilog self loger writting to file {path}");

                var file = File.CreateText(path);
                Serilog.Debugging.SelfLog.Enable(TextWriter.Synchronized(file));
            }
            return logger.CreateLogger();
        }
    }
}
