using AttendanceDevice.Model;

using AttendanceDevice.ViewModel;

using Newtonsoft.Json;

using Newtonsoft.Json.Linq;

using System;

using System.Collections.Generic;

using System.Globalization;

using System.Linq;



namespace AttendanceDevice.Config_Class

{

    internal static class AttendanceRecordMapper

    {

        public static List<Attendance_Record> FromTodayAttendanceJson(string content)

        {

            if (string.IsNullOrWhiteSpace(content))

                return new List<Attendance_Record>();



            var parsed = TryParseTodayAttendanceRecords(content);

            if (parsed.Any())

                return parsed;



            try

            {

                var dtos = JsonConvert.DeserializeObject<List<AttendanceRecordApiDto>>(content);

                if (dtos == null || !dtos.Any())

                    return new List<Attendance_Record>();



                return dtos

                    .Select(ToLocalRecord)

                    .Where(r => r != null)

                    .ToList();

            }

            catch

            {

                return new List<Attendance_Record>();

            }

        }



        private static List<Attendance_Record> TryParseTodayAttendanceRecords(string content)

        {

            var list = new List<Attendance_Record>();



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

                    try

                    {

                        if (item?.Type != JTokenType.Object)

                            continue;



                        var deviceId = ParseJsonInt(item["deviceID"] ?? item["DeviceID"]);

                        if (deviceId <= 0)

                            continue;



                        var scheduleId = ParseJsonInt(item["scheduleID"] ?? item["ScheduleID"]);

                        var attendanceDate = ParseJsonDateString(item["attendanceDate"] ?? item["AttendanceDate"])

                                             ?? LocalData.Instance.GetAttendanceDateString();



                        list.Add(new Attendance_Record

                        {

                            DeviceID = deviceId,

                            ScheduleID = scheduleId,

                            AttendanceDate = attendanceDate,

                            AttendanceStatus = ReadString(item["attendanceStatus"] ?? item["AttendanceStatus"]),

                            ExitStatus = ReadString(item["exitStatus"] ?? item["ExitStatus"]),

                            Is_OUT = ParseJsonBool(item["is_OUT"] ?? item["Is_OUT"]),

                            EntryTime = ScheduleTimeHelper.NormalizeForStorage(

                                ParseJsonTimeString(item["entryTime"] ?? item["EntryTime"])),

                            ExitTime = ScheduleTimeHelper.NormalizeForStorage(

                                ParseJsonTimeString(item["exitTime"] ?? item["ExitTime"])),

                            Is_Sent = ParseJsonBool(item["is_Sent"] ?? item["Is_Sent"], true),

                            Is_Updated = ParseJsonBool(item["is_Updated"] ?? item["Is_Updated"], true)

                        });

                    }

                    catch

                    {

                        // Skip malformed attendance row.

                    }

                }

            }

            catch

            {

                // Invalid JSON.

            }



            return list;

        }



        public static Attendance_Record ToLocalRecord(AttendanceRecordApiDto dto)

        {

            if (dto == null || dto.DeviceID <= 0)

                return null;



            return new Attendance_Record

            {

                DeviceID = dto.DeviceID,

                ScheduleID = dto.ScheduleID,

                AttendanceDate = dto.AttendanceDate == default(DateTime)

                    ? LocalData.Instance.GetAttendanceDateString()

                    : dto.AttendanceDate.ToString("dd-MMM-yy", CultureInfo.InvariantCulture),

                AttendanceStatus = dto.AttendanceStatus,

                ExitStatus = dto.ExitStatus,

                Is_OUT = dto.Is_OUT,

                EntryTime = FormatStoredTime(dto.EntryTime),

                ExitTime = FormatStoredTime(dto.ExitTime),

                Is_Sent = dto.Is_Sent,

                Is_Updated = dto.Is_Updated

            };

        }



        private static string FormatStoredTime(TimeSpan? value)

        {

            if (!value.HasValue)

                return null;



            return value.Value.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);

        }



        public static List<AttendanceRecordApiDto> ToApiPayload(IEnumerable<Attendance_Record> records)

        {

            return records.Select(ToApiPayload).ToList();

        }



        public static AttendanceRecordApiDto ToApiPayload(Attendance_Record record)

        {

            return new AttendanceRecordApiDto

            {

                RecordID = record.RecordID,

                DeviceID = record.DeviceID,

                ScheduleID = record.ScheduleID,

                AttendanceDate = ParseAttendanceDate(record.AttendanceDate),

                AttendanceStatus = record.AttendanceStatus,

                ExitStatus = record.ExitStatus,

                Is_OUT = record.Is_OUT,

                EntryTime = ParseTime(record.EntryTime),

                ExitTime = ParseTime(record.ExitTime),

                Is_Sent = record.Is_Sent,

                Is_Updated = record.Is_Updated

            };

        }



        private static DateTime ParseAttendanceDate(string value)

        {

            if (!string.IsNullOrWhiteSpace(value))

            {

                if (DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out var parsed))

                    return parsed.Date;



                if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))

                    return parsed.Date;

            }



            return LocalData.Instance.GetAttendanceDate();

        }



        private static TimeSpan? ParseTime(string value)

        {

            if (string.IsNullOrWhiteSpace(value))

                return null;



            if (TimeSpan.TryParse(value, out var parsed))

                return parsed;



            if (DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out var dt))

                return dt.TimeOfDay;



            return null;

        }



        private static string ReadString(JToken token)

        {

            if (token == null || token.Type == JTokenType.Null)

                return null;



            if (token.Type == JTokenType.Date)

                return token.ToObject<DateTime>().ToString("dd-MMM-yy", CultureInfo.InvariantCulture);



            return token.ToString()?.Trim();

        }



        private static string ParseJsonDateString(JToken token)

        {

            if (token == null || token.Type == JTokenType.Null)

                return null;



            if (token.Type == JTokenType.Date)

                return token.ToObject<DateTime>().ToString("dd-MMM-yy", CultureInfo.InvariantCulture);



            var text = token.ToString()?.Trim();

            if (string.IsNullOrWhiteSpace(text))

                return null;



            return AttendanceDateHelper.Normalize(text);

        }



        private static string ParseJsonTimeString(JToken token)

        {

            if (token == null || token.Type == JTokenType.Null)

                return null;



            if (token.Type == JTokenType.TimeSpan)

                return token.ToObject<TimeSpan>().ToString();



            if (token.Type == JTokenType.Date)

                return token.ToObject<DateTime>().ToString("HH:mm:ss", CultureInfo.InvariantCulture);



            return token.ToString()?.Trim();

        }



        private static bool ParseJsonBool(JToken token, bool defaultValue = false)

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

    }

}


