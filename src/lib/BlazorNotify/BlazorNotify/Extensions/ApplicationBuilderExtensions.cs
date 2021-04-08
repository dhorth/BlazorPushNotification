using BlazorNotify.SqlLite;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorNotify.Service
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UsePushSubscriptionStore(this IApplicationBuilder app)
        {
                app.UseSqlitePushSubscriptionStore();

            return app;
        }

        public static IApplicationBuilder UseSqlitePushSubscriptionStore(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                PushSubscriptionContext context = serviceScope.ServiceProvider.GetService<PushSubscriptionContext>();
                context.Database.EnsureCreated();
            }

            return app;
        }

    }
}
