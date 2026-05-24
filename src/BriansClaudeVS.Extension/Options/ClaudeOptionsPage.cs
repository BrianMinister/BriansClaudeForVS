using System.Runtime.InteropServices;
using System.Windows;
using BriansClaudeVS.Core.Api;
using Microsoft.VisualStudio.Shell;

namespace BriansClaudeVS.Options;

[ComVisible(true)]
[Guid("80E18F5B-575D-49E2-880F-40314FC7656A")]
public class ClaudeOptionsPage : DialogPage
{
    private OptionsPageControl? _control;

    // Persisted (non-sensitive) settings — API key is handled separately via DPAPI
    public string ChatModelId { get; set; } = "claude-opus-4-5";
    public string InlineModelId { get; set; } = "claude-haiku-4-5-20251001";
    public bool EnableInlineCompletions { get; set; } = true;
    public int MaxContextTokens { get; set; } = 8000;

    protected override UIElement Child => _control ??= new OptionsPageControl(this);

    protected override void OnApply(PageApplyEventArgs e)
    {
        _control?.Apply();
        base.OnApply(e);

        // Push updated model IDs to the API service singleton if already initialized
        if (ServiceLocator.ApiService is { } svc)
        {
            svc.ChatModelId = ChatModelId;
            svc.InlineModelId = InlineModelId;
        }
    }
}
