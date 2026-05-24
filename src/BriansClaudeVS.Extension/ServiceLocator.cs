using BriansClaudeVS.Core.Api;
using BriansClaudeVS.Core.Credentials;
using BriansClaudeVS.Core.SlashCommands;
using Microsoft.Extensions.Http;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace BriansClaudeVS;

// Simple service locator initialized by the package at startup.
// Not a DI container — kept intentionally minimal for a VS extension.
internal static class ServiceLocator
{
    public static ICredentialStore? CredentialStore { get; private set; }
    public static IClaudeApiService? ApiService { get; private set; }
    public static ISlashCommandParser SlashCommandParser { get; } = new SlashCommandParser();

    public static void Initialize(AsyncPackage package)
    {
        var settingsStore = new VsSettingsStoreAccessor(package);
        CredentialStore = new DpapiCredentialStore(settingsStore);

        var httpFactory = new DefaultHttpClientFactory();
        ApiService = new ClaudeApiService(CredentialStore, httpFactory);
    }
}

// Minimal IHttpClientFactory that creates plain HttpClient instances.
// Replace with Microsoft.Extensions.Http in a DI-enabled host if needed.
internal sealed class DefaultHttpClientFactory : IHttpClientFactory
{
    public HttpClient CreateClient(string name) => new()
    {
        Timeout = TimeSpan.FromSeconds(60)
    };
}

// Bridge from VS IVsWritableSettingsStore to our ISettingsStoreAccessor abstraction.
internal sealed class VsSettingsStoreAccessor : ISettingsStoreAccessor
{
    private readonly AsyncPackage _package;

    public VsSettingsStoreAccessor(AsyncPackage package)
    {
        _package = package;
    }

    private IVsWritableSettingsStore GetStore()
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        var settingsManager = (IVsSettingsManager)_package.GetService(typeof(SVsSettingsManager))!;
        settingsManager.GetWritableSettingsStore(
            (uint)__VsEnclosingScopes.EnclosingScopes_UserSettings,
            out var store);
        return store;
    }

    public void SetString(string collectionPath, string propertyName, string value)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        var store = GetStore();
        if (!store.CollectionExists(collectionPath, out _))
            store.CreateCollection(collectionPath);
        store.SetString(collectionPath, propertyName, value);
    }

    public string? GetString(string collectionPath, string propertyName)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        var store = GetStore();
        if (!store.CollectionExists(collectionPath, out _)) return null;
        store.GetString(collectionPath, propertyName, out var result);
        return string.IsNullOrEmpty(result) ? null : result;
    }

    public void DeleteString(string collectionPath, string propertyName)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        var store = GetStore();
        if (store.CollectionExists(collectionPath, out _))
            store.DeleteProperty(collectionPath, propertyName);
    }
}
