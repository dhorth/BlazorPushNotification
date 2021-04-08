using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Lib.Net.Http.WebPush;
using Microsoft.Extensions.Hosting;
using BlazorNotify.SqlLite;

namespace BlazorNotify.Service
{
    public static class PushServiceServiceCollectionExtensions
    {
        public static IServiceCollection AddPushServicePushNotificationService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMemoryCache();
            services.AddMemoryVapidTokenCache();
            services.AddPushServiceClient(options =>
            {
                IConfigurationSection pushNotificationServiceConfigurationSection = configuration.GetSection(nameof(PushServiceClient));

                options.Subject = pushNotificationServiceConfigurationSection.GetValue<string>(nameof(options.Subject));
                options.PublicKey = pushNotificationServiceConfigurationSection.GetValue<string>(nameof(options.PublicKey));
                options.PrivateKey = pushNotificationServiceConfigurationSection.GetValue<string>(nameof(options.PrivateKey));
            });
            services.AddTransient<IPushNotificationService, PushServicePushNotificationService>();

            return services;
        }

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
