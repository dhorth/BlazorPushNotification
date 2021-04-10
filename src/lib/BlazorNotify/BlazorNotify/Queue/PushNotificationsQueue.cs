using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Lib.Net.Http.WebPush;
using Serilog;

namespace BlazorNotify.Service
{
    public interface IPushNotificationsQueue
    {
        void Enqueue(PushMessage message);
        Task<PushMessage> DequeueAsync(CancellationToken cancellationToken);
    }

    internal class PushNotificationsQueue : IPushNotificationsQueue
    {
        private readonly ConcurrentQueue<PushMessage> _messages = new ConcurrentQueue<PushMessage>();
        private readonly SemaphoreSlim _messageEnqueuedSignal = new SemaphoreSlim(0);

        public void Enqueue(PushMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            Log.Logger.Debug($"Enqueue message {message.Topic}");
            _messages.Enqueue(message);
            _messageEnqueuedSignal.Release();
            Log.Logger.Debug($"Message Enqueued {message.Topic}");
        }

        public async Task<PushMessage> DequeueAsync(CancellationToken cancellationToken)
        {
            await _messageEnqueuedSignal.WaitAsync(cancellationToken);

            Log.Logger.Debug("Dequeue message");
            _messages.TryDequeue(out PushMessage message);
            Log.Logger.Debug($"Dequeue message {message.Topic}");

            return message;
        }
    }
}
