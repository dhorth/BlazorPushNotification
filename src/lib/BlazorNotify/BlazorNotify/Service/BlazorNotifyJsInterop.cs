using BlazorNotify.Model;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorNotify.Service
{
    public class BlazorNotifyJsInterop : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;

        public BlazorNotifyJsInterop(IJSRuntime jsRuntime)
        {
            moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
               "import", "./_content/BlazorNotify/pushNotifications.js").AsTask());
        }

        public async ValueTask<string> Prompt(string message)
        {
            var module = await moduleTask.Value;
            return await module.InvokeAsync<string>("showPrompt", message);
        }

        public async ValueTask DisposeAsync()
        {
            if (moduleTask.IsValueCreated)
            {
                var module = await moduleTask.Value;
                await module.DisposeAsync();
            }
        }

        public async ValueTask<bool> IsBrowserSupported()
        {
            return true;
        }
        public async ValueTask<NotificationSubscription> GetSubscription()
        {
            return new NotificationSubscription();
        }
    }
}