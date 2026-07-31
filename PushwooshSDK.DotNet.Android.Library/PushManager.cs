using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pushwoosh;
using Pushwoosh.Function;
using Android.Runtime;
using System.Globalization;
using System.Linq;
using PushwooshSDK.DotNet.Core;
using Result = Pushwoosh.Function.Result;
using Pushwoosh.InApp;
using Pushwoosh.Notification;

namespace Pushwoosh.Android
{
    public class PushManager : PushwooshSDK.DotNet.Core.PushManager
    {
        internal static event EventHandler? InitFinished;
        public static new PushManager? Instance => global::PushwooshSDK.DotNet.Core.PushManager.Instance as PushManager;

        public static PushwooshError? ErrorFromJavaException(Java.Lang.Exception error)
        {
            return error != null ? new PushwooshError { NativeError = error, Description = error.Message } : null;
        }

        public static PushwooshError? ErrorFromJavaException(Java.Lang.Object error)
        {
            return ErrorFromJavaException(error.JavaCast<Java.Lang.Exception>());
        }

        public static PushNotification NotificationFromNative(Notification.PushMessage message) 
        {
            return new PushNotification()
            {
                Payload = message.ToJson().ToString(),
                Title = message.Header,
                Message = message.Message,
                CustomData = message.CustomData
            };
        }

        public class Callback : Java.Lang.Object, ICallback
        {
            public Action<Result>? ResultCallback { get; set; }

            public void Process(Result result) => ResultCallback?.Invoke(result);
        }

        PushNotificationsManager nativeManager;
        public PushManager()
        {
            nativeManager = PushNotificationsManager.Instance;
            InAppManager = new InAppManager();
            InboxManager = new InboxManager();
        }

        public bool MultiNotificationsMode
        {
            set 
            {
                PushwooshNotificationSettings.SetMultiNotificationMode(value);
            }
        }

        bool showPushNotificationsAlert = true;
        public override bool ShowPushNotificationAlerts 
        {
            get
            {
                return showPushNotificationsAlert;
            }
            set
            {
                showPushNotificationsAlert = value;
            }
        }

        public override string Language
        {
            get
            {
                return nativeManager.Language;
            }
            set
            {
                nativeManager.Language = value;
            }
        }

        public override string AppCode
        {
            get
            {
                return nativeManager.AppId;
            }
            set
            {
                nativeManager.AppId = value;
            }
        }

        public override string HardwareId => nativeManager.Hwid;

        public override string PushToken => nativeManager.PushToken;

        public override Task<RequestResult<TagsBundle>> LoadTagsAsync()
        {
            var source = new TaskCompletionSource<RequestResult<TagsBundle>>();
            nativeManager.GetTags(new Callback()
            {
                ResultCallback = (Result obj) =>
                {
                    var bundle = obj.Data as Tags.TagsBundle;
                    var tags = new TagsBundle();
                    if (bundle != null)
                    {
                        var jsonObject = bundle.ToJson();
                        var keysIterator = jsonObject.Keys();

                        while (keysIterator.HasNext)
                        {
                            var key = (keysIterator.Next() as Java.Lang.String).ToString();
                            var val = jsonObject.Get(key);
                            if (val is Java.Lang.String)
                                tags.PutString(key, val.ToString());
                            else if (val is Java.Lang.Integer)
                                tags.PutInteger(key, (val as Java.Lang.Integer).IntValue());
                            else if (val is Org.Json.JSONArray)
                            {
                                var jsonArray = val as Org.Json.JSONArray;
                                string[] list = new string[jsonArray.Length()];
                                for (int i = 0; i < list.Length; ++i)
                                {
                                    list[i] = jsonArray.Get(i).ToString();
                                }
                                tags.PutList(key, list);
                            }
                        }
                    }
                    if (obj.Exception != null)
                    {
                        source.SetResult(new RequestResult<TagsBundle>() { Error = ErrorFromJavaException(obj.Exception) });
                    }
                    else
                    {
                        source.SetResult(new RequestResult<TagsBundle>() { Result = tags });
                    }
                }
            });
            return source.Task;
        }

        public override void Register()
        {
            // The callback overload is what makes Registered/RegisterError fire: the
            // no-argument one reports nothing back, so the managed layer stayed silent
            // on Android while the native SDK registered fine.
            nativeManager.RegisterForPushNotifications(new Callback()
            {
                ResultCallback = (Result result) =>
                {
                    // Held in a local because each access crosses JNI; the other call sites in
                    // this class still read it twice, so this is a local choice, not a rule.
                    var nativeError = result.Exception;
                    if (nativeError == null)
                    {
                        OnRegistered();
                        return;
                    }

                    // The ?? never fires — ErrorFromJavaException only returns null for a null
                    // argument, and nativeError is non-null here. It is what convinces the
                    // nullable analysis that OnRegisterError's non-nullable parameter is satisfied,
                    // since the analysis cannot see through the call.
                    OnRegisterError(ErrorFromJavaException(nativeError)
                        ?? new PushwooshError { NativeError = nativeError });
                }
            });
        }

        public override Task<RequestResult> SendTagsAsync(TagsBundle tags)
        {
            // Returning null from a Task-returning method makes the caller's await throw
            // a NullReferenceException from inside the await machinery, with nothing in the
            // stack pointing at the real mistake.
            if (tags == null)
                throw new ArgumentNullException(nameof(tags));
            var source = new TaskCompletionSource<RequestResult>();
            var dict = tags.ToDictionary;
            Tags.TagsBundle.Builder tagsBundleBuilder = new Tags.TagsBundle.Builder();
            foreach (var kv in dict)
            {
                if (kv.Value is Nullable<int>)
                    tagsBundleBuilder.PutInt(kv.Key, ((int?)kv.Value).Value);
                else if (kv.Value is string)
                    tagsBundleBuilder.PutString(kv.Key, (string)kv.Value);
                else if (kv.Value is TagsBundle.IncrementalInteger)
                    tagsBundleBuilder.IncrementInt(kv.Key, ((TagsBundle.IncrementalInteger)kv.Value).Value);
                else if (kv.Value is IList<string>)
                    tagsBundleBuilder.PutList(kv.Key, (IList<string>)kv.Value);
            }
            nativeManager.SendTags(tagsBundleBuilder.Build(), new Callback() { ResultCallback = (Result obj) => {
                source.SetResult(new RequestResult { Error = ErrorFromJavaException(obj.Exception) });
                } } );
            return source.Task;
        }

        public override void Unregister()
        {
            nativeManager.UnregisterForPushNotifications();
        }

        protected override PushNotificationSettings GetNotificationSettings()
        {
            return new PushNotificationSettings() { Enabled = Notification.PushwooshNotificationSettings.AreNotificationsEnabled() };
        }

        public override int ScheduleLocalNotification(string body, int delay)
        {
            var notifiaction = new Notification.LocalNotification.Builder().SetMessage(body).SetDelay(delay).Build();
            return PushNotificationsManager.Instance.ScheduleLocalNotification(notifiaction).RequestId;
        }

        public override void CancelLocalNotification(int notificationId)
        {
            Notification.LocalNotificationReceiver.CancelNotification(notificationId);
        }

        public override void CancelAllLocalNotifications()
        {
            Notification.LocalNotificationReceiver.CancelAll();
        }

        public static void Init()
        {
            PushwooshSDK.DotNet.Core.PushManager.Instance = new PushManager();
            if (InitFinished != null)
                InitFinished(null, null);
        }

    }
}
