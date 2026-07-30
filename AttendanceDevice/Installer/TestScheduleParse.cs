using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

class TestScheduleParse
{
    static void Main()
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        var json = new WebClient().DownloadString("https://api.sikkhaloy.com/api/Users/1012/schedule");
        Console.WriteLine("Len=" + json.Length);

        try
        {
            var dtos = JsonConvert.DeserializeObject<List<Dto>>(json,
                new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
            Console.WriteLine("DtoCount=" + (dtos != null ? dtos.Count : 0));
        }
        catch (Exception ex)
        {
            Console.WriteLine("DtoFail=" + ex.GetType().Name + " " + ex.Message);
        }

        try
        {
            var array = JArray.Parse(json);
            Console.WriteLine("ArrayCount=" + array.Count);
            var parsed = 0;
            foreach (var item in array)
            {
                var scheduleId = item["scheduleID"] ?? item["ScheduleID"];
                if (scheduleId == null) continue;
                var dayToken = item["day"];
                var day = dayToken != null ? dayToken.ToString().Trim() : null;
                if (string.IsNullOrWhiteSpace(day)) continue;
                parsed++;
            }
            Console.WriteLine("TokenParsed=" + parsed);
        }
        catch (Exception ex)
        {
            Console.WriteLine("TokenFail=" + ex.GetType().Name + " " + ex.Message);
        }
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
}
