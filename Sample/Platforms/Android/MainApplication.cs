using Android.App;
using Android.Runtime;
using Pushwoosh.Android;

namespace Sample;

[Application]
public class MainApplication : MauiApplication
{
	public MainApplication(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
	}

	public override void OnCreate()
	{
		// base.OnCreate builds the MAUI app and constructs App, and that constructor
		// already needs PushManager.Instance — it subscribes to the SDK events and
		// applies the stored application code. MainActivity.OnCreate is too late:
		// Application.OnCreate runs first, so Instance would still be null there.
		// This mirrors the iOS side, where Init() precedes base.FinishedLaunching.
		PushManager.Init();
		base.OnCreate();
	}

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}

