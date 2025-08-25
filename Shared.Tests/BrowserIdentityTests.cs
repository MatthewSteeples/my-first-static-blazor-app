using BlazorApp.Shared;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BlazorApp.Shared.Tests
{
    [TestClass]
    public class BrowserIdentityTests
    {
        [TestMethod]
        public void BrowserIdentity_ShouldInitializeWithDefaults()
        {
            // Arrange & Act
            var identity = new BrowserIdentity();

            // Assert
            Assert.AreNotEqual(Guid.Empty.ToString(), identity.Id);
            Assert.AreEqual("ECDSA", identity.Algorithm);
            Assert.AreEqual("P-256", identity.Curve);
            Assert.IsTrue(identity.CreatedAt <= DateTime.UtcNow);
            Assert.IsTrue(identity.CreatedAt > DateTime.UtcNow.AddSeconds(-5));
        }

        [TestMethod]
        public void BrowserIdentity_ShouldBeSerializable()
        {
            // Arrange
            var identity = new BrowserIdentity
            {
                PublicKey = "test-public-key",
                PrivateKey = "test-private-key"
            };

            // Act
            var json = JsonSerializer.Serialize(identity, SerializationContext.Default.BrowserIdentity);
            var deserializedIdentity = JsonSerializer.Deserialize<BrowserIdentity>(json, SerializationContext.Default.BrowserIdentity);

            // Assert
            Assert.IsNotNull(deserializedIdentity);
            Assert.AreEqual(identity.Id, deserializedIdentity.Id);
            Assert.AreEqual(identity.PublicKey, deserializedIdentity.PublicKey);
            Assert.AreEqual(identity.PrivateKey, deserializedIdentity.PrivateKey);
            Assert.AreEqual(identity.Algorithm, deserializedIdentity.Algorithm);
            Assert.AreEqual(identity.Curve, deserializedIdentity.Curve);
        }
    }
}