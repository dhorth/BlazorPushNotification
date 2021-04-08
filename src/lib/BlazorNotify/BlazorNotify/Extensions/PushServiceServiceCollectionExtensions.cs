using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Lib.Net.Http.WebPush;
using Microsoft.Extensions.Hosting;
using BlazorNotify.SqlLite;
using Microsoft.EntityFrameworkCore;

namespace BlazorNotify.Service
{
    public static class PushServiceServiceCollectionExtensions
    {

        public static IServiceCollection AddPushSubscriptionStore(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddDbContext<PushSubscriptionContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("PushSubscriptionSqliteDatabase"))
            );

            services.AddTransient<IPushSubscriptionStore, SqlitePushSubscriptionStore>();
            services.AddHttpContextAccessor();
            services.AddSingleton<IPushSubscriptionStoreAccessorProvider, SqlitePushSubscriptionStoreAccessorProvider>();

            return services;
        }

        public static IServiceCollection AddPushNotificationService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();

            services.AddMemoryCache();
            services.AddMemoryVapidTokenCache();
            services.AddPushServiceClient(options =>
            {
                IConfigurationSection pushNotificationServiceConfigurationSection = configuration.GetSection(nameof(PushServiceClient));

                options.Subject = pushNotificationServiceConfigurationSection.GetValue<string>(nameof(options.Subject));
                options.PublicKey = pushNotificationServiceConfigurationSection.GetValue<string>(nameof(options.PublicKey));
                options.PrivateKey = pushNotificationServiceConfigurationSection.GetValue<string>(nameof(options.PrivateKey));
            });
            //services.AddTransient<IPushNotificationService, PushServicePushNotificationService>();
            services.AddScoped<IBlazorNotificationService, BlazorNotificationService>();

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
