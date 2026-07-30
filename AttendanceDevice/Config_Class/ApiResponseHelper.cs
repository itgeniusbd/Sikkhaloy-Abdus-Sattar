using AttendanceDevice.APIClass;
using AttendanceDevice.Model;
using AttendanceDevice.ViewModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

namespace AttendanceDevice.Config_Class
{
    internal static class ApiResponseHelper
    {
        public static string ReadContent(IRestResponse response)
        {
            if (response == null)
                return null;

            // Prefer UTF-8 bytes — RestSharp Content can mis-decode Bengali schedule names on some PCs.
            if (response.RawBytes != null && response.RawBytes.Length > 0)
            {
                var fromBytes = Encoding.UTF8.GetString(response.RawBytes)
                    .Trim('\uFEFF', ' ', '\r', '\n', '\t');
                if (!string.IsNullOrWhiteSpace(fromBytes))
                    return fromBytes;
            }

            var content = response.Content;
            if (!string.IsNullOrWhiteSpace(content))
                return content.Trim('\uFEFF', ' ', '\r', '\n', '\t');

            return content;
        }

        public static string GetAccessToken(IRestResponse response)
        {
            if (response == null || response.StatusCode != HttpStatusCode.OK)
                return null;

            var token = ParseToken(ReadContent(response));
            return string.IsNullOrWhiteSpace(token?.access_token) ? null : token.access_token.Trim();
        }

        public static Token ParseToken(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return null;

            try
            {
                return JsonConvert.DeserializeObject<Token>(content);
            }
            catch
            {
                return null;
            }
        }

        public static SchoolApiDto ParseSchoolApi(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return null;

            try
            {
                var root = JToken.Parse(content);
                if (root.Type != JTokenType.Object)
                    return null;

                return new SchoolApiDto
                {
                    SchoolID = ParseJsonInt(root["schoolID"] ?? root["SchoolID"]),
                    InstitutionName = ReadString(root["institutionName"] ?? root["InstitutionName"]),
                    Image_Link = ReadString(root["image_Link"] ?? root["Image_Link"]),
                    Logo = ParseLogo(root["logo"] ?? root["Logo"]),
                    UserName = ReadString(root["userName"] ?? root["UserName"]),
                    IsValid = ParseJsonBool(root["isValid"] ?? root["IsValid"], false),
                    SettingKey = ReadString(root["settingKey"] ?? root["SettingKey"]),
                    Is_Device_Attendance_Enable = ParseJsonBool(
                        root["is_Device_Attendance_Enable"] ?? root["Is_Device_Attendance_Enable"], true),
                    Is_Student_Attendance_Enable = ParseJsonBool(
                        root["is_Student_Attendance_Enable"] ?? root["Is_Student_Attendance_Enable"], true),
                    Is_Employee_Attendance_Enable = ParseJsonBool(
                        root["is_Employee_Attendance_Enable"] ?? root["Is_Employee_Attendance_Enable"], true),
                    Is_Today_Holiday = ParseJsonBool(root["is_Today_Holiday"] ?? root["Is_Today_Holiday"], false),
                    Holiday_Active = ParseJsonBool(root["holiday_Active"] ?? root["Holiday_Active"], false),
                    LastUpdateDate = ReadString(root["lastUpdateDate"] ?? root["LastUpdateDate"]),
                    Current_Datetime = ParseJsonDateTime(root["current_Datetime"] ?? root["Current_Datetime"]) ?? DateTime.Now
                };
            }
            catch
            {
                return null;
            }
        }

        public static List<User_Leave_Record> ParseLeaveRecords(string content)
        {
            var list = new List<User_Leave_Record>();
            if (string.IsNullOrWhiteSpace(content))
                return list;

            try
            {
                var token = JToken.Parse(content);
                var array = token.Type == JTokenType.Array
                    ? (JArray)token
                    : token["$values"] as JArray ?? token["data"] as JArray ?? token["items"] as JArray;

                if (array == null)
                    return list;

                foreach (var item in array)
                {
                    if (item?.Type != JTokenType.Object)
                        continue;

                    var deviceId = ParseJsonInt(item["deviceID"] ?? item["DeviceID"]);
                    if (deviceId <= 0)
                        continue;

                    var leaveDate = ReadString(item["leaveDate"] ?? item["LeaveDate"]);
                    list.Add(new User_Leave_Record
                    {
                        DeviceID = deviceId,
                        LeaveDate = leaveDate
                    });
                }
            }
            catch
            {
                // Invalid JSON; return whatever was parsed.
            }

            return list;
        }

        public static List<DataUpdateList> ParseUpdateNotifications(string content)
        {
            var list = new List<DataUpdateList>();
            if (string.IsNullOrWhiteSpace(content))
                return list;

            try
            {
                var token = JToken.Parse(content);
                var array = token.Type == JTokenType.Array
                    ? (JArray)token
                    : token["$values"] as JArray ?? token["data"] as JArray ?? token["items"] as JArray;

                if (array == null)
                    return list;

                var id = 1;
                foreach (var item in array)
                {
                    if (item?.Type != JTokenType.Object)
                        continue;

                    list.Add(new DataUpdateList
                    {
                        DateUpdateID = id++,
                        UpdateType = ReadString(item["updateType"] ?? item["UpdateType"]),
                        UpdateDescription = ReadString(item["updateDescription"] ?? item["UpdateDescription"]),
                        UpdateDate = ReadString(item["updateDate"] ?? item["UpdateDate"])
                    });
                }
            }
            catch
            {
                // Invalid JSON; return whatever was parsed.
            }

            return list;
        }

        private static string ReadString(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null)
                return null;

            if (token.Type == JTokenType.Date)
                return token.ToObject<DateTime>().ToString("dd-MMM-yy", CultureInfo.InvariantCulture);

            return token.ToString()?.Trim();
        }

        private static byte[] ParseLogo(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null)
                return null;

            if (token.Type == JTokenType.Bytes)
                return token.ToObject<byte[]>();

            if (token.Type == JTokenType.String)
            {
                var text = token.ToString();
                if (string.IsNullOrWhiteSpace(text))
                    return null;

                try
                {
                    return Convert.FromBase64String(text);
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        private static bool ParseJsonBool(JToken token, bool defaultValue)
        {
            if (token == null || token.Type == JTokenType.Null)
                return defaultValue;

            if (token.Type == JTokenType.Boolean)
                return token.Value<bool>();

            if (token.Type == JTokenType.Integer)
                return token.Value<int>() != 0;

            var text = token.ToString().Trim();
            if (string.Equals(text, "1", StringComparison.Ordinal) ||
                string.Equals(text, "yes", StringComparison.OrdinalIgnoreCase))
                return true;

            if (string.Equals(text, "0", StringComparison.Ordinal) ||
                string.Equals(text, "no", StringComparison.OrdinalIgnoreCase))
                return false;

            return bool.TryParse(text, out var parsed) ? parsed : defaultValue;
        }

        private static int ParseJsonInt(JToken token, int defaultValue = 0)
        {
            if (token == null || token.Type == JTokenType.Null)
                return defaultValue;

            if (token.Type == JTokenType.Integer)
                return token.Value<int>();

            return int.TryParse(token.ToString().Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : defaultValue;
        }

        private static DateTime? ParseJsonDateTime(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null)
                return null;

            if (token.Type == JTokenType.Date)
                return token.ToObject<DateTime>();

            var text = token.ToString()?.Trim();
            if (string.IsNullOrWhiteSpace(text))
                return null;

            if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsed))
                return parsed;

            if (DateTime.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out parsed))
                return parsed;

            return null;
        }
    }
}
