export function requestPermission() {
    return Notification.permission;
}


export function showPrompt(message) {
    return prompt(message, 'Type anything here');
}

export function getSubscription() {
    // We need the service worker registration to check for a subscription
    var ret= navigator.serviceWorker.ready.then(function (serviceWorkerRegistration) {
        return serviceWorkerRegistration.pushManager.getSubscription().then(function (pushSubscription) {
            return pushSubscription.endpoint;
        }).catch(function (error) {
            console.log('Failed to get subscription from Push Notifications: ' + error);
        });
    });
    return ret;
}

export function subscribeForPushNotifications(applicationServerPublicKey) {
    var ret=navigator.serviceWorker.ready.then(function (serviceWorkerRegistration) {
        return serviceWorkerRegistration.pushManager.subscribe({
            userVisibleOnly: true,
            applicationServerKey: applicationServerPublicKey
        }).then(function (pushSubscription) {
            return pushSubscription;
        }).catch(function (error) {
            if (Notification.permission === 'denied') {
                console.log('Denied to subscribe for Push Notifications: ' + error);
            } else {
                console.log('Failed to subscribe for Push Notifications: ' + error);
            }
        });
    });
    return ret;
}

export function unsubscribeFromPushNotifications() {
    var ret=navigator.serviceWorker.ready.then(function (serviceWorkerRegistration) {
        return serviceWorkerRegistration.pushManager.getSubscription().then(function (pushSubscription) {
            if (pushSubscription) {
                var rc=pushSubscription.unsubscribe().then(function () {
                    return true;
                }).catch(function (error) {
                    console.log('Failed to unsubscribe from Push Notifications: ' + error);
                });
                if (rc)
                    return pushSubscription;
            }
        });
    });
    return ret;
}
