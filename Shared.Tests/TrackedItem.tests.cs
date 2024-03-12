using BlazorApp.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Tests
{
    [TestClass]
    public class TrackedItemTests
    {
        [TestMethod]
        public void Test_MultipleTargets()
        {
            var trackedItem = new TrackedItem()
            {
                Name = "Tablet",
                Targets = new List<Target>
                {
                    new Target
                    {
                        Qty = 4,
                        Frequency = TimeSpan.FromDays(1)
                    },
                    new Target
                    {
                        Qty = 1,
                        Frequency = TimeSpan.FromHours(4)
                    },
                },
                PastOccurrences = [],
            };

            var now = new DateTime(2024, 2, 28, 12, 0, 0);
            var futureOccurrences = trackedItem.GetFutureOccurrences(now, 5).ToList();

            Assert.AreEqual(5, futureOccurrences.Count);
            Assert.AreEqual(new DateTime(2024, 2, 28, 12, 0, 0), futureOccurrences[0]);
            Assert.AreEqual(new DateTime(2024, 2, 28, 16, 0, 0), futureOccurrences[1]);
            Assert.AreEqual(new DateTime(2024, 2, 28, 20, 0, 0), futureOccurrences[2]);
            Assert.AreEqual(new DateTime(2024, 2, 29, 0, 0, 0), futureOccurrences[3]);
            Assert.AreEqual(new DateTime(2024, 2, 29, 12, 0, 0), futureOccurrences[4]);
        }

        [TestMethod]
        public void Test_SingleTargetSpacesOut()
        {
            var trackedItem = new TrackedItem()
            {
                Name = "Tablet",
                Targets = new List<Target>
                {
                    new Target
                    {
                        Qty = 4,
                        Frequency = TimeSpan.FromDays(1)
                    },
                },
                PastOccurrences = [],
            };

            var now = new DateTime(2024, 2, 28, 12, 0, 0);
            var futureOccurrences = trackedItem.GetFutureOccurrences(now, 5).ToList();

            Assert.AreEqual(5, futureOccurrences.Count);
            Assert.AreEqual(new DateTime(2024, 2, 28, 12, 0, 0), futureOccurrences[0]);
            Assert.AreEqual(new DateTime(2024, 2, 28, 18, 0, 0), futureOccurrences[1]);
            Assert.AreEqual(new DateTime(2024, 2, 29, 0, 0, 0), futureOccurrences[2]);
            Assert.AreEqual(new DateTime(2024, 2, 29, 6, 0, 0), futureOccurrences[3]);
            Assert.AreEqual(new DateTime(2024, 2, 29, 12, 0, 0), futureOccurrences[4]);
        }

        [TestMethod]
        public void Test_SingleTarget_Status()
        {
            var trackedItem = new TrackedItem()
            {
                Name = "Tablet",
                Targets = new List<Target>
                {
                    new Target
                    {
                        Qty = 4,
                        Frequency = TimeSpan.FromDays(1)
                    },
                },
                PastOccurrences = [],
            };

            var now = new DateTime(2024, 2, 28, 12, 0, 0);

            Assert.AreEqual(StatusEnum.Ok, trackedItem.GetStatus(now));
            trackedItem.AddOccurrence(now);
            Assert.AreEqual(StatusEnum.Ok, trackedItem.GetStatus(now));
            trackedItem.AddOccurrence(now);
            Assert.AreEqual(StatusEnum.Ok, trackedItem.GetStatus(now));
            trackedItem.AddOccurrence(now);
            Assert.AreEqual(StatusEnum.Ok, trackedItem.GetStatus(now));
            trackedItem.AddOccurrence(now);
            Assert.AreEqual(StatusEnum.AtLimit, trackedItem.GetStatus(now));
            trackedItem.AddOccurrence(now);
            Assert.AreEqual(StatusEnum.OverLimit, trackedItem.GetStatus(now));
            trackedItem.AddOccurrence(now);
        }

        [TestMethod]
        public void Test_CanAddOccurrence()
        {
            var trackedItem = new TrackedItem()
            {
                Name = "Tablet",
                Targets = new List<Target>
                {
                    new Target
                    {
                        Qty = 4,
                        Frequency = TimeSpan.FromDays(1)
                    },
                    new Target
                    {
                        Qty = 1,
                        Frequency = TimeSpan.FromHours(4)
                    },
                },
                PastOccurrences = [],
            };

            trackedItem.AddOccurrence(new DateTime(2024, 2, 28, 12, 0, 0));
            Assert.AreEqual(new DateTime(2024, 2, 28, 12, 0, 0), trackedItem.PastOccurrences.ElementAt(0).ActualTimestamp);
            Assert.AreEqual(new DateTime(2024, 2, 28, 12, 0, 0), trackedItem.PastOccurrences.ElementAt(0).SafetyTimestamp);

            trackedItem.AddOccurrence(new DateTime(2024, 2, 28, 16, 0, 0));
            Assert.AreEqual(new DateTime(2024, 2, 28, 16, 0, 0), trackedItem.PastOccurrences.ElementAt(1).ActualTimestamp);
            Assert.AreEqual(new DateTime(2024, 2, 28, 16, 0, 0), trackedItem.PastOccurrences.ElementAt(1).SafetyTimestamp);
            
            trackedItem.AddOccurrence(new DateTime(2024, 2, 28, 19, 0, 0));
            Assert.AreEqual(new DateTime(2024, 2, 28, 19, 0, 0), trackedItem.PastOccurrences.ElementAt(2).ActualTimestamp);
            Assert.AreEqual(new DateTime(2024, 2, 28, 20, 0, 0), trackedItem.PastOccurrences.ElementAt(2).SafetyTimestamp);
            
            trackedItem.AddOccurrence(new DateTime(2024, 2, 29, 2, 0, 0));
            Assert.AreEqual(new DateTime(2024, 2, 29, 2, 0, 0), trackedItem.PastOccurrences.ElementAt(3).ActualTimestamp);
            Assert.AreEqual(new DateTime(2024, 2, 29, 2, 0, 0), trackedItem.PastOccurrences.ElementAt(3).SafetyTimestamp);

            trackedItem.AddOccurrence(new DateTime(2024, 2, 29, 11, 0, 0));
            Assert.AreEqual(new DateTime(2024, 2, 29, 11, 0, 0), trackedItem.PastOccurrences.ElementAt(4).ActualTimestamp);
            Assert.AreEqual(new DateTime(2024, 2, 29, 12, 0, 0), trackedItem.PastOccurrences.ElementAt(4).SafetyTimestamp);
            
            trackedItem.AddOccurrence(new DateTime(2024, 2, 29, 15, 0, 0));
            Assert.AreEqual(new DateTime(2024, 2, 29, 15, 0, 0), trackedItem.PastOccurrences.ElementAt(5).ActualTimestamp);
            Assert.AreEqual(new DateTime(2024, 2, 29, 16, 0, 0), trackedItem.PastOccurrences.ElementAt(5).SafetyTimestamp);
            
            trackedItem.AddOccurrence(new DateTime(2024, 2, 29, 21, 0, 0));
            Assert.AreEqual(new DateTime(2024, 2, 29, 21, 0, 0), trackedItem.PastOccurrences.ElementAt(6).ActualTimestamp);
            Assert.AreEqual(new DateTime(2024, 2, 29, 21, 0, 0), trackedItem.PastOccurrences.ElementAt(6).SafetyTimestamp);
            
            trackedItem.AddOccurrence(new DateTime(2024, 3, 1, 3, 0, 0));
            Assert.AreEqual(new DateTime(2024, 3, 1, 3, 0, 0), trackedItem.PastOccurrences.ElementAt(7).ActualTimestamp);
            Assert.AreEqual(new DateTime(2024, 3, 1, 3, 0, 0), trackedItem.PastOccurrences.ElementAt(7).SafetyTimestamp);

            
            trackedItem.AddOccurrence(new DateTime(2024, 3, 1, 7, 0, 0));
            Assert.AreEqual(new DateTime(2024, 3, 1, 7, 0, 0), trackedItem.PastOccurrences.ElementAt(8).ActualTimestamp);
            Assert.AreEqual(new DateTime(2024, 3, 1, 12, 0, 0), trackedItem.PastOccurrences.ElementAt(8).SafetyTimestamp);
            
            trackedItem.AddOccurrence(new DateTime(2024, 3, 1, 11, 0, 0));
            Assert.AreEqual(new DateTime(2024, 3, 1, 11, 0, 0), trackedItem.PastOccurrences.ElementAt(9).ActualTimestamp);
            Assert.AreEqual(new DateTime(2024, 3, 1, 16, 0, 0), trackedItem.PastOccurrences.ElementAt(9).SafetyTimestamp);


            Assert.AreEqual(10, trackedItem.PastOccurrences.Count);
        }

        [TestMethod]
        public void Test_CanAddOccurrence_OutOfOrder()
        {
            var trackedItem = new TrackedItem()
            {
                Name = "Tablet",
                Targets = new List<Target>
                {
                    new Target
                    {
                        Qty = 4,
                        Frequency = TimeSpan.FromDays(1)
                    },
                    new Target
                    {
                        Qty = 1,
                        Frequency = TimeSpan.FromHours(4)
                    },
                },
                PastOccurrences = [],
            };

            trackedItem.AddOccurrence(new DateTime(2024, 2, 28, 12, 0, 0));
            Assert.AreEqual(new DateTime(2024, 2, 28, 12, 0, 0), trackedItem.PastOccurrences.ElementAt(0).ActualTimestamp);
            Assert.AreEqual(new DateTime(2024, 2, 28, 12, 0, 0), trackedItem.PastOccurrences.ElementAt(0).SafetyTimestamp);

            trackedItem.AddOccurrence(new DateTime(2024, 2, 28, 16, 0, 0));
            Assert.AreEqual(new DateTime(2024, 2, 28, 16, 0, 0), trackedItem.PastOccurrences.ElementAt(1).ActualTimestamp);
            Assert.AreEqual(new DateTime(2024, 2, 28, 16, 0, 0), trackedItem.PastOccurrences.ElementAt(1).SafetyTimestamp);

            trackedItem.AddOccurrence(new DateTime(2024, 2, 29, 2, 0, 0));
            Assert.AreEqual(new DateTime(2024, 2, 29, 2, 0, 0), trackedItem.PastOccurrences.ElementAt(2).ActualTimestamp);
            Assert.AreEqual(new DateTime(2024, 2, 29, 2, 0, 0), trackedItem.PastOccurrences.ElementAt(2).SafetyTimestamp);

            trackedItem.AddOccurrence(new DateTime(2024, 2, 28, 23, 0, 0));
            Assert.AreEqual(new DateTime(2024, 2, 28, 23, 0, 0), trackedItem.PastOccurrences.ElementAt(3).ActualTimestamp);
            Assert.AreEqual(new DateTime(2024, 2, 28, 23, 0, 0), trackedItem.PastOccurrences.ElementAt(3).SafetyTimestamp);

            //Check that previous elements have been recalculated
            Assert.AreEqual(new DateTime(2024, 2, 29, 2, 0, 0), trackedItem.PastOccurrences.ElementAt(2).ActualTimestamp);
            Assert.AreEqual(new DateTime(2024, 2, 29, 3, 0, 0), trackedItem.PastOccurrences.ElementAt(2).SafetyTimestamp);


            Assert.AreEqual(4, trackedItem.PastOccurrences.Count);
        }
    }
}
