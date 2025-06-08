using BlazorApp.Shared;
using System.Net.Http.Json;

namespace BlazorApp.Client.Services
{
    public interface IAuthService
    {
        Task<AuthUser?> GetCurrentUserAsync();
        Task<bool> IsAuthenticatedAsync();
    }

    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private AuthUser? _cachedUser;
        private bool _hasChecked = false;

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AuthUser?> GetCurrentUserAsync()
        {
            if (_hasChecked)
                return _cachedUser;

            try
            {
                var response = await _httpClient.GetFromJsonAsync<AuthResponse>("/.auth/me");
                _cachedUser = response?.ClientPrincipal;
                _hasChecked = true;
                return _cachedUser;
            }
            catch
            {
                _hasChecked = true;
                _cachedUser = null;
                return null;
            }
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var user = await GetCurrentUserAsync();
            return user != null;
        }
    }
}