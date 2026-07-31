using System.Globalization;
using PushwooshSDK.DotNet.Core;

namespace Sample;

public partial class ActionsPage : ContentPage
{
    public ActionsPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        EventLog.EnsureAttached();
    }

    static PushManager? Sdk => PushManager.Instance;

    static bool Missing()
    {
        if (Sdk != null)
            return false;

        EventLog.Add(EntryKind.Error, "SDK not initialised", "PushManager.Init() was not called.");
        return true;
    }

    // ---------------------------------------------------------------- tags ---

    bool ReadTagInput(out string name, out string value)
    {
        name = TagNameEntry.Text?.Trim() ?? "";
        value = TagValueEntry.Text?.Trim() ?? "";

        if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(value))
            return true;

        EventLog.Add(EntryKind.Error, "Tag not sent", "Both a name and a value are required.");
        return false;
    }

    async void OnSendString(object sender, EventArgs e)
    {
        if (Missing() || !ReadTagInput(out var name, out var value))
            return;

        var tags = new TagsBundle();
        tags.PutString(name, value);
        await Send(tags, $"{name} = \"{value}\"");
    }

    async void OnSendInteger(object sender, EventArgs e)
    {
        if (Missing() || !ReadTagInput(out var name, out var value))
            return;

        if (!int.TryParse(value, out var number))
        {
            EventLog.Add(EntryKind.Error, "Tag not sent", $"\"{value}\" is not a whole number.");
            return;
        }

        var tags = new TagsBundle();
        tags.PutInteger(name, number);
        await Send(tags, $"{name} = {number}");
    }

    async void OnSendIncrement(object sender, EventArgs e)
    {
        if (Missing() || !ReadTagInput(out var name, out var value))
            return;

        if (!int.TryParse(value, out var delta))
        {
            EventLog.Add(EntryKind.Error, "Tag not sent", $"\"{value}\" is not a whole number.");
            return;
        }

        var tags = new TagsBundle();
        tags.PutIncrementInteger(name, delta);
        await Send(tags, $"{name} += {delta}");
    }

    async void OnSendList(object sender, EventArgs e)
    {
        if (Missing() || !ReadTagInput(out var name, out var value))
            return;

        var items = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (items.Length == 0)
        {
            EventLog.Add(EntryKind.Error, "Tag not sent", "Provide at least one list item.");
            return;
        }

        var tags = new TagsBundle();
        tags.PutList(name, items);
        await Send(tags, $"{name} = [{string.Join(", ", items)}]");
    }

    // Shows PutAll: several tags of mixed types in a single request.
    async void OnSendBatch(object sender, EventArgs e)
    {
        if (Missing())
            return;

        var batch = new Dictionary<string, object>
        {
            ["Name"] = "Sample user",
            ["Age"] = 30,
            ["Favourites"] = new List<string> { "books", "music" }
        };

        var tags = new TagsBundle();
        tags.PutAll(batch);
        await Send(tags, string.Join("\n", batch.Select(kv => $"{kv.Key} = {Render(kv.Value)}")));
    }

    static string Render(object value) =>
        value is IEnumerable<string> list ? $"[{string.Join(", ", list)}]" : value.ToString() ?? "";

    async Task Send(TagsBundle tags, string detail)
    {
        var result = await Sdk!.SendTagsAsync(tags);
        Report("Tags sent", result?.Error?.Description, detail);
    }

    async void OnLoadTags(object sender, EventArgs e)
    {
        if (Missing())
            return;

        var result = await Sdk!.LoadTagsAsync();

        if (result?.Error != null)
        {
            EventLog.Add(EntryKind.Error, "Loading tags failed", result.Error.Description);
            return;
        }

        var tags = result?.Result?.ToDictionary;
        var detail = tags == null || tags.Count == 0
            ? "no tags set"
            : string.Join("\n", tags.Select(kv => $"{kv.Key} = {kv.Value}"));

        EventLog.Add(EntryKind.Api, $"Loaded {tags?.Count ?? 0} tags", detail);
    }

    // ---------------------------------------------------------------- user ---

    void OnSetUserId(object sender, EventArgs e)
    {
        if (Missing())
            return;

        var userId = UserIdEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(userId))
        {
            EventLog.Add(EntryKind.Error, "User id not set", "Enter a user id first.");
            return;
        }

        Sdk!.InAppManager?.SetUserId(userId);
        EventLog.Add(EntryKind.Api, "User id set", userId);
    }

    async void OnMergeUsers(object sender, EventArgs e)
    {
        if (Missing())
            return;

        var oldId = OldUserEntry.Text?.Trim();
        var newId = NewUserEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(oldId) || string.IsNullOrWhiteSpace(newId))
        {
            EventLog.Add(EntryKind.Error, "Users not merged", "Both user ids are required.");
            return;
        }

        var inApp = Sdk!.InAppManager;
        if (inApp == null)
        {
            EventLog.Add(EntryKind.Error, "Users not merged", "In-app manager is unavailable.");
            return;
        }

        var result = await inApp.MergeUserId(oldId, newId, MergeSwitch.IsToggled);
        Report("Users merged", result?.Error?.Description,
            $"{oldId} -> {newId}, keep data: {MergeSwitch.IsToggled}");
    }

    // -------------------------------------------------------------- events ---

    async void OnPostEvent(object sender, EventArgs e)
    {
        if (Missing())
            return;

        var name = EventEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            EventLog.Add(EntryKind.Error, "Event not posted", "Enter an event name first.");
            return;
        }

        var inApp = Sdk!.InAppManager;
        if (inApp == null)
        {
            EventLog.Add(EntryKind.Error, "Event not posted", "In-app manager is unavailable.");
            return;
        }

        var attributes = ParseAttributes(EventAttributesEntry.Text);
        var result = await inApp.PostEventAsync(name, attributes);

        var detail = attributes == null || attributes.Count == 0
            ? null
            : string.Join("\n", attributes.Select(kv => $"{kv.Key} = {kv.Value}"));

        Report($"Posted event {name}", result?.Error?.Description, detail);
    }

    static Dictionary<string, string>? ParseAttributes(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var pairs = raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var result = new Dictionary<string, string>();

        foreach (var pair in pairs)
        {
            var parts = pair.Split('=', 2);
            if (parts.Length == 2 && !string.IsNullOrWhiteSpace(parts[0]))
                result[parts[0].Trim()] = parts[1].Trim();
        }

        return result.Count == 0 ? null : result;
    }

    // -------------------------------------------------- local notifications ---

    void OnScheduleLocal(object sender, EventArgs e)
    {
        if (Missing())
            return;

        var text = LocalTextEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            EventLog.Add(EntryKind.Error, "Nothing scheduled", "Enter a message first.");
            return;
        }

        if (!int.TryParse(DelayEntry.Text?.Trim(), out var delay) || delay < 0)
            delay = 10;

        var id = Sdk!.ScheduleLocalNotification(text, delay);
        LocalIdEntry.Text = id.ToString();
        EventLog.Add(EntryKind.Api, "Local notification scheduled", $"id {id}, fires in {delay}s");
    }

    void OnCancelLocalById(object sender, EventArgs e)
    {
        if (Missing())
            return;

        if (!int.TryParse(LocalIdEntry.Text?.Trim(), out var id))
        {
            EventLog.Add(EntryKind.Error, "Nothing cancelled", "Enter the notification id to cancel.");
            return;
        }

        Sdk!.CancelLocalNotification(id);
        EventLog.Add(EntryKind.Api, "Local notification cancelled", $"id {id}");
    }

    void OnCancelLocal(object sender, EventArgs e)
    {
        if (Missing())
            return;

        Sdk!.CancelAllLocalNotifications();
        EventLog.Add(EntryKind.Api, "Cancelled all local notifications");
    }

    static void Report(string action, string? error, string? detail = null)
    {
        if (string.IsNullOrWhiteSpace(error))
            EventLog.Add(EntryKind.Api, action, detail);
        else
            EventLog.Add(EntryKind.Error, $"{action} failed", error);
    }
}
