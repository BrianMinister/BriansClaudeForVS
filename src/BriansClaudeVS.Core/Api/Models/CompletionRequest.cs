namespace BriansClaudeVS.Core.Api.Models;

public record CompletionRequest(
    string SystemPrompt,
    IReadOnlyList<ChatMessage> Messages,
    string ModelId,
    int MaxTokens = 4096,
    bool Stream = false);

public record InlineCompletionRequest(
    string Prefix,
    string Suffix,
    string LanguageId,
    string ModelId,
    int MaxTokens = 256);
