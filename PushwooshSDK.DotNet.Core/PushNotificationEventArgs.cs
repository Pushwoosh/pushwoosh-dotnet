using System;
namespace PushwooshSDK.DotNet.Core
{
    public class PushNotificationEventArgs : EventArgs
    {
        public PushNotification? Notification { get; set; }
        public bool FromBackground { get; set; }
    }
}
