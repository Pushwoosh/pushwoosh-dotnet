namespace Sample;

public partial class SetupPage : ContentPage
{
    public SetupPage()
    {
        InitializeComponent();
    }

    void OnContinue(object sender, EventArgs e)
    {
        var code = CodeEntry.Text?.Trim();

        if (!AppCodeStore.LooksValid(code))
        {
            ErrorLabel.Text = "An application code looks like XXXXX-XXXXX.";
            ErrorLabel.IsVisible = true;
            return;
        }

        ErrorLabel.IsVisible = false;
        AppCodeStore.Apply(code!);

        Application.Current!.MainPage = new AppShell();
    }
}
