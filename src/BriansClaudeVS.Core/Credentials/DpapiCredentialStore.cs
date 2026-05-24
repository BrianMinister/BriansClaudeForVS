using System.Security.Cryptography;
using System.Text;
using ProtectedData = System.Security.Cryptography.ProtectedData;

namespace BriansClaudeVS.Core.Credentials;

// Encrypts the API key with DPAPI (current-user scope) before persisting.
// The VS settings store holds only the encrypted ciphertext; the plaintext key
// never touches disk unprotected.
public class DpapiCredentialStore : ICredentialStore
{
    private readonly ISettingsStoreAccessor _store;
    private const string Collection = "BriansClaudeVS";
    private const string KeyName = "AnthropicApiKey";

    public DpapiCredentialStore(ISettingsStoreAccessor store)
    {
        _store = store;
    }

    public void SaveApiKey(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
        {
            DeleteApiKey();
            return;
        }

        byte[] encrypted = ProtectedData.Protect(
            Encoding.UTF8.GetBytes(plaintext),
            null,
            DataProtectionScope.CurrentUser);

        _store.SetString(Collection, KeyName, Convert.ToBase64String(encrypted));
    }

    public string? LoadApiKey()
    {
        var b64 = _store.GetString(Collection, KeyName);
        if (string.IsNullOrEmpty(b64)) return null;

        try
        {
            byte[] decrypted = ProtectedData.Unprotect(
                Convert.FromBase64String(b64),
                null,
                DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decrypted);
        }
        catch
        {
            // Key was saved by a different user or is corrupted — treat as missing.
            return null;
        }
    }

    public void DeleteApiKey()
    {
        _store.DeleteString(Collection, KeyName);
    }
}
