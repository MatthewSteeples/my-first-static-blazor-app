using BlazorApp.Shared;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BlazorApp.Shared.Tests
{
    [TestClass]
    public class SyncEventTests
    {
        [TestMethod]
        public void SyncEvent_ShouldInitializeWithDefaults()
        {
            // Arrange & Act
            var syncEvent = new SyncEvent();

            // Assert
            Assert.AreNotEqual(Guid.Empty, syncEvent.EventId);
            Assert.AreEqual(SyncEventType.Created, syncEvent.EventType);
            Assert.AreEqual(Guid.Empty, syncEvent.ItemId);
            Assert.IsNull(syncEvent.Payload);
            Assert.IsTrue(syncEvent.Timestamp <= DateTimeOffset.UtcNow);
            Assert.IsTrue(syncEvent.Timestamp > DateTimeOffset.UtcNow.AddSeconds(-5));
        }

        [TestMethod]
        public void SyncEvent_ShouldRoundTripSerialize()
        {
            // Arrange
            var syncEvent = new SyncEvent
            {
                EventId = Guid.NewGuid(),
                EventType = SyncEventType.Updated,
                ItemId = Guid.NewGuid(),
                Timestamp = DateTimeOffset.UtcNow,
                Payload = "{\"Name\":\"Test Item\"}"
            };

            // Act
            var json = JsonSerializer.Serialize(syncEvent, SerializationContext.Default.SyncEvent);
            var deserialized = JsonSerializer.Deserialize<SyncEvent>(json, SerializationContext.Default.SyncEvent);

            // Assert
            Assert.IsNotNull(deserialized);
            Assert.AreEqual(syncEvent.EventId, deserialized.EventId);
            Assert.AreEqual(syncEvent.EventType, deserialized.EventType);
            Assert.AreEqual(syncEvent.ItemId, deserialized.ItemId);
            Assert.AreEqual(syncEvent.Timestamp, deserialized.Timestamp);
            Assert.AreEqual(syncEvent.Payload, deserialized.Payload);
        }

        [TestMethod]
        public void SyncEvent_DeletedEvent_ShouldSerializeWithNullPayload()
        {
            // Arrange
            var syncEvent = new SyncEvent
            {
                EventType = SyncEventType.Deleted,
                ItemId = Guid.NewGuid(),
                Payload = null
            };

            // Act
            var json = JsonSerializer.Serialize(syncEvent, SerializationContext.Default.SyncEvent);
            var deserialized = JsonSerializer.Deserialize<SyncEvent>(json, SerializationContext.Default.SyncEvent);

            // Assert
            Assert.IsNotNull(deserialized);
            Assert.AreEqual(SyncEventType.Deleted, deserialized.EventType);
            Assert.IsNull(deserialized.Payload);
        }
    }
}
