using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;

namespace PushwooshSDK.DotNet.Core
{
    public abstract class PushManager
    {
		/// <summary>
		/// Occurs when push actually was receved by application
		/// </summary>
        public event EventHandler<PushNotificationEventArgs>? PushReceived;
		/// <summary>
		/// Occurs when application received user tap on notification
		/// </summary>
        public event EventHandler<PushNotificationEventArgs>? PushAccepted;
		/// <summary>
		/// Occurs when application successfuly registered for push notifications.
		/// <para>
		/// The two platforms differ, and the difference is not going away — code against the weaker
		/// of the two guarantees:
		/// </para>
		/// <para>
		/// <b>Android</b> — raised only for registrations started through <see cref="Register"/>; an
		/// implicit FCM token refresh does not raise it. Not a guaranteed completion signal either:
		/// the native SDK can defer or drop a registration without reporting anything, so never
		/// block the UI waiting for one.
		/// </para>
		/// <para>
		/// <b>iOS</b> — raised whenever the app forwards a registration outcome to the SDK, which
		/// includes re-registrations the native SDK starts on its own, with no
		/// <see cref="Register"/> call behind them. Expect it to fire more than once per app run
		/// and without a preceding <see cref="Register"/>.
		/// </para>
		/// </summary>
		public event EventHandler? Registered;
		/// <summary>
		/// Occurs when application failed to register for push notifications.
		/// Platform differences are the same as for <see cref="Registered"/>.
		/// <para>
		/// On Android this can arrive <b>synchronously, before <see cref="Register"/> returns</b>,
		/// on whichever thread called it — that is the path taken when the native SDK is not
		/// initialised yet or when communication with Pushwoosh is disabled. A registration that
		/// actually reached the network reports back on the Android main thread instead. So a
		/// handler must be safe to run on an arbitrary thread, and must not assume
		/// <see cref="Register"/> has already returned. Exceptions thrown out of a handler on the
		/// synchronous path are swallowed and logged by the native SDK, not surfaced here.
		/// </para>
		/// </summary>
        public event EventHandler<PushwooshError>? RegisterError;

        /// <summary>
        /// Show push notifications alert when push notification is received while the app is running, default is `YES`
        /// </summary>
        public abstract bool ShowPushNotificationAlerts { get; set; }

        /// <summary>
        /// Set custom application language. Must be a lowercase two-letter code according to ISO-639-1 standard ("en", "de", "fr", etc.).
        /// Device language used by default.
        /// Set to nil if you want to use device language again.
        /// </summary>
        public abstract string Language { get; set; }

        /// <summary>
        /// Pushwoosh application code
        /// </summary>
        public abstract string AppCode { get; set; }
		/// <summary>
		/// Unique device identifier that used in all API calls with Pushwoosh.
		/// </summary>
		public abstract string HardwareId { get; }
		/// <summary>
		/// Current push token. May be null if no push token is available yet
		/// </summary>
		public abstract string PushToken { get; }

		/// <summary>
		/// Send tags to server. Tag names have to be created in the Pushwoosh Control Panel. Possible tag types: Integer, String, Incremental (integer only), List tags (array of values).
		/// </summary>
		public abstract Task<RequestResult> SendTagsAsync(TagsBundle tags);
		/// <summary>
		/// Get tags from server
		/// </summary>
		public abstract Task<RequestResult<TagsBundle>> LoadTagsAsync();
		/// <summary>
		/// Register for push notifications
		/// </summary>
		public abstract void Register();
		/// <summary>
		/// Unregisters device from push notifications
		/// </summary>
		public abstract void Unregister();

        //needed for NotificationSettigns covariance
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected abstract PushNotificationSettings GetNotificationSettings();

        /// <summary>
        /// Enabled remote notificaton types
        /// </summary>
        public PushNotificationSettings NotificationSettings => GetNotificationSettings();
        /// <summary>
        /// Schedules local notification
        /// </summary>
        public abstract int ScheduleLocalNotification(string body, int delay);
        /// <summary>
        /// Cancel local notification by id
        /// </summary>
        public abstract void CancelLocalNotification(int notificationId);
        /// <summary>
        /// Cancel all local notifications
        /// </summary>
        public abstract void CancelAllLocalNotifications();

        public IInAppManager? InAppManager { get; protected set; }
        public IInboxManager? InboxManager { get; protected set; }

        static PushManager? manager = null;
        public static PushManager? Instance 
        {
            get 
            {
                return manager;
            }
            [EditorBrowsable(EditorBrowsableState.Never)]
            set
            {
                manager = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void OnPushReceived(PushNotification notification, bool fromBackground)
        {
            PushReceived?.Invoke(this, new PushNotificationEventArgs() { Notification = notification, FromBackground = fromBackground });
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void OnPushAccepted(PushNotification notification, bool fromBackground)
        {
            PushAccepted?.Invoke(this, new PushNotificationEventArgs() { Notification = notification, FromBackground = fromBackground });
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void OnRegistered()
        {
            Registered?.Invoke(this, new EventArgs());
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void OnRegisterError(PushwooshError error)
        {
            RegisterError?.Invoke(this, error);
        }
    }
}
