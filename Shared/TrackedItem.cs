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
