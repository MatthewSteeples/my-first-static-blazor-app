using BlazorApp.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;
using System;

namespace BlazorApp.Shared.Tests
{
    [TestClass]
    public class BrowserIdentityIntegrationTests
    {
        private static ExportData CreateExportData() => new()
        {
            Identity = new BrowserIdentity
            {
                Id = "test-id-123",
                PublicKey = "test-public-key",
                PrivateKey = "test-private-key"
            },
            TrackedItems =
            [
                new TrackedItem
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    Name = "Test Item",
                    Targets = [new Target { Qty = 2, Frequency = TimeSpan.FromHours(4) }]
                }
            ]
        };

        [TestMethod]
        public void ExportFormat_ShouldContainTrackedItemsAsObjects()
        {
            var json = JsonSerializer.Serialize(CreateExportData(), SerializationContext.Default.ExportData);

            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            Assert.IsTrue(root.TryGetProperty("Identity", out var identityElement));
            Assert.AreEqual("test-public-key", identityElement.GetProperty("PublicKey").GetString());

            var items = root.GetProperty("TrackedItems");
            Assert.AreEqual(JsonValueKind.Array, items.ValueKind);
            Assert.AreEqual(1, items.GetArrayLength());
            Assert.AreEqual(JsonValueKind.Object, items[0].ValueKind);
            Assert.AreEqual("Test Item", items[0].GetProperty("Name").GetString());
        }

        [TestMethod]
        public void ExportFormat_ShouldRoundTrip()
        {
            var json = JsonSerializer.Serialize(CreateExportData(), SerializationContext.Default.ExportData);

            var parsed = JsonSerializer.Deserialize(json, SerializationContext.Default.ExportData);

            Assert.IsNotNull(parsed);
            Assert.AreEqual("test-id-123", parsed.Identity.Id);
            Assert.AreEqual("test-public-key", parsed.Identity.PublicKey);
            Assert.AreEqual("test-private-key", parsed.Identity.PrivateKey);
            Assert.AreEqual(1, parsed.TrackedItems.Count);
            Assert.AreEqual("Test Item", parsed.TrackedItems[0].Name);
            Assert.AreEqual(TimeSpan.FromHours(4), parsed.TrackedItems[0].Targets[0].Frequency);
        }
    }
}
