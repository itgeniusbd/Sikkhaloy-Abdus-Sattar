using System;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace AttendanceDevice.Config_Class
{
    internal static class ScheduleTimeHelper
    {
        public static bool TryParse(string value, out TimeSpan time)
        {
            time = TimeSpan.Zero;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            value = value.Trim();

            if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out time))
                return true;

            if (TimeSpan.TryParse(value, CultureInfo.CurrentCulture, out time))
                return true;

            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            {
                time = dt.TimeOfDay;
                return true;
            }

            if (DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out dt))
            {
                time = dt.TimeOfDay;
                return true;
            }

            return false;
        }

        public static string NormalizeForStorage(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            try
            {
                if (TryParse(value, out var time))
                    return time.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);

                return value.Trim();
            }
            catch
            {
                return value.Trim();
            }
        }

        public static string FromJsonToken(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null)
                return null;

            try
            {
                if (token.Type == JTokenType.TimeSpan)
                    return NormalizeForStorage(token.ToObject<TimeSpan>().ToString());

                if (token.Type == JTokenType.Date)
                    return NormalizeForStorage(token.ToObject<DateTime>().ToString(CultureInfo.InvariantCulture));

                if (token.Type == JTokenType.Integer || token.Type == JTokenType.Float)
                {
                    try
                    {
                        return NormalizeForStorage(TimeSpan.FromTicks(token.Value<long>()).ToString());
                    }
                    catch
                    {
                        // Not ticks; fall through.
                    }
                }

                if (token.Type == JTokenType.Object)
                {
                    var ticks = token["ticks"] ?? token["Ticks"];
                    if (ticks != null)
                    {
                        try
                        {
                            return NormalizeForStorage(TimeSpan.FromTicks(ReadJsonInt64(ticks)).ToString());
                        }
                        catch
                        {
                            // fall through
                        }
                    }

                    var hours = token["hours"] ?? token["Hours"];
                    var minutes = token["minutes"] ?? token["Minutes"];
                    var seconds = token["seconds"] ?? token["Seconds"];
                    if (hours != null || minutes != null || seconds != null)
                    {
                        var h = ReadJsonInt(hours);
                        var m = ReadJsonInt(minutes);
                        var s = ReadJsonInt(seconds);
                        return NormalizeForStorage(new TimeSpan(h, m, s).ToString());
                    }
                }

                return NormalizeForStorage(token.ToString());
            }
            catch
            {
                return token.ToString()?.Trim();
            }
        }

        private static int ReadJsonInt(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null)
                return 0;

            if (token.Type == JTokenType.Integer)
                return token.Value<int>();

            if (token.Type == JTokenType.Float)
                return Convert.ToInt32(Math.Round(token.Value<double>()));

            return int.TryParse(token.ToString().Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : 0;
        }

        private static long ReadJsonInt64(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null)
                return 0;

            if (token.Type == JTokenType.Integer)
                return token.Value<long>();

            if (token.Type == JTokenType.Float)
                return Convert.ToInt64(Math.Round(token.Value<double>()));

            return long.TryParse(token.ToString().Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : 0;
        }

        public static string FormatDisplayTime(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            if (TryParse(value, out var time))
                return DateTime.Today.Add(time).ToString("hh:mm tt", CultureInfo.CurrentCulture);

            if (DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out var dt))
                return dt.ToString("hh:mm tt", CultureInfo.CurrentCulture);

            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt.ToString("hh:mm tt", CultureInfo.CurrentCulture);

            return value;
        }
    }
}
