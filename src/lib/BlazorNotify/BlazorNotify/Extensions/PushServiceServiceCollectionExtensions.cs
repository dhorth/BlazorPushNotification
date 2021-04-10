using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Lib.Net.Http.WebPush;
using Microsoft.Extensions.Hosting;
using BlazorNotify.SqlLite;
using Microsoft.EntityFrameworkCore;
using BlazorNotify.Exceptions;
using Serilog;

namespace BlazorNotify.Service
{
    public static class PushServiceServiceCollectionExtensions
    {

        public static IServiceCollection AddPushSubscriptionStore(this IServiceCollection services, IConfiguration configuration)
        {
            var connString = configuration.GetConnectionString("PushSubscriptionSqliteDatabase");
            if (string.IsNullOrWhiteSpace(connString))
            {
                Log.Logger.Warning($"No Connection string specified, using default 'pushsubscription.sqlitedb'");
                connString = "Filename=./../pushsubscription.sqlitedb";
            }

            services.AddDbContext<PushSubscriptionContext>(options => options.UseSqlite(connString));

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

                var subject = pushNotificationServiceConfigurationSection.GetValue<string>(nameof(options.Subject));
                if (string.IsNullOrWhiteSpace(subject) || subject.Equals("<-- Replace with your value -->", System.StringComparison.OrdinalIgnoreCase))
                    throw new BlazorNotifyConfigurationException("PushServiceClient:Subject");
                options.Subject = subject;

                var publicKey = pushNotificationServiceConfigurationSection.GetValue<string>(nameof(options.PublicKey));
                if (string.IsNullOrWhiteSpace(publicKey) || subject.Equals("<-- Replace with your value -->", System.StringComparison.OrdinalIgnoreCase))
                    throw new BlazorNotifyConfigurationException("PushServiceClient:PublicKey");
                options.PublicKey = publicKey;

                var privateKey = pushNotificationServiceConfigurationSection.GetValue<string>(nameof(options.PrivateKey));
                if (string.IsNullOrWhiteSpace(privateKey) || subject.Equals("<-- Replace with your value -->", System.StringComparison.OrdinalIgnoreCase))
                    throw new BlazorNotifyConfigurationException("PushServiceClient:PrivateKey");
                options.PrivateKey = privateKey;

            });
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
