using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlazorApp.Shared.Comparers
{
    public class TrackedItemComparerDescending : IComparer<TrackedItem>
    {
        private readonly TrackedItemComparer _comparer = new TrackedItemComparer();

        public int Compare(TrackedItem x, TrackedItem y)
        {
            return 0 - _comparer.Compare(x, y);
        }
    }

    public class TrackedItemComparer : IComparer<TrackedItem>
    {
        public int Compare(TrackedItem x, TrackedItem y)
        {
            if (x.PastOccurrences.Any())
            {
                if (y.PastOccurrences.Any())
                {
                    var maxX = x.PastOccurrences.Max(o => o.ActualTimestamp);
                    var maxY = y.PastOccurrences.Max(o => o.ActualTimestamp);

                    var diff = maxX - maxY;

                    if (diff > TimeSpan.FromDays(1))
                    {
                        return 1;
                    }
                    else if (diff < TimeSpan.FromDays(-1))
                    {
                        return -1;
                    }
                    else
                    {
                        var nextX = x.GetFutureOccurrences(DateTime.UtcNow, 1).DefaultIfEmpty(DateTime.MaxValue).First();
                        var nextY = y.GetFutureOccurrences(DateTime.UtcNow, 1).DefaultIfEmpty(DateTime.MaxValue).First();

                        return 0 - nextX.CompareTo(nextY); //Intentionally reversed as we want the next occurrence to be top of the list
                    }
                }
                else
                {
                    return 1;
                }
            }
            else if (y.PastOccurrences.Any())
            {
                return -1;
            }

            return 0;
        }
    }
}
