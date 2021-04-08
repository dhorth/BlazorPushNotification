using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Lib.Net.Http.WebPush;
using BlazorNotify.SqlLite;
using System;
using System.Net;
using Serilog;

namespace BlazorNotify.Service
{
    internal class PushNotificationsDequeuer : IHostedService
    {
        private readonly IPushSubscriptionStoreAccessorProvider _subscriptionStoreAccessorProvider;
        private readonly IPushNotificationsQueue _messagesQueue;
        private readonly CancellationTokenSource _stopTokenSource = new CancellationTokenSource();
        private readonly PushServiceClient _pushClient;

        private Task _dequeueMessagesTask;

        public PushNotificationsDequeuer(IPushNotificationsQueue messagesQueue,
            IPushSubscriptionStoreAccessorProvider subscriptionStoreAccessorProvider,
            PushServiceClient pushClient
            )
        {
            _subscriptionStoreAccessorProvider = subscriptionStoreAccessorProvider;
            _messagesQueue = messagesQueue;
            _pushClient = pushClient;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _dequeueMessagesTask = Task.Run(DequeueMessagesAsync);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _stopTokenSource.Cancel();

            return Task.WhenAny(_dequeueMessagesTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }

        private async Task DequeueMessagesAsync()
        {
            while (!_stopTokenSource.IsCancellationRequested)
            {
                PushMessage message = await _messagesQueue.DequeueAsync(_stopTokenSource.Token);

                if (!_stopTokenSource.IsCancellationRequested)
                {
                    using (IPushSubscriptionStoreAccessor subscriptionStoreAccessor = _subscriptionStoreAccessorProvider.GetPushSubscriptionStoreAccessor())
                    {
                        await subscriptionStoreAccessor.PushSubscriptionStore.ForEachSubscriptionAsync((PushSubscription subscription) =>
                        {
                            // Fire-and-forget 
                            //_notificationService.SendNotificationAsync(subscription, message, _stopTokenSource.Token);
                            try
                            {
                                _pushClient.RequestPushMessageDeliveryAsync(subscription, message, _stopTokenSource.Token);
                                Log.Logger.Information($"Message set to {subscription.Endpoint}.");
                            }
                            catch (PushServiceClientException ex)
                            {
                                if (ex.StatusCode == HttpStatusCode.NotFound || ex.StatusCode == HttpStatusCode.Gone)
                                {
                                    using (IPushSubscriptionStoreAccessor subscriptionStoreAccessor = _subscriptionStoreAccessorProvider.GetPushSubscriptionStoreAccessor())
                                    {
                                        subscriptionStoreAccessor.PushSubscriptionStore.DiscardSubscriptionAsync(subscription.Endpoint);
                                    }
                                    Log.Logger.Information("Subscription has expired or is no longer valid and has been removed.");
                                }
                            }
                            catch (Exception ex)
                            {
                                    Log.Logger.Error(ex, "Failed requesting push message delivery to {0}.", subscription.Endpoint);
                            }

                        }, _stopTokenSource.Token);
                    }

                }
            }

        }
    }
}
