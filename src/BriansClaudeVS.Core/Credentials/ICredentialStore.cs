namespace BriansClaudeVS.Core.Credentials;

public interface ICredentialStore
{
    void SaveApiKey(string plaintext);
    string? LoadApiKey();
    void DeleteApiKey();
}
