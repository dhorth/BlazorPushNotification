using BlazorNotify.Model;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace BlazorNotify
{
    public class BlazorNotifyExceptionBase:Exception
    {
        public BlazorNotifyExceptionBase(string message):base(message)
        {

        }
    }

    public class BlazorNotifyPushException : BlazorNotifyExceptionBase
    {
        public BlazorNotifyPushException(string message, NotificationSubscription pushSubscription, HttpResponseMessage responseMessage) : base(message)
        {
            PushSubscription = pushSubscription;
            HttpResponseMessage = responseMessage;
        }

        public HttpStatusCode StatusCode => HttpResponseMessage.StatusCode;

        public HttpResponseHeaders Headers => HttpResponseMessage.Headers;
        public NotificationSubscription PushSubscription { get; set; }
        public HttpResponseMessage HttpResponseMessage { get; set; }
    }
}
