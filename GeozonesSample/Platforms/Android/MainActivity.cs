using Android.App;
using Android.Content.PM;

[assembly: MetaData("com.pushwoosh.appid", Value = "XXXXX-XXXXX")]

namespace GeozonesSample;

// PushManager.Init() and LocationManager.Init() are called from MainApplication.OnCreate,
// which runs before any activity exists.
[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
}

