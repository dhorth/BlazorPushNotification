using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorNotify
{
    /// <summary>
    /// <seealso cref="https://developer.mozilla.org/en-US/docs/Web/API/ServiceWorkerRegistration/showNotification"/>
    /// </summary>
    public class NotificationOptions
    {
        public string Badge { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }
        public string Body { get; set; }
        public object Data { get; set; }
        public string Dir { get; set; } = "auto";
        public string Image { get; set; }
        public string Lang { get; set; } = "en";
        public bool? Renotify { get; set; }
        public bool? RequireInteraction { get; set; }
        public bool? Silent { get; set; }
        public string Tag { get; set; }
        public DateTime? TimeStamp { get; set; } = DateTime.UtcNow;
        public bool? Vibrate { get; set; }
    }
}
