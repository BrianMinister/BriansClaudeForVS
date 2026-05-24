using BriansClaudeVS.Core.Api.Models;

namespace BriansClaudeVS.Core.Api;

public class ContextBuilder
{
    private const int DefaultMaxTokens = 8000;

    public IReadOnlyList<ChatMessage> BuildChatMessages(
        IReadOnlyList<ChatMessage> history,
        int maxTokens = DefaultMaxTokens)
    {
        var result = new List<ChatMessage>(history);

        // Rough token estimate: 4 chars ≈ 1 token
        while (result.Count > 2 && result.Sum(m => m.Content.Length / 4) > maxTokens)
            result.RemoveAt(0);

        return result;
    }

    public string BuildCodeActionContext(
        string selectedCode,
        string fullFileContent,
        string languageId)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Language: {languageId}");
        sb.AppendLine();
        sb.AppendLine("Selected code:");
        sb.AppendLine("```");
        sb.AppendLine(selectedCode);
        sb.AppendLine("```");
        sb.AppendLine();
        sb.AppendLine("Full file context:");
        sb.AppendLine("```");
        // Truncate full file to avoid token explosion
        var truncated = fullFileContent.Length > 8000
            ? fullFileContent[..8000] + "\n... (truncated)"
            : fullFileContent;
        sb.AppendLine(truncated);
        sb.AppendLine("```");
        return sb.ToString();
    }
}
