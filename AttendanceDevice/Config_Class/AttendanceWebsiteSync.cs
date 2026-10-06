using AttendanceDevice.Model;
using AttendanceDevice.ViewModel;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace AttendanceDevice.Config_Class
{
    internal sealed class AttendanceWebsiteSyncResult
    {
        public bool Changed { get; set; }
        public int StudentPosted { get; set; }
        public int StudentUpdated { get; set; }
        public int EmployeePosted { get; set; }
        public int EmployeeUpdated { get; set; }
        public string Message { get; set; }
        public List<string> Notes { get; } = new List<string>();

        public int TotalPushed => StudentPosted + StudentUpdated + EmployeePosted + EmployeeUpdated;
    }

    /// <summary>
    /// Sends today's local attendance to sikkhaloy.com.
    /// POST inserts new rows; UPDATE overwrites Abs / Late Abs + 12:00 AM placeholders.
    /// </summary>
    internal static class AttendanceWebsiteSync
    {
        // One push at a time: the display timer and the Settings buttons must not upload (and mark) the
        // same rows concurrently.
        private static readonly SemaphoreSlim PushLock = new SemaphoreSlim(1, 1);

        public static async Task<AttendanceWebsiteSyncResult> PushPendingAsync(
            Institution institution,
            Action<string> progress = null)
        {
            await PushLock.WaitAsync();
            try
            {
                return await PushPendingCoreAsync(institution, progress);
            }
            finally
            {
                PushLock.Release();
            }
        }

        private static async Task<AttendanceWebsiteSyncResult> PushPendingCoreAsync(
            Institution institution,
            Action<string> progress)
        {
            var result = new AttendanceWebsiteSyncResult();
            if (institution == null)
            {
                result.Message = "Institution is not loaded.";
                return result;
            }

            var internet = await ApiUrl.IsNoNetConnection();
            if (internet)
            {
                result.Message = "No Internet";
                return result;
            }

            var server = await ApiUrl.IsServerUnavailable();
            if (server)
            {
                result.Message = "Server unavailable";
                return result;
            }

            var client = new RestClient(ApiUrl.EndPoint);
            LocalData.Instance.RepairStudentUserFlags();
            progress?.Invoke("Replaying Device Logs...");
            await Machine.ReplayRejectedLogsAsAttendance(institution);
            LocalData.Instance.FlagIncompleteRecordsForResync();
            LocalData.Instance.RecalculateTodayEntryStatusesFromSchedules();

            // Loop in batches until every unsynced row of any date is sent (or a batch makes no progress).
            const int maxRounds = 50;
            for (var round = 1; round <= maxRounds; round++)
            {
                var before = LocalData.Instance.GetPendingAttendanceCount();
                if (before == 0)
                    break;

                progress?.Invoke($"Uploading batch {round} ({before} pending)...");
                using (var db = new ModelContext())
                {
                    result.StudentPosted += await PostStudentsAsync(db, client, institution, result);
                    result.StudentUpdated += await PutStudentsAsync(db, client, institution, result);
                    result.EmployeePosted += await PostEmployeesAsync(db, client, institution, result);
                    result.EmployeeUpdated += await PutEmployeesAsync(db, client, institution, result);
                }

                var after = LocalData.Instance.GetPendingAttendanceCount();
                if (after >= before)
                    break; // no progress (server reject / DeviceID mismatch) — stop to avoid a loop
            }

            var remaining = LocalData.Instance.GetPendingAttendanceCount();

            if (result.TotalPushed > 0)
            {
                AddNote(result,
                    remaining > 0
                        ? $"Pending sync uploaded {result.TotalPushed} record(s). Still pending: {remaining}."
                        : $"Pending sync uploaded {result.TotalPushed} record(s). Pending clear.");
            }
            else if (remaining > 0)
            {
                AddNote(result,
                    $"No records uploaded. Pending {remaining}. Check DeviceID match on server.");
            }

            if (result.Notes.Count > 0)
                result.Message = string.Join("\n", result.Notes.Distinct());
            else if (string.IsNullOrWhiteSpace(result.Message))
            {
                result.Message = remaining > 0
                    ? $"No records were uploaded. {remaining} pending record(s) remain - check internet/server or DeviceID match."
                    : "No pending attendance to send. Download today's punches from the device first.";
            }

            try { LocalData.Instance.CleanupSyncedLogBackups(); } catch { /* best-effort */ }
            return result;
        }

        private static async Task<int> PostStudentsAsync(
            ModelContext db,
            RestClient client,
            Institution institution,
            AttendanceWebsiteSyncResult result)
        {
            var studentLog = (await LocalData.Instance.StudentLog_Post())
                .Take(LocalData.WebsiteSyncBatchSize).ToList();
            if (studentLog.Count == 0)
                return 0;

            var request = new RestRequest("api/Attendance/{id}/Students", Method.POST);
            request.AddUrlSegment("id", institution.SchoolID);
            ApiRequestHelper.AddAuthorizedJsonHeaders(request, institution.Token);
            ApiRequestHelper.AddCamelCaseJsonBody(request, AttendanceRecordMapper.ToApiPayload(studentLog));

            var response = await client.ExecutePostTaskAsync(request);
            var syncResult = AttendanceSyncResponseParser.Parse(response);

            if (response.StatusCode == HttpStatusCode.OK &&
                (AttendanceSyncResponseParser.IsLegacyEmptyOk(response) ||
                 (syncResult != null && (syncResult.Matched > 0 || syncResult.Inserted > 0))))
            {
                var synced = FilterSyncedLogs(studentLog, syncResult, syncResult);
                await MarkAttendanceRecordsAsync(db, synced, markSent: true, markUpdated: true);
                result.Changed = true;
                var partial = BuildPartialSyncMessage("Student", studentLog.Count, synced.Count, syncResult);
                if (!string.IsNullOrWhiteSpace(partial))
                    AddNote(result, partial);
                return synced.Count;
            }

            var backupCount = await PostStudentsViaBackupAsync(db, client, institution, studentLog);
            if (backupCount > 0)
            {
                result.Changed = true;
                AddNote(result, $"Student backup sync sent {backupCount} record(s).");
                return backupCount;
            }

            if (AttendanceSyncResponseParser.TryGetStudentPostFailureMessage(
                response, syncResult, out var failureMessage))
            {
                AddNote(result, failureMessage);
            }

            return 0;
        }

        private static async Task<int> PutStudentsAsync(
            ModelContext db,
            RestClient client,
            Institution institution,
            AttendanceWebsiteSyncResult result)
        {
            var studentLogUpdate = (await LocalData.Instance.StudentLog_Put())
                .Take(LocalData.WebsiteSyncBatchSize).ToList();
            if (studentLogUpdate.Count == 0)
                return 0;

            var request = new RestRequest("api/Attendance/{id}/StudentsUpdate", Method.POST);
            request.AddUrlSegment("id", institution.SchoolID);
            ApiRequestHelper.AddAuthorizedJsonHeaders(request, institution.Token);
            ApiRequestHelper.AddCamelCaseJsonBody(request, AttendanceRecordMapper.ToApiPayload(studentLogUpdate));

            var response = await client.ExecutePostTaskAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                await MarkAttendanceRecordsAsync(db, studentLogUpdate, markSent: true, markUpdated: true);
                result.Changed = true;
                return studentLogUpdate.Count;
            }

            AddNote(result, string.IsNullOrWhiteSpace(response.Content)
                ? $"Student update failed ({(int)response.StatusCode})"
                : $"Student update failed ({(int)response.StatusCode}): {response.Content}");
            return 0;
        }

        private static async Task<int> PostStudentsViaBackupAsync(
            ModelContext db,
            RestClient client,
            Institution institution,
            List<Attendance_Record> studentLog)
        {
            var payload = new List<object>();
            foreach (var record in studentLog)
            {
                if (ScheduleRoutingHelper.IsPlaceholderEntryTime(record.EntryTime))
                    continue;
                if (!AttendanceDateHelper.TryParse(record.AttendanceDate, out var date))
                    date = LocalData.Instance.GetAttendanceDate();
                if (!ScheduleTimeHelper.TryParse(record.EntryTime, out var time))
                    continue;

                payload.Add(new
                {
                    DeviceID = record.DeviceID,
                    EntryTime = time,
                    EntryDate = date.Date,
                    EntryDay = ScheduleDayHelper.EnglishDayName(date),
                    Is_Student = true,
                    ScheduleID = record.ScheduleID
                });
            }

            if (payload.Count == 0)
                return 0;

            var request = new RestRequest("api/Attendance/{id}/backup_data", Method.POST);
            request.AddUrlSegment("id", institution.SchoolID);
            ApiRequestHelper.AddAuthorizedJsonHeaders(request, institution.Token);
            ApiRequestHelper.AddCamelCaseJsonBody(request, payload);

            var response = await client.ExecutePostTaskAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
                return 0;

            var applied = studentLog.Where(a => !ScheduleRoutingHelper.IsPlaceholderEntryTime(a.EntryTime)).ToList();
            await MarkAttendanceRecordsAsync(db, applied, markSent: true, markUpdated: true);
            return applied.Count;
        }

        private static async Task<int> PostEmployeesAsync(
            ModelContext db,
            RestClient client,
            Institution institution,
            AttendanceWebsiteSyncResult result)
        {
            var empLog = (await LocalData.Instance.EmpLog_Post())
                .Take(LocalData.WebsiteSyncBatchSize).ToList();
            if (empLog.Count == 0)
                return 0;

            var request = new RestRequest("api/Attendance/{id}/Employees", Method.POST);
            request.AddUrlSegment("id", institution.SchoolID);
            ApiRequestHelper.AddAuthorizedJsonHeaders(request, institution.Token);
            ApiRequestHelper.AddCamelCaseJsonBody(request, AttendanceRecordMapper.ToApiPayload(empLog));

            var response = await client.ExecutePostTaskAsync(request);
            var syncResult = AttendanceSyncResponseParser.Parse(response);

            if (response.StatusCode == HttpStatusCode.OK &&
                (AttendanceSyncResponseParser.IsLegacyEmptyOk(response) ||
                 (syncResult != null && (syncResult.Matched > 0 || syncResult.Inserted > 0))))
            {
                var synced = FilterSyncedLogs(empLog, syncResult, syncResult);
                await MarkAttendanceRecordsAsync(db, synced, markSent: true, markUpdated: true);
                result.Changed = true;
                var partial = BuildPartialSyncMessage("Employee", empLog.Count, synced.Count, syncResult);
                if (!string.IsNullOrWhiteSpace(partial))
                    AddNote(result, partial);
                return synced.Count;
            }

            if (AttendanceSyncResponseParser.TryGetEmployeePostFailureMessage(
                response, syncResult, out var failureMessage))
            {
                AddNote(result, failureMessage);
            }

            return 0;
        }

        private static async Task<int> PutEmployeesAsync(
            ModelContext db,
            RestClient client,
            Institution institution,
            AttendanceWebsiteSyncResult result)
        {
            var empLogUpdate = (await LocalData.Instance.EmpLog_Put())
                .Take(LocalData.WebsiteSyncBatchSize).ToList();
            if (empLogUpdate.Count == 0)
                return 0;

            var request = new RestRequest("api/Attendance/{id}/EmployeesUpdate", Method.POST);
            request.AddUrlSegment("id", institution.SchoolID);
            ApiRequestHelper.AddAuthorizedJsonHeaders(request, institution.Token);
            ApiRequestHelper.AddCamelCaseJsonBody(request, AttendanceRecordMapper.ToApiPayload(empLogUpdate));

            var response = await client.ExecutePostTaskAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                AddNote(result, string.IsNullOrWhiteSpace(response.Content)
                    ? $"Employee update failed ({(int)response.StatusCode})"
                    : $"Employee update failed ({(int)response.StatusCode}): {response.Content}");
                return 0;
            }

            await MarkAttendanceRecordsAsync(db, empLogUpdate, markSent: true, markUpdated: true);
            result.Changed = true;
            return empLogUpdate.Count;
        }

        /// <summary>
        /// Marks uploaded rows as synced, reading them fresh from SQLite (not from the long-lived push
        /// context, which would return stale tracked copies). A row that changed while the request was in
        /// flight (e.g. the teacher's first punch landed on a row that was being uploaded as Abs) is NOT
        /// marked: it stays pending and is uploaded with its new entry time in the next cycle.
        /// </summary>
        private static async Task MarkAttendanceRecordsAsync(
            ModelContext db,
            IEnumerable<Attendance_Record> logs,
            bool markSent,
            bool markUpdated)
        {
            using (var fresh = new ModelContext())
            {
                foreach (var log in logs)
                {
                    if (!log.Is_OUT &&
                        !string.Equals(log.AttendanceStatus, "Abs", StringComparison.OrdinalIgnoreCase) &&
                        string.IsNullOrWhiteSpace(log.EntryTime))
                        continue;

                    var record = log.RecordID > 0
                        ? await fresh.attendance_Records.SingleOrDefaultAsync(r => r.RecordID == log.RecordID)
                        : null;

                    if (record == null)
                    {
                        record = await fresh.attendance_Records.FirstOrDefaultAsync(r =>
                            r.DeviceID == log.DeviceID &&
                            r.ScheduleID == log.ScheduleID &&
                            r.AttendanceDate == log.AttendanceDate);
                    }

                    if (record == null) continue;

                    if (!IsSameUploadedState(record, log))
                    {
                        StartupLogger.LogStage(
                            $"sync: row changed during upload, kept pending: device {record.DeviceID}, {record.AttendanceDate}, " +
                            $"sent {log.AttendanceStatus} {log.EntryTime}/{log.ExitTime}, now {record.AttendanceStatus} {record.EntryTime}/{record.ExitTime}");
                        continue;
                    }

                    if (markSent) record.Is_Sent = true;
                    if (markUpdated) record.Is_Updated = true;
                }

                await fresh.SaveChangesAsync();
            }
        }

        private static bool IsSameUploadedState(Attendance_Record current, Attendance_Record sent)
        {
            return current.ScheduleID == sent.ScheduleID &&
                   current.Is_OUT == sent.Is_OUT &&
                   string.Equals(current.AttendanceStatus ?? string.Empty, sent.AttendanceStatus ?? string.Empty,
                       StringComparison.OrdinalIgnoreCase) &&
                   SameTime(current.EntryTime, sent.EntryTime) &&
                   SameTime(current.ExitTime, sent.ExitTime);
        }

        private static bool SameTime(string left, string right)
        {
            var leftEmpty = ScheduleRoutingHelper.IsPlaceholderEntryTime(left);
            var rightEmpty = ScheduleRoutingHelper.IsPlaceholderEntryTime(right);
            if (leftEmpty || rightEmpty)
                return leftEmpty == rightEmpty;

            return ScheduleTimeHelper.TryParse(left, out var l) &&
                   ScheduleTimeHelper.TryParse(right, out var r) &&
                   l == r;
        }

        private static void AddNote(AttendanceWebsiteSyncResult result, string note)
        {
            if (result == null || string.IsNullOrWhiteSpace(note))
                return;
            result.Notes.Add(note.Trim());
        }

        private static List<Attendance_Record> FilterSyncedLogs(
            List<Attendance_Record> logs,
            AttendanceSyncResultDto syncResult,
            AttendanceSyncResultDto syncMeta = null)
        {
            if (logs == null || logs.Count == 0)
                return logs ?? new List<Attendance_Record>();

            syncMeta = syncMeta ?? syncResult;
            if (syncResult?.MatchedDeviceIds != null && syncResult.MatchedDeviceIds.Length > 0)
            {
                var matched = new HashSet<int>(syncResult.MatchedDeviceIds);
                var filtered = logs.Where(l => matched.Contains(l.DeviceID)).ToList();
                // Server said it matched/inserted rows but DeviceID list did not align — mark all sent rows.
                if (filtered.Count == 0 && syncMeta != null && (syncMeta.Matched > 0 || syncMeta.Inserted > 0))
                    return logs;
                return filtered;
            }

            return logs;
        }

        private static string BuildPartialSyncMessage(
            string entityLabel,
            int sentCount,
            int syncedCount,
            AttendanceSyncResultDto syncResult)
        {
            if (syncResult == null || string.IsNullOrWhiteSpace(syncResult.Message))
                return string.Empty;

            // Only surface server text when some rows did not sync.
            if (syncedCount >= sentCount)
                return string.Empty;

            return $"{entityLabel} partial sync ({syncedCount}/{sentCount}): {syncResult.Message}";
        }
    }
}
