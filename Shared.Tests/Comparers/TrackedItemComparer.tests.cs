using BlazorApp.Shared;
using BlazorApp.Shared.Comparers;

namespace Shared.Tests.Comparers
{
    [TestClass]
    public class TrackedItemComparerTests
    {
        private static readonly DateTime ReferenceTime = new(2024, 2, 28, 12, 0, 0, DateTimeKind.Utc);

        [TestMethod]
        public void Test_TrackedIdenticalInstances_AreEqual()
        {
            var x = new TrackedItem();

            var comparer = new TrackedItemComparer(ReferenceTime);

            Assert.AreEqual(0, comparer.Compare(x, x));
        }

        [TestMethod]
        public void Test_TrackedNullInstances_AreEqual()
        {
            var comparer = new TrackedItemComparer(ReferenceTime);

            Assert.AreEqual(0, comparer.Compare(null, null));
        }

        [TestMethod]
        public void Test_TrackedOneNullInstance_AreNotEqual()
        {
            var comparer = new TrackedItemComparer(ReferenceTime);

            Assert.AreEqual(-1, comparer.Compare(null, new TrackedItem()));
            Assert.AreEqual(1, comparer.Compare(new TrackedItem(), null));
        }

        [TestMethod]
        public void Test_TrackedItemsWithNoOccurrences_AreEqual()
        {
            var x = new TrackedItem();
            var y = new TrackedItem();

            var comparer = new TrackedItemComparer(ReferenceTime);

            Assert.AreEqual(0, comparer.Compare(x, y));
            Assert.AreEqual(0, comparer.Compare(y, x));
        }

        [TestMethod]
        public void Test_TrackedItemsWithSomeOccurrences_AreDifferent()
        {
            var x = new TrackedItem();
            x.AddOccurrence(ReferenceTime);

            var y = new TrackedItem();

            var comparer = new TrackedItemComparer(ReferenceTime);

            Assert.AreEqual(1, comparer.Compare(x, y));
            Assert.AreEqual(-1, comparer.Compare(y, x));
        }

        [TestMethod]
        public void Test_TrackedItemsWithSameOccurrences_AreEqual()
        {
            var x = new TrackedItem();
            x.AddOccurrence(ReferenceTime);

            var y = new TrackedItem();
            y.AddOccurrence(ReferenceTime);

            var comparer = new TrackedItemComparer(ReferenceTime);

            Assert.AreEqual(0, comparer.Compare(x, y));
            Assert.AreEqual(0, comparer.Compare(y, x));
        }

        [TestMethod]
        public void Test_TrackedItemsWithSingleDifferentOccurrences_OnSameDay_AreEqual()
        {
            var x = new TrackedItem();
            x.AddOccurrence(ReferenceTime);

            var y = new TrackedItem();
            y.AddOccurrence(ReferenceTime.AddMinutes(5));

            var comparer = new TrackedItemComparer(ReferenceTime);

            Assert.AreEqual(0, comparer.Compare(x, y));
            Assert.AreEqual(0, comparer.Compare(y, x));
        }

        [TestMethod]
        public void Test_TrackedItemsWithSingleDifferentOccurrences_OnDifferentDays_AreEqual()
        {
            var x = new TrackedItem();
            x.AddOccurrence(ReferenceTime);

            var y = new TrackedItem();
            y.AddOccurrence(ReferenceTime.AddDays(2));

            var comparer = new TrackedItemComparer(ReferenceTime);

            Assert.AreEqual(-1, comparer.Compare(x, y));
            Assert.AreEqual(1, comparer.Compare(y, x));
        }

        [TestMethod]
        public void Test_TrackedItemsWithMultipleDifferentOccurrences_OnDifferentDays_AreEqual()
        {
            var x = new TrackedItem();
            x.AddOccurrence(ReferenceTime);
            x.AddOccurrence(ReferenceTime.AddDays(2));

            var y = new TrackedItem();
            y.AddOccurrence(ReferenceTime);
            y.AddOccurrence(ReferenceTime.AddDays(2));

            var comparer = new TrackedItemComparer(ReferenceTime);

            Assert.AreEqual(0, comparer.Compare(x, y));
            Assert.AreEqual(0, comparer.Compare(y, x));
        }

