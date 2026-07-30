using Attendance_API.DB_Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;

namespace Attendance_API.Helpers
{
    internal static class AttendanceSmsQueueWriter
    {
        public static StudentSmsQueueResult SaveQueueRows(EduContext db, List<Attendance_SMS> smsRows)
        {
            var result = new StudentSmsQueueResult();
            if (smsRows == null || smsRows.Count == 0)
                return result;

            result.Eligible = smsRows.Count;

            try
            {
                db.Attendance_sms.AddRange(smsRows);
                db.SaveChanges();
            }
            catch (Exception ex) when (IsIgnorableInsteadOfInsertException(ex))
            {
                DetachSmsRows(db, smsRows);
            }
            catch (Exception ex)
            {
                DetachSmsRows(db, smsRows);
                result.SaveError = ex.InnerException?.Message ?? ex.Message;
            }

            result.Queued = CountPersistedQueueRows(db, smsRows);

            if (result.Queued >= smsRows.Count || !string.IsNullOrWhiteSpace(result.SaveError))
                return result;

            foreach (var row in smsRows.Where(r => !IsPersisted(db, r)))
            {
                try
                {
                    InsertQueueRowRaw(db, row);
                }
                catch (Exception ex)
                {
                    result.SaveError = ex.InnerException?.Message ?? ex.Message;
                }
            }

            result.Queued = CountPersistedQueueRows(db, smsRows);
            return result;
        }

        public static bool HasQueuedSmsToday(
            EduContext db,
            int schoolId,
            int studentId,
            int employeeId,
            DateTime attendanceDate,
            string attendanceStatus)
        {
            if (string.IsNullOrWhiteSpace(attendanceStatus))
                return false;

            var date = attendanceDate.Date;

            if (studentId > 0)
            {
                return db.Attendance_sms.Any(s =>
                    s.SchoolID == schoolId &&
                    s.StudentID == studentId &&
                    s.AttendanceDate == date &&
                    s.AttendanceStatus == attendanceStatus);
            }

            if (employeeId > 0)
            {
                return db.Attendance_sms.Any(s =>
                    s.SchoolID == schoolId &&
                    s.EmployeeID == employeeId &&
                    s.AttendanceDate == date &&
                    s.AttendanceStatus == attendanceStatus);
            }

            return false;
        }

        public static bool HasSentAttendanceSmsToday(
            EduContext db,
            int schoolId,
            int studentId,
            int employeeId,
            DateTime attendanceDate,
            string smsText)
        {
            if (string.IsNullOrWhiteSpace(smsText))
                return false;

            var date = attendanceDate.Date;

            return db.SMS_Send_Record.Any(sr =>
                sr.PurposeOfSMS == "Device Attendance" &&
                sr.TextSMS == smsText &&
                DbFunctions.TruncateTime(sr.Date) == date &&
                db.SMS_OtherInfo.Any(o =>
                    o.SMS_Send_ID == sr.SMS_Send_ID &&
                    o.SchoolID == schoolId &&
                    ((studentId > 0 && o.StudentID == studentId) ||
                     (employeeId > 0 && o.TeacherID == employeeId))));
        }

        public static bool ShouldSkipQueueToday(
            EduContext db,
            int schoolId,
            int studentId,
            int employeeId,
            DateTime attendanceDate,
            string attendanceStatus,
            string smsText)
        {
            return HasQueuedSmsToday(db, schoolId, studentId, employeeId, attendanceDate, attendanceStatus)
                || HasSentAttendanceSmsToday(db, schoolId, studentId, employeeId, attendanceDate, smsText);
        }

        private static int CountPersistedQueueRows(EduContext db, IEnumerable<Attendance_SMS> smsRows)
        {
            return smsRows.Count(r => IsPersisted(db, r));
        }

        private static bool IsPersisted(EduContext db, Attendance_SMS row)
        {
            if (row == null)
                return false;

            var date = row.AttendanceDate.Date;

            if (row.StudentID > 0)
            {
                return db.Attendance_sms.Any(s =>
                    s.SchoolID == row.SchoolID &&
                    s.StudentID == row.StudentID &&
                    s.AttendanceDate == date &&
                    s.AttendanceStatus == row.AttendanceStatus);
            }

            if (row.EmployeeID > 0)
            {
                return db.Attendance_sms.Any(s =>
                    s.SchoolID == row.SchoolID &&
                    s.EmployeeID == row.EmployeeID &&
                    s.AttendanceDate == date &&
                    s.AttendanceStatus == row.AttendanceStatus);
            }

            return false;
        }

        private static void InsertQueueRowRaw(EduContext db, Attendance_SMS row)
        {
            const string sql = @"
INSERT INTO dbo.Attendance_SMS
(
    SchoolID, StudentID, EmployeeID, CreateTime, SentTime, ScheduleTime,
    AttendanceDate, SMS_Text, MobileNo, AttendanceStatus, SMS_TimeOut, Is_Send, InsertDate
)
VALUES
(
    @SchoolID, @StudentID, @EmployeeID, @CreateTime, @SentTime, @ScheduleTime,
    @AttendanceDate, @SMS_Text, @MobileNo, @AttendanceStatus, @SMS_TimeOut, @Is_Send, @InsertDate
)";

            db.Database.ExecuteSqlCommand(
                sql,
                new SqlParameter("@SchoolID", row.SchoolID),
                new SqlParameter("@StudentID", row.StudentID),
                new SqlParameter("@EmployeeID", row.EmployeeID),
                new SqlParameter("@CreateTime", row.CreateTime),
                new SqlParameter("@SentTime", (object)row.SentTime ?? DBNull.Value),
                new SqlParameter("@ScheduleTime", row.ScheduleTime),
                new SqlParameter("@AttendanceDate", row.AttendanceDate.Date),
                new SqlParameter("@SMS_Text", (object)row.SMS_Text ?? DBNull.Value),
                new SqlParameter("@MobileNo", (object)row.MobileNo ?? DBNull.Value),
                new SqlParameter("@AttendanceStatus", (object)row.AttendanceStatus ?? DBNull.Value),
                new SqlParameter("@SMS_TimeOut", row.SMS_TimeOut),
                new SqlParameter("@Is_Send", row.Is_Send),
                new SqlParameter("@InsertDate", row.InsertDate == default(DateTime) ? DateTime.Now : row.InsertDate));
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
