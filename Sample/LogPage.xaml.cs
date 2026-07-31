namespace Sample;

public partial class LogPage : ContentPage
{
    public LogPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        EventLog.EnsureAttached();
    }

    void OnClear(object sender, EventArgs e) => EventLog.Entries.Clear();
}
