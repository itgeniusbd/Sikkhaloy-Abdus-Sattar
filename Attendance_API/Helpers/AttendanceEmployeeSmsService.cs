using Attendance_API.DB_Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Attendance_API.Helpers
{
    internal static class AttendanceEmployeeSmsService
    {
        public static bool HasQueuedSmsToday(
            EduContext db,
            int schoolId,
            int employeeId,
            DateTime attendanceDate,
            string attendanceStatus,
            TimeSpan? scheduleTime = null)
        {
            if (employeeId <= 0 || string.IsNullOrWhiteSpace(attendanceStatus))
                return false;

            return AttendanceSmsQueueWriter.HasQueuedSmsToday(
                db,
                schoolId,
                0,
                employeeId,
                attendanceDate,
                attendanceStatus);
        }

        public static StudentSmsQueueResult QueueMissingEmployeeSmsForToday(
            EduContext db,
            int schoolId,
            Attendance_Device_Setting settings,
            string schoolName)
        {
            if (settings == null)
                return new StudentSmsQueueResult();

            var today = DateTime.Today;
            var records = db.Employee_Attendance_Records
                .Where(r => r.SchoolID == schoolId
                    && r.EmployeeID > 0
                    && DbFunctions.TruncateTime(r.AttendanceDate) == today)
                .ToList();

            return QueueEmployeeSmsDetailed(db, schoolId, settings, schoolName, records);
        }

        public static int QueueEmployeeSms(
            EduContext db,
            int schoolId,
            Attendance_Device_Setting settings,
            string schoolName,
            IEnumerable<Employee_Attendance_Record> records)
        {
            return QueueEmployeeSmsDetailed(db, schoolId, settings, schoolName, records).Queued;
        }

        public static StudentSmsQueueResult QueueEmployeeSmsDetailed(
            EduContext db,
            int schoolId,
            Attendance_Device_Setting settings,
            string schoolName,
            IEnumerable<Employee_Attendance_Record> records)
        {
            var result = new StudentSmsQueueResult();

            if (settings == null || records == null)
                return result;

            if (!settings.Is_All_SMS_On || !settings.Is_Employee_SMS_Active)
                return result;

            var recordList = records.Where(r => r != null).ToList();
            if (!recordList.Any())
                return result;

            var templateHelper = new AttendanceSmsTemplateHelper(schoolId);
            var empSettings = LoadEmployeeSmsSettings(db, schoolId);
            var smsRows = new List<Attendance_SMS>();

            foreach (var record in recordList)
            {
                if (record.EmployeeID <= 0)
                {
                    result.Skipped++;
                    continue;
                }

                var empSetting = ResolveEmployeeSetting(empSettings, settings, record);
                if (empSetting == null)
                {
                    result.Skipped++;
                    continue;
                }

                var scheduleId = record.Attendance_ScheduleID ?? 0;
                if (scheduleId == 0 && empSetting.ScheduleID > 0)
                    scheduleId = empSetting.ScheduleID;

                var attendanceType = record.AttendanceStatus ?? string.Empty;
                if (!ShouldSend(settings, empSetting, record, attendanceType))
                {
                    result.Skipped++;
                    continue;
                }

                result.Eligible++;

                if (HasQueuedSmsToday(
                        db,
                        schoolId,
                        record.EmployeeID,
                        record.AttendanceDate,
                        attendanceType,
                        GetScheduleTime(record, empSetting, attendanceType)))
                {
                    result.Skipped++;
                    continue;
                }

                var scheduleName = templateHelper.GetScheduleName(
                    scheduleId > 0 ? (int?)scheduleId : record.Attendance_ScheduleID,
                    0);
                var toOwnNumber = settings.Is_Employee_SMS_OwnNumber;
                var smsText = templateHelper.BuildEmployeeMessage(
                    attendanceType,
                    empSetting.Name,
                    schoolName,
                    record.AttendanceDate,
                    record.EntryTime,
                    empSetting.StartTime,
                    scheduleName,
                    settings.Is_English_SMS,
                    toOwnNumber);

                var mobileNo = ResolveMobileNo(settings, empSetting);
                if (string.IsNullOrWhiteSpace(smsText) || string.IsNullOrWhiteSpace(mobileNo))
                {
                    result.Skipped++;
                    continue;
                }

                if (AttendanceSmsQueueWriter.ShouldSkipQueueToday(
                        db,
                        schoolId,
                        0,
                        record.EmployeeID,
                        record.AttendanceDate,
                        attendanceType,
                        smsText))
                {
                    result.Skipped++;
                    continue;
                }

                var now = DateTime.Now;
                var scheduleTime = AttendanceSmsScheduleHelper.EnsureSendableScheduleTime(
                    GetScheduleTime(record, empSetting, attendanceType),
                    settings.SMS_TimeOut_Minute);

                smsRows.Add(new Attendance_SMS
                {
                    SchoolID = schoolId,
                    StudentID = 0,
                    EmployeeID = record.EmployeeID,
                    CreateTime = now.TimeOfDay,
                    ScheduleTime = scheduleTime,
                    AttendanceDate = record.AttendanceDate.Date,
                    SMS_Text = smsText,
                    MobileNo = mobileNo,
                    AttendanceStatus = attendanceType,
                    SMS_TimeOut = settings.SMS_TimeOut_Minute,
                    Is_Send = false,
                    InsertDate = now
                });
            }

            if (smsRows.Count == 0)
                return result;

            var saveResult = AttendanceSmsQueueWriter.SaveQueueRows(db, smsRows);
            result.Queued = saveResult.Queued;
            result.SaveError = saveResult.SaveError;

            return result;
        }

        private static List<VW_Attendance_Emp_Setting> LoadEmployeeSmsSettings(EduContext db, int schoolId)
        {
            return db.VW_Attendance_Emp_Settings
                .AsNoTracking()
                .Where(s => s.SchoolID == schoolId)
                .ToList();
        }

        private static VW_Attendance_Emp_Setting ResolveEmployeeSetting(
            List<VW_Attendance_Emp_Setting> empSettings,
            Attendance_Device_Setting settings,
            Employee_Attendance_Record record)
        {
            var candidates = empSettings
                .Where(s => s.EmployeeID == record.EmployeeID)
                .ToList();

            if (!candidates.Any())
                return null;

            var scheduleId = record.Attendance_ScheduleID ?? 0;
            if (scheduleId > 0)
            {
                var exact = candidates.FirstOrDefault(s => s.ScheduleID == scheduleId);
                if (exact != null)
                    return exact;
            }

            var attendanceType = record.AttendanceStatus ?? string.Empty;
            var eligible = candidates.FirstOrDefault(s => ShouldSend(settings, s, record, attendanceType));
            if (eligible != null)
                return eligible;

            return candidates.First();
        }

        private static string ResolveMobileNo(
            Attendance_Device_Setting settings,
            VW_Attendance_Emp_Setting empSetting)
        {
            if (settings.Is_Employee_SMS_OwnNumber)
                return string.IsNullOrWhiteSpace(empSetting?.Phone) ? null : empSetting.Phone.Trim();

            return string.IsNullOrWhiteSpace(settings.Employee_SMS_Number)
                ? null
                : settings.Employee_SMS_Number.Trim();
        }

        private static void DetachSmsRows(EduContext db, List<Attendance_SMS> smsRows)
        {
            foreach (var row in smsRows)
            {
                var entry = db.Entry(row);
                if (entry != null)
                    entry.State = EntityState.Detached;
            }
        }

        private static bool ShouldSend(
            Attendance_Device_Setting settings,
            VW_Attendance_Emp_Setting empSetting,
            Employee_Attendance_Record record,
            string attendanceType)
        {
            if (record.Is_OUT)
                return false;

            switch (attendanceType)
            {
                case "Abs":
                case "Late Abs":
                    return settings.Is_Employee_Abs_SMS_ON && empSetting.Is_Abs_SMS;
                case "Late":
                    return settings.Is_Employee_Late_SMS_ON && empSetting.Is_Late_SMS;
                default:
                    return false;
            }
        }

        private static TimeSpan GetScheduleTime(
            Employee_Attendance_Record record,
            VW_Attendance_Emp_Setting empSetting,
            string attendanceType)
        {
            switch (attendanceType)
            {
                case "Abs":
                    return empSetting.LateEntryTime;
                case "Late Abs":
                    return record.EntryTime ?? empSetting.LateEntryTime;
                case "Late":
                    return record.EntryTime ?? empSetting.LateEntryTime;
                default:
                    return record.EntryTime ?? empSetting.StartTime;
            }
        }

        private static bool IsIgnorableInsteadOfInsertException(Exception exception)
        {
            for (var ex = exception; ex != null; ex = ex.InnerException)
            {
                if (ex.Message.IndexOf("committed successfully", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
                if (ex.Message.IndexOf("unexpected number of rows", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }
    }
}
