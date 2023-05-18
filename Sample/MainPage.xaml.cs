using PushwooshSDK.DotNet.Core;

namespace Sample;


public partial class MainPage : ContentPage
{
    string tagName;
    string tagValue;

    public MainPage()
    {
        InitializeComponent();
    }

    private void OnRegisterClicked(object sender, EventArgs e)
    {
        PushManager.Instance.Register();

        SemanticScreenReader.Announce(RegisterBtn.Text);
    }

    private void OnUserIdInput(object sender, EventArgs e)
    {
        string userId = ((Entry)sender).Text;
        PushManager.Instance.InAppManager.SetUserId(userId);

    }

    private void OnPostEventClicked(object sender, EventArgs e)
    {
        PushManager.Instance.InAppManager.PostEventAsync("test", null);
    }

    private void OnTagNameInput(object sender, EventArgs e)
    {
        tagName = ((Entry)sender).Text;
    }
    private void OnTagValueInput(object sender, EventArgs e)
    {
        tagValue = ((Entry)sender).Text;
    }
    private void OnSetTagsClicked(object sender, EventArgs e)
    {
        TagsBundle tagsBundle = new TagsBundle();
        tagsBundle.PutString(tagName, tagValue);
        PushManager.Instance.SendTagsAsync(tagsBundle);
    }
}


