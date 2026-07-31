using Android.App;
using Android.Content.PM;

namespace Sample;

// PushManager.Init() is called from MainApplication.OnCreate, not here: by the time an
// activity is created, App has already been constructed and has needed Instance.
[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
}

