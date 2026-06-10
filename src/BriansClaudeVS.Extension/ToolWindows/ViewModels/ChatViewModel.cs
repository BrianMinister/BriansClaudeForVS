using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using BriansClaudeVS.Core.Api.Models;
using BriansClaudeVS.Core.SlashCommands;

namespace BriansClaudeVS.ToolWindows.ViewModels;

public enum MessageSide { User, Assistant }

public class ChatMessageViewModel : INotifyPropertyChanged
{
    private string _text = string.Empty;

    public MessageSide Side { get; init; }

    public string Text
    {
        get => _text;
        set { _text = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class ChatViewModel : INotifyPropertyChanged
{
    private string _inputText = string.Empty;
    private bool _isBusy;
    private CancellationTokenSource? _cts;
    private readonly List<ChatMessage> _history = [];

    public ObservableCollection<ChatMessageViewModel> Messages { get; } = [];

    public string InputText
    {
        get => _inputText;
        set { _inputText = value; OnPropertyChanged(); OnPropertyChanged(nameof(SlashMatches)); }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set { _isBusy = value; OnPropertyChanged(); }
    }

    public IReadOnlyList<SlashCommandDefinition> SlashMatches =>
        InputText.StartsWith('/') && !InputText.Contains(' ')
            ? ServiceLocator.SlashCommandParser.GetMatches(InputText)
            : [];

    public ICommand SendCommand { get; }
    public ICommand ClearCommand { get; }

    public ChatViewModel()
    {
        SendCommand = new RelayCommand(
            execute: () => _ = SendAsync(),
            canExecute: () => !IsBusy && !string.IsNullOrWhiteSpace(InputText));

        ClearCommand = new RelayCommand(
            execute: () => { Messages.Clear(); _history.Clear(); });
    }

    public void PreFill(string text)
    {
        InputText = text;
    }

    private async Task SendAsync()
    {
        var input = InputText.Trim();
        if (string.IsNullOrEmpty(input)) return;

        InputText = string.Empty;
        IsBusy = true;
        if (_cts is { } previousCts)
            await previousCts.CancelAsync();
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        var parsed = ServiceLocator.SlashCommandParser.Parse(input);

        string systemPrompt;
        string userContent;

        if (parsed.IsSlashCommand && parsed.Command != null)
        {
            systemPrompt = parsed.Command.SystemPrompt;
            userContent = string.IsNullOrEmpty(parsed.Argument)
                ? input
                : parsed.Argument;
        }
        else
        {
            systemPrompt = "You are a helpful coding assistant integrated into Visual Studio. " +
                           "Be concise and practical. Format code in markdown code blocks.";
            userContent = input;
        }

        var userMsg = new ChatMessageViewModel { Side = MessageSide.User, Text = input };
        Messages.Add(userMsg);

        _history.Add(ChatMessage.User(userContent));

        var assistantMsg = new ChatMessageViewModel { Side = MessageSide.Assistant, Text = string.Empty };
        Messages.Add(assistantMsg);

        try
        {
            var svc = ServiceLocator.ApiService;
            if (svc == null)
            {
                assistantMsg.Text = "Error: Claude service is not initialized. Check your API key in Tools > Options > Brian's Claude > General.";
                return;
            }

            var contextMessages = new BriansClaudeVS.Core.Api.ContextBuilder()
                .BuildChatMessages(_history);

            await foreach (var token in svc.StreamAsync(systemPrompt, contextMessages, ct))
            {
                assistantMsg.Text += token;
            }

            _history.Add(ChatMessage.Assistant(assistantMsg.Text));
        }
        catch (OperationCanceledException)
        {
            if (string.IsNullOrEmpty(assistantMsg.Text))
                Messages.Remove(assistantMsg);
        }
        catch (Exception ex)
        {
            assistantMsg.Text = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

internal sealed class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
    public void Execute(object? parameter) => _execute();
    public event EventHandler? CanExecuteChanged
    {
        add => System.Windows.Input.CommandManager.RequerySuggested += value;
        remove => System.Windows.Input.CommandManager.RequerySuggested -= value;
    }
}
