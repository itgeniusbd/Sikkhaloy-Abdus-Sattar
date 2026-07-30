using AttendanceDevice.Config_Class;
using AttendanceDevice.Model;
using AttendanceDevice.ViewModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;

namespace AttendanceDevice.APIClass
{
    public static class ScheduleAssignmentSync
    {
        public static async Task<bool> SyncAllFromServerAsync(IRestClient client, int schoolId, string token)
        {
            var daysResult = await SyncScheduleDaysFromServerAsync(client, schoolId, token);
            var assignOk = await SyncAssignmentsFromServerAsync(client, schoolId, token);
            return daysResult.Success || assignOk;
        }

        public static async Task TrySyncAllFromServerAsync(IRestClient client, int schoolId, string token)
        {
            try
            {
                await SyncAllFromServerAsync(client, schoolId, token);
            }
            catch
            {
                // Keep local data; next tick will retry.
            }
        }

        public static Task SyncFromServerAsync(IRestClient client, int schoolId, string token)
        {
            return SyncAssignmentsFromServerAsync(client, schoolId, token);
        }

        public static async Task<ScheduleDaysSyncResult> EnsureScheduleBundleAsync(
            IRestClient client, int schoolId, string token)
        {
            var result = await SyncScheduleDaysFromServerAsync(client, schoolId, token);
            await SyncAssignmentsFromServerAsync(client, schoolId, token);

            if (!result.Success && !LocalData.Instance.Schedules_Get().Any())
            {
                result = await SyncScheduleDaysFromServerAsync(client, schoolId, token);
                await SyncAssignmentsFromServerAsync(client, schoolId, token);
            }

            return result;
        }

        public static async Task<ScheduleDaysSyncResult> SyncScheduleDaysFromServerAsync(
            IRestClient client, int schoolId, string token)
        {
            if (schoolId <= 0)
            {
                LogSyncFailure("schedule-days-schoolId", schoolId, 0, "(invalid school id)");
                return ScheduleDaysSyncResult.Failed();
            }

            try
            {
                var content = await FetchScheduleJsonAsync(schoolId, token);
                HttpStatusCode statusCode = HttpStatusCode.OK;

                if (string.IsNullOrWhiteSpace(content))
                {
                    var request = new RestRequest("api/Users/{id}/schedule", Method.GET);
                    request.AddUrlSegment("id", schoolId);
                    request.AddHeader("Authorization", "Bearer " + token);
                    request.AddHeader("Accept", "application/json");

                    var response = await client.ExecuteTaskAsync(request);
                    statusCode = response.StatusCode;
                    content = ApiResponseHelper.ReadContent(response);

                    if (response.StatusCode != HttpStatusCode.OK || string.IsNullOrWhiteSpace(content))
                    {
                        LogSyncFailure("schedule-days", schoolId, response.StatusCode, content);
                        return ScheduleDaysSyncResult.Failed();
                    }
                }

                if (!IsCompleteJsonArray(content))
                {
                    LogSyncFailure("schedule-days-incomplete", schoolId, statusCode, content);
                    content = await FetchScheduleJsonAsync(schoolId, token);
                }

                var days = ParseScheduleDays(content);
                if (!days.Any())
                {
                    LogSyncFailure("schedule-days-parse", schoolId, statusCode, content);
                    return ScheduleDaysSyncResult.Failed();
                }

                EnsureScheduleRowIds(days);
                StartupLogger.LogStage(
                    $"schedule-sync: downloaded {days.Count} rows, " +
                    $"{days.Select(d => d.ScheduleID).Distinct().Count()} schedules");
                return await SaveScheduleDaysAsync(days, schoolId, statusCode, content, "json-parse");
            }
            catch (Exception ex)
            {
                StartupLogger.LogFailure("schedule-days", ex);
                LogSyncFailure("schedule-days-exception", schoolId, 0, ex.Message);
                return ScheduleDaysSyncResult.Failed();
            }
        }

