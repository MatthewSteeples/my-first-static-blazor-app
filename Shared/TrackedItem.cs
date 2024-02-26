using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlazorApp.Shared
{
    public class TrackedItem
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public ICollection<Occurrence> PastOccurrences { get; set; }

        public ICollection<Target> Targets { get; set; }

        public IEnumerable<DateTime> GetFutureOccurrences(DateTime now, int count)
        {
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
            var nextOccurrence = Targets.Select(t => t.GetNextOccurrence(timestamp, PastOccurrences.Select(o => o.SafetyTimestamp))).Max();

            PastOccurrences.Add(new Occurrence { ActualTimestamp = timestamp, SafetyTimestamp = nextOccurrence });
        }
    }
}
