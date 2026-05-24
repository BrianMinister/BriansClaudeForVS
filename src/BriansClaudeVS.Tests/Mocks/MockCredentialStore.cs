using BriansClaudeVS.Core.Credentials;

namespace BriansClaudeVS.Tests.Mocks;

public class MockCredentialStore : ICredentialStore
{
    private string? _key;

    public void SaveApiKey(string plaintext) => _key = plaintext;
    public string? LoadApiKey() => _key;
    public void DeleteApiKey() => _key = null;
}

public class MockSettingsStoreAccessor : ISettingsStoreAccessor
{
    private readonly Dictionary<string, string> _store = new();

    public void SetString(string collectionPath, string propertyName, string value)
        => _store[$"{collectionPath}/{propertyName}"] = value;

    public string? GetString(string collectionPath, string propertyName)
        => _store.TryGetValue($"{collectionPath}/{propertyName}", out var v) ? v : null;

    public void DeleteString(string collectionPath, string propertyName)
        => _store.Remove($"{collectionPath}/{propertyName}");
}