        private static async Task<string> FetchScheduleJsonAsync(int schoolId, string token)
        {
            try
            {
                using (var http = new HttpClient { Timeout = TimeSpan.FromSeconds(90) })
                {
                    http.DefaultRequestHeaders.Accept.Clear();
                    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    if (!string.IsNullOrWhiteSpace(token))
                        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Trim());

                    var url = ApiUrl.EndPoint.TrimEnd('/') + "/api/Users/" + schoolId + "/schedule";
                    var json = await http.GetStringAsync(url).ConfigureAwait(false);
                    return string.IsNullOrWhiteSpace(json)
                        ? null
                        : json.Trim('\uFEFF', ' ', '\r', '\n', '\t');
                }
            }
            catch (Exception ex)
            {
                StartupLogger.LogFailure("schedule-days-fetch", ex);
                return null;
            }
        }

        private static bool IsCompleteJsonArray(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return false;

            content = content.Trim();
            return content.StartsWith("[", StringComparison.Ordinal) &&
                   content.EndsWith("]", StringComparison.Ordinal);
        }

        private static void EnsureScheduleRowIds(List<Attendance_Schedule_Day> days)
        {
            if (days == null || !days.Any())
                return;

            var seed = 100000;
            for (var i = 0; i < days.Count; i++)
            {
                if (days[i].id > 0)
                    continue;

                days[i].id = seed + i + (days[i].ScheduleID * 10);
            }
        }

        private static async Task<ScheduleDaysSyncResult> SaveScheduleDaysAsync(
            List<Attendance_Schedule_Day> days,
            int schoolId,
            HttpStatusCode statusCode,
            string rawContent,
            string source)
        {
            try
            {
                ApplyScheduleDayIdsFromJson(days, rawContent);
                NormalizeScheduleDayRows(days);
                ApplyScheduleNameFallbacks(days);

                StartupLogger.LogStage(
                    $"schedule-sync ({source}): parsed {days.Count} day rows, " +
                    $"{days.Select(d => d.ScheduleID).Distinct().Count()} schedules");

                var userMismatch = await LocalData.Instance.ScheduleDataHandling(days);
                LocalData.Instance.ReconcileUserScheduleAssignments();
                return ScheduleDaysSyncResult.Ok(userMismatch);
            }
            catch (Exception ex)
            {
                StartupLogger.LogFailure("schedule-days-save", ex);
                LogSyncFailure("schedule-days-save", schoolId, statusCode, ex.Message);
                return ScheduleDaysSyncResult.Failed();
            }
        }

        /// <summary>Download schedule days from server (used by Settings UI and bootstrap).</summary>
        public static async Task<bool> TryDownloadScheduleDaysAsync()
        {
            try
            {
                var ins = LocalData.Instance.institution;
                if (ins == null || string.IsNullOrWhiteSpace(ins.Token))
                    return false;

                var schoolId = LocalData.Instance.GetEffectiveSchoolId();
                var client = new RestClient(ApiUrl.EndPoint);
                var result = await SyncScheduleDaysFromServerAsync(client, schoolId, ins.Token.Trim());
                return result.Success;
            }
            catch (Exception ex)
            {
                StartupLogger.LogFailure("TryDownloadScheduleDaysAsync", ex);
                return false;
            }
        }

        public static async Task<bool> SyncAssignmentsFromServerAsync(IRestClient client, int schoolId, string token)
        {
            try
            {
                var request = new RestRequest("api/Users/{id}/schedules", Method.GET);
                request.AddUrlSegment("id", schoolId);
                request.AddHeader("Authorization", "Bearer " + token);

                var response = await client.ExecuteTaskAsync(request);
                var content = ApiResponseHelper.ReadContent(response);
                if (response.StatusCode != HttpStatusCode.OK || string.IsNullOrWhiteSpace(content))
                    return false;

                var assignments = ParseAssignments(content);
                if (!assignments.Any())
                    return false;

                var userSchedules = assignments
                    .Select(a => new User_Schedule
                    {
                        DeviceID = a.DeviceID,
                        ScheduleID = a.ScheduleID,
                        Is_Student = a.IsStudent
                    })
                    .GroupBy(u => new { u.DeviceID, u.ScheduleID })
                    .Select(g => g.First())
                    .ToList();

                await LocalData.Instance.UserScheduleDataHandling(userSchedules, fromServer: true);
                return true;
            }
            catch (Exception ex)
            {
                StartupLogger.LogFailure("schedule-assignments", ex);
                return false;
            }
        }

