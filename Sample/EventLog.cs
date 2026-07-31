using System.Collections.ObjectModel;
using PushwooshSDK.DotNet.Core;

namespace Sample;

public enum EntryKind
{
    Push,
    Api,
    Error,
    Info
}

public class LogEntry
{
    public DateTime At { get; init; } = DateTime.Now;
    public EntryKind Kind { get; init; }
    public string Title { get; init; } = "";
    public string? Detail { get; init; }

    public string Time => At.ToString("HH:mm:ss");
    public bool HasDetail => !string.IsNullOrWhiteSpace(Detail);

    // Coral is reserved for inbound pushes — the event the SDK exists for.
    // Colours come from the theme so they are defined in exactly one place.
    public Color Accent => Kind switch
    {
        EntryKind.Push => Token("PwCoral"),
        EntryKind.Error => Token("PwBad"),
        EntryKind.Api => Token("PwGood"),
        _ => Token("PwInkSoft")
    };

    static Color Token(string key) =>
        Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Color color
            ? color
            : Colors.Gray;

    public string Badge => Kind switch
    {
        EntryKind.Push => "PUSH",
        EntryKind.Error => "FAIL",
        EntryKind.Api => "API",
        _ => "INFO"
    };
}

/// <summary>
/// Single place every SDK callback and API result is written to, so the demo
/// always shows what actually happened instead of failing silently.
/// </summary>
public static class EventLog
{
    public static ObservableCollection<LogEntry> Entries { get; } = new();

    static bool attached;
    static readonly object gate = new();

    public static void Add(EntryKind kind, string title, string? detail = null)
    {
        var entry = new LogEntry { Kind = kind, Title = title, Detail = detail };

        // SDK callbacks arrive off the UI thread.
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Entries.Insert(0, entry);
            while (Entries.Count > 200)
                Entries.RemoveAt(Entries.Count - 1);
        });
    }

    /// <summary>
    /// Subscribes to the SDK events once. Safe to call from every page.
    /// </summary>
    public static void EnsureAttached()
    {
        lock (gate)
        {
            if (attached)
                return;

            var manager = PushManager.Instance;
            if (manager == null)
            {
                Add(EntryKind.Error, "SDK not initialised",
                    "PushManager.Instance is null — Init() was not called on this platform.");
                return;
            }

            manager.PushReceived += (_, e) =>
                Add(EntryKind.Push, Describe(e.Notification, "Push received", e.FromBackground),
                    e.Notification?.Payload);

            manager.PushAccepted += (_, e) =>
                Add(EntryKind.Push, Describe(e.Notification, "Push opened", e.FromBackground),
                    e.Notification?.Payload);

            manager.Registered += (_, _) =>
                Add(EntryKind.Api, "Registered for push notifications", manager.PushToken);

            manager.RegisterError += (_, error) =>
                Add(EntryKind.Error, "Registration failed", error?.Description);

            attached = true;
            Add(EntryKind.Info, "Listening for SDK events");
        }
    }

    static string Describe(PushNotification? notification, string prefix, bool fromBackground)
    {
        var where = fromBackground ? "background" : "foreground";
        var message = notification?.Message ?? notification?.Title;
        return string.IsNullOrWhiteSpace(message)
            ? $"{prefix} ({where})"
            : $"{prefix} ({where}): {message}";
    }
}
