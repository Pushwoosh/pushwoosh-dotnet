using Pushwoosh.Android;

namespace PushwooshSDK.DotNet.Android.Sample;

[Activity(Label = "@string/app_name", MainLauncher = true)]
public class MainActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        PushManager.Init();
        PushManager manager = PushManager.Instance;
            manager.Register();
            manager.InAppManager.SetUserId("test");
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
            keyValuePairs.Add("test", "test2");
            manager.InAppManager.PostEventAsync("test", keyValuePairs);
        // Set our view from the "main" layout resource
        SetContentView(Resource.Layout.activity_main);
    }
}
