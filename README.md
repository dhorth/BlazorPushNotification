# RainMachine .Net Core API 

This is a .Net core facade for [RainMachine](https://www.rainmachine.com/) REST API.

| Branch | Status |
|:------:|:------:|
|main|[![Build Status](https://https://github.com/dhorth/RainMachineNet?branchName=master&label=build)](https://https://github.com/dhorth/RainMachineNet)|
# Configuration

**1. Copy service-worker.js to your wwwroot directory**
```
    self.addEventListener('install', async event => {
        console.log('Installing service worker...');
        self.skipWaiting(); });
    
    self.addEventListener('fetch', event => {
        // You can add custom logic here for controlling whether to use cached data if offline, etc.
        // The following line opts out, so requests go directly to the network as usual.
        return null; });
    
    self.addEventListener('push', event => {
        if (!(self.Notification && self.Notification.permission === 'granted')) {
            return;
        }
    
        var data = {};
        if (event.data) {
            data = event.data.json();
        }
        var title = data.Title || "Something Has Happened";
        var message = data.Body || "Here's something you might want to check out.";
        var url = data.Url || "";
        var icon = data.Icon || "/img/push-notification-icon.png";
    
        event.waitUntil(
            self.registration.showNotification(title, {
                body: message,
                icon: icon,
                vibrate: [100, 50, 100],
                data: { url: url }
            })
        ); });
    
    self.addEventListener('notificationclick', event => {
        event.notification.close();
        event.waitUntil(clients.openWindow(event.notification.data.url)); 
        });
```

**2.  Add the following snippet to your _Host.html**
```
    <script>
        navigator.serviceWorker.register('service-worker.js', { scope: "/" }).then((registration) => {
            if (registration.installing) {
                console.log("Install succeeded as the max allowed scope was overriden to '/'.");
                serviceWorker = registration.installing;
            } else if (registration.waiting) {
                console.log("service working waiting.");
                serviceWorker = registration.waiting;
            } else if (registration.active) {
                console.log("service working active.");
                serviceWorker = registration.active;
            }
            if (serviceWorker) {
                if (serviceWorker.state == "activated") {
                    console.log("service working activated.");
                    //initalize(registration);
                }
                serviceWorker.addEventListener("statechange", function (e) {
                    console.log("service working state changed "+e.target.state+".");
                    if (e.target.state == "activated") {
                        //initalize(registration);
                    }
                });
            }

        }).catch((err) => {
            console.error("SW registration failed with error " + err);
        });
    </script>
```
**3. Startup.cs** 
```
   using BlazorNotify.Service;
...
  public void ConfigureServices(IServiceCollection services)
  {
	 services.AddPushSubscriptionStore(Configuration)
       .AddPushNotificationService(Configuration)
       .AddPushNotificationsQueue();
...
   }
   
  public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
  {
...
      app.UsePushSubscriptionStore();
...
  }
```
 **4. appsettings.json**
 ```
 {
  "ConnectionStrings": {
    "PushSubscriptionSqliteDatabase": "Filename=./../pushsubscription.sqlitedb"
  },
  // ----------------------------------
  //Requires a VAPID key, use online generator to get a key pair
  //Goto https://tools.reactpwa.com/vapid
  // ----------------------------------
  "PushServiceClient": {
    "subject": "mailto:name@world.com",
    "publicKey": "< VAPID public key >",
    "privateKey": "< VAPID private key >"
  },
  "Serilog": {
    "MinimumLevel": "Debug"
  },
  "AllowedHosts": "*"
}

 ```
# Files

All methods are defined in RainMaker.cs

# Configuration
## Login
    Task<bool> LoginAsync(string netName, string userId, string pwd, string deviceId = "", int pollingTimeSeconds = 5);

NetName
User ID
Password
Device ID
Polling Seconds

## Event Subscription
### WateringEvent
OnCompleted
OnError
OnNext

    public class WateringEventTest : WateringNotificationSubscriber<WateringEvent>
    {
        private bool _watering;
        private int tick;
        private int zoneCount;
        public WateringEventTest()
        {
            _watering = true;
        }

        public override void OnNext(WateringEvent ev)
        {
            zoneCount=ev.Watering.zones.Max(a=>a.uid);
            foreach (var e in ev.Watering.zones)
            {
                Debugger.Log(1, "Test", $"Zone {e.uid}-{e.name} is currently {e.state}\r\n");
                Console.SetCursorPosition(5, e.uid);
                switch (e.state)
                {
                    case RainMachineNet.Model.Shared.WateringState.NotRunning:
                        Console.ForegroundColor= ConsoleColor.White;
                        break;
                    case RainMachineNet.Model.Shared.WateringState.Running:
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;
                    case RainMachineNet.Model.Shared.WateringState.Queued:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                }
                Console.WriteLine($"Zone {e.uid}-{e.name} is {e.state}");
            }
            tick++;
            if(tick>12)
                tick=1;

            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(0, zoneCount + 1);
            var indicator= "|".PadRight(tick, '.').PadRight(12, '-') + "|";
            Console.Write(indicator);

            _watering = ev.Watering.zones.Any(a => a.state == RainMachineNet.Model.Shared.WateringState.Running);
            base.OnNext(ev);
        }

        public bool IsWatering => _watering;
    }
}

## Exceptions
### RainMakerLoginException
### RainMakerExecuteException
### RainMakerAuthenicationException
### RainMachineNotificationSubscriberException

