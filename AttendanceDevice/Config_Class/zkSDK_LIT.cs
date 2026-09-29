using AttendanceDevice.Model;
using AttendanceDevice.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace AttendanceDevice.Config_Class
{
    public static class Machine
    {
        public static readonly int Number = 1;

        public static async Task SaveLogsOrAttendanceInPc(List<LogView> prevLog, List<LogView> todayLog, Institution institution, Device device, bool useSessionLedger = true)
        {
            var DuplicatePunchCountableMin = 10;

            LocalData.Instance.EnsureScheduleDataForPunch();
            await LocalData.Instance.EnsureScheduleBootstrapFromNetworkIfStaleAsync();

            var todayString = LocalData.Instance.GetAttendanceDateString();
            var todayDate = LocalData.Instance.GetAttendanceDate().Date;

            using (var db = new ModelContext())
            {
                var existingBackups = db.attendanceLog_Backups.ToList();
                var localDeviceIds = new HashSet<int>(db.Users.Select(u => u.DeviceID).ToList());

                // Previous-day punches inside the auto-push window (missed while the PC was off / the app was
                // restarting or updating) are applied to their own date below. Only older ones, or punches of
                // users unknown to this PC, stay in Device Data as "Not Current Data" (never duplicated).
                var recoverablePrev = new List<LogView>();
                foreach (var a in prevLog.OrderBy(l => l.EntryDateTime))
                {
                    var isRecoverable = institution.Is_Device_Attendance_Enable &&
                                        localDeviceIds.Contains(a.DeviceId) &&
                                        a.EntryDateTime.Date < todayDate &&
                                        LocalData.Instance.IsInRecoveryWindow(a.EntryDateTime.Date);
                    if (isRecoverable)
                    {
                        recoverablePrev.Add(a);
                        continue;
                    }

                    AddBackupIfMissing(db, existingBackups, a.DeviceId, a.EntryDate, a.EntryTime.ToString(), a.EntryDay, "Not Current Data");
                }

                if (!institution.Is_Device_Attendance_Enable)
                {
                    foreach (var a in todayLog)
                        AddBackupIfMissing(db, existingBackups, a.DeviceId, a.EntryDate, a.EntryTime.ToString(), a.EntryDay, "Device Attendance Disable");
                }
                else
                {
                    foreach (var log in recoverablePrev.Concat(todayLog.OrderBy(l => l.EntryDateTime)))
                    {
                        var dt = log.EntryDateTime;
                        var time = log.EntryTime;

                        var isToday = AttendanceDateHelper.DatesMatch(log.EntryDate, todayString);
                        if (!isToday && !recoverablePrev.Contains(log)) continue;

                        var user = await db.Users.FirstOrDefaultAsync(u => u.DeviceID == log.DeviceId);
                        if (user == null) continue;

                        // Same physical punch already processed this session (realtime or earlier import).
                        if (useSessionLedger && !PunchLedger.TryClaim(log.DeviceId, dt)) continue;

                        var activeSchedule = LocalData.Instance.ResolveScheduleForPunch(log.DeviceId, dt);

                        var isStuDisable = user.Is_Student && !institution.Is_Student_Attendance_Enable;
                        var isEmpDisable = !user.Is_Student && !institution.Is_Employee_Attendance_Enable;

                        // Backups are de-duplicated (same device, date and minute) so replays never pile up copies.
                        if (activeSchedule == null)
                        {
                            AddBackupIfMissing(db, existingBackups, log.DeviceId, log.EntryDate, dt.ToShortTimeString(), dt.ToString("dddd"), "No active schedule for this time");
                        }
                        else if (isStuDisable)
                        {
                            AddBackupIfMissing(db, existingBackups, log.DeviceId, log.EntryDate, dt.ToShortTimeString(), dt.ToString("dddd"), "Student Attendance Disable");
                        }
                        // Employee Attendance Disable
                        else if (isEmpDisable)
                        {
                            AddBackupIfMissing(db, existingBackups, log.DeviceId, log.EntryDate, dt.ToShortTimeString(), dt.ToString("dddd"), "Employee Attendance Disable");
                        }

                        //Holiday attendance disable (today's flag only; a past day's holiday is not known on the PC)
                        else if (isToday && institution.Is_Today_Holiday && !institution.Holiday_NotActive)
                        {
                            AddBackupIfMissing(db, existingBackups, log.DeviceId, log.EntryDate, dt.ToShortTimeString(), dt.ToString("dddd"), "Holiday attendance disable");
                        }
                        // A previous day is applied only to a schedule that was ON for that weekday.
                        else if (!isToday &&
                                 !(activeSchedule.Is_OnDay && ScheduleDayHelper.IsSameDay(activeSchedule.Day, dt)))
                        {
                            AddBackupIfMissing(db, existingBackups, log.DeviceId, log.EntryDate, log.EntryTime.ToString(), log.EntryDay, "Not Current Data (schedule off that day)");
                        }
                        // Insert or Update Attendance Records
                        else
                        {
                            var deviceRecords = await db.attendance_Records
                                .Where(a => a.DeviceID == log.DeviceId)
                                .ToListAsync();
                            var todayDeviceRecords = deviceRecords
                                .Where(a => AttendanceDateHelper.DatesMatch(a.AttendanceDate, log.EntryDate))
                                .ToList();
                            var attRecord = todayDeviceRecords.FirstOrDefault(a => a.ScheduleID == activeSchedule.ScheduleID);
                            if (!LocalData.TryGetScheduleTimes(activeSchedule, out var sStartTime, out var sLateTime, out var sEndTime))
                                continue;

                            // A punch that already closed/opened another schedule must never also open this one.
                            var opensEntry = attRecord == null || ScheduleRoutingHelper.ShouldApplyAsFirstEntry(
                                attRecord.AttendanceStatus, attRecord.EntryTime, attRecord.Is_OUT);
                            if (opensEntry && ScheduleRoutingHelper.IsPunchAlreadyAppliedToOtherSchedule(
                                    todayDeviceRecords, activeSchedule.ScheduleID, time))
                            {
                                StartupLogger.LogStage($"log punch ignored (already applied to another schedule): device {log.DeviceId} at {dt:HH:mm:ss}, target schedule {activeSchedule.ScheduleID}");
                                continue;
                            }

                            if (attRecord == null)
                            {
                                attRecord = new Attendance_Record
                                {
                                    AttendanceDate = log.EntryDate,
                                    DeviceID = log.DeviceId,
                                    ScheduleID = activeSchedule.ScheduleID,
                                    EntryTime = time.ToString()
                                };

                                if (time > sEndTime)
                                {
                                    //Enroll after end time (as first enroll)
                                    attRecord.AttendanceStatus = "Late Abs";
                                }
                                else
                                {
                                    attRecord.AttendanceStatus = ScheduleRoutingHelper.ClassifyEntryStatus(
                                        time, sStartTime, sLateTime, sEndTime);

                                }
                                attRecord.Is_Sent = false;
                                attRecord.Is_Updated = false;
                                db.attendance_Records.Add(attRecord);
                                db.Entry(attRecord).State = EntityState.Added;
                            }
                            else
                            {
                                var isDuplicatePunch = false;
                                var hasNoEntryYet = ScheduleRoutingHelper.ShouldApplyAsFirstEntry(
                                    attRecord.AttendanceStatus, attRecord.EntryTime, attRecord.Is_OUT);

                                if (!hasNoEntryYet)
                                {
                                    if (attRecord.Is_OUT)
                                    {
                                        if (TimeSpan.TryParse(attRecord.ExitTime, out var previousOutTime))
                                        {
                                            isDuplicatePunch = previousOutTime.TotalMinutes + DuplicatePunchCountableMin > time.TotalMinutes;
                                        }
                                    }
                                    else if (ScheduleTimeHelper.TryParse(attRecord.EntryTime, out var previousTime))
                                    {
                                        isDuplicatePunch = previousTime.TotalMinutes + DuplicatePunchCountableMin > time.TotalMinutes;
                                    }
                                }

                                if (!hasNoEntryYet && isDuplicatePunch)
                                {
                                    StartupLogger.LogStage($"log punch ignored (duplicate within {DuplicatePunchCountableMin} min): device {log.DeviceId} at {dt:HH:mm:ss}, schedule {activeSchedule.ScheduleID}");
                                }

                                if (hasNoEntryYet || !isDuplicatePunch)
                                {
                                    if (hasNoEntryYet)
                                    {
                                        attRecord.EntryTime = time.ToString();

                                        attRecord.AttendanceStatus = ScheduleRoutingHelper.ClassifyEntryStatus(
                                    time, sStartTime, sLateTime, sEndTime);

                                        attRecord.Is_Sent = false;
                                        attRecord.Is_Updated = false;
                                    }
                                    else if (attRecord.AttendanceStatus == "Leave")
                                    {
                                        // no insert
                                    }
                                    else
                                    {
                                        if (time > sLateTime && time < sEndTime)
                                        {
                                            attRecord.ExitStatus = "Early Leave";
                                        }
                                        else if (time > sEndTime)
                                        {
                                            attRecord.ExitStatus = "Out";
                                        }

                                        if (!ScheduleRoutingHelper.CanMarkExit(
                                                time,
                                                sStartTime,
                                                sEndTime,
                                                activeSchedule.StartTime,
                                                attRecord.EntryTime))
                                            continue;

                                        attRecord.Is_Updated = false;
                                        attRecord.ExitTime = time.ToString();
                                        attRecord.Is_OUT = true;
                                    }

                                    db.Entry(attRecord).State = EntityState.Modified;
                                }
                            }

                            // Persist before the next punch: ResolveScheduleForPunch reads committed rows.
                            await db.SaveChangesAsync();
                        }

                    }

                    await db.SaveChangesAsync();
                }

                //Device last update time record
                prevLog.AddRange(todayLog);
                var maxDateTime = DateTime.Now;


                if (prevLog.Count > 0)
                {
                    maxDateTime = prevLog.Max(l => l.EntryDateTime).AddSeconds(1);

                }

                if (device != null)
                {
                    device.Last_Down_Log_Time = maxDateTime.ToString("yyyy-MM-dd HH:mm:ss");

                    db.Devices.Add(device);
                    db.Entry(device).State = EntityState.Modified;

                    await db.SaveChangesAsync();
                }
            }
        }

        private static void AddBackupIfMissing(
            ModelContext db,
            List<AttendanceLog_Backup> existingBackups,
            int deviceId,
            string entryDate,
            string entryTime,
            string entryDay,
            string reason)
        {
            ScheduleTimeHelper.TryParse(entryTime, out var time);
            var minute = new TimeSpan(time.Hours, time.Minutes, 0);

            var exists = existingBackups.Any(b =>
                b.DeviceID == deviceId &&
                AttendanceDateHelper.DatesMatch(b.Entry_Date, entryDate) &&
                ScheduleTimeHelper.TryParse(b.Entry_Time, out var t) &&
                new TimeSpan(t.Hours, t.Minutes, 0) == minute);
            if (exists)
                return;

            var backup = new AttendanceLog_Backup
            {
                DeviceID = deviceId,
                Entry_Date = entryDate,
                Entry_Time = entryTime,
                Entry_Day = entryDay,
                Backup_Reason = reason
            };
            existingBackups.Add(backup);
            db.attendanceLog_Backups.Add(backup);
        }

        internal sealed class BackupReplayResult
        {
            public int Applied { get; set; }
            public int StillWaiting { get; set; }
            public SortedDictionary<DateTime, int> AppliedByDate { get; } = new SortedDictionary<DateTime, int>();

            public string AppliedByDateText =>
                AppliedByDate.Count == 0
                    ? string.Empty
                    : " (" + string.Join(", ", AppliedByDate.Select(kv => $"{kv.Key:dd-MMM}: {kv.Value}")) + ")";
        }

        /// <summary>
        /// Re-apply Device Data backups (today's "No active schedule" and "Not Current Data" punches of the
        /// last <see cref="LocalData.AutoPushWindowDays"/> days) as attendance of their own date.
        /// </summary>
        public static async Task<int> ReplayRejectedLogsAsAttendance(
            Institution institution,
            bool refreshSchedulesFromNetwork = true)
        {
            var result = await ReplayBackupLogsAsync(institution, refreshSchedulesFromNetwork);
            return result.Applied;
        }

        internal static async Task<BackupReplayResult> ReplayBackupLogsAsync(
            Institution institution,
            bool refreshSchedulesFromNetwork = true)
        {
            var result = new BackupReplayResult();
            if (institution == null)
                return result;

            LocalData.Instance.EnsureScheduleDataForPunch();
            if (refreshSchedulesFromNetwork)
                await LocalData.Instance.EnsureScheduleBootstrapAsync(forceNetwork: true);

            var today = LocalData.Instance.GetAttendanceDate().Date;
            var todayLogs = new List<LogView>();
            var pastLogs = new List<LogView>();
            var candidateIds = new HashSet<int>();

            using (var db = new ModelContext())
            {
                foreach (var backup in db.attendanceLog_Backups.ToList())
                {
                    if (!AttendanceDateHelper.TryParse(backup.Entry_Date, out var date))
                        continue;
                    if (!LocalData.Instance.IsInRecoveryWindow(date))
                        continue;

                    var reason = backup.Backup_Reason ?? string.Empty;
                    var replayable = string.IsNullOrWhiteSpace(reason) ||
                                     reason.IndexOf("No active schedule", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                     reason.IndexOf("Not Current Data", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (!replayable)
                        continue;

                    if (!ScheduleTimeHelper.TryParse(backup.Entry_Time, out var time))
                        continue;

                    candidateIds.Add(backup.LogId);
                    var log = new LogView
                    {
                        DeviceId = backup.DeviceID,
                        EntryTime = time,
                        EntryDate = date.ToString("dd-MMM-yy"),
                        EntryDay = date.ToString("dddd"),
                        EntryDateTime = date.Date.Add(time)
                    };

                    if (date.Date == today)
                        todayLogs.Add(log);
                    else
                        pastLogs.Add(log);
                }
            }

            if (!candidateIds.Any())
                return result;

            await SaveLogsOrAttendanceInPc(
                pastLogs.OrderBy(l => l.EntryDateTime).ToList(),
                todayLogs.OrderBy(l => l.EntryDateTime).ToList(),
                institution,
                null,
                useSessionLedger: false);

            using (var db = new ModelContext())
            {
                var records = db.attendance_Records.ToList();
                var remove = new List<AttendanceLog_Backup>();
                foreach (var backup in db.attendanceLog_Backups.ToList())
                {
                    if (!candidateIds.Contains(backup.LogId))
                        continue;

                    if (!LocalData.IsBackupReflectedInRecord(backup, records, requireSynced: false))
                    {
                        result.StillWaiting++;
                        continue;
                    }

                    result.Applied++;
                    if (AttendanceDateHelper.TryParse(backup.Entry_Date, out var date))
                    {
                        result.AppliedByDate.TryGetValue(date.Date, out var n);
                        result.AppliedByDate[date.Date] = n + 1;
                    }

                    // Keep the backup until the row holding this punch is confirmed by the website.
                    if (LocalData.IsBackupReflectedInRecord(backup, records, requireSynced: true))
                        remove.Add(backup);
                }

                if (remove.Any())
                {
                    db.attendanceLog_Backups.RemoveRange(remove);
                    await db.SaveChangesAsync();
                }
            }

            return result;
        }

        public static List<Attendance_view> GetDailyAttendanceRecords(AttType attType)
        {
            var attendanceRecords = new List<Attendance_view>();
            var imageLink = LocalData.Instance.institution?.Image_Link;
            using (var db = new ModelContext())
            {
                var q = from a in db.attendance_Records
                        join u in db.Users
                            on a.DeviceID equals u.DeviceID
                        select new Attendance_view
                        {
                            DeviceID = u.DeviceID,
                            ID = u.ID,
                            Name = u.Name,
                            Designation = u.Designation,
                            AttendanceStatus = a.AttendanceStatus,
                            AttendanceDate = a.AttendanceDate,
                            Is_OUT = a.Is_OUT,
                            Is_Student = u.Is_Student,
                            EntryTime = a.EntryTime,
                            ExitTime = a.ExitTime
                        };

                switch (attType)
                {
                    case AttType.All:
                        attendanceRecords = q.ToList();
                        break;
                    case AttType.AllStudent:
                        attendanceRecords = q.Where(a => a.Is_Student)
                            .ToList();
                        break;
                    case AttType.StudentIn:
                        attendanceRecords = q.Where(a => a.Is_Student && !a.Is_OUT)
                            .ToList();
                        break;
                    case AttType.StudentOut:
                        attendanceRecords = q.Where(a => a.Is_Student && a.Is_OUT)
                            .ToList();
                        break;
                    case AttType.AllEmployee:
                        attendanceRecords = q.Where(a => !a.Is_Student)
                            .ToList();
                        break;
                    case AttType.EmployeeIn:
                        attendanceRecords = q.Where(a => !a.Is_Student && !a.Is_OUT)
                            .ToList();
                        break;
                    case AttType.EmployeeOut:
                        attendanceRecords = q.Where(a => !a.Is_Student && a.Is_OUT)
                            .ToList();
                        break;
                    case AttType.AllIn:
                        attendanceRecords = q.Where(a => a.AttendanceStatus != "Abs" && !a.Is_OUT)
                            .ToList();
                        break;
                    case AttType.AllOut:
                        attendanceRecords = q.Where(a => a.Is_OUT)
                            .ToList();
                        break;
                }
            }


            return attendanceRecords
                .Where(a => AttendanceDateHelper.IsSameDay(a.AttendanceDate, DateTime.Today))
                .OrderByDescending(a => a.EntryTime)
                .ThenBy(a => a.ID)
                .Select(a =>
                {
                    a.ImgLink = UserPhotoHelper.ResolvePhotoUri(imageLink, a.ID);
                    a.EntryTime = ScheduleTimeHelper.FormatDisplayTime(a.EntryTime);
                    a.ExitTime = ScheduleTimeHelper.FormatDisplayTime(a.ExitTime);

                    return a;
                })
                .ToList();
        }
    }
}
