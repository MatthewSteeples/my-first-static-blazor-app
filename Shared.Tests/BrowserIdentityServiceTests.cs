using BlazorApp.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;
using System;

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

        [TestMethod]
        public void ImportFormat_CanParseNewFormatWithIdentity()
        {
            // Arrange
            var identity = new BrowserIdentity
            {
                Id = "test-id-123",
                PublicKey = "test-public-key",
                PrivateKey = "test-private-key"
            };
            
            var trackedItems = new[] { "{\"Id\":\"00000000-0000-0000-0000-000000000001\",\"Name\":\"Test Item\"}" };
            
            var exportData = new
            {
                identity = identity,
                trackedItems = trackedItems
            };

            var exportJson = JsonSerializer.Serialize(exportData, new JsonSerializerOptions { WriteIndented = true });

            // Act
            var document = JsonDocument.Parse(exportJson);
            var root = document.RootElement;

            // Assert - Can parse new format
            Assert.IsTrue(root.TryGetProperty("identity", out var identityElement));
            Assert.IsTrue(root.TryGetProperty("trackedItems", out var trackedItemsElement));
            
            var parsedIdentity = JsonSerializer.Deserialize<BrowserIdentity>(identityElement.GetRawText());
            Assert.IsNotNull(parsedIdentity);
            Assert.AreEqual("test-id-123", parsedIdentity.Id);
            Assert.AreEqual("test-public-key", parsedIdentity.PublicKey);
            Assert.AreEqual("test-private-key", parsedIdentity.PrivateKey);
            
            Assert.AreEqual(JsonValueKind.Array, trackedItemsElement.ValueKind);
            Assert.AreEqual(1, trackedItemsElement.GetArrayLength());
        }

        [TestMethod]
        public void ImportFormat_CanParseLegacyArrayFormat()
        {
            // Arrange
            var trackedItems = new[]
            {
                new TrackedItem { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), Name = "Test Item 1" },
                new TrackedItem { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), Name = "Test Item 2" }
            };

            var legacyJson = JsonSerializer.Serialize(trackedItems, SerializationContext.Default.TrackedItemArray);

            // Act
            var document = JsonDocument.Parse(legacyJson);
            var root = document.RootElement;

            // Assert - Should be an array without identity property
            Assert.AreEqual(JsonValueKind.Array, root.ValueKind);
            
            var parsedItems = JsonSerializer.Deserialize<TrackedItem[]>(legacyJson, SerializationContext.Default.TrackedItemArray);
            Assert.IsNotNull(parsedItems);
            Assert.AreEqual(2, parsedItems.Length);
            Assert.AreEqual("Test Item 1", parsedItems[0].Name);
            Assert.AreEqual("Test Item 2", parsedItems[1].Name);
        }
    }
}