using System;
using System.Globalization;

namespace AttendanceDevice.Config_Class
{
    internal static class AttendanceDateHelper
    {
        public static bool TryParse(string value, out DateTime date)
        {
            date = default;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            value = value.Trim();

            if (DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out date))
            {
                date = date.Date;
                return true;
            }

            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                date = date.Date;
                return true;
            }

            if (DateTime.TryParseExact(value, "dd-MMM-yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                date = date.Date;
                return true;
            }

            if (DateTime.TryParseExact(value, "dd-MMM-yy", CultureInfo.CurrentCulture, DateTimeStyles.None, out date))
            {
                date = date.Date;
                return true;
            }

            return false;
        }

        public static string Normalize(string value)
        {
            if (TryParse(value, out var date))
                return date.ToString("dd-MMM-yy", CultureInfo.InvariantCulture);

            return value?.Trim();
        }

        public static bool DatesMatch(string left, string right)
        {
            if (string.Equals(left, right, StringComparison.OrdinalIgnoreCase))
                return true;

            if (TryParse(left, out var d1) && TryParse(right, out var d2))
                return d1 == d2;

            return false;
        }

        public static bool IsSameDay(string value, DateTime day)
        {
            return TryParse(value, out var parsed) && parsed == day.Date;
        }
    }
}
