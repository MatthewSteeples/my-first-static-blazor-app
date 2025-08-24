using BlazorApp.Shared;
using Blazored.LocalStorage;
using Microsoft.JSInterop;
using System.Text.Json;

namespace BlazorApp.Client.Services
{
    public interface IBrowserIdentityService
    {
        Task<BrowserIdentity> GetOrCreateIdentityAsync();
        Task<BrowserIdentity?> GetIdentityAsync();
        Task UpdateIdentityKeysAsync(string publicKey, string privateKey);
    }

    public class BrowserIdentityService : IBrowserIdentityService
    {
        private const string IDENTITY_KEY = "BrowserIdentity";
        private readonly ILocalStorageService _localStorage;
        private readonly IJSRuntime _jsRuntime;
        private BrowserIdentity? _cachedIdentity;

        public BrowserIdentityService(ILocalStorageService localStorage, IJSRuntime jsRuntime)
        {
            _localStorage = localStorage;
            _jsRuntime = jsRuntime;
        }

        public async Task<BrowserIdentity?> GetIdentityAsync()
        {
            if (_cachedIdentity != null)
                return _cachedIdentity;

            try
            {
                var identityJson = await _localStorage.GetItemAsStringAsync(IDENTITY_KEY);
                if (!string.IsNullOrEmpty(identityJson))
                {
                    _cachedIdentity = JsonSerializer.Deserialize<BrowserIdentity>(identityJson);
                    return _cachedIdentity;
                }
            }
            catch
            {
                // If there's any error deserializing, we'll create a new identity
            }

            return null;
        }

        public async Task<BrowserIdentity> GetOrCreateIdentityAsync()
        {
            var existing = await GetIdentityAsync();
            if (existing != null)
                return existing;

            // Create new identity using WebCrypto API
            var identity = await CreateNewIdentityAsync();
            await SaveIdentityAsync(identity);
            _cachedIdentity = identity;
            return identity;
        }

        private async Task<BrowserIdentity> CreateNewIdentityAsync()
        {
            var identity = new BrowserIdentity();

            try
            {
                // Use JavaScript to generate keys with WebCrypto API
                var keyPair = await _jsRuntime.InvokeAsync<JsonElement>("generateKeyPair");
                
                identity.PublicKey = keyPair.GetProperty("publicKey").GetString() ?? "";
                identity.PrivateKey = keyPair.GetProperty("privateKey").GetString() ?? "";
            }
            catch
            {
                // Fallback to a simple UUID-based identity if WebCrypto fails
                identity.PublicKey = $"pub_{Guid.NewGuid():N}";
                identity.PrivateKey = $"priv_{Guid.NewGuid():N}";
            }

            return identity;
        }

        private async Task SaveIdentityAsync(BrowserIdentity identity)
        {
            var identityJson = JsonSerializer.Serialize(identity);
            await _localStorage.SetItemAsStringAsync(IDENTITY_KEY, identityJson);
        }

        public async Task UpdateIdentityKeysAsync(string publicKey, string privateKey)
        {
            var identity = await GetOrCreateIdentityAsync();
            identity.PublicKey = publicKey;
            identity.PrivateKey = privateKey;
            await SaveIdentityAsync(identity);
            _cachedIdentity = identity;
        }
    }
}