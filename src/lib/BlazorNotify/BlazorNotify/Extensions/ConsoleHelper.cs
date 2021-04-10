using BlazorNotify.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace BlazorNotify.Extensions
{
    public static class BlazorNotifyConsoleHelper
    {
        public static async Task<IBlazorNotificationService> RunBlazorNotifyConsole(IConfiguration config)
        {
            IServiceCollection _services = null;
            //setup our DI
            var hostBuilder = new HostBuilder();
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                _services = services;
                services.AddSingleton(config);

                services.AddServerSideBlazor();
                services.AddPushSubscriptionStore(config);
                services.AddPushNotificationService(config);
                services.AddPushNotificationsQueue();
            });
            var host = await hostBuilder.StartAsync();
            var notificationService = host.Services.GetService<IBlazorNotificationService>();
            return notificationService;
        }
    }
}
