namespace BriansClaudeVS.Core.Credentials;

// Thin abstraction over the VS writable settings store so Core has no VS SDK dep.
// The Extension project provides the real implementation; Tests provide a mock.
public interface ISettingsStoreAccessor
{
    void SetString(string collectionPath, string propertyName, string value);
    string? GetString(string collectionPath, string propertyName);
    void DeleteString(string collectionPath, string propertyName);
}
