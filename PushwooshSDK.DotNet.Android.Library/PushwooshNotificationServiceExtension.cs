using System;
using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Runtime;

[assembly: MetaData("com.pushwoosh.notification_service_extension",
                    Value = "com.pushwoosh.plugin.PushwooshNotificationServiceExtension")]
namespace Pushwoosh.Android
{
    [Register("com/pushwoosh/plugin/PushwooshNotificationServiceExtension")]
    public class PushwooshNotificationServiceExtension : Notification.NotificationServiceExtension
    {
        // These lists are written from the thread that delivers the push — PushFcmIntentService,
        // a background thread — and read from whichever thread called PushManager.Init(), normally
        // main. A plain List across those two gives either "Collection was modified" during the
        // foreach or a push dropped between the loop and Clear(), so every access is under this
        // lock. It only matters when a push arrives before Init(), which means Init() was not
        // called from Application.OnCreate as the docs require — but that is exactly the case this
        // whole mechanism exists to cover, so it should hold up.
        readonly object rememberedLock = new object();
        List<Notification.PushMessage> rememberedReceivedPushes = new List<Notification.PushMessage>();
        List<Notification.PushMessage> rememberedOpenedPushes = new List<Notification.PushMessage>();

        protected override bool OnMessageReceived(Notification.PushMessage pushMessage)
        {
            base.OnMessageReceived(pushMessage);
            if (PushManager.Instance == null)
            {
                Remember(rememberedReceivedPushes, pushMessage);
                return false;
            }
            else
            {
                return CallPushReceived(pushMessage);
            }
        }

        // Subscribing per push used to add a handler every time and never remove one, so N pushes
        // before Init() meant N handlers, all kept alive for the life of the process.
        void Remember(List<Notification.PushMessage> into, Notification.PushMessage pushMessage)
        {
            lock (rememberedLock)
            {
                into.Add(pushMessage);
                PushManager.InitFinished -= OnPushwooshInitialized;
                PushManager.InitFinished += OnPushwooshInitialized;
            }
        }

        bool CallPushReceived(Notification.PushMessage pushMessage) {
            new Handler(ApplicationContext.MainLooper).Post(() =>
            {
                PushManager.Instance.OnPushReceived(PushManager.NotificationFromNative(pushMessage), !IsAppOnForeground);
            });
            return IsAppOnForeground && !PushManager.Instance.ShowPushNotificationAlerts;
        }

        protected override void OnMessageOpened(Notification.PushMessage pushMessage)
        {
            base.OnMessageOpened(pushMessage);
            if (PushManager.Instance == null)
            {
                Remember(rememberedOpenedPushes, pushMessage);
            }
            else
            {
                CallPushAccepted(pushMessage);
            }
        }

        void CallPushAccepted(Notification.PushMessage pushMessage) {
            new Handler(ApplicationContext.MainLooper).Post(() =>
            {
                PushManager.Instance.OnPushAccepted(PushManager.NotificationFromNative(pushMessage), !IsAppOnForeground);
            });
        }

        void OnPushwooshInitialized(object sender, EventArgs args) {
            if (PushManager.Instance == null)
                return;

            // Take both lists under the lock and hand back empty ones, then deliver outside it:
            // CallPushReceived posts to the main looper and reads PushManager.Instance, and holding
            // a lock across that is asking for trouble. A push arriving mid-delivery lands in the
            // fresh list and is picked up by the next Init, rather than being lost to a Clear().
            List<Notification.PushMessage> received;
            List<Notification.PushMessage> opened;
            lock (rememberedLock)
            {
                PushManager.InitFinished -= OnPushwooshInitialized;
                received = rememberedReceivedPushes;
                opened = rememberedOpenedPushes;
                rememberedReceivedPushes = new List<Notification.PushMessage>();
                rememberedOpenedPushes = new List<Notification.PushMessage>();
            }

            foreach (Notification.PushMessage message in received)
            {
                CallPushReceived(message);
            }
            foreach (Notification.PushMessage message in opened)
            {
                CallPushAccepted(message);
            }
        }
    }
}