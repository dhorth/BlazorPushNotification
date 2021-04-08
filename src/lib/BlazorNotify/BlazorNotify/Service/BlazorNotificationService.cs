using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using BlazorNotify.Model;
using BlazorNotify.SqlLite;
using Lib.Net.Http.WebPush;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using Serilog;

namespace BlazorNotify.Service
{
    public enum PermissionType
    {
        Default = 0,
        Granted,
        Denied
    }

    public enum SubscriptionStatus
    {
        Default = 0,
        Granted,
        Denied
    }

    public interface IBlazorNotificationService
    {
        Task<bool> IsBrowserSupported { get; }
        Task<string> GetPublicKey();
        Task<bool> Subscribe();
        Task<bool> UnSubscribe();
        Task<PermissionType> RequestPermissionAsync();
        Task<SubscriptionStatus> GetSubscriptionStatus();
        Task<string> SaveSubscribe();
        Task<bool> NotifyAsync(string title, string message, PushMessageUrgency icon);
    }
    public class BlazorNotificationService : IBlazorNotificationService, IDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> _moduleTask;
        private readonly IPushSubscriptionStore _subscriptionStore;
        private readonly IPushNotificationService _notificationService;
        private readonly IPushNotificationsQueue _pushNotificationsQueue;

        public Task<bool> IsBrowserSupported => throw new NotImplementedException();

        public BlazorNotificationService(IJSRuntime jsRuntime, IPushSubscriptionStore subscriptionStore, IPushNotificationService notificationService, IPushNotificationsQueue pushNotificationsQueue)
        {
            _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
               "import", "./_content/BlazorNotify/pushNotifications.js").AsTask());
            _subscriptionStore = subscriptionStore;
            _notificationService = notificationService;
            _pushNotificationsQueue = pushNotificationsQueue;
        }

        public async Task<bool> Subscribe()
        {
            bool rc = false;
            try
            {
                var module = await _moduleTask.Value;
                var subscription = await module.InvokeAsync<PushSubscription>("subscribeForPushNotifications", _notificationService.PublicKey);
                await _subscriptionStore.StoreSubscriptionAsync(subscription);

            }
            catch (Exception ex)
            {
                Log.Logger.Error("", ex);
            }
            return rc;
        }
        public async Task<bool> UnSubscribe()
        {
            bool rc = false;
            try
            {
                var module = await _moduleTask.Value;
                var subscription = await module.InvokeAsync<PushSubscription>("unsubscribeFromPushNotifications");
                await _subscriptionStore.DiscardSubscriptionAsync(subscription.Endpoint);
            }
            catch (Exception ex)
            {
                Log.Logger.Error("", ex);
            }
            return rc;
        }

        public async Task<bool> IsSupportedByBrowserAsync()
        {
            bool rc = false;
            try
            {
                var module = await _moduleTask.Value;
                rc = await module.InvokeAsync<bool>("showPrompt", "test");
            }
            catch (Exception ex)
            {
                Log.Logger.Error("", ex);
            }
            return rc;
        }
        public async Task<SubscriptionStatus> GetSubscriptionStatus()
        {
            SubscriptionStatus ret = SubscriptionStatus.Default;
            try
            {
                var module = await _moduleTask.Value;
                var subscription = await module.InvokeAsync<string>("getSubscription");

                if (string.IsNullOrWhiteSpace(subscription))
                    ret = SubscriptionStatus.Default;
                else if (subscription.Equals(_notificationService.PublicKey))
                    return SubscriptionStatus.Granted;
                else
                    return SubscriptionStatus.Denied;

            }
            catch (Exception ex)
            {
                Log.Logger.Error("", ex);
            }

            return ret;
        }
        public async Task<PermissionType> RequestPermissionAsync()
        {
            try
            {
                var module = await _moduleTask.Value;
                string permission = await module.InvokeAsync<string>("requestPermission");

                if (permission.Equals("granted", StringComparison.InvariantCultureIgnoreCase))
                    return PermissionType.Granted;

                if (permission.Equals("denied", StringComparison.InvariantCultureIgnoreCase))
                    return PermissionType.Denied;
            }
            catch (Exception ex)
            {
                Log.Logger.Error("", ex);
            }

            return PermissionType.Default;
        }

        public async Task<bool> NotifyAsync(string topic, string message, PushMessageUrgency urgency)
        {
            bool rc = false;
            try
            {
                var content = new BlazorNotifyMessage
                {
                    Title = topic,
                    Body = message,
                    Icon = "",
                    Url = "",
                };

                _pushNotificationsQueue.Enqueue(new PushMessage(content.ToJson())
                {
                    Topic = topic,
                    Urgency = PushMessageUrgency.High
                });
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"NotifyAsync {topic}", ex);
                rc = false;
            }
            return rc;
        }


        public Task<string> GetPublicKey()
        {
            throw new NotImplementedException();
        }
        public Task<string> SaveSubscribe()
        {
            throw new NotImplementedException();
        }



        public void Dispose()
        {
            Task.Run(async () =>
            {
                await DisposeAsync();
            });

        }

        public async Task DisposeAsync()
        {
            if (_moduleTask.IsValueCreated)
            {
                var module = await _moduleTask.Value;
                await module.DisposeAsync();
            }
        }
    }
}
