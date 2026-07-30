using Attendance_API.DB_Model;

using System;

using System.Collections.Generic;

using System.Data.Entity;

using System.Data.SqlClient;

using System.Linq;



namespace Attendance_API.Helpers

{

    internal sealed class StudentSmsQueueResult

    {

        public int Queued { get; set; }

        public int Eligible { get; set; }

        public int Skipped { get; set; }

        public string SaveError { get; set; }

    }



    internal static class AttendanceStudentSmsService

    {

        public static bool HasQueuedSmsToday(

            EduContext db,

            int schoolId,

            int studentId,

            DateTime attendanceDate,

            string attendanceStatus,

            TimeSpan? scheduleTime = null)

        {

            if (studentId <= 0 || string.IsNullOrWhiteSpace(attendanceStatus))

                return false;



            return AttendanceSmsQueueWriter.HasQueuedSmsToday(
                db,
                schoolId,
                studentId,
                0,
                attendanceDate,
                attendanceStatus);

        }



        public static StudentSmsQueueResult QueueMissingStudentSmsForToday(

            EduContext db,

            int schoolId,

            Attendance_Device_Setting settings,

            string schoolName)

        {

            if (settings == null)

                return new StudentSmsQueueResult();



            var today = DateTime.Today;

            var records = db.Attendance_Records

                .Where(r => r.SchoolID == schoolId

                    && r.StudentID > 0

                    && DbFunctions.TruncateTime(r.AttendanceDate) == today)

                .ToList();



            return QueueStudentSmsDetailed(db, schoolId, settings, schoolName, records);

        }



        public static int QueueStudentSms(

            EduContext db,

            int schoolId,

            Attendance_Device_Setting settings,

            string schoolName,

            IEnumerable<Attendance_Record> records)

        {

            return QueueStudentSmsDetailed(db, schoolId, settings, schoolName, records).Queued;

        }



        public static StudentSmsQueueResult QueueStudentSmsDetailed(

            EduContext db,

            int schoolId,

            Attendance_Device_Setting settings,

            string schoolName,

            IEnumerable<Attendance_Record> records)

