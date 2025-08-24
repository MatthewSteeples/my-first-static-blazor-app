using BlazorApp.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;

namespace BlazorApp.Shared.Tests
{
    [TestClass]
    public class BrowserIdentityIntegrationTests
    {
        [TestMethod]
        public void ExportFormat_ShouldIncludeIdentityField()
        {
            // Arrange
            var identity = new BrowserIdentity
            {
                PublicKey = "test-public-key",
                PrivateKey = "test-private-key"
            };
            
            var trackedItems = new[] { "item1", "item2" };
            
            var exportData = new
            {
                identity = identity,
                trackedItems = trackedItems
            };

            // Act
            var exportJson = JsonSerializer.Serialize(exportData, new JsonSerializerOptions { WriteIndented = true });
            var deserializedData = JsonSerializer.Deserialize<JsonElement>(exportJson);

            // Assert
            Assert.IsTrue(deserializedData.TryGetProperty("identity", out var identityElement));
            Assert.IsTrue(deserializedData.TryGetProperty("trackedItems", out var trackedItemsElement));
            
            Assert.IsTrue(identityElement.TryGetProperty("Id", out var idElement));
            Assert.IsTrue(identityElement.TryGetProperty("PublicKey", out var publicKeyElement));
            Assert.IsTrue(identityElement.TryGetProperty("PrivateKey", out var privateKeyElement));
            
            Assert.AreEqual(identity.Id, idElement.GetString());
            Assert.AreEqual("test-public-key", publicKeyElement.GetString());
            Assert.AreEqual("test-private-key", privateKeyElement.GetString());
            
            Assert.AreEqual(JsonValueKind.Array, trackedItemsElement.ValueKind);
            Assert.AreEqual(2, trackedItemsElement.GetArrayLength());
        }
    }
}