        [TestMethod]
        public void Test_TrackedItemsWithSameOccurrences_AndDifferentFutureOccurrences_AreNotEqual()
        {
            var x = new TrackedItem() { Targets = [new() { Frequency = TimeSpan.FromHours(4), Qty = 1 }] };
            x.AddOccurrence(ReferenceTime);

            var y = new TrackedItem();
            y.AddOccurrence(ReferenceTime);

            var comparer = new TrackedItemComparer(ReferenceTime);

            Assert.AreEqual(1, comparer.Compare(x, y));
            Assert.AreEqual(-1, comparer.Compare(y, x));
        }

        [TestMethod]
        public void Test_TrackedItemsWithSameOccurrences_AndDifferentFutureOccurrences2_AreNotEqual()
        {
            var x = new TrackedItem() { Targets = [new() { Frequency = TimeSpan.FromHours(4), Qty = 1 }] };
            x.AddOccurrence(ReferenceTime);

            var y = new TrackedItem() { Targets = [new() { Frequency = TimeSpan.FromHours(6), Qty = 1 }] };
            y.AddOccurrence(ReferenceTime);

            var comparer = new TrackedItemComparer(ReferenceTime);

            Assert.AreEqual(1, comparer.Compare(x, y));
            Assert.AreEqual(-1, comparer.Compare(y, x));
        }

        [TestMethod]
        public void Test_TrackedItemsSorting_Scenario1()
        {
            var now = ReferenceTime;

            var a = new TrackedItem() { Targets = [new() { Frequency = TimeSpan.FromHours(4), Qty = 1 }, new() { Frequency = TimeSpan.FromDays(1), Qty = 4 }] };
            a.AddOccurrence(now);
            a.AddOccurrence(now.AddHours(-4));
            a.AddOccurrence(now.AddHours(-8));
            a.AddOccurrence(now.AddHours(-12));

            var b = new TrackedItem() { Targets = [new() { Frequency = TimeSpan.FromHours(4), Qty = 1 }, new() { Frequency = TimeSpan.FromDays(1), Qty = 4 }] };
            b.AddOccurrence(now);
            b.AddOccurrence(now.AddHours(-4));
            b.AddOccurrence(now.AddHours(-8));

            var c = new TrackedItem() { Targets = [new() { Frequency = TimeSpan.FromHours(4), Qty = 1 }, new() { Frequency = TimeSpan.FromDays(1), Qty = 4 }] };
            
            var d = new TrackedItem() { Targets = [new() { Frequency = TimeSpan.FromHours(4), Qty = 1 }, new() { Frequency = TimeSpan.FromDays(1), Qty = 4 }] };
            d.AddOccurrence(now.AddDays(-1));

            var e = new TrackedItem() { Targets = [new() { Frequency = TimeSpan.FromHours(4), Qty = 1 }, new() { Frequency = TimeSpan.FromDays(1), Qty = 4 }] };
            e.AddOccurrence(now.AddDays(-3));

            var f = new TrackedItem();

            var g = new TrackedItem();
            g.AddOccurrence(now);

            var h = new TrackedItem();
            h.AddOccurrence(now.AddDays(-3));

            List<TrackedItem> items = [a, b, c, d, e, f, g, h];

            items.Sort(new TrackedItemComparer(now));
            items.Reverse();

            Assert.AreEqual(d, items[0]);
            Assert.AreEqual(b, items[1]);
            Assert.AreEqual(a, items[2]);
            Assert.AreEqual(g, items[3]);
            Assert.AreEqual(e, items[4]);
            Assert.AreEqual(h, items[5]);
            Assert.AreEqual(f, items[6]);
            Assert.AreEqual(c, items[7]);
        }
    }
}
