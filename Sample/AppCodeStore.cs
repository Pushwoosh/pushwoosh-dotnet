using PushwooshSDK.DotNet.Core;

namespace Sample;

/// <summary>
/// The sample ships without an application code so no real one ends up in the
/// public repository. The code is entered on first launch, kept in
/// preferences, and pushed into the SDK on every start.
/// </summary>
public static class AppCodeStore
{
    const string Key = "pushwoosh_app_code";

    public static string? Saved
    {
        get
        {
            var value = Preferences.Default.Get(Key, string.Empty);
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
    }

    public static bool HasCode => Saved != null || !string.IsNullOrWhiteSpace(PushManager.Instance?.AppCode);

    /// <summary>
    /// Applies the stored code to the SDK. Setting AppCode is what initialises
    /// the native SDK on iOS, so it has to happen before Register().
    /// </summary>
    public static void ApplySaved()
    {
        var code = Saved;
        if (code == null)
            return;

        Apply(code);
    }

    public static void Apply(string code)
    {
        var manager = PushManager.Instance;
        if (manager == null)
        {
            EventLog.Add(EntryKind.Error, "App code not applied", "The SDK is not initialised.");
            return;
        }

        manager.AppCode = code;
        Preferences.Default.Set(Key, code);
        EventLog.Add(EntryKind.Api, "App code applied", code);
    }

    public static void Clear()
    {
        Preferences.Default.Remove(Key);
        EventLog.Add(EntryKind.Info, "Stored app code cleared", "Restart the app to enter a new one.");
    }

    /// <summary>
    /// Pushwoosh application codes look like XXXXX-XXXXX.
    /// </summary>
    public static bool LooksValid(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        var parts = code.Trim().Split('-');
        return parts.Length == 2 && parts.All(p => p.Length >= 4);
    }
}
