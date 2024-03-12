using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorApp.Shared
{
    [TestClass]
    public class TargetTests
    {
        [TestMethod]
        public void Test_GetNextOccurrence_OncePerDay()
        {
            var target = new Target
            {
                Qty = 1,
                Frequency = TimeSpan.FromDays(1)
            };

            var pastOccurrences = new List<DateTime>
            {
                new DateTime(2024,2,26,12,0,0),
                new DateTime(2024,2,27,12,0,0),
            };

            DateTime now = new DateTime(2024, 2, 28, 12, 0, 0);
            var nextOccurrence = target.GetEarliestOccurrence(now, pastOccurrences);
            Assert.AreEqual(now, nextOccurrence);

            pastOccurrences.Add(nextOccurrence);
            now = new DateTime(2024, 2, 29, 12, 0, 0);
            nextOccurrence = target.GetEarliestOccurrence(now, pastOccurrences);
            Assert.AreEqual(now, nextOccurrence);

            pastOccurrences.Add(nextOccurrence);
            nextOccurrence = target.GetEarliestOccurrence(now, pastOccurrences);
            Assert.AreEqual(new DateTime(2024, 3, 1, 12, 0, 0), nextOccurrence);
        }

        [TestMethod]
        public void Test_Status_OncePerDay()
        {
            var target = new Target
            {
                Qty = 1,
                Frequency = TimeSpan.FromDays(1)
            };

            var pastOccurrences = new List<DateTime>
            {
                new DateTime(2024,2,26,12,0,0),
                new DateTime(2024,2,27,12,0,0),
            };

            Assert.AreEqual(StatusEnum.AtLimit, target.GetStatus(new DateTime(2024, 2, 28, 11, 59, 59), pastOccurrences));
            Assert.AreEqual(StatusEnum.Ok, target.GetStatus(new DateTime(2024, 2, 28, 12, 0, 0), pastOccurrences));

            DateTime now = new DateTime(2024, 2, 27, 12, 0, 0);
            pastOccurrences.Add(now);

            Assert.AreEqual(StatusEnum.OverLimit, target.GetStatus(new DateTime(2024, 2, 27, 12, 0, 0), pastOccurrences));
            Assert.AreEqual(StatusEnum.Ok, target.GetStatus(new DateTime(2024, 2, 28, 12, 0, 0), pastOccurrences));
        }

        [TestMethod]
        public void Test_GetNextOccurrence_TwicePerDay()
        {
            var target = new Target
            {
                Qty = 2,
                Frequency = TimeSpan.FromDays(1)
            };

            var pastOccurrences = new List<DateTime>
            {
                new DateTime(2024,2,26,12,0,0),
                new DateTime(2024,2,27,12,0,0),
            };

            DateTime now = new DateTime(2024, 2, 28, 12, 0, 0);
            var nextOccurrence = target.GetEarliestOccurrence(now, pastOccurrences);
            Assert.AreEqual(now, nextOccurrence);

            pastOccurrences.Add(nextOccurrence);
            nextOccurrence = target.GetEarliestOccurrence(now, pastOccurrences);
            Assert.AreEqual(now, nextOccurrence);

            pastOccurrences.Add(nextOccurrence);
            nextOccurrence = target.GetEarliestOccurrence(now, pastOccurrences);
            Assert.AreEqual(new DateTime(2024, 2, 29, 12, 0, 0), nextOccurrence);
        }

        [TestMethod]
        public void Test_GetNextOccurrence_FourTimesPerDay()
        {
            var target = new Target
            {
                Qty = 4,
                Frequency = TimeSpan.FromDays(1)
            };

            var pastOccurrences = new List<DateTime>();

            DateTime now = new DateTime(2024, 2, 28, 12, 0, 0);
            var nextOccurrence = target.GetEarliestOccurrence(now, pastOccurrences);
            Assert.AreEqual(now, nextOccurrence);

            pastOccurrences.Add(now);
            nextOccurrence = target.GetEarliestOccurrence(now, pastOccurrences);
            Assert.AreEqual(new DateTime(2024, 2, 28, 12, 0, 0), nextOccurrence);

            pastOccurrences.Add(now);
            nextOccurrence = target.GetEarliestOccurrence(now, pastOccurrences);
            Assert.AreEqual(new DateTime(2024, 2, 28, 12, 0, 0), nextOccurrence);

            pastOccurrences.Add(now);
            nextOccurrence = target.GetEarliestOccurrence(now, pastOccurrences);
            Assert.AreEqual(new DateTime(2024, 2, 28, 12, 0, 0), nextOccurrence);

            pastOccurrences.Add(now);
            nextOccurrence = target.GetEarliestOccurrence(now, pastOccurrences);
            Assert.AreEqual(new DateTime(2024, 2, 29, 12, 0, 0), nextOccurrence);
        }

        [TestMethod]
        public void Test_GetNextOccurrence_FourTimesPerDay_IsSpreadOut()
        {
            var target = new Target
            {
                Qty = 4,
                Frequency = TimeSpan.FromDays(1)
            };

            var pastOccurrences = new List<DateTime>();

            DateTime now = new DateTime(2024, 2, 28, 12, 0, 0);
            var nextOccurrence = target.GetSpacedOccurrence(now, pastOccurrences);
            Assert.AreEqual(now, nextOccurrence);

            pastOccurrences.Add(now);
            nextOccurrence = target.GetSpacedOccurrence(now, pastOccurrences);
            Assert.AreEqual(new DateTime(2024, 2, 28, 18, 0, 0), nextOccurrence);

            pastOccurrences.Add(now);
            nextOccurrence = target.GetSpacedOccurrence(now, pastOccurrences);
            Assert.AreEqual(new DateTime(2024, 2, 29, 0, 0, 0), nextOccurrence);

            pastOccurrences.Add(now);
            nextOccurrence = target.GetSpacedOccurrence(now, pastOccurrences);
            Assert.AreEqual(new DateTime(2024, 2, 29, 6, 0, 0), nextOccurrence);

            pastOccurrences.Add(now);
            nextOccurrence = target.GetSpacedOccurrence(now, pastOccurrences);
            Assert.AreEqual(new DateTime(2024, 2, 29, 12, 0, 0), nextOccurrence);
        }
    }
}