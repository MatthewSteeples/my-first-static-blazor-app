using System;
using System.Collections.Generic;
using System.Linq;

namespace BlazorApp.Shared
{
    public static class TrackedItemStatistics
    {
        public static List<DailyUsagePoint> GetDailyUsage(TrackedItem item, DateTime utcNow, int days, TimeZoneInfo timeZone)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (timeZone == null)
                throw new ArgumentNullException(nameof(timeZone));

            if (days <= 0)
                throw new ArgumentOutOfRangeException(nameof(days));

            var endDate = ConvertFromUtc(utcNow, timeZone).Date;
            var startDate = endDate.AddDays(-(days - 1));

            var points = Enumerable.Range(0, days)
                .Select(offset => new DailyUsagePoint { Date = startDate.AddDays(offset) })
                .ToDictionary(point => point.Date);

            foreach (var occurrence in item.PastOccurrences ?? [])
            {
                var occurrenceDate = ConvertFromUtc(occurrence.ActualTimestamp, timeZone).Date;

                if (occurrenceDate < startDate || occurrenceDate > endDate)
                    continue;

                var point = points[occurrenceDate];
                point.IncidentCount += 1;
                point.StockUsed += occurrence.StockUsed ?? 0;
            }

            return points.Values
                .OrderBy(point => point.Date)
                .ToList();
        }

        public static List<UsageSummaryPoint> GetUsageSummary(IEnumerable<DailyUsagePoint> points, UsageGroupingPeriod period, DayOfWeek firstDayOfWeek)
        {
            if (points == null)
                throw new ArgumentNullException(nameof(points));

            if (!Enum.IsDefined(period))
                throw new ArgumentOutOfRangeException(nameof(period));

            if (!Enum.IsDefined(firstDayOfWeek))
                throw new ArgumentOutOfRangeException(nameof(firstDayOfWeek));

            return points
                .GroupBy(point => GetBucketStart(point.Date.Date, period, firstDayOfWeek))
                .Select(group => new UsageSummaryPoint
                {
                    BucketStart = group.Key,
                    PeriodStart = group.Min(point => point.Date.Date),
                    PeriodEnd = group.Max(point => point.Date.Date),
                    IncidentCount = group.Sum(point => point.IncidentCount),
                    StockUsed = group.Sum(point => point.StockUsed)
                })
                .OrderBy(point => point.BucketStart)
                .ToList();
        }

        public static bool HasStockTracking(TrackedItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            return item.DefaultStockUsage.GetValueOrDefault() > 0
                || (item.StockAcquisitions?.Any() ?? false)
                || (item.PastOccurrences?.Any(occurrence => occurrence.StockUsed.HasValue) ?? false);
        }

        private static DateTime GetBucketStart(DateTime date, UsageGroupingPeriod period, DayOfWeek firstDayOfWeek)
        {
            return period switch
            {
                UsageGroupingPeriod.Day => date,
                UsageGroupingPeriod.Week => date.AddDays(-((7 + (date.DayOfWeek - firstDayOfWeek)) % 7)),
                UsageGroupingPeriod.Month => new DateTime(date.Year, date.Month, 1),
                _ => throw new ArgumentOutOfRangeException(nameof(period))
            };
        }

        private static DateTime ConvertFromUtc(DateTime value, TimeZoneInfo timeZone)
        {
            var utcValue = value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
            };

            return TimeZoneInfo.ConvertTimeFromUtc(utcValue, timeZone);
        }
    }

    public enum UsageGroupingPeriod
    {
        Day,
        Week,
        Month
    }

    public class DailyUsagePoint
    {
        public DateTime Date { get; set; }
        public int IncidentCount { get; set; }
        public decimal StockUsed { get; set; }
    }

    public class UsageSummaryPoint
    {
        public DateTime BucketStart { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int IncidentCount { get; set; }
        public decimal StockUsed { get; set; }
    }
}
