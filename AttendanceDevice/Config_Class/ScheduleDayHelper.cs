using System;
using System.Globalization;

namespace AttendanceDevice.Config_Class
{
    internal static class ScheduleDayHelper
    {
        public static string EnglishDayName(DateTime date)
        {
            return CultureInfo.GetCultureInfo("en-US").DateTimeFormat.GetDayName(date.DayOfWeek);
        }

        public static bool IsSameDay(string storedDay, DateTime date)
        {
            if (string.IsNullOrWhiteSpace(storedDay))
                return false;

            storedDay = storedDay.Trim();

            if (string.Equals(storedDay, EnglishDayName(date), StringComparison.OrdinalIgnoreCase))
                return true;

            var localDay = date.ToString("dddd", CultureInfo.CurrentCulture);
            return string.Equals(storedDay, localDay, StringComparison.OrdinalIgnoreCase);
        }
    }
}
