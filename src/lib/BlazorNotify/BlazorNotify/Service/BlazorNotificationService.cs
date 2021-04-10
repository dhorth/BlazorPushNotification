using System;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
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
        ValueTask<bool> Subscribe();
        ValueTask<bool> UnSubscribe();
        ValueTask<PermissionType> RequestPermissionAsync();
        ValueTask<SubscriptionStatus> GetSubscriptionStatus();
        ValueTask<bool> NotifyAsync(string title, string message);
    }
    public class BlazorNotificationService : IBlazorNotificationService, IDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> _moduleTask;
        private readonly IPushSubscriptionStore _subscriptionStore;
        private readonly IPushNotificationsQueue _pushNotificationsQueue;
        private readonly PushServiceClient _pushClient;


        public BlazorNotificationService(IJSRuntime jsRuntime,
            PushServiceClient pushClient,
            IPushSubscriptionStore subscriptionStore,
            IPushNotificationsQueue pushNotificationsQueue)
        {
            _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/BlazorNotify/pushNotifications.js").AsTask());
            _pushClient = pushClient;
            _subscriptionStore = subscriptionStore;
            _pushNotificationsQueue = pushNotificationsQueue;
        }

        public async ValueTask<bool> Subscribe()
        {
            bool rc = false;
            try
            {
                Log.Logger.Debug("Subscribe and save subscription");
                var module = await _moduleTask.Value;
                var subscription = await module.InvokeAsync<PushSubscription>("subscribeForPushNotifications", _pushClient.DefaultAuthentication.PublicKey);
                await _subscriptionStore.StoreSubscriptionAsync(subscription);
            }
            catch (Exception ex)
            {
                Log.Logger.Error("Subscribe", ex);
            }
            Log.Logger.Information($"Subscribe() => {rc}");
            return rc;
        }
        public async ValueTask<bool> UnSubscribe()
        {
            bool rc = false;
            try
            {
                Log.Logger.Debug("UnSubscribe and remove subscription");
                var module = await _moduleTask.Value;
                var subscription = await module.InvokeAsync<PushSubscription>("unsubscribeFromPushNotifications");
                await _subscriptionStore.DiscardSubscriptionAsync(subscription.Endpoint);
            }
            catch (Exception ex)
            {
                Log.Logger.Error("UnSubscribe", ex);
            }
            Log.Logger.Information($"UnSubscribe() => {rc}");
            return rc;
        }

        public async ValueTask<SubscriptionStatus> GetSubscriptionStatus()
        {
            var ret = SubscriptionStatus.Default;
            try
            {
                Log.Logger.Debug("GetSubscriptionStatus()");
                var module = await _moduleTask.Value;
                var subscription = await module.InvokeAsync<string>("getSubscription");

                if (string.IsNullOrWhiteSpace(subscription))
                    ret = SubscriptionStatus.Default;
                else if (subscription.Equals(_pushClient.DefaultAuthentication.PublicKey))
                    ret = SubscriptionStatus.Granted;
                else
                    ret = SubscriptionStatus.Denied;

            }
            catch (Exception ex)
            {
                Log.Logger.Error("GetSubscriptionStatus", ex);
            }
            Log.Logger.Information($"GetSubscriptionStatus() => {ret}");
            return ret;
        }
        public async ValueTask<PermissionType> RequestPermissionAsync()
        {
            var ret = PermissionType.Default;
            try
            {
                Log.Logger.Debug("RequestPermissionAsync()");
                var module = await _moduleTask.Value;
                string permission = await module.InvokeAsync<string>("requestPermission");

                if (permission.Equals("granted", StringComparison.InvariantCultureIgnoreCase))
                    ret = PermissionType.Granted;

                if (permission.Equals("denied", StringComparison.InvariantCultureIgnoreCase))
                    ret = PermissionType.Denied;
            }
            catch (Exception ex)
            {
                Log.Logger.Error("RequestPermissionAsync", ex);
            }
            Log.Logger.Information($"RequestPermissionAsync() => {ret}");
            return ret;
        }
        public async ValueTask<bool> NotifyAsync(string topic, string message)
        {
            bool rc = false;
            try
            {
                await Task.Run(() =>
                {
                    Log.Logger.Debug($"NotifyAsync({topic},{message})");
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
                    rc = true;
                });
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"NotifyAsync {topic}", ex);
                rc = false;
            }
            Log.Logger.Information($"NotifyAsync({topic},{message}) => {rc}");
            return rc;
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
