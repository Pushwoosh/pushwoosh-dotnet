namespace PushwooshSDK.DotNet.iOS.Sample;

using Com.Pushwoosh.iOS;

using UserNotifications;

[Register ("AppDelegate")]
public class AppDelegate : UIApplicationDelegate {
	public override UIWindow? Window {
		get;
		set;
	}

    public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
	{
		// create a new window instance based on the screen size
		Window = new UIWindow (UIScreen.MainScreen.Bounds);

		PushNotificationManager pushmanager = PushNotificationManager.PushManager;
		pushmanager.Delegate = new PushDelegate();
		UNUserNotificationCenter.Current.Delegate = pushmanager.NotificationCenterDelegate;

		PWInAppManager inAppManager = PWInAppManager.SharedManager;

		pushmanager.RegisterForPushNotifications();
		inAppManager.SetUserId((NSString)"test");

		// create a UIViewController with a single UILabel
		var vc = new UIViewController ();
		vc.View!.AddSubview (new UILabel (Window!.Frame) {
			BackgroundColor = UIColor.SystemBackground,
			TextAlignment = UITextAlignment.Center,
			Text = "Hello, iOS!",
			AutoresizingMask = UIViewAutoresizing.All,
		});
		Window.RootViewController = vc;

		// make the window visible
		Window.MakeKeyAndVisible ();

		return true;
	}

	public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
	{
		PushNotificationManager.PushManager.HandlePushRegistration(deviceToken);
	}


	public override void ReceivedRemoteNotification(UIApplication application, NSDictionary userInfo)
	{
		PushNotificationManager.PushManager.HandlePushReceived(userInfo);
	}
}

public class PushDelegate : PushNotificationDelegate
{
	public override void OnPushAccepted(PushNotificationManager pushManager, NSDictionary pushNotification)
	{
		Console.WriteLine("Push accepted: " + pushNotification);
	}

	public override void OnPushReceived(PushNotificationManager pushManager, NSDictionary pushNotification, bool onStart)
	{
		Console.WriteLine("Push received: " + pushNotification);
	}

	public override void OnDidRegisterForRemoteNotificationsWithDeviceToken(NSString token)
	{
		Console.WriteLine("Registered for push notifications: " + token);
	}

	public override void OnDidFailToRegisterForRemoteNotificationsWithError(NSError error)
	{
		Console.WriteLine("Error: " + error);
	}
}

