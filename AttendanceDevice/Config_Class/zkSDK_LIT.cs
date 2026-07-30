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

        public static async Task SaveLogsOrAttendanceInPc(List<LogView> prevLog, List<LogView> todayLog, Institution institution, Device device)
        {
            var DuplicatePunchCountableMin = 10;

            await LocalData.Instance.EnsureScheduleBootstrapAsync();

            using (var db = new ModelContext())
            {
                var previousLogs = prevLog.Select(a => new AttendanceLog_Backup
                {
                    DeviceID = a.DeviceId,
                    Entry_Date = a.EntryDate,
                    Entry_Time = a.EntryTime.ToString(),
                    Entry_Day = a.EntryDay,
                    Backup_Reason = "Not Current Data"
                });

                db.attendanceLog_Backups.AddRange(previousLogs);

                if (!institution.Is_Device_Attendance_Enable)
                {
                    var logs = todayLog.Select(a => new AttendanceLog_Backup
                    {
                        DeviceID = a.DeviceId,
                        Entry_Date = a.EntryDate,
                        Entry_Time = a.EntryTime.ToString(),
                        Entry_Day = a.EntryDay,
                        Backup_Reason = "Device Attendance Disable"
                    });

                    db.attendanceLog_Backups.AddRange(logs);
                }
                else
                {
                    foreach (var log in todayLog)
                    {
                        var dt = log.EntryDateTime;
                        var time = log.EntryTime;

                        if (log.EntryDate != DateTime.Today.ToString("dd-MMM-yy")) continue;

                        var user = await db.Users.FirstOrDefaultAsync(u => u.DeviceID == log.DeviceId);
                        if (user == null) continue;

                        var activeSchedule = LocalData.Instance.ResolveScheduleForPunch(log.DeviceId, dt);

                        var isStuDisable = user.Is_Student && !institution.Is_Student_Attendance_Enable;
                        var isEmpDisable = !user.Is_Student && !institution.Is_Employee_Attendance_Enable;

                        if (activeSchedule == null)
                        {
                            var entryTime = dt.ToShortTimeString();
                            var alreadyBackedUp = await db.attendanceLog_Backups.AnyAsync(b =>
                                b.DeviceID == log.DeviceId &&
                                b.Entry_Date == log.EntryDate &&
                                b.Entry_Time == entryTime);

                            if (!alreadyBackedUp)
                            {
                                var logBackup = new AttendanceLog_Backup()
                                {
                                    DeviceID = log.DeviceId,
                                    Entry_Date = log.EntryDate,
                                    Entry_Time = entryTime,
                                    Entry_Day = dt.ToString("dddd"),
                                    Backup_Reason = "No active schedule for this time"
                                };

                                db.attendanceLog_Backups.Add(logBackup);
                            }
                        }
                        else if (isStuDisable)
                        {
                            var logBackup = new AttendanceLog_Backup()
                            {
                                DeviceID = log.DeviceId,
                                Entry_Date = log.EntryDate,
                                Entry_Time = dt.ToShortTimeString(),
                                Entry_Day = dt.ToString("dddd"),
                                Backup_Reason = "Student Attendance Disable"
                            };

                            db.attendanceLog_Backups.Add(logBackup);
                        }
                        // Employee Attendance Disable
                        else if (isEmpDisable)
                        {
                            var logBackup = new AttendanceLog_Backup()
                            {
                                DeviceID = log.DeviceId,
                                Entry_Date = log.EntryDate,
                                Entry_Time = dt.ToShortTimeString(),
                                Entry_Day = dt.ToString("dddd"),
                                Backup_Reason = "Employee Attendance Disable"
                            };

                            db.attendanceLog_Backups.Add(logBackup);
                        }

                        //Holiday attendance disable
                        else if (institution.Is_Today_Holiday && !institution.Holiday_NotActive)
                        {
                            var logBackup = new AttendanceLog_Backup()
                            {
                                DeviceID = log.DeviceId,
                                Entry_Date = log.EntryDate,
                                Entry_Time = dt.ToShortTimeString(),
                                Entry_Day = dt.ToString("dddd"),
                                Backup_Reason = "Holiday attendance disable"
                            };

                            db.attendanceLog_Backups.Add(logBackup);

                        }
                        // Insert or Update Attendance Records
                        else
                        {
                            var attRecords = await db.attendance_Records
                                .Where(a => a.DeviceID == log.DeviceId && a.ScheduleID == activeSchedule.ScheduleID)
                                .ToListAsync();
                            var attRecord = attRecords.FirstOrDefault(a => AttendanceDateHelper.DatesMatch(a.AttendanceDate, log.EntryDate));
                            if (!LocalData.TryGetScheduleTimes(activeSchedule, out var sStartTime, out var sLateTime, out var sEndTime))
                                continue;

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
                                    if (time <= sStartTime)
                                    {
                                        attRecord.AttendanceStatus = "Pre";
                                    }
                                    else if (time <= sLateTime)
                                    {
                                        attRecord.AttendanceStatus = "Late";
                                    }
                                    else if (time <= sEndTime)
                                    {
                                        attRecord.AttendanceStatus = "Late Abs";
                                    }

                                }
                                attRecord.Is_Sent = false;
                                attRecord.Is_Updated = false;
                                db.attendance_Records.Add(attRecord);
                                db.Entry(attRecord).State = EntityState.Added;
                            }
                            else
                            {
                                var isDuplicatePunch = false;
                                var hasNoEntryYet = string.IsNullOrWhiteSpace(attRecord.EntryTime) && !attRecord.Is_OUT;

                                if (!hasNoEntryYet)
                                {
                                    if (attRecord.Is_OUT)
                                    {
                                        if (TimeSpan.TryParse(attRecord.ExitTime, out var previousOutTime))
                                        {
                                            isDuplicatePunch = previousOutTime.TotalMinutes + DuplicatePunchCountableMin > time.TotalMinutes;
                                        }
                                    }
                                    else if (TimeSpan.TryParse(attRecord.EntryTime, out var previousTime))
                                    {
                                        isDuplicatePunch = previousTime.TotalMinutes + DuplicatePunchCountableMin > time.TotalMinutes;
                                    }
                                }

                                if (hasNoEntryYet || !isDuplicatePunch)
                                {
                                    if (hasNoEntryYet || attRecord.AttendanceStatus == "Abs")
                                    {
                                        attRecord.EntryTime = time.ToString();

                                        if (time > sEndTime)
                                            attRecord.AttendanceStatus = "Late Abs";
                                        else if (time <= sStartTime)
                                            attRecord.AttendanceStatus = "Pre";
                                        else if (time <= sLateTime)
                                            attRecord.AttendanceStatus = "Late";
                                        else
                                            attRecord.AttendanceStatus = "Late Abs";

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

                                        attRecord.Is_Updated = false;
                                        attRecord.ExitTime = time.ToString();
                                        attRecord.Is_OUT = true;
                                    }

                                    db.Entry(attRecord).State = EntityState.Modified;
                                }
                            }

                        }

                        await db.SaveChangesAsync();
                    }
                }

                //Device last update time record
                prevLog.AddRange(todayLog);
                var maxDateTime = DateTime.Now;


                if (prevLog.Count > 0)
                {
                    maxDateTime = prevLog.Max(l => l.EntryDateTime).AddSeconds(1);

                }

                device.Last_Down_Log_Time = maxDateTime.ToString("yyyy-MM-dd HH:mm:ss");

                db.Devices.Add(device);
                db.Entry(device).State = EntityState.Modified;

                await db.SaveChangesAsync();
            }
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
                        attendanceRecords = q.Where(a => a.Is_Student && !a.Is_OUT)
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
