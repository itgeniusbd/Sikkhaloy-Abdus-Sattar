using System;
using Newtonsoft.Json.Linq;

class TestTokenTypes
{
    static void Main(string[] args)
    {
        var path = args.Length > 0 ? args[0] : @"F:\SIKKHALOY-V3\AttendanceDevice\Installer\schedule-sample.json";
        var json = System.IO.File.ReadAllText(path);
        var array = JArray.Parse(json);
        Console.WriteLine("Count=" + array.Count);
        foreach (var item in array)
        {
            var st = item["startTime"] ?? item["StartTime"];
            var iso = item["is_OnDay"] ?? item["isOnDay"];
            if (st != null && st.Type != JTokenType.String)
                Console.WriteLine("startTime type=" + st.Type + " val=" + st + " scheduleID=" + item["scheduleID"]);
            if (iso != null && iso.Type != JTokenType.Boolean)
                Console.WriteLine("is_OnDay type=" + iso.Type + " val=" + iso + " scheduleID=" + item["scheduleID"]);
        }
        Console.WriteLine("Done");
    }
}
