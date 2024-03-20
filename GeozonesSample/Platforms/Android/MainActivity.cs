using Android.App;
using Android.Content.PM;
using Android.OS;
using Pushwoosh.Android;
using PushwooshSDK.DotNet.Geozones.Android;

[assembly: MetaData("com.pushwoosh.appid", Value = "A8B44-0B460")]
[assembly: MetaData("com.pushwoosh.senderid", Value = "@string/fcm_sender_id")]

namespace GeozonesSample;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        PushManager.Init();
        LocationManager.Init();
    }
}

