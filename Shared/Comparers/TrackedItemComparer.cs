using System;
using System.Collections.Generic;
using System.Linq;

namespace BlazorApp.Shared.Comparers
{
    public class TrackedItemComparerDescending(DateTime referenceTime) : IComparer<TrackedItem>
    {
        private readonly TrackedItemComparer _comparer = new(referenceTime);

        public int Compare(TrackedItem? x, TrackedItem? y)
        {
            return 0 - _comparer.Compare(x, y);
        }
    }

    public class TrackedItemComparer(DateTime referenceTime) : IComparer<TrackedItem>
    {
        public int Compare(TrackedItem? x, TrackedItem? y)
        {
            if (x == null && y == null) 
                return 0;

            if (x == null) 
                return -1;

            if (y == null) 
                return 1;

            if (x.Favourite && y.Favourite is false)
            {
                return 1;
            }
            else if (y.Favourite && x.Favourite is false)
            {
                return -1;
            }

            if (x.PastOccurrences.Count != 0)
            {
                if (y.PastOccurrences.Count != 0)
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
                        var nextX = x.GetFutureOccurrences(referenceTime, 1).DefaultIfEmpty(DateTime.MaxValue).First();
                        var nextY = y.GetFutureOccurrences(referenceTime, 1).DefaultIfEmpty(DateTime.MaxValue).First();

                        return 0 - nextX.CompareTo(nextY); //Intentionally reversed as we want the next occurrence to be top of the list
                    }
                }
                else
                {
                    return 1;
                }
            }
            else if (y.PastOccurrences.Count != 0)
            {
                return -1;
            }

            return 0;
        }
    }
}
