using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace BriansClaudeVS.ToolWindows;

[Guid("0E9F5166-434A-4393-84C7-0DB8AE5541F4")]
public class ChatToolWindow : ToolWindowPane
{
    public ChatToolWindow() : base(null)
    {
        Caption = "Brian's Claude";
        Content = new ChatToolWindowControl();
    }
}
