using System;

namespace BlazorApp.Shared
{
    public static class OccurrenceTimestampHelper
    {
        public static DateTime GetEditableLocalTimestamp(DateTime utcTimestamp)
        {
            var localTime = utcTimestamp.ToLocalTime();
            return new DateTime(localTime.Year, localTime.Month, localTime.Day, localTime.Hour, localTime.Minute, 0, localTime.Kind);
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
    }
}
