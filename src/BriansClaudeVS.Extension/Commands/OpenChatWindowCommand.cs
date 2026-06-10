using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace BriansClaudeVS.Commands;

internal sealed class OpenChatWindowCommand
{
    private readonly AsyncPackage _package;

    private OpenChatWindowCommand(AsyncPackage package, OleMenuCommandService commandService)
    {
        _package = package;
        var cmdId = new CommandID(PackageIds.CommandSetGuid, PackageIds.OpenChatWindowCommandId);
        commandService.AddCommand(new MenuCommand(Execute, cmdId));
    }

    public static async Task InitializeAsync(AsyncPackage package)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
        var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService
            ?? throw new InvalidOperationException("Could not obtain IMenuCommandService.");
        new OpenChatWindowCommand(package, commandService);
    }

    private void Execute(object? sender, EventArgs e)
    {
        _ = _package.JoinableTaskFactory.RunAsync(async () =>
        {
            await _package.ShowToolWindowAsync(
                typeof(ToolWindows.ChatToolWindow),
                id: 0,
                create: true,
                cancellationToken: _package.DisposalToken);
        });
    }
}
