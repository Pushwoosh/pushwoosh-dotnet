using Foundation;
using UIKit;
using Pushwoosh.iOS;

namespace Sample;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        PushManager.Init();
        return base.FinishedLaunching(application, launchOptions);
    }

    [Export("application:didReceiveRemoteNotification:fetchCompletionHandler:")]
    public void DidReceiveRemoteNotification(UIKit.UIApplication application, NSDictionary userInfo, Action<UIKit.UIBackgroundFetchResult> completionHandler)
    {
        PushManager.Instance.ReceivedRemoteNotification(userInfo);
        Console.WriteLine("Received PUSH SOOQA NAH");
    }

    [Export("application:didRegisterForRemoteNotificationsWithDeviceToken:")]
    public void RegisteredForRemoteNotifications(UIKit.UIApplication application, NSData deviceToken)
    {
        PushManager.Instance.RegisteredForRemoteNotifications(deviceToken);
    }

    [Export("application:didFailToRegisterForRemoteNotificationsWithError:")]
    public void FailedToRegisterForRemoteNotifications(UIKit.UIApplication application, NSError error)
    {
        PushManager.Instance.FailedToRegisterForRemoteNotifications(error);
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}

