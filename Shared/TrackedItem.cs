using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlazorApp.Shared
{
    public class TrackedItem
    {
        public TrackedItem() { }
        public TrackedItem(string name)
        {
            Name = name;
        }

        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Category { get; set; }
        public ICollection<Occurrence> PastOccurrences { get; set; } = new List<Occurrence>();

        public ICollection<Target> Targets { get; set; } = new List<Target>();

        public IEnumerable<DateTime> GetFutureOccurrences(DateTime now, int count)
        {
            if (Targets.Count == 0)
                yield break;

            var pastOccurrences = PastOccurrences
                .Select(o => o.SafetyTimestamp)
                .ToList();

            for (int i = 0; i < count; i++)
            {
                var nextOccurrence = Targets.Select(t => t.GetNextOccurrence(now, pastOccurrences)).Max();
                yield return nextOccurrence;
                pastOccurrences.Add(nextOccurrence);
            }
        }

        public void AddOccurrence(DateTime timestamp)
        {
            if (Targets.Count == 0)
            {
                PastOccurrences.Add(new Occurrence { ActualTimestamp = timestamp, SafetyTimestamp = timestamp });
                return;
            }

            var nextOccurrence = Targets.Select(t => t.GetNextOccurrence(timestamp, PastOccurrences.Where(a => a.ActualTimestamp < timestamp).Select(o => o.SafetyTimestamp))).Max();

            var futureOccurrences = PastOccurrences.Where(o => o.ActualTimestamp > timestamp).ToList();

            PastOccurrences.Add(new Occurrence { ActualTimestamp = timestamp, SafetyTimestamp = nextOccurrence });

            foreach (var item in futureOccurrences)
            {
                item.SafetyTimestamp = Targets.Select(t => t.GetNextOccurrence(item.ActualTimestamp, PastOccurrences.Where(a => a.ActualTimestamp < item.ActualTimestamp).Select(o => o.SafetyTimestamp))).Max();
            }
        }
    }
}
