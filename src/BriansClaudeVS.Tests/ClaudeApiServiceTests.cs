using System.Net;
using System.Text;
using System.Text.Json;
using BriansClaudeVS.Core.Api;
using BriansClaudeVS.Core.Api.Models;
using BriansClaudeVS.Tests.Mocks;
using Moq;
using Xunit;

namespace BriansClaudeVS.Tests;

public class ClaudeApiServiceTests
{
    private static ClaudeApiService BuildService(
        string? apiKey,
        HttpResponseMessage response)
    {
        var credStore = new MockCredentialStore();
        if (apiKey != null) credStore.SaveApiKey(apiKey);

        var handler = new MockHttpMessageHandler(response);
        var httpClient = new HttpClient(handler);
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        return new ClaudeApiService(credStore, factory.Object);
    }

    [Fact]
    public async Task CompleteAsync_ReturnsTextFromResponse()
    {
        var fakeResponse = new
        {
            content = new[] { new { type = "text", text = "Hello from Claude" } }
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(fakeResponse),
                Encoding.UTF8, "application/json")
        };

        var svc = BuildService("sk-test-key", response);
        var result = await svc.CompleteAsync("system", [ChatMessage.User("hi")]);
        Assert.Equal("Hello from Claude", result);
    }

    [Fact]
    public async Task CompleteAsync_NoApiKey_ThrowsInvalidOperation()
    {
        var svc = BuildService(null, new HttpResponseMessage(HttpStatusCode.OK));
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => svc.CompleteAsync("system", [ChatMessage.User("hi")]));
    }
}

internal class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage _response;
    public MockHttpMessageHandler(HttpResponseMessage response) => _response = response;
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
        => Task.FromResult(_response);
}
