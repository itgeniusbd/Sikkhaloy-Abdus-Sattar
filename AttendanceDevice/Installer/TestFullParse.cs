using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

class TestFullParse
{
    static void Main()
    {
        var json = System.IO.File.ReadAllText(@"F:\SIKKHALOY-V3\AttendanceDevice\Installer\schedule-sample.json");
        TestDto(json);
        TestTokens(json);
    }

    static void TestDto(string content)
    {
        try
        {
            var settings = new JsonSerializerSettings
            {
                DateParseHandling = DateParseHandling.None,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            var dtos = JsonConvert.DeserializeObject<List<Dto>>(content, settings);
            var rows = dtos
                .Where(d => d != null && d.ScheduleID > 0 && !string.IsNullOrWhiteSpace(d.Day))
                .Select(ToLocal)
                .ToList();
            Console.WriteLine("DtoRows=" + rows.Count);
        }
        catch (Exception ex)
        {
            Console.WriteLine("DtoFail=" + ex);
        }
    }

    static Row ToLocal(Dto dto)
    {
        return new Row
        {
            ScheduleID = dto.ScheduleID,
            Day = dto.Day?.Trim(),
            StartTime = Normalize(Format(dto.StartTime)),
            LateEntryTime = Normalize(Format(dto.LateEntryTime)),
            EndTime = Normalize(Format(dto.EndTime)),
            Is_OnDay = dto.Is_OnDay ?? dto.IsOnDay,
            ScheduleName = dto.ScheduleName?.Trim()
        };
    }

    static void TestTokens(string content)
    {
        var list = new List<Row>();
        var array = JArray.Parse(content);
        foreach (var item in array)
        {
            try
            {
                var scheduleId = ParseInt(item["scheduleID"] ?? item["ScheduleID"]);
                if (scheduleId <= 0) continue;
                var day = (item["day"] ?? item["Day"])?.ToString()?.Trim();
                if (string.IsNullOrWhiteSpace(day)) continue;
                list.Add(new Row
                {
                    ScheduleID = scheduleId,
                    Day = day,
                    StartTime = FromJsonToken(item["startTime"] ?? item["StartTime"]),
                    LateEntryTime = FromJsonToken(item["lateEntryTime"] ?? item["LateEntryTime"]),
                    EndTime = FromJsonToken(item["endTime"] ?? item["EndTime"]),
                    Is_OnDay = ParseBool(item["isOnDay"] ?? item["is_OnDay"], true),
                    ScheduleName = (item["scheduleName"] ?? item["ScheduleName"])?.ToString()?.Trim()
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("RowFail scheduleID=" + item["scheduleID"] + " " + ex.Message);
            }
        }
        Console.WriteLine("TokenRows=" + list.Count);
    }

    static string Format(string v)
    {
        return string.IsNullOrWhiteSpace(v) ? v : v.Trim();
    }

    static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var t) ||
            TimeSpan.TryParse(value, CultureInfo.CurrentCulture, out t))
            return t.ToString(@"HH\:mm\:ss", CultureInfo.InvariantCulture);
        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt) ||
            DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out dt))
            return dt.TimeOfDay.ToString(@"HH\:mm\:ss", CultureInfo.InvariantCulture);
        return value.Trim();
    }

    static string FromJsonToken(JToken token)
    {
        if (token == null || token.Type == JTokenType.Null) return null;
        if (token.Type == JTokenType.TimeSpan)
            return Normalize(token.ToObject<TimeSpan>().ToString());
        if (token.Type == JTokenType.Date)
            return Normalize(token.ToObject<DateTime>().ToString(CultureInfo.InvariantCulture));
        if (token.Type == JTokenType.Integer || token.Type == JTokenType.Float)
        {
            try { return Normalize(TimeSpan.FromTicks(token.Value<long>()).ToString()); }
            catch { }
        }
        if (token.Type == JTokenType.Object)
        {
            var hours = token["hours"] ?? token["Hours"];
            var minutes = token["minutes"] ?? token["Minutes"];
            var seconds = token["seconds"] ?? token["Seconds"];
            if (hours != null || minutes != null || seconds != null)
            {
                var h = hours?.Value<int>() ?? 0;
                var m = minutes?.Value<int>() ?? 0;
                var s = seconds?.Value<int>() ?? 0;
                return Normalize(new TimeSpan(h, m, s).ToString());
            }
        }
        return Normalize(token.ToString());
    }

    static int ParseInt(JToken token)
    {
        if (token == null || token.Type == JTokenType.Null) return 0;
        if (token.Type == JTokenType.Integer) return token.Value<int>();
        if (token.Type == JTokenType.Float) return Convert.ToInt32(Math.Round(token.Value<double>()));
        int.TryParse(token.ToString().Trim(), out var p);
        return p;
    }

    static bool ParseBool(JToken token, bool def)
    {
        if (token == null || token.Type == JTokenType.Null) return def;
        if (token.Type == JTokenType.Boolean) return token.Value<bool>();
        if (token.Type == JTokenType.Integer) return token.Value<int>() != 0;
        return bool.TryParse(token.ToString().Trim(), out var p) ? p : def;
    }

    class Dto
    {
        [JsonProperty("scheduleDayID")] public int ScheduleDayID { get; set; }
        [JsonProperty("scheduleID")] public int ScheduleID { get; set; }
        [JsonProperty("schoolID")] public int SchoolID { get; set; }
        [JsonProperty("day")] public string Day { get; set; }
        [JsonProperty("startTime")] public string StartTime { get; set; }
        [JsonProperty("lateEntryTime")] public string LateEntryTime { get; set; }
        [JsonProperty("endTime")] public string EndTime { get; set; }
        [JsonProperty("isOnDay")] public bool IsOnDay { get; set; }
        [JsonProperty("is_OnDay")] public bool? Is_OnDay { get; set; }
        [JsonProperty("scheduleName")] public string ScheduleName { get; set; }
    }

    class Row
    {
        public int ScheduleID;
        public string Day;
        public string StartTime;
        public string LateEntryTime;
        public string EndTime;
        public bool Is_OnDay;
        public string ScheduleName;
    }
}
