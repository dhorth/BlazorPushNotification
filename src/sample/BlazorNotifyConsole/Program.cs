using BlazorNotify.Extensions;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BlazorNotifyConsole
{
    class Program
    {
        public static readonly string AppName = "BlazorNotifyConsole";
        static async Task<int> Main(string[] args)
        {
            var config = LoadConfiguration();
            Log.Logger = CreateSerilogLogger(config, AppName);
            Log.Logger.Information("Starting application");

            var notificationService = await BlazorNotifyConsoleHelper.RunBlazorNotifyConsole(config);


            var msg = "Test Message";
            if (args.Length > 0)
                msg = args[0];

            //do the actual work here
            await notificationService.NotifyAsync("Blazor Notify Console", msg);

            Log.Logger.Information("All done!  Press any key to exit");
            Console.ReadKey();

            return 0;
        }



        public static ILogger CreateSerilogLogger(IConfiguration configuration, string appName)
        {
            var logger = new LoggerConfiguration()
                .Enrich.WithProperty("ApplicationContext", appName)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .ReadFrom.Configuration(configuration);
            
            return logger.CreateLogger();
        }


        public static IConfiguration LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            return builder.Build();
        }
    }

}