using System;
using Android.App;
using Android.Runtime;

//BADGES
[assembly: MetaData("com.pushwoosh.plugin.badge", Value = "com.pushwoosh.badge.BadgePlugin")]
//samsung
[assembly: UsesPermission("com.sec.android.provider.badge.permission.READ")]
[assembly: UsesPermission("com.sec.android.provider.badge.permission.WRITE")]
//htc
[assembly: UsesPermission("com.htc.launcher.permission.READ_SETTINGS")]
[assembly: UsesPermission("com.htc.launcher.permission.UPDATE_SHORTCUT")]
//sony
[assembly: UsesPermission("com.sonyericsson.home.permission.BROADCAST_BADGE")]
[assembly: UsesPermission("com.sonymobile.home.permission.PROVIDER_INSERT_BADGE")]
//apex
[assembly: UsesPermission("com.anddoes.launcher.permission.UPDATE_COUNT")]
//solid
[assembly: UsesPermission("com.majeur.launcher.permission.UPDATE_BADGE")]
//huawei
[assembly: UsesPermission("com.huawei.android.launcher.permission.CHANGE_BADGE")]
[assembly: UsesPermission("com.huawei.android.launcher.permission.READ_SETTINGS")]
[assembly: UsesPermission("com.huawei.android.launcher.permission.WRITE_SETTINGS")]
//ZUK
[assembly: UsesPermission("android.permission.READ_APP_BADGE")]
//OPPO
[assembly: UsesPermission("com.oppo.launcher.permission.READ_SETTINGS")]
[assembly: UsesPermission("com.oppo.launcher.permission.WRITE_SETTINGS")]
//EvMe
[assembly: UsesPermission("me.everything.badger.permission.BADGE_COUNT_READ")]
[assembly: UsesPermission("me.everything.badger.permission.BADGE_COUNT_WRITE")]


//Binding Inbox module AndroidManifest
[assembly: MetaData("com.pushwoosh.plugin.inbox", Value = "com.pushwoosh.inbox.PushwooshInboxPlugin")]

//Binding InboxDateFormatter interface
namespace Pushwoosh.Inbox
{
    [Register("com/pushwoosh/inbox/ui/model/customizing/formatter/InboxDateFormatter", DoNotGenerateAcw = true)]
    public interface IInboxDateFormatter : IJavaObject, IDisposable
    {
        [Register("transform", "(Ljava/util/Date;)Ljava/lang/String;", "GetTransformHandler:Pushwoosh.Inbox.IInboxDateFormatterInvoker, Pushwoosh.Inbox.Droid.Bindings")]
        Java.Lang.String Transform(Java.Util.Date date);
    }

    class IInboxDateFormatterInvoker : Java.Lang.Object, IInboxDateFormatter
    {

        IntPtr class_ref;

        public IInboxDateFormatterInvoker(IntPtr handle, JniHandleOwnership transfer)
            : base(handle, transfer)
        {
            IntPtr lref = JNIEnv.GetObjectClass(Handle);
            class_ref = JNIEnv.NewGlobalRef(lref);
            JNIEnv.DeleteLocalRef(lref);
        }

        protected override void Dispose(bool disposing)
        {
            if (this.class_ref != IntPtr.Zero)
                JNIEnv.DeleteGlobalRef(this.class_ref);
            this.class_ref = IntPtr.Zero;
            base.Dispose(disposing);
        }

        protected override Type ThresholdType
        {
            get { return typeof(IInboxDateFormatterInvoker); }
        }

        protected override IntPtr ThresholdClass
        {
            get { return class_ref; }
        }

        public static IInboxDateFormatter GetObject(IntPtr handle, JniHandleOwnership transfer)
        {
            return new IInboxDateFormatterInvoker(handle, transfer);
        }

        #region Transform
        IntPtr id_transform;
        public Java.Lang.String Transform(Java.Util.Date date)
        {
            if (id_transform == IntPtr.Zero)
                id_transform = JNIEnv.GetMethodID(class_ref, "transform",
                        "(Ljava/util/Date;)Ljava/lang/String;");
            IntPtr returnObject = JNIEnv.CallObjectMethod(Handle, id_transform,
                    new JValue(JNIEnv.ToJniHandle(date)));
            return Java.Lang.Object.GetObject<Java.Lang.String>(returnObject, JniHandleOwnership.DoNotTransfer);
        }

#pragma warning disable 0169
        static Delegate cb_transform;
        static Delegate GetTransformHandler()
        {
            if (cb_transform == null)
                cb_transform = JNINativeWrapper.CreateDelegate((Func<IntPtr, IntPtr, IntPtr, IntPtr>)n_Transform);
            return cb_transform;
        }

        static IntPtr n_Transform(IntPtr jnienv, IntPtr lrefThis, IntPtr date)
        {
            IInboxDateFormatter __this = Java.Lang.Object.GetObject<IInboxDateFormatter>(lrefThis, JniHandleOwnership.DoNotTransfer);
            using (var _date = GetObject<Java.Util.Date>(date, JniHandleOwnership.DoNotTransfer))
            {
                return __this.Transform(_date).Handle;
            }
        }
#pragma warning restore 0169
        #endregion
    }
}

namespace Pushwoosh.Inbox.UI
{
    public partial class PushwooshInboxStyle : Java.Lang.Object
    {
        private static IntPtr clazz_ref = JNIEnv.FindClass("com/pushwoosh/inbox/ui/PushwooshInboxStyle");
        private static IntPtr id_setDateFormatter = JNIEnv.GetMethodID(clazz_ref, "setDateFormatter", "(Lcom/pushwoosh/inbox/ui/model/customizing/formatter/InboxDateFormatter;)V");
        private static IntPtr id_getDateFormatter = JNIEnv.GetMethodID(clazz_ref, "getDateFormatter", "()Lcom/pushwoosh/inbox/ui/model/customizing/formatter/InboxDateFormatter;");
        public unsafe global::Pushwoosh.Inbox.IInboxDateFormatter DateFormatter
        {
            get
            {
                return GetObject<global::Pushwoosh.Inbox.IInboxDateFormatter>(JNIEnv.CallObjectMethod(Handle, id_getDateFormatter), JniHandleOwnership.DoNotTransfer);
            }
            set
            {
                JNIEnv.CallVoidMethod(Handle, id_setDateFormatter, new JValue(value));
            }
        }
    }
}