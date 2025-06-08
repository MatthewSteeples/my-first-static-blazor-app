using System;
using System.Collections.Generic;

namespace BlazorApp.Shared
{
    public class TrackedItemArchive
    {
        public Guid TrackedItemId { get; set; }
        public int ArchiveNumber { get; set; }
        public List<Occurrence> ArchivedOccurrences { get; set; } = [];
        public DateTime CreatedAt { get; set; }
    }
}