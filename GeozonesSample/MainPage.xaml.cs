﻿namespace GeozonesSample;

using PushwooshSDK.DotNet.Geozones;
using PushwooshSDK.DotNet.Core;
public partial class MainPage : ContentPage
{
	int count = 0;

	public MainPage()
	{
		InitializeComponent();
	}

	private void OnCounterClicked(object sender, EventArgs e)
	{
		count++;

		if (count == 1)
			CounterBtn.Text = $"Clicked {count} time";
		else
			CounterBtn.Text = $"Clicked {count} times";

		SemanticScreenReader.Announce(CounterBtn.Text);
		PushManager.Instance?.Register();
		LocationManager.Instance?.StartLocationTracking();
	}
}


