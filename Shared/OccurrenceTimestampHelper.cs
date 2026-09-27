using System;
using System.Globalization;

namespace BlazorApp.Shared
{
    public static class OccurrenceTimestampHelper
    {
        private static readonly string[] AcceptedTimeFormats = ["HH:mm", "HH:mm:ss", "HH:mm:ss.fff"];

        public static DateTime TruncateToMinute(DateTime timestamp)
        {
            return new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, timestamp.Minute, 0, timestamp.Kind);
        }

        public static DateTime GetEditableLocalTimestamp(DateTime utcTimestamp)
        {
            return TruncateToMinute(utcTimestamp.ToLocalTime());
        }

        public static string GetEditableLocalTimeText(DateTime utcTimestamp)
        {
            return GetEditableLocalTimestamp(utcTimestamp).ToString("HH:mm", CultureInfo.InvariantCulture);
        }

        public static DateTime ApplyLocalDate(DateTime utcTimestamp, DateTime localDate)
        {
            var currentLocalTime = GetEditableLocalTimestamp(utcTimestamp);
            return new DateTime(localDate.Year, localDate.Month, localDate.Day, currentLocalTime.Hour, currentLocalTime.Minute, 0, DateTimeKind.Local).ToUniversalTime();
        }

        public static DateTime ApplyLocalTime(DateTime utcTimestamp, DateTime localTime)
        {
            var currentLocalTime = GetEditableLocalTimestamp(utcTimestamp);
            return new DateTime(currentLocalTime.Year, currentLocalTime.Month, currentLocalTime.Day, localTime.Hour, localTime.Minute, 0, DateTimeKind.Local).ToUniversalTime();
        }

        public static DateTime ApplyLocalTimeText(DateTime utcTimestamp, string localTimeText)
        {
            if (string.IsNullOrWhiteSpace(localTimeText))
            {
                return TruncateToMinute(utcTimestamp);
            }

            if (DateTime.TryParseExact(localTimeText, AcceptedTimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var localTime) is false)
            {
                return TruncateToMinute(utcTimestamp);
            }

            return ApplyLocalTime(utcTimestamp, localTime);
        }
    }
}
