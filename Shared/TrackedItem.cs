using System;
using System.Collections.Generic;
using System.Linq;

namespace BlazorApp.Shared
{
    public class TrackedItem
    {
        public TrackedItem() { }
        public TrackedItem(string name)
        {
            Name = name;
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public bool Favourite { get; set; }
        public List<Occurrence> PastOccurrences { get; set; } = [];
        public List<Target> Targets { get; set; } = [];
        public decimal? DefaultStockUsage { get; set; }
        public List<StockAcquisition> StockAcquisitions { get; set; } = [];

        public StatusEnum GetStatus(DateTime now)
        {
            var eligibleTargets = Targets.Where(t => t.Qty > 0 && t.Frequency > TimeSpan.Zero);

            if (eligibleTargets.Any() is false)
                return StatusEnum.Ok;

            var statuses = eligibleTargets.Select(t => t.GetStatus(now, PastOccurrences.Select(o => o.SafetyTimestamp)));

            return statuses.Max();
        }

        public IEnumerable<DateTime> GetFutureOccurrences(DateTime now, int count)
        {
            var eligibleTargets = Targets
                .Where(t => t.Qty > 0 && t.Frequency > TimeSpan.Zero)
                .ToList();

            if (eligibleTargets.Any() is false)
                yield break;

            var pastOccurrences = PastOccurrences
                .Select(o => o.SafetyTimestamp)
                .ToList();

            for (int i = 0; i < count; i++)
            {
                var nextOccurrence = eligibleTargets.Select(t => t.GetEarliestOccurrence(now, pastOccurrences)).Max();

                if (nextOccurrence == now && eligibleTargets.All(a => a.Qty > 1))
                {
                    nextOccurrence = eligibleTargets.Select(t => t.GetSpacedOccurrence(now, pastOccurrences)).Max();
                }

                yield return nextOccurrence;
                pastOccurrences.Add(nextOccurrence);
            }
        }

        public IEnumerable<DateTime> GetFutureSpacedOccurrences(DateTime now, int count)
        {
            var eligibleTargets = Targets
                .Where(t => t.Qty > 0 && t.Frequency > TimeSpan.Zero)
                .ToList();

            if (eligibleTargets.Any() is false)
                yield break;

            var pastOccurrences = PastOccurrences
                .Select(o => o.SafetyTimestamp)
                .ToList();

            DateTime nextOccurrence = now;

            for (int i = 0; i < count; i++)
            {
                nextOccurrence = eligibleTargets.Select(t => t.GetSpacedOccurrence(nextOccurrence, pastOccurrences)).Max();

                yield return nextOccurrence;
                pastOccurrences.Add(nextOccurrence);
            }
        }

        public void AddOccurrence(DateTime timestamp) => AddOccurrence(timestamp, DefaultStockUsage);

        public void AddOccurrence(DateTime timestamp, decimal? stockUsed)
        {
            var eligibleTargets = Targets
                .Where(t => t.Qty > 0 && t.Frequency > TimeSpan.Zero)
                .ToList();

            if (eligibleTargets.Any() is false)
            {
                PastOccurrences.Add(new Occurrence { ActualTimestamp = timestamp, SafetyTimestamp = timestamp, StockUsed = stockUsed });
                return;
            }

            var nextOccurrence = eligibleTargets.Select(t => t.GetEarliestOccurrence(timestamp, PastOccurrences.Where(a => a.ActualTimestamp < timestamp).Select(o => o.SafetyTimestamp))).Max();

            var futureOccurrences = PastOccurrences.Where(o => o.ActualTimestamp > timestamp).ToList();

            PastOccurrences.Add(new Occurrence { ActualTimestamp = timestamp, SafetyTimestamp = nextOccurrence, StockUsed = stockUsed });

            foreach (var item in futureOccurrences)
            {
                item.SafetyTimestamp = eligibleTargets.Select(t => t.GetEarliestOccurrence(item.ActualTimestamp, PastOccurrences.Where(a => a.ActualTimestamp < item.ActualTimestamp).Select(o => o.SafetyTimestamp))).Max();
            }
        }

        public TrackedItemArchive CheckForArchiving()
        {
            if (PastOccurrences.Count <= 200)
                return null;

            // Sort by ActualTimestamp to get the oldest occurrences
            var sortedOccurrences = PastOccurrences.OrderBy(o => o.ActualTimestamp).ToList();
            var toArchive = sortedOccurrences.Take(100).ToList();
            var toKeep = sortedOccurrences.Skip(100).ToList();

            // Create archive with the oldest 100 occurrences
            var archive = new TrackedItemArchive
            {
                TrackedItemId = Id,
                ArchivedOccurrences = toArchive,
                CreatedAt = DateTime.UtcNow
            };

            // Keep only the newest occurrences
            PastOccurrences = toKeep;

            return archive;
        }

        public decimal CalculateCurrentStockLevel() // Paabd
        {
            var totalAcquired = StockAcquisitions.Sum(sa => sa.Quantity);
            var totalUsed = PastOccurrences.Sum(po => po.StockUsed ?? 0);
            return totalAcquired - totalUsed;
        }
    }
}
