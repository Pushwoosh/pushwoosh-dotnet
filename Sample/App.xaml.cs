namespace Sample;
using PushwooshSDK.DotNet.Core;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		MainPage = new AppShell();
    }

    protected override void OnStart()
    {
        base.OnStart();

        PushManager.Instance.PushAccepted += (object sender, PushNotificationEventArgs e) => {
            MainPage.DisplayAlert("Push Accepted", e.Notification.Payload, "OK");
        };
        PushManager.Instance.PushReceived += (object sender, PushNotificationEventArgs e) => {
            MainPage.DisplayAlert("Push Received", e.Notification.Payload, "OK");
        };
    }
}

