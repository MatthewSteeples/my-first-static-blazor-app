using System;
using System.Text.Json;

namespace BlazorApp.Shared
{
    public class BrowserIdentity
    {
        public string Id { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public string PrivateKey { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Algorithm { get; set; } = "ECDSA";
        public string Curve { get; set; } = "P-256";

        public BrowserIdentity()
        {
            Id = Guid.NewGuid().ToString();
            CreatedAt = DateTime.UtcNow;
        }
    }
}