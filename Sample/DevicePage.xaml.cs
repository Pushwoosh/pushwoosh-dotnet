using PushwooshSDK.DotNet.Core;

namespace Sample;

public partial class DevicePage : ContentPage
{
    bool loading;

    public DevicePage()
    {
        InitializeComponent();

#if ANDROID
        MultiNotificationRow.IsVisible = true;
#endif
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        EventLog.EnsureAttached();

        // Registration finishes asynchronously, so the page has to be told
        // rather than only reading the state when the tab is opened.
        var manager = PushManager.Instance;
        if (manager != null && !subscribed)
        {
            manager.Registered += (_, _) => Reload();
            manager.RegisterError += (_, _) => Reload();
            subscribed = true;
        }

        Refresh();
    }

    bool subscribed;

    void Reload() => MainThread.BeginInvokeOnMainThread(Refresh);

    void Refresh()
    {
        var manager = PushManager.Instance;

        if (manager == null)
        {
            StateLabel.Text = "SDK not initialised";
            StateHint.Text = "PushManager.Init() was not called on this platform.";
            return;
        }

        loading = true;

        HwidLabel.Text = Blank(manager.HardwareId) ? "—" : manager.HardwareId;
        AppCodeLabel.Text = Blank(manager.AppCode) ? "—" : manager.AppCode;
        AppCodeEntry.Text = manager.AppCode;
        LanguageEntry.Text = manager.Language;
        AlertsSwitch.IsToggled = manager.ShowPushNotificationAlerts;

        var token = manager.PushToken;
        var registered = !Blank(token);

        TokenLabel.Text = registered ? token : "Not issued yet";
        CopyTokenButton.IsEnabled = registered;

        StateLabel.Text = registered ? "Registered" : "Not registered";
        StateHint.Text = registered
            ? "This device can receive push notifications."
            : "Register to receive push notifications on this device.";

        DescribeNotificationSettings(manager);

        loading = false;
    }

    /// <summary>
    /// NotificationSettings reports what the native SDK knows about remote
    /// notifications, which is not the same as the iOS permission switch: a
    /// simulator never gets an APNS token, so it reads as unavailable there.
    /// </summary>
    void DescribeNotificationSettings(PushManager manager)
    {
        var settings = manager.NotificationSettings;

        if (settings == null)
        {
            PermissionLabel.Text = "unknown";
            SettingsDetail.Text = "The SDK did not report notification settings.";
            return;
        }

        PermissionLabel.Text = settings.Enabled ? "available" : "unavailable";

#if IOS
        if (settings is Pushwoosh.iOS.PushNotificationSettings ios)
        {
            SettingsDetail.Text =
                $"alert {On(ios.Alert)} · badge {On(ios.Badge)} · sound {On(ios.Sound)}"
                + (settings.Enabled ? "" : "\nNo APNS token yet — tap Register, and allow notifications when iOS asks.");
            return;
        }
#endif

        SettingsDetail.Text = settings.Enabled
            ? "The device can be reached by remote notifications."
            : "The device is not registered for remote notifications yet.";
    }

    static string On(bool value) => value ? "on" : "off";

    static bool Blank(string? value) => string.IsNullOrWhiteSpace(value);

    async void OnCopyHwid(object sender, EventArgs e)
    {
        var value = PushManager.Instance?.HardwareId;
        if (Blank(value))
            return;

        await Clipboard.SetTextAsync(value);
        EventLog.Add(EntryKind.Info, "Hardware id copied", value);
    }

    async void OnCopyToken(object sender, EventArgs e)
    {
        var value = PushManager.Instance?.PushToken;
        if (Blank(value))
            return;

        await Clipboard.SetTextAsync(value);
        EventLog.Add(EntryKind.Info, "Push token copied", value);
    }

    void OnRegister(object sender, EventArgs e)
    {
        EventLog.Add(EntryKind.Info, "Requesting registration");
        PushManager.Instance?.Register();
    }

    void OnUnregister(object sender, EventArgs e)
    {
        PushManager.Instance?.Unregister();
        EventLog.Add(EntryKind.Info, "Unregistered");
        Refresh();
    }

    void OnApplyAppCode(object sender, EventArgs e)
    {
        var manager = PushManager.Instance;
        var code = AppCodeEntry.Text?.Trim();

        if (manager == null || Blank(code))
        {
            EventLog.Add(EntryKind.Error, "App code not applied", "Enter an application code first.");
            return;
        }

        if (!AppCodeStore.LooksValid(code))
        {
            EventLog.Add(EntryKind.Error, "App code not applied", "An application code looks like XXXXX-XXXXX.");
            return;
        }

        AppCodeStore.Apply(code!);
        Refresh();
    }

    void OnForgetAppCode(object sender, EventArgs e)
    {
        AppCodeStore.Clear();
        Application.Current!.MainPage = new SetupPage();
    }

    void OnApplyLanguage(object sender, EventArgs e)
    {
        var manager = PushManager.Instance;
        var language = LanguageEntry.Text?.Trim();

        if (manager == null || Blank(language))
        {
            EventLog.Add(EntryKind.Error, "Language not applied", "Enter a two-letter language code.");
            return;
        }

        manager.Language = language;
        EventLog.Add(EntryKind.Api, "Language set", language);
    }

    void OnAlertsToggled(object sender, ToggledEventArgs e)
    {
        if (loading)
            return;

        var manager = PushManager.Instance;
        if (manager == null)
            return;

        manager.ShowPushNotificationAlerts = e.Value;
        EventLog.Add(EntryKind.Api, $"Foreground alerts {(e.Value ? "on" : "off")}");
    }

    void OnMultiNotificationToggled(object sender, ToggledEventArgs e)
    {
        if (loading)
            return;

#if ANDROID
        var android = Pushwoosh.Android.PushManager.Instance;
        if (android == null)
            return;

        android.MultiNotificationsMode = e.Value;
        EventLog.Add(EntryKind.Api, $"Multi-notification mode {(e.Value ? "on" : "off")}");
#endif
    }
}
