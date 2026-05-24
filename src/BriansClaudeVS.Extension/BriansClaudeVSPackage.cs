using System.Runtime.InteropServices;
using BriansClaudeVS.Commands;
using BriansClaudeVS.Options;
using BriansClaudeVS.ToolWindows;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace BriansClaudeVS;

[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[Guid(PackageIds.PackageGuidString)]
[ProvideMenuResource("Menus.ctmenu", 1)]
[ProvideToolWindow(typeof(ChatToolWindow), Style = VsDockStyle.Tabbed, DockedWidth = 400,
    Window = "DocumentWell", Orientation = ToolWindowOrientation.Right)]
[ProvideOptionPage(typeof(ClaudeOptionsPage),
    "Brian's Claude", "General",
    categoryResourceID: 0, pageResourceID: 0,
    supportsAutomation: true)]
[ProvideAutoLoad(Microsoft.VisualStudio.Shell.Interop.UIContextGuids80.SolutionExists,
    PackageAutoLoadFlags.BackgroundLoad)]
public sealed class BriansClaudeVSPackage : AsyncPackage
{
    internal static BriansClaudeVSPackage? Instance { get; private set; }

    protected override async Task InitializeAsync(
        CancellationToken cancellationToken,
        IProgress<ServiceProgressData> progress)
    {
        await base.InitializeAsync(cancellationToken, progress);
        Instance = this;

        await OpenChatWindowCommand.InitializeAsync(this);
    }
}
