namespace BriansClaudeVS.Core.Api.Models;

public enum ChatRole { User, Assistant, System }

public record ChatMessage(ChatRole Role, string Content)
{
    public static ChatMessage User(string content) => new(ChatRole.User, content);
    public static ChatMessage Assistant(string content) => new(ChatRole.Assistant, content);
    public static ChatMessage System(string content) => new(ChatRole.System, content);
}
