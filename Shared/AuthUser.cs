using System.Text.Json.Serialization;

namespace BlazorApp.Shared
{
    public class AuthUser
    {
        [JsonPropertyName("identityProvider")]
        public string IdentityProvider { get; set; } = string.Empty;
        
        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;
        
        [JsonPropertyName("userDetails")]
        public string UserDetails { get; set; } = string.Empty;
        
        [JsonPropertyName("userRoles")]
        public string[] UserRoles { get; set; } = [];
        
        [JsonPropertyName("claims")]
        public AuthClaim[] Claims { get; set; } = [];

        public string GetClaimValue(string claimType)
        {
            foreach (var claim in Claims)
            {
                if (claim.Type == claimType)
                    return claim.Value;
            }
            return string.Empty;
        }

        public string FirstName => GetClaimValue("given_name");
        public string LastName => GetClaimValue("family_name");
        public string Email => GetClaimValue("emails");
        public string Name => GetClaimValue("name");
    }

    public class AuthClaim
    {
        [JsonPropertyName("typ")]
        public string Type { get; set; } = string.Empty;
        
        [JsonPropertyName("val")]
        public string Value { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        [JsonPropertyName("clientPrincipal")]
        public AuthUser ClientPrincipal { get; set; }
    }
}