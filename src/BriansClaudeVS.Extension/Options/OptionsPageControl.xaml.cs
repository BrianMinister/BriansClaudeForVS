using System.Windows;
using System.Windows.Controls;
using BriansClaudeVS.Core.Api;
using Microsoft.VisualStudio.Shell;

namespace BriansClaudeVS.Options;

public partial class OptionsPageControl : UserControl
{
    private readonly ClaudeOptionsPage _page;
    private bool _apiKeyChanged;

    public OptionsPageControl(ClaudeOptionsPage page)
    {
        _page = page;
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Show masked placeholder if a key is already saved
        if (ServiceLocator.CredentialStore?.LoadApiKey() is not null)
            ApiKeyBox.Password = new string('•', 32);

        SelectComboByTag(ChatModelCombo, _page.ChatModelId);
        SelectComboByTag(InlineModelCombo, _page.InlineModelId);
        EnableInlineCheck.IsChecked = _page.EnableInlineCompletions;
        MaxTokensSlider.Value = _page.MaxContextTokens;
        MaxTokensSlider.ValueChanged += (_, a) =>
            MaxTokensLabel.Text = $"{(int)a.NewValue:N0} tokens";
        MaxTokensLabel.Text = $"{_page.MaxContextTokens:N0} tokens";
    }

    private void ApiKeyBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        _apiKeyChanged = true;
        VerifyStatus.Text = string.Empty;
    }

    private void VerifyButton_Click(object sender, RoutedEventArgs e)
    {
        _ = VerifyKeyAsync();
    }

    private async Task VerifyKeyAsync()
    {
        VerifyStatus.Text = "Verifying…";
        VerifyStatus.Foreground = System.Windows.Media.Brushes.Gray;

        var key = ApiKeyBox.Password;
        if (string.IsNullOrWhiteSpace(key))
        {
            VerifyStatus.Text = "Enter an API key first.";
            return;
        }

        try
        {
            // Temporarily save the key for the test call
            ServiceLocator.CredentialStore?.SaveApiKey(key);
            var svc = ServiceLocator.ApiService;
            if (svc == null) throw new InvalidOperationException("API service not available.");

            await svc.CompleteAsync(
                "You are a test assistant.",
                [BriansClaudeVS.Core.Api.Models.ChatMessage.User("Reply with the single word: OK")]);

            VerifyStatus.Text = "✓ Key verified successfully.";
            VerifyStatus.Foreground = System.Windows.Media.Brushes.Green;
            _apiKeyChanged = false;
        }
        catch (Exception ex)
        {
            VerifyStatus.Text = $"✗ {ex.Message}";
            VerifyStatus.Foreground = System.Windows.Media.Brushes.Red;
        }
    }

    public void Apply()
    {
        if (_apiKeyChanged && !string.IsNullOrWhiteSpace(ApiKeyBox.Password))
            ServiceLocator.CredentialStore?.SaveApiKey(ApiKeyBox.Password);

        _page.ChatModelId = (ChatModelCombo.SelectedItem as ComboBoxItem)?.Tag as string
            ?? "claude-opus-4-5";
        _page.InlineModelId = (InlineModelCombo.SelectedItem as ComboBoxItem)?.Tag as string
            ?? "claude-haiku-4-5-20251001";
        _page.EnableInlineCompletions = EnableInlineCheck.IsChecked == true;
        _page.MaxContextTokens = (int)MaxTokensSlider.Value;
    }

    private static void SelectComboByTag(ComboBox combo, string tag)
    {
        foreach (ComboBoxItem item in combo.Items)
        {
            if (item.Tag as string == tag)
            {
                combo.SelectedItem = item;
                return;
            }
        }
    }
}
