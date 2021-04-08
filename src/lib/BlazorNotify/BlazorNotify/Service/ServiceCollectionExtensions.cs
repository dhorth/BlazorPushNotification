using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BlazorNotify.SqlLite;

namespace BlazorNotify.Service
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPushSubscriptionStore(this IServiceCollection services, IConfiguration configuration)
        {
                    services.AddSqlitePushSubscriptionStore(configuration);
            return services;
        }

        public static IServiceCollection AddPushNotificationService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();

            services.AddPushServicePushNotificationService(configuration);

            return services;
        }

        public static IServiceCollection AddPushNotificationsQueue(this IServiceCollection services)
        {
            services.AddSingleton<IPushNotificationsQueue, PushNotificationsQueue>();
            services.AddSingleton<IHostedService, PushNotificationsDequeuer>();

            return services;
        }
    }
}
