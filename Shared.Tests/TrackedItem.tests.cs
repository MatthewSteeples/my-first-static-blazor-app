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
                Targets =
                [
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
                ],
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
        public void Test_MultipleTargets_Spaced()
        {
            var trackedItem = new TrackedItem()
            {
                Name = "Tablet",
                Targets =
                [
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
                ],
                PastOccurrences = [],
            };

            var now = new DateTime(2024, 2, 28, 12, 0, 0);
            var futureOccurrences = trackedItem.GetFutureSpacedOccurrences(now, 5).ToList();

            Assert.AreEqual(5, futureOccurrences.Count);
            Assert.AreEqual(new DateTime(2024, 2, 28, 12, 0, 0), futureOccurrences[0]);
            Assert.AreEqual(new DateTime(2024, 2, 28, 18, 0, 0), futureOccurrences[1]);
            Assert.AreEqual(new DateTime(2024, 2, 29, 0, 0, 0), futureOccurrences[2]);
            Assert.AreEqual(new DateTime(2024, 2, 29, 6, 0, 0), futureOccurrences[3]);
            Assert.AreEqual(new DateTime(2024, 2, 29, 12, 0, 0), futureOccurrences[4]);
        }

        [TestMethod]
        public void Test_MultipleTargets_WithStartingValues()
        {
            var trackedItem = new TrackedItem()
            {
                Name = "Tablet",
                Targets =
                [
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
                ],
                PastOccurrences = [],
            };

            var now = new DateTime(2024, 2, 28, 12, 0, 0);

            trackedItem.AddOccurrence(new DateTime(2024, 2, 28, 4, 0, 0));
            trackedItem.AddOccurrence(new DateTime(2024, 2, 28, 8, 0, 0));

            var futureOccurrences = trackedItem.GetFutureOccurrences(now, 5).ToList();

            Assert.AreEqual(5, futureOccurrences.Count);
            Assert.AreEqual(new DateTime(2024, 2, 28, 12, 0, 0), futureOccurrences[0]);
            Assert.AreEqual(new DateTime(2024, 2, 28, 16, 0, 0), futureOccurrences[1]);
            Assert.AreEqual(new DateTime(2024, 2, 29, 4, 0, 0), futureOccurrences[2]);
            Assert.AreEqual(new DateTime(2024, 2, 29, 8, 0, 0), futureOccurrences[3]);
            Assert.AreEqual(new DateTime(2024, 2, 29, 12, 0, 0), futureOccurrences[4]);
        }

        [TestMethod]
        public void Test_MultipleTargets_Spaced_WithStartingValues()
        {
            var trackedItem = new TrackedItem()
            {
                Name = "Tablet",
                Targets =
                [
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
                ],
                PastOccurrences = [],
            };

            var now = new DateTime(2024, 2, 28, 12, 0, 0);

            trackedItem.AddOccurrence(new DateTime(2024, 2, 28, 4, 0, 0));
            trackedItem.AddOccurrence(new DateTime(2024, 2, 28, 8, 0, 0));

            var futureOccurrences = trackedItem.GetFutureSpacedOccurrences(now, 5).ToList();

            Assert.AreEqual(5, futureOccurrences.Count);
            Assert.AreEqual(new DateTime(2024, 2, 28, 16, 0, 0), futureOccurrences[0]);
            Assert.AreEqual(new DateTime(2024, 2, 28, 22, 0, 0), futureOccurrences[1]);
            Assert.AreEqual(new DateTime(2024, 2, 29, 4, 0, 0), futureOccurrences[2]);
            Assert.AreEqual(new DateTime(2024, 2, 29, 8, 0, 0), futureOccurrences[3]);
            Assert.AreEqual(new DateTime(2024, 2, 29, 16, 0, 0), futureOccurrences[4]);
        }

        [TestMethod]
        public void Test_MultipleTargets_Spaced_WithStartingValues2()
        {
            var trackedItem = new TrackedItem()
            {
                Name = "Tablet",
                Targets =
                [
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
                ],
                PastOccurrences = [],
            };

            var now = new DateTime(2024, 2, 28, 13, 0, 0);

            trackedItem.AddOccurrence(new DateTime(2024, 2, 27, 18, 0, 0));
            trackedItem.AddOccurrence(new DateTime(2024, 2, 28, 12, 0, 0));

            var futureOccurrences = trackedItem.GetFutureSpacedOccurrences(now, 1).ToList();

            Assert.AreEqual(1, futureOccurrences.Count);
            Assert.AreEqual(new DateTime(2024, 2, 28, 16, 0, 0), futureOccurrences[0]);
        }

        [TestMethod]
        public void Test_SingleTargetSpacesOut()
        {
            var trackedItem = new TrackedItem()
            {
                Name = "Tablet",
                Targets =
                [
                    new Target
                    {
                        Qty = 4,
                        Frequency = TimeSpan.FromDays(1)
                    },
                ],
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
                Targets =
                [
                    new Target
                    {
                        Qty = 4,
                        Frequency = TimeSpan.FromDays(1)
                    },
                ],
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
                Targets =
                [
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
                ],
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
                Targets =
                [
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
                ],
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

        [TestMethod]
        public void Test_CheckForArchiving_ReturnsNullWhenUnder200()
        {
            var trackedItem = new TrackedItem()
            {
                Id = Guid.NewGuid(),
                Name = "Test Item",
                PastOccurrences = []
            };

            // Add 199 occurrences
            for (int i = 0; i < 199; i++)
            {
                trackedItem.PastOccurrences.Add(new Occurrence 
                { 
                    ActualTimestamp = DateTime.UtcNow.AddDays(-i), 
                    SafetyTimestamp = DateTime.UtcNow.AddDays(-i) 
                });
            }

            var archive = trackedItem.CheckForArchiving();
            Assert.IsNull(archive);
            Assert.AreEqual(199, trackedItem.PastOccurrences.Count);
        }

        [TestMethod]
        public void Test_CheckForArchiving_ArchivesOldest100WhenOver200()
        {
            var trackedItem = new TrackedItem()
            {
                Id = Guid.NewGuid(),
                Name = "Test Item",
                PastOccurrences = []
            };

            // Add 250 occurrences (oldest first)
            var baseDate = new DateTime(2024, 1, 1);
            for (int i = 0; i < 250; i++)
            {
                trackedItem.PastOccurrences.Add(new Occurrence 
                { 
                    ActualTimestamp = baseDate.AddDays(i), 
                    SafetyTimestamp = baseDate.AddDays(i) 
                });
            }

            var archive = trackedItem.CheckForArchiving();
            
            // Should return an archive with the oldest 100 occurrences
            Assert.IsNotNull(archive);
            Assert.AreEqual(trackedItem.Id, archive.TrackedItemId);
            Assert.AreEqual(100, archive.ArchivedOccurrences.Count);
            
            // Archived occurrences should be the oldest ones (first 100)
            Assert.AreEqual(baseDate, archive.ArchivedOccurrences[0].ActualTimestamp);
            Assert.AreEqual(baseDate.AddDays(99), archive.ArchivedOccurrences[99].ActualTimestamp);
            
            // TrackedItem should have the newest 150 occurrences
            Assert.AreEqual(150, trackedItem.PastOccurrences.Count);
            Assert.AreEqual(baseDate.AddDays(100), trackedItem.PastOccurrences.Min(o => o.ActualTimestamp));
            Assert.AreEqual(baseDate.AddDays(249), trackedItem.PastOccurrences.Max(o => o.ActualTimestamp));
        }

        [TestMethod]
        public void Test_CheckForArchiving_ExactlyAt200()
        {
            var trackedItem = new TrackedItem()
            {
                Id = Guid.NewGuid(),
                Name = "Test Item",
                PastOccurrences = []
            };

            // Add exactly 200 occurrences
            var baseDate = new DateTime(2024, 1, 1);
            for (int i = 0; i < 200; i++)
            {
                trackedItem.PastOccurrences.Add(new Occurrence 
                { 
                    ActualTimestamp = baseDate.AddDays(i), 
                    SafetyTimestamp = baseDate.AddDays(i) 
                });
            }

            var archive = trackedItem.CheckForArchiving();
            Assert.IsNull(archive);
            Assert.AreEqual(200, trackedItem.PastOccurrences.Count);
        }

        [TestMethod]
        public void Test_CheckForArchiving_ExactlyAt201()
        {
            var trackedItem = new TrackedItem()
            {
                Id = Guid.NewGuid(),
                Name = "Test Item",
                PastOccurrences = []
            };

            // Add exactly 201 occurrences
            var baseDate = new DateTime(2024, 1, 1);
            for (int i = 0; i < 201; i++)
            {
                trackedItem.PastOccurrences.Add(new Occurrence 
                { 
                    ActualTimestamp = baseDate.AddDays(i), 
                    SafetyTimestamp = baseDate.AddDays(i) 
                });
            }

            var archive = trackedItem.CheckForArchiving();
            
            // Should return an archive with the oldest 100 occurrences
            Assert.IsNotNull(archive);
            Assert.AreEqual(100, archive.ArchivedOccurrences.Count);
            Assert.AreEqual(101, trackedItem.PastOccurrences.Count);
        }

        [TestMethod]
        public void Test_ArchiveIntegration_AddOccurrenceTriggersArchiving()
        {
            var trackedItem = new TrackedItem()
            {
                Id = Guid.NewGuid(),
                Name = "Integration Test Item",
                PastOccurrences = []
            };

            // Add 200 occurrences to get to the threshold
            var baseDate = new DateTime(2024, 1, 1);
            for (int i = 0; i < 200; i++)
            {
                trackedItem.PastOccurrences.Add(new Occurrence 
                { 
                    ActualTimestamp = baseDate.AddDays(i), 
                    SafetyTimestamp = baseDate.AddDays(i) 
                });
            }

            // Verify we're at exactly 200 and no archiving happens yet
            var archiveBefore = trackedItem.CheckForArchiving();
            Assert.IsNull(archiveBefore);
            Assert.AreEqual(200, trackedItem.PastOccurrences.Count);

            // Add one more occurrence to trigger archiving
            trackedItem.AddOccurrence(baseDate.AddDays(200));

            // Now archiving should be needed
            var archiveAfter = trackedItem.CheckForArchiving();
            Assert.IsNotNull(archiveAfter);
            Assert.AreEqual(100, archiveAfter.ArchivedOccurrences.Count);
            Assert.AreEqual(101, trackedItem.PastOccurrences.Count);

            // Verify the archived occurrences are the oldest ones
            Assert.AreEqual(baseDate, archiveAfter.ArchivedOccurrences[0].ActualTimestamp);
            Assert.AreEqual(baseDate.AddDays(99), archiveAfter.ArchivedOccurrences[99].ActualTimestamp);

            // Verify the remaining occurrences are the newest ones
            Assert.AreEqual(baseDate.AddDays(100), trackedItem.PastOccurrences.Min(o => o.ActualTimestamp));
            Assert.AreEqual(baseDate.AddDays(200), trackedItem.PastOccurrences.Max(o => o.ActualTimestamp));
        }
    }
}
