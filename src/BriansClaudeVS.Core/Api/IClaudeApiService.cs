using BriansClaudeVS.Core.Api.Models;

namespace BriansClaudeVS.Core.Api;

public interface IClaudeApiService
{
    string ChatModelId { get; set; }
    string InlineModelId { get; set; }

    Task<string> CompleteAsync(
        string systemPrompt,
        IReadOnlyList<ChatMessage> messages,
        CancellationToken ct = default);

    IAsyncEnumerable<string> StreamAsync(
        string systemPrompt,
        IReadOnlyList<ChatMessage> messages,
        CancellationToken ct = default);

    Task<IReadOnlyList<string>> GetInlineCompletionsAsync(
        string prefix,
        string suffix,
        string languageId,
        CancellationToken ct = default);
}
