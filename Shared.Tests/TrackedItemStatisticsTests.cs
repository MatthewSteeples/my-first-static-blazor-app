using BlazorApp.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Shared.Tests
{
    [TestClass]
    public class TrackedItemStatisticsTests
    {
        [TestMethod]
        public void Test_GetDailyUsage_GroupsOccurrencesAndZeroFills()
        {
            var trackedItem = new TrackedItem
            {
                PastOccurrences =
                [
                    new Occurrence { ActualTimestamp = new DateTime(2024, 3, 1, 8, 0, 0, DateTimeKind.Utc), StockUsed = 1.5m },
                    new Occurrence { ActualTimestamp = new DateTime(2024, 3, 1, 18, 0, 0, DateTimeKind.Utc), StockUsed = 0.5m },
                    new Occurrence { ActualTimestamp = new DateTime(2024, 3, 3, 6, 0, 0, DateTimeKind.Utc) }
                ]
            };

            var points = TrackedItemStatistics.GetDailyUsage(trackedItem, new DateTime(2024, 3, 3, 12, 0, 0, DateTimeKind.Utc), 3, TimeZoneInfo.Utc);

            Assert.AreEqual(3, points.Count);

            Assert.AreEqual(new DateTime(2024, 3, 1), points[0].Date.Date);
            Assert.AreEqual(2, points[0].IncidentCount);
            Assert.AreEqual(2.0m, points[0].StockUsed);

            Assert.AreEqual(new DateTime(2024, 3, 2), points[1].Date.Date);
            Assert.AreEqual(0, points[1].IncidentCount);
            Assert.AreEqual(0m, points[1].StockUsed);

            Assert.AreEqual(new DateTime(2024, 3, 3), points[2].Date.Date);
            Assert.AreEqual(1, points[2].IncidentCount);
            Assert.AreEqual(0m, points[2].StockUsed);
        }

        [TestMethod]
        public void Test_GetDailyUsage_UsesRequestedTimeZone()
        {
            var trackedItem = new TrackedItem
            {
                PastOccurrences =
                [
                    new Occurrence { ActualTimestamp = new DateTime(2024, 3, 1, 23, 30, 0, DateTimeKind.Utc), StockUsed = 1m }
                ]
            };

            var timeZone = TimeZoneInfo.CreateCustomTimeZone("UtcPlusTen", TimeSpan.FromHours(10), "UtcPlusTen", "UtcPlusTen");

            var points = TrackedItemStatistics.GetDailyUsage(trackedItem, new DateTime(2024, 3, 2, 12, 0, 0, DateTimeKind.Utc), 2, timeZone);

            Assert.AreEqual(new DateTime(2024, 3, 1), points[0].Date.Date);
            Assert.AreEqual(0, points[0].IncidentCount);

            Assert.AreEqual(new DateTime(2024, 3, 2), points[1].Date.Date);
            Assert.AreEqual(1, points[1].IncidentCount);
            Assert.AreEqual(1m, points[1].StockUsed);
        }

        [TestMethod]
        public void Test_HasStockTracking_DetectsConfiguredAndRecordedStockUsage()
        {
            var trackedWithDefault = new TrackedItem
            {
                DefaultStockUsage = 1m
            };

            var trackedWithOccurrence = new TrackedItem
            {
                PastOccurrences =
                [
                    new Occurrence { StockUsed = 0.5m }
                ]
            };

            var trackedWithAcquisition = new TrackedItem
            {
                StockAcquisitions =
                [
                    new StockAcquisition { DateAcquired = DateTime.UtcNow, Quantity = 8m, Note = "Refill" }
                ]
            };

            Assert.IsTrue(TrackedItemStatistics.HasStockTracking(trackedWithDefault));
            Assert.IsTrue(TrackedItemStatistics.HasStockTracking(trackedWithOccurrence));
            Assert.IsTrue(TrackedItemStatistics.HasStockTracking(trackedWithAcquisition));
            Assert.IsFalse(TrackedItemStatistics.HasStockTracking(new TrackedItem()));
        }

        [TestMethod]
        public void Test_GetUsageSummary_GroupsByWeekUsingRequestedWeekStart()
        {
            var points = new[]
            {
                new DailyUsagePoint { Date = new DateTime(2024, 3, 1), IncidentCount = 1, StockUsed = 0.5m },
                new DailyUsagePoint { Date = new DateTime(2024, 3, 2), IncidentCount = 2, StockUsed = 1.0m },
                new DailyUsagePoint { Date = new DateTime(2024, 3, 3), IncidentCount = 0, StockUsed = 0m },
                new DailyUsagePoint { Date = new DateTime(2024, 3, 4), IncidentCount = 3, StockUsed = 1.5m },
                new DailyUsagePoint { Date = new DateTime(2024, 3, 5), IncidentCount = 4, StockUsed = 2.0m }
            };

            var summaries = TrackedItemStatistics.GetUsageSummary(points, UsageGroupingPeriod.Week, DayOfWeek.Monday);

            Assert.AreEqual(2, summaries.Count);

            Assert.AreEqual(new DateTime(2024, 2, 26), summaries[0].BucketStart);
            Assert.AreEqual(new DateTime(2024, 3, 1), summaries[0].PeriodStart);
            Assert.AreEqual(new DateTime(2024, 3, 3), summaries[0].PeriodEnd);
            Assert.AreEqual(3, summaries[0].IncidentCount);
            Assert.AreEqual(1.5m, summaries[0].StockUsed);

            Assert.AreEqual(new DateTime(2024, 3, 4), summaries[1].BucketStart);
            Assert.AreEqual(new DateTime(2024, 3, 4), summaries[1].PeriodStart);
            Assert.AreEqual(new DateTime(2024, 3, 5), summaries[1].PeriodEnd);
            Assert.AreEqual(7, summaries[1].IncidentCount);
            Assert.AreEqual(3.5m, summaries[1].StockUsed);
        }

        [TestMethod]
        public void Test_GetUsageSummary_GroupsByMonth()
        {
            var points = new[]
            {
                new DailyUsagePoint { Date = new DateTime(2024, 2, 28), IncidentCount = 1, StockUsed = 0.25m },
                new DailyUsagePoint { Date = new DateTime(2024, 2, 29), IncidentCount = 2, StockUsed = 0.75m },
                new DailyUsagePoint { Date = new DateTime(2024, 3, 1), IncidentCount = 3, StockUsed = 1.50m },
                new DailyUsagePoint { Date = new DateTime(2024, 3, 2), IncidentCount = 4, StockUsed = 2.25m }
            };

            var summaries = TrackedItemStatistics.GetUsageSummary(points, UsageGroupingPeriod.Month, DayOfWeek.Sunday);

            Assert.AreEqual(2, summaries.Count);

            Assert.AreEqual(new DateTime(2024, 2, 1), summaries[0].BucketStart);
            Assert.AreEqual(new DateTime(2024, 2, 28), summaries[0].PeriodStart);
            Assert.AreEqual(new DateTime(2024, 2, 29), summaries[0].PeriodEnd);
            Assert.AreEqual(3, summaries[0].IncidentCount);
            Assert.AreEqual(1.0m, summaries[0].StockUsed);

            Assert.AreEqual(new DateTime(2024, 3, 1), summaries[1].BucketStart);
            Assert.AreEqual(new DateTime(2024, 3, 1), summaries[1].PeriodStart);
            Assert.AreEqual(new DateTime(2024, 3, 2), summaries[1].PeriodEnd);
            Assert.AreEqual(7, summaries[1].IncidentCount);
            Assert.AreEqual(3.75m, summaries[1].StockUsed);
        }
    }
}
