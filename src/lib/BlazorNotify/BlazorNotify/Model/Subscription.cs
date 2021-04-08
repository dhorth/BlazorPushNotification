using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorNotify.Model
{
    public class NotificationSubscription
    {
        public NotificationSubscription() { }
        public NotificationSubscription(string endpoint, string p256dh, string auth)
        {
            Endpoint = endpoint;
            P256DH = p256dh;
            Auth = auth;
        }

        public string Endpoint { get; set; }
        public string P256DH { get; set; }
        public string Auth { get; set; }
    }
}