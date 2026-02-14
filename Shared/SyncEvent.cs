using System;

namespace BlazorApp.Shared
{
    public class SyncEvent
    {
        public Guid EventId { get; set; } = Guid.NewGuid();

        public SyncEventType EventType { get; set; }

        public Guid ItemId { get; set; }

        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// JSON snapshot of the TrackedItem. Null for delete events.
        /// </summary>
        public string? Payload { get; set; }
    }
}
