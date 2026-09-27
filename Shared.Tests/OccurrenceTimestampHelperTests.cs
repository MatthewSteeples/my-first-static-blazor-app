using BlazorApp.Shared;

namespace BlazorApp.Shared.Tests
{
    [TestClass]
    public class OccurrenceTimestampHelperTests
    {
        [TestMethod]
        public void GetEditableLocalTimestamp_RemovesSeconds()
        {
            var utcTimestamp = new DateTime(2026, 9, 27, 13, 44, 38, DateTimeKind.Local).ToUniversalTime();

            var editableTimestamp = OccurrenceTimestampHelper.GetEditableLocalTimestamp(utcTimestamp);

            Assert.AreEqual(new DateTime(2026, 9, 27, 13, 44, 0, DateTimeKind.Local), editableTimestamp);
        }

        [TestMethod]
        public void ApplyLocalDate_PreservesTimeOfDay()
        {
            var utcTimestamp = new DateTime(2026, 9, 27, 13, 44, 38, DateTimeKind.Local).ToUniversalTime();
            var localDate = new DateTime(2026, 9, 28, 0, 0, 0, DateTimeKind.Local);

            var updatedTimestamp = OccurrenceTimestampHelper.ApplyLocalDate(utcTimestamp, localDate).ToLocalTime();

            Assert.AreEqual(new DateTime(2026, 9, 28, 13, 44, 0, DateTimeKind.Local), updatedTimestamp);
        }

        [TestMethod]
        public void ApplyLocalTime_PreservesDate()
        {
            var utcTimestamp = new DateTime(2026, 9, 27, 13, 44, 38, DateTimeKind.Local).ToUniversalTime();
            var localTime = new DateTime(2026, 1, 1, 16, 7, 52, DateTimeKind.Local);

            var updatedTimestamp = OccurrenceTimestampHelper.ApplyLocalTime(utcTimestamp, localTime).ToLocalTime();

            Assert.AreEqual(new DateTime(2026, 9, 27, 16, 7, 0, DateTimeKind.Local), updatedTimestamp);
        }
    }
}