        private static bool ParseJsonBool(JToken token, bool defaultValue = true)
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

            if (bool.TryParse(text, out var parsed))
                return parsed;

            return defaultValue;
        }

        private static int ParseJsonInt(JToken token, int defaultValue = 0)
        {
            if (token == null || token.Type == JTokenType.Null)
                return defaultValue;

            if (token.Type == JTokenType.Integer)
                return token.Value<int>();

            if (token.Type == JTokenType.Float)
                return Convert.ToInt32(Math.Round(token.Value<double>()));

            if (int.TryParse(token.ToString().Trim(), out var parsed))
                return parsed;

            return defaultValue;
        }

        private static List<Attendance_Schedule_Day> ParseScheduleDays(string content)
        {
            try
            {
                var fromTokens = ParseScheduleDaysFromTokens(content);
                if (fromTokens.Any())
                {
                    ApplyScheduleNameFallbacks(fromTokens);
                    return fromTokens;
                }

                var dtoRows = TryDeserializeScheduleDays(content);
                if (dtoRows.Any())
                {
                    ApplyScheduleNameFallbacks(dtoRows);
                    return dtoRows;
                }
            }
            catch (Exception ex)
            {
                StartupLogger.LogFailure("schedule-days-parse", ex);
            }

            return new List<Attendance_Schedule_Day>();
        }

        private static void NormalizeScheduleDayRows(List<Attendance_Schedule_Day> days)
        {
            if (days == null)
                return;

            foreach (var item in days)
            {
                item.StartTime = ScheduleTimeHelper.NormalizeForStorage(item.StartTime);
                item.LateEntryTime = ScheduleTimeHelper.NormalizeForStorage(item.LateEntryTime);
                item.EndTime = ScheduleTimeHelper.NormalizeForStorage(item.EndTime);
            }
        }

        private static void ApplyScheduleDayIdsFromJson(List<Attendance_Schedule_Day> days, string content)
        {
            if (days == null || !days.Any() || string.IsNullOrWhiteSpace(content))
                return;

            if (days.All(d => d.id > 0))
                return;

            try
            {
                var array = ParseRootArray(content);
                if (array == null)
                    return;

                foreach (var item in array)
                {
                    var scheduleId = ParseJsonInt(item["scheduleID"] ?? item["ScheduleID"]);
                    var day = (item["day"] ?? item["Day"])?.ToString()?.Trim();
                    if (scheduleId <= 0 || string.IsNullOrWhiteSpace(day))
                        continue;

                    var row = days.FirstOrDefault(d =>
                        d.ScheduleID == scheduleId &&
                        string.Equals(d.Day?.Trim(), day, StringComparison.OrdinalIgnoreCase));

                    if (row == null || row.id > 0)
                        continue;

                    var idToken = item["scheduleDayID"] ?? item["ScheduleDayID"] ?? item["id"];
                    row.id = ParseJsonInt(idToken);
                }
            }
            catch
            {
                // ignored
            }
        }

        private static void ApplyScheduleNameFallbacks(List<Attendance_Schedule_Day> days)
        {
            if (days == null || !days.Any())
                return;

            var namesBySchedule = days
                .Where(d => !string.IsNullOrWhiteSpace(d.ScheduleName))
                .GroupBy(d => d.ScheduleID)
                .ToDictionary(g => g.Key, g => g.First().ScheduleName.Trim());

            foreach (var day in days)
            {
                if (!string.IsNullOrWhiteSpace(day.ScheduleName))
                    continue;

                if (namesBySchedule.TryGetValue(day.ScheduleID, out var name))
                    day.ScheduleName = name;
                else
                    day.ScheduleName = $"Schedule {day.ScheduleID}";
            }
        }

        private static JArray ParseRootArray(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return null;

            var token = JToken.Parse(content.Trim('\uFEFF', ' ', '\r', '\n', '\t'));
            if (token.Type == JTokenType.String)
                token = JToken.Parse(token.ToString());

            if (token.Type == JTokenType.Array)
                return (JArray)token;

            return token["$values"] as JArray ?? token["data"] as JArray ?? token["items"] as JArray;
        }

        private static List<Attendance_Schedule_Day> TryDeserializeScheduleDays(string content)
        {
            var list = new List<Attendance_Schedule_Day>();

            try
            {
                var array = ParseRootArray(content);
                if (array == null)
                    return list;

                var settings = new JsonSerializerSettings
                {
                    DateParseHandling = DateParseHandling.None,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    Error = (_, args) => { args.ErrorContext.Handled = true; }
                };
                var serializer = JsonSerializer.Create(settings);

                foreach (var item in array)
                {
                    if (item?.Type != JTokenType.Object)
                        continue;

                    try
                    {
                        var dto = item.ToObject<ScheduleDayApiDto>(serializer);
                        if (dto == null || dto.ScheduleID <= 0 || string.IsNullOrWhiteSpace(dto.Day))
                            continue;

                        list.Add(ToLocalScheduleDay(dto));
                    }
                    catch (Exception ex)
                    {
                        StartupLogger.LogFailure("schedule-days-dto-row", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                StartupLogger.LogFailure("schedule-days-dto", ex);
            }

            return list;
        }

        private static List<Attendance_Schedule_Day> ParseScheduleDaysFromTokens(string content)
        {
            var list = new List<Attendance_Schedule_Day>();
            var skippedRows = 0;

            try
            {
                var array = ParseRootArray(content);
                if (array == null)
                {
                    StartupLogger.LogStage("schedule-sync: token parse found no JSON array");
                    return list;
                }

                foreach (var item in array)
                {
                    if (item?.Type != JTokenType.Object)
                    {
                        skippedRows++;
                        continue;
                    }

                    try
                    {
                        var scheduleIdToken = item["scheduleID"] ?? item["ScheduleID"];
                        if (scheduleIdToken == null)
                        {
                            skippedRows++;
                            continue;
                        }

                        var scheduleId = ParseJsonInt(scheduleIdToken);
                        if (scheduleId <= 0)
                        {
                            skippedRows++;
                            continue;
                        }

                        var dayToken = item["day"] ?? item["Day"];
                        var day = dayToken?.ToString()?.Trim();
                        if (string.IsNullOrWhiteSpace(day))
                        {
                            skippedRows++;
                            continue;
                        }

                        var idToken = item["scheduleDayID"] ?? item["ScheduleDayID"] ?? item["id"];
                        var schoolIdToken = item["schoolID"] ?? item["SchoolID"];
                        var isOnDayToken = item["isOnDay"] ?? item["is_OnDay"] ?? item["Is_OnDay"];

                        list.Add(new Attendance_Schedule_Day
                        {
                            id = ParseJsonInt(idToken),
                            ScheduleID = scheduleId,
                            SchoolID = ParseJsonInt(schoolIdToken),
                            Day = day,
                            StartTime = ScheduleTimeHelper.FromJsonToken(item["startTime"] ?? item["StartTime"]),
                            LateEntryTime = ScheduleTimeHelper.FromJsonToken(item["lateEntryTime"] ?? item["LateEntryTime"]),
                            EndTime = ScheduleTimeHelper.FromJsonToken(item["endTime"] ?? item["EndTime"]),
                            Is_OnDay = ParseJsonBool(isOnDayToken, true),
                            Is_Abs_Count = false,
                            ScheduleName = ReadScheduleName(item, scheduleId)
                        });
                    }
                    catch (Exception ex)
                    {
                        skippedRows++;
                        StartupLogger.LogFailure("schedule-days-token-row", ex);
                    }
                }

                StartupLogger.LogStage(
                    $"schedule-sync: token parse {array.Count} items -> {list.Count} rows (skipped {skippedRows})");
            }
            catch (Exception ex)
            {
                StartupLogger.LogFailure("schedule-days-token", ex);
            }

            return list;
        }

        private static Attendance_Schedule_Day ToLocalScheduleDay(ScheduleDayApiDto dto)
        {
            return new Attendance_Schedule_Day
            {
                id = dto.ScheduleDayID,
                ScheduleID = dto.ScheduleID,
                SchoolID = dto.SchoolID,
                Day = dto.Day?.Trim(),
                StartTime = ScheduleTimeHelper.NormalizeForStorage(FormatApiTime(dto.StartTime)),
                LateEntryTime = ScheduleTimeHelper.NormalizeForStorage(FormatApiTime(dto.LateEntryTime)),
                EndTime = ScheduleTimeHelper.NormalizeForStorage(FormatApiTime(dto.EndTime)),
                Is_OnDay = dto.Is_OnDay ?? dto.IsOnDay,
                Is_Abs_Count = false,
                ScheduleName = dto.ScheduleName?.Trim()
            };
        }

        private static string ReadScheduleName(JToken item, int scheduleId)
        {
            var name = (item["scheduleName"] ?? item["ScheduleName"])?.ToString()?.Trim();
            if (!string.IsNullOrWhiteSpace(name))
                return name;

            return $"Schedule {scheduleId}";
        }

        private static string FormatApiTime(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? value : value.Trim();
        }

        private static List<DeviceScheduleAssignmentDto> ParseAssignments(string content)
        {
            try
            {
                var parsed = JsonConvert.DeserializeObject<List<DeviceScheduleAssignmentDto>>(content);
                if (parsed != null && parsed.Any())
                {
                    return parsed
                        .Where(a => a.DeviceID > 0 && a.ScheduleID > 0)
                        .ToList();
                }
            }
            catch
            {
                // fall back to manual parse
            }

            var list = new List<DeviceScheduleAssignmentDto>();

            try
            {
                var array = JArray.Parse(content);
                foreach (var item in array)
                {
                    try
                    {
                        var deviceIdToken = item["deviceID"] ?? item["DeviceID"];
                        var scheduleIdToken = item["scheduleID"] ?? item["ScheduleID"];
                        var isStudentToken = item["isStudent"] ?? item["IsStudent"];

                        if (deviceIdToken == null || scheduleIdToken == null)
                            continue;

                        var deviceId = ParseJsonInt(deviceIdToken);
                        var scheduleId = ParseJsonInt(scheduleIdToken);
                        if (deviceId <= 0 || scheduleId <= 0)
                            continue;

                        list.Add(new DeviceScheduleAssignmentDto
                        {
                            DeviceID = deviceId,
                            ScheduleID = scheduleId,
                            IsStudent = ParseJsonBool(isStudentToken, true)
                        });
                    }
                    catch
                    {
                        // Skip malformed assignment row.
                    }
                }
            }
            catch
            {
                // Invalid JSON.
            }

            return list;
        }

        public static void RedirectToUserInfoIfScheduleMismatch(ScheduleDaysSyncResult result, Window windowToClose)
        {
            if (result == null || !result.UserScheduleMismatch)
                return;

            LocalData.Current_Error.Message =
                "Not all User assigned in the schedule on PC, Update User from server!";
            LocalData.Current_Error.Type = Error_Type.UserInfoPage;

            new Settings.Setting().Show();
            windowToClose?.Close();
        }

        private static void LogSyncFailure(string stage, int schoolId, HttpStatusCode statusCode, string content)
        {
            try
            {
                var preview = string.IsNullOrWhiteSpace(content)
                    ? "(empty)"
                    : content.Length <= 250 ? content : content.Substring(0, 250) + "...";

                var logPath = Path.Combine(AppPaths.LogsDirectory, "schedule-sync.log");
                File.AppendAllText(logPath,
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {stage} schoolId={schoolId} status={(int)statusCode} body={preview}{Environment.NewLine}");
            }
            catch
            {
                // ignored
            }
        }
    }
}
