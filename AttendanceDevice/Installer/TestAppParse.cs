using System;
using System.Collections.Generic;
using AttendanceDevice.APIClass;
using AttendanceDevice.Config_Class;
using AttendanceDevice.ViewModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

class TestAppParse
{
    static void Main()
    {
        var client = new RestClient("https://api.sikkhaloy.com/");
        var request = new RestRequest("api/Users/1012/schedule", Method.GET);
        var response = client.Execute(request);
        var content = ApiResponseHelper.ReadContent(response);
        Console.WriteLine("Status=" + (int)response.StatusCode + " Len=" + (content != null ? content.Length : 0));

        try
        {
            var settings = new JsonSerializerSettings
            {
                DateParseHandling = DateParseHandling.None,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            var dtos = JsonConvert.DeserializeObject<List<ScheduleDayApiDto>>(content, settings);
            Console.WriteLine("DtoCount=" + (dtos != null ? dtos.Count : 0));
            if (dtos != null && dtos.Count > 0)
            {
                var first = dtos[0];
                Console.WriteLine("FirstStart=" + first.StartTime + " type=" + (first.StartTime != null ? first.StartTime.GetType().FullName : "null"));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("DtoFail=" + ex);
        }

        try
        {
            var array = JArray.Parse(content);
            int ok = 0, fail = 0;
            foreach (var item in array)
            {
                try
                {
                    var st = item["startTime"] ?? item["StartTime"];
                    ScheduleTimeHelper.FromJsonToken(st);
                    ok++;
                }
                catch (Exception ex)
                {
                    fail++;
                    Console.WriteLine("RowFail=" + ex.Message);
                }
            }
            Console.WriteLine("FromJsonToken ok=" + ok + " fail=" + fail);
        }
        catch (Exception ex)
        {
            Console.WriteLine("ArrayFail=" + ex);
        }
    }
}