        {

            var result = new StudentSmsQueueResult();



            if (settings == null || records == null)

                return result;



            if (!settings.Is_All_SMS_On || !settings.Is_Student_All_SMS_Active)

                return result;



            var recordList = records.Where(r => r != null).ToList();

            if (!recordList.Any())

                return result;



            var templateHelper = new AttendanceSmsTemplateHelper(schoolId);

            var stuSettings = LoadStudentSmsSettings(db, schoolId);

            var studentPhones = LoadStudentPhones(db, schoolId, recordList.Select(r => r.StudentID));



            var smsRows = new List<Attendance_SMS>();



            foreach (var record in recordList)

            {

                if (record.StudentID <= 0)

                {

                    result.Skipped++;

                    continue;

                }



                var stuSetting = ResolveStudentSetting(stuSettings, settings, record);

                if (stuSetting == null)

                {

                    result.Skipped++;

                    continue;

                }



                var scheduleId = record.Attendance_ScheduleID ?? 0;

                if (scheduleId == 0 && stuSetting.ScheduleID > 0)

                    scheduleId = stuSetting.ScheduleID;



                var attendanceType = record.Attendance ?? string.Empty;

                if (!ShouldSend(settings, stuSetting, record, attendanceType))

                {

                    result.Skipped++;

                    continue;

                }



                result.Eligible++;



                if (HasQueuedSmsToday(

                        db,

                        schoolId,

                        record.StudentID,

                        record.AttendanceDate,

                        attendanceType,

                        GetScheduleTime(record, stuSetting, attendanceType)))

                {

                    result.Skipped++;

                    continue;

                }



                var classInfo = templateHelper.GetStudentClassInfo(record.StudentID);

                var displayId = string.IsNullOrWhiteSpace(classInfo.displayId)

                    ? record.StudentID.ToString()

                    : classInfo.displayId;

                var scheduleName = templateHelper.GetScheduleName(

                    scheduleId > 0 ? (int?)scheduleId : record.Attendance_ScheduleID,

                    record.StudentID);

                var smsText = templateHelper.BuildMessage(

                    attendanceType,

                    stuSetting.StudentsName,

                    displayId,

                    schoolName,

                    record.AttendanceDate,

                    record.EntryTime,

                    record.ExitTime,

                    stuSetting.StartTime,

                    classInfo.className,

                    classInfo.roll,

                    scheduleName,

                    settings.Is_English_SMS);



                var mobileNo = ResolveMobileNo(stuSetting, studentPhones, record.StudentID);

                if (string.IsNullOrWhiteSpace(smsText) || string.IsNullOrWhiteSpace(mobileNo))

                {

                    result.Skipped++;

                    continue;

                }

                if (AttendanceSmsQueueWriter.ShouldSkipQueueToday(
                        db,
                        schoolId,
                        record.StudentID,
                        0,
                        record.AttendanceDate,
                        attendanceType,
                        smsText))
                {
                    result.Skipped++;
                    continue;
                }



                var now = DateTime.Now;
                var scheduleTime = AttendanceSmsScheduleHelper.EnsureSendableScheduleTime(
                    GetScheduleTime(record, stuSetting, attendanceType),
                    settings.SMS_TimeOut_Minute);

                smsRows.Add(new Attendance_SMS

                {

                    SchoolID = schoolId,

                    StudentID = record.StudentID,

                    EmployeeID = 0,

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



        private static List<VW_Attendance_Stu_Setting> LoadStudentSmsSettings(EduContext db, int schoolId)

        {

            return db.VW_Attendance_Stu_Settings

                .AsNoTracking()

                .Where(s => s.SchoolID == schoolId)

                .ToList();

        }



        private static Dictionary<int, string> LoadStudentPhones(

            EduContext db,

            int schoolId,

            IEnumerable<int> studentIds)

        {

            var ids = studentIds.Where(id => id > 0).Distinct().ToList();

            var phones = new Dictionary<int, string>();

            if (!ids.Any())

                return phones;



            var idList = string.Join(",", ids);

            var sql = $@"

                SELECT StudentID, SMSPhoneNo

                FROM Student

                WHERE SchoolID = @SchoolID

                  AND StudentID IN ({idList})";



            foreach (var row in db.Database.SqlQuery<StudentPhoneRow>(

                         sql,

                         new SqlParameter("@SchoolID", schoolId)))

            {

                if (row.StudentID > 0 && !string.IsNullOrWhiteSpace(row.SMSPhoneNo))

                    phones[row.StudentID] = row.SMSPhoneNo.Trim();

            }



            return phones;

        }



        private static string ResolveMobileNo(

            VW_Attendance_Stu_Setting stuSetting,

            Dictionary<int, string> studentPhones,

            int studentId)

        {

            if (!string.IsNullOrWhiteSpace(stuSetting?.SMSPhoneNo))

                return stuSetting.SMSPhoneNo.Trim();



            string phone;

            if (studentPhones != null && studentPhones.TryGetValue(studentId, out phone))

                return phone;



            return null;

        }



        private static VW_Attendance_Stu_Setting ResolveStudentSetting(

            List<VW_Attendance_Stu_Setting> stuSettings,

            Attendance_Device_Setting settings,

            Attendance_Record record)

        {

            var candidates = stuSettings

                .Where(s => s.StudentID == record.StudentID)

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



            var attendanceType = record.Attendance ?? string.Empty;

            var eligible = candidates.FirstOrDefault(s => ShouldSend(settings, s, record, attendanceType));

            if (eligible != null)

                return eligible;



            return candidates.First();

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

            VW_Attendance_Stu_Setting stuSetting,

            Attendance_Record record,

            string attendanceType)

        {

            if (record.Is_OUT)

                return settings.Is_Student_Exit_SMS_ON && stuSetting.Exit_Confirmation;



            switch (attendanceType)

            {

                case "Abs":

                    return settings.Is_Student_Abs_SMS_ON && stuSetting.Is_Abs_SMS;

                case "Late Abs":

                    return settings.Is_Student_Abs_SMS_ON && stuSetting.Is_Abs_SMS;

                case "Pre":

                    return settings.Is_Student_Entry_SMS_ON && stuSetting.Entry_Confirmation;

                case "Late":

                    return settings.Is_Student_Late_SMS_ON && stuSetting.Is_Late_SMS;

                default:

                    return false;

            }

        }



        private static TimeSpan GetScheduleTime(

            Attendance_Record record,

            VW_Attendance_Stu_Setting stuSetting,

            string attendanceType)

        {

            if (record.Is_OUT)

                return record.ExitTime ?? stuSetting.EndTime;



            switch (attendanceType)

            {

                case "Abs":

                    return stuSetting.LateEntryTime;

                case "Late Abs":

                    return record.EntryTime ?? stuSetting.LateEntryTime;

                case "Pre":

                    return record.EntryTime ?? stuSetting.StartTime;

                case "Late":

                    return record.EntryTime ?? stuSetting.LateEntryTime;

                default:

                    return record.EntryTime ?? stuSetting.StartTime;

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

        private sealed class StudentPhoneRow

        {

            public int StudentID { get; set; }

            public string SMSPhoneNo { get; set; }

        }

    }

}


