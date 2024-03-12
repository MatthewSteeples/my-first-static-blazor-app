namespace BlazorApp.Client
{
    public static class DateTimeHelper
    {
        public static string ToRelativeDateString(this DateTime dateTime)
        {
            return dateTime.ToRelativeDateString(DateTime.Now);
        }

        public static string ToRelativeDateString(this DateTime dateTime, DateTime relativeTo)
        {
            if (dateTime.Date == relativeTo.Date)
            {
                if (dateTime.TimeOfDay == relativeTo.TimeOfDay)
                {
                    return "Now";
                }
                else
                {
                    return dateTime.ToShortTimeString();
                }
            }

            return dateTime.ToString("g");
        }
    }
}
