self.addEventListener('install', async event => {
    console.log('Installing service worker...');
    self.skipWaiting();
});

self.addEventListener('fetch', event => {
    // You can add custom logic here for controlling whether to use cached data if offline, etc.
    // The following line opts out, so requests go directly to the network as usual.
    return null;
});

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
    );
});

self.addEventListener('notificationclick', event => {
    event.notification.close();
    event.waitUntil(clients.openWindow(event.notification.data.url));
});

