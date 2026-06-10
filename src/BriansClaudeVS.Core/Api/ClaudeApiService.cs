using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using BriansClaudeVS.Core.Api.Models;
using BriansClaudeVS.Core.Credentials;

namespace BriansClaudeVS.Core.Api;

public class ClaudeApiService : IClaudeApiService
{
    private const string ApiBase = "https://api.anthropic.com/v1/messages";
    private const string AnthropicVersion = "2023-06-01";

    private readonly ICredentialStore _credentials;
    private readonly IHttpClientFactory _httpFactory;

    public string ChatModelId { get; set; } = "claude-opus-4-5";
    public string InlineModelId { get; set; } = "claude-haiku-4-5-20251001";

    public ClaudeApiService(ICredentialStore credentials, IHttpClientFactory httpFactory)
    {
        _credentials = credentials;
        _httpFactory = httpFactory;
    }

    public async Task<string> CompleteAsync(
        string systemPrompt,
        IReadOnlyList<ChatMessage> messages,
        CancellationToken ct = default)
    {
        var apiKey = _credentials.LoadApiKey()
            ?? throw new InvalidOperationException("Anthropic API key is not configured. Set it in Tools > Options > Brian's Claude > General.");

        using var client = _httpFactory.CreateClient("anthropic");
        client.DefaultRequestHeaders.Add("x-api-key", apiKey);
        client.DefaultRequestHeaders.Add("anthropic-version", AnthropicVersion);

        var body = BuildRequestBody(systemPrompt, messages, ChatModelId, 4096, stream: false);
        var response = await client.PostAsJsonAsync(ApiBase, body, ct);
        response.EnsureSuccessStatusCode();

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        return doc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString() ?? string.Empty;
    }

    public async IAsyncEnumerable<string> StreamAsync(
        string systemPrompt,
        IReadOnlyList<ChatMessage> messages,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var apiKey = _credentials.LoadApiKey()
            ?? throw new InvalidOperationException("Anthropic API key is not configured. Set it in Tools > Options > Brian's Claude > General.");

        using var client = _httpFactory.CreateClient("anthropic");
        client.DefaultRequestHeaders.Add("x-api-key", apiKey);
        client.DefaultRequestHeaders.Add("anthropic-version", AnthropicVersion);

        var body = BuildRequestBody(systemPrompt, messages, ChatModelId, 4096, stream: true);
        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Post, ApiBase) { Content = content };
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        while (!ct.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(ct);
            if (line == null) break;
            if (!line.StartsWith("data: ", StringComparison.Ordinal)) continue;

            var data = line["data: ".Length..];
            if (data == "[DONE]") break;

            string? delta = null;
            try
            {
                using var doc = JsonDocument.Parse(data);
                var root = doc.RootElement;
                if (root.TryGetProperty("type", out var typeEl) &&
                    typeEl.GetString() == "content_block_delta" &&
                    root.TryGetProperty("delta", out var deltaEl) &&
                    deltaEl.TryGetProperty("text", out var textEl))
                {
                    delta = textEl.GetString();
                }
            }
            catch (JsonException) { }

            if (delta != null)
                yield return delta;
        }
    }

    public async Task<IReadOnlyList<string>> GetInlineCompletionsAsync(
        string prefix,
        string suffix,
        string languageId,
        CancellationToken ct = default)
    {
        var apiKey = _credentials.LoadApiKey();
        if (apiKey == null) return [];

        const string systemPrompt =
            "You are a code completion engine. Output ONLY the completion text — no explanations, " +
            "no markdown, no code fences. Match the indentation and style of the surrounding code exactly.";

        var userMessage = $"""
            Complete this {languageId} code. Output only what goes between <prefix> and <suffix>:
            <prefix>
            {prefix}
            </prefix>
            <suffix>
            {suffix}
            </suffix>
            """;

        var messages = new[] { ChatMessage.User(userMessage) };
        var body = BuildRequestBody(systemPrompt, messages, InlineModelId, 256, stream: false);

        using var client = _httpFactory.CreateClient("anthropic");
        client.DefaultRequestHeaders.Add("x-api-key", apiKey);
        client.DefaultRequestHeaders.Add("anthropic-version", AnthropicVersion);

        var response = await client.PostAsJsonAsync(ApiBase, body, ct);
        if (!response.IsSuccessStatusCode) return [];

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        var text = doc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString();

        return string.IsNullOrWhiteSpace(text) ? [] : [text.TrimEnd()];
    }

    private static object BuildRequestBody(
        string systemPrompt,
        IReadOnlyList<ChatMessage> messages,
        string modelId,
        int maxTokens,
        bool stream)
    {
        var apiMessages = messages
            .Where(m => m.Role != ChatRole.System)
            .Select(m => new { role = m.Role == ChatRole.User ? "user" : "assistant", content = m.Content })
            .ToList();

        return new
        {
            model = modelId,
            max_tokens = maxTokens,
            system = systemPrompt,
            messages = apiMessages,
            stream
        };
    }
}
