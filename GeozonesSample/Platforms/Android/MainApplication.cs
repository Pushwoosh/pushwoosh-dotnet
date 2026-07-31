using Android.App;
using Android.Runtime;
using Pushwoosh.Android;
using PushwooshSDK.DotNet.Geozones.Android;

namespace GeozonesSample;

[Application]
public class MainApplication : MauiApplication
{
	public MainApplication(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
	}

	public override void OnCreate()
	{
		// Both before base.OnCreate, which builds the MAUI app: a page that reaches for
		// Instance during construction must not find a null. See Sample/ for the detail.
		PushManager.Init();
		LocationManager.Init();
		base.OnCreate();
	}

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}

