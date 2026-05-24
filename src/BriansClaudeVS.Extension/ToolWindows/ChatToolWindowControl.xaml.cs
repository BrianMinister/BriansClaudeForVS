using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BriansClaudeVS.Core.SlashCommands;
using BriansClaudeVS.ToolWindows.ViewModels;

namespace BriansClaudeVS.ToolWindows;

public partial class ChatToolWindowControl : UserControl
{
    private readonly ChatViewModel _vm;

    public ChatToolWindowControl()
    {
        InitializeComponent();
        _vm = new ChatViewModel();
        DataContext = _vm;

        _vm.Messages.CollectionChanged += (_, _) =>
        {
            // Auto-scroll to bottom when new messages arrive
            MessageScroller.ScrollToEnd();
        };
    }

    private void InputBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
        {
            e.Handled = true;
            if (_vm.SendCommand.CanExecute(null))
                _vm.SendCommand.Execute(null);
        }
    }

    private void SlashCommand_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: SlashCommandDefinition cmd })
        {
            _vm.InputText = cmd.Name + " ";
            InputBox.Focus();
            InputBox.CaretIndex = InputBox.Text.Length;
        }
    }

    // Called by code actions to pre-populate the chat with a slash command + code
    public void PreFill(string text)
    {
        _vm.PreFill(text);
        InputBox.Focus();
    }
}
