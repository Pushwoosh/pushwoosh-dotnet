using System.Collections.ObjectModel;
using PushwooshSDK.DotNet.Core;

namespace Sample;

public partial class InboxPage : ContentPage
{
    readonly ObservableCollection<InboxMessage> messages = new();

    public InboxPage()
    {
        InitializeComponent();
        MessagesView.ItemsSource = messages;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        EventLog.EnsureAttached();
    }

    static IInboxManager? Inbox => PushManager.Instance?.InboxManager;

    static bool Missing()
    {
        if (Inbox != null)
            return false;

        EventLog.Add(EntryKind.Error, "Inbox unavailable",
            "PushManager.Instance.InboxManager is null — the SDK is not initialised.");
        return true;
    }

    void OnLoad(object sender, EventArgs e) => Load();

    void Load()
    {
        if (Missing())
            return;

        Inbox!.LoadMessages((loaded, error) =>
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                EventLog.Add(EntryKind.Error, "Loading inbox failed", error);
                return;
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                messages.Clear();
                foreach (var message in loaded ?? new List<InboxMessage>())
                    messages.Add(message);
            });

            var count = loaded?.Count ?? 0;
            var detail = count == 0
                ? "inbox is empty"
                : string.Join("\n", loaded!.Take(10)
                    .Select(m => $"{(m.IsRead ? "read  " : "unread")}  {m.Title}"));

            EventLog.Add(EntryKind.Api, $"Loaded {count} inbox message(s)", detail);
        });

        Counts();
    }

    void OnCounts(object sender, EventArgs e) => Counts();

    void Counts()
    {
        if (Missing())
            return;

        Inbox!.MessagesCount((count, error) => Show(TotalLabel, count, error, "Total messages"));
        Inbox!.UnreadMessagesCount((count, error) => Show(UnreadLabel, count, error, "Unread messages"));
        Inbox!.MessagesWithNoActionPerformedCount((count, error) =>
            Show(NoActionLabel, count, error, "Messages with no action performed"));
    }

    static void Show(Label label, int count, string error, string title)
    {
        if (!string.IsNullOrWhiteSpace(error))
        {
            EventLog.Add(EntryKind.Error, $"{title} failed", error);
            return;
        }

        MainThread.BeginInvokeOnMainThread(() => label.Text = count.ToString());
        EventLog.Add(EntryKind.Api, $"{title}: {count}");
    }

    static InboxMessage? From(object sender) => (sender as Button)?.BindingContext as InboxMessage;

    void OnReadOne(object sender, EventArgs e)
    {
        var message = From(sender);
        if (Missing() || message?.Code == null)
            return;

        Inbox!.ReadMessage(message.Code);
        EventLog.Add(EntryKind.Api, "Message marked as read", message.Code);
        Load();
    }

    void OnPerformAction(object sender, EventArgs e)
    {
        var message = From(sender);
        if (Missing() || message?.Code == null)
            return;

        Inbox!.PerformAction(message.Code);
        EventLog.Add(EntryKind.Api, "Message action performed", message.Code);
    }

    void OnDeleteOne(object sender, EventArgs e)
    {
        var message = From(sender);
        if (Missing() || message?.Code == null)
            return;

        Inbox!.DeleteMessage(message.Code);
        EventLog.Add(EntryKind.Api, "Message deleted", message.Code);
        Load();
    }

    void OnReadAll(object sender, EventArgs e)
    {
        if (Missing())
            return;

        var codes = Codes();
        if (codes.Count == 0)
        {
            EventLog.Add(EntryKind.Error, "Nothing to read", "Load the inbox first.");
            return;
        }

        Inbox!.ReadMessages(codes);
        EventLog.Add(EntryKind.Api, $"Marked {codes.Count} message(s) as read");
        Load();
    }

    void OnDeleteAll(object sender, EventArgs e)
    {
        if (Missing())
            return;

        var codes = Codes();
        if (codes.Count == 0)
        {
            EventLog.Add(EntryKind.Error, "Nothing to delete", "Load the inbox first.");
            return;
        }

        Inbox!.DeleteMessages(codes);
        EventLog.Add(EntryKind.Api, $"Deleted {codes.Count} message(s)");
        Load();
    }

    List<string> Codes() =>
        messages.Where(m => !string.IsNullOrWhiteSpace(m.Code)).Select(m => m.Code!).ToList();
}
