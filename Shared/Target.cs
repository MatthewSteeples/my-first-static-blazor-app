using System;
using System.Collections.Generic;
using System.Linq;

namespace BlazorApp.Shared
{
    public partial class Target
    {
        public int Qty { get; set; }
        public TimeSpan Frequency { get; set; }

        public StatusEnum GetStatus(DateTime now, IEnumerable<DateTime> pastOccurrences)
        {
            var relevantPeriod = now - Frequency;

            var relevantPastOccurrencesCount = pastOccurrences
                .Where(o => o > relevantPeriod)
                .Count();

            if (relevantPastOccurrencesCount < Qty)
            {
                return StatusEnum.Ok;
            }
            else if (relevantPastOccurrencesCount == Qty)
            {
                return StatusEnum.AtLimit;
            }
            else
            {
                return StatusEnum.OverLimit;
            }
        }

        public DateTime GetEarliestOccurrence(DateTime now, IEnumerable<DateTime> pastOccurrences)
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

        public DateTime GetSpacedOccurrence(DateTime now, IEnumerable<DateTime> pastOccurrences)
        {
            var relevantPeriod = now - Frequency;

            var relevantPastOccurrences = pastOccurrences
                .Where(o => o >= relevantPeriod)
                .ToList();

            if (relevantPastOccurrences.Count == 0)
            {
                return now;
            }
            else if (relevantPastOccurrences.Count < Qty)
            {
                var mostRecentOccurrence = relevantPastOccurrences.Where(a => a <= now).DefaultIfEmpty().Min();

                var diff = Qty - (Qty - relevantPastOccurrences.Count);
                //var diff = 1;
                var proRata = TimeSpan.FromMilliseconds((Frequency.TotalMilliseconds / Qty) * diff);

                mostRecentOccurrence = mostRecentOccurrence.Add(proRata);

                if (mostRecentOccurrence > now)
                {
                    return mostRecentOccurrence;
                }

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
