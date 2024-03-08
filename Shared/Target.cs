using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlazorApp.Shared
{
    public partial class Target
    {
        public int Qty { get; set; }
        public TimeSpan Frequency { get; set; }

        public DateTime GetNextOccurrence(DateTime now, IEnumerable<DateTime> pastOccurrences)
        {
            var relevantPeriod = now - Frequency;

            var relevantPastOccurrences = pastOccurrences
                .Where(o => o >= relevantPeriod)
                .ToList();

            if (relevantPastOccurrences.Count < Qty)
            {
                return now;
            }
            else if (relevantPastOccurrences.Count == Qty)
            {
                return relevantPastOccurrences.Min().Add(Frequency);
            }
            else
            {
                return relevantPastOccurrences[relevantPastOccurrences.Count - Qty].Add(Frequency);
            }
        }
    }
}
