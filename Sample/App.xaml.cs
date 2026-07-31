namespace Sample;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		// Events go to the Log tab instead of interrupting with alerts.
		EventLog.EnsureAttached();

		// No application code is baked into the sample, so ask for one once
		// and keep it in preferences afterwards.
		if (AppCodeStore.HasCode)
		{
			AppCodeStore.ApplySaved();
			MainPage = new AppShell();
		}
		else
		{
			MainPage = new SetupPage();
		}
	}
}
