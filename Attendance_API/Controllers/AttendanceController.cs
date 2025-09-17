using Attendance_API.DB_Model;
using Attendance_API.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace Attendance_API.Controllers
{
    [Authorize]
    public class AttendanceController : ApiController
    {
        private readonly TimeSpan _clearTime = new TimeSpan(0, 0, 0);

        [Route("api/Attendance/{id}/Students")]
        [HttpPost]
        public IHttpActionResult PostStudents(int id, [FromBody] List<AttendanceRecordAPI> logRecord)
        {
            try
            {
                if (logRecord == null) return Ok();
                if (logRecord.Count < 1) return Ok();

                using (var db = new EduContext())
                {
                    var today = DateTime.Now;

                    var attSetting = db.Attendance_Device_Settings.FirstOrDefault(s => s.SchoolID == id);
                    var schoolName = db.SchoolInfos.Find(id)?.SchoolName;

                    if (attSetting == null) return Ok();
                    if (!attSetting.Is_Device_Attendance_Enable && !attSetting.Is_Student_Attendance_Enable) return Ok();

                    var attendanceRecords = db.VW_Attendance_Stus.Where(a => a.SchoolID == id).ToList();

                    var newRecords = (from s in attendanceRecords
                                      join a in logRecord
                                      on s.DeviceID equals a.DeviceID
                                      where s.SchoolID == id
                                      select new Attendance_Record
                                      {
                                          SchoolID = s.SchoolID,
                                          ClassID = s.ClassID,
                                          EducationYearID = s.EducationYearID,
                                          StudentID = s.StudentID,
                                          StudentClassID = s.StudentClassID,
                                          Attendance = a.AttendanceStatus,
                                          AttendanceDate = a.AttendanceDate,
                                          EntryTime = a.EntryTime == _clearTime ? (TimeSpan?)null : a.EntryTime,
                                          ExitStatus = a.ExitStatus,
                                          ExitTime = a.ExitTime == _clearTime ? (TimeSpan?)null : a.ExitTime,
                                          Is_OUT = a.Is_OUT
                                      }).ToList();

                    db.Attendance_Records.AddRange(newRecords);
                    db.SaveChanges();


                    ////var newAttendanceRecords = newRecord.Where(na => !db.Attendance_Records.Any(sa => na.SchoolID == sa.SchoolID & na.StudentID == sa.StudentID & na.AttendanceDate == sa.AttendanceDate)).ToList();
                    ////if (newAttendanceRecords.Count <= 0) return Ok();

                    //////-----------New code start ----------------------
                    //var newAttendanceRecords = new List<Attendance_Record>();

                    //foreach (var attendanceRecord in newRecord)
                    //{
                    //    var added = db.Attendance_Records.Any(sa => attendanceRecord.SchoolID == sa.SchoolID & attendanceRecord.StudentID == sa.StudentID & attendanceRecord.AttendanceDate == sa.AttendanceDate);
                    //    if (added) continue;
                    //    db.Attendance_Records.Add(attendanceRecord);
                    //    newAttendanceRecords.Add(attendanceRecord);
                    //    db.SaveChanges();
                    //}
                    //if (newAttendanceRecords.Count <= 0) return Ok();
                    //////-----------New code end----------------------

                    //var smsList = new List<Attendance_SMS>();

                    //if (attSetting.Is_All_SMS_On && attSetting.Is_Student_All_SMS_Active)
                    //{
                    //    ////------Abs & LateAbs SMS
                    //    var stuAbsSetting = db.VW_Attendance_Stu_Settings.Where(a => a.SchoolID == id && a.Is_Abs_SMS).ToList();
                    //    if (stuAbsSetting.Count > 0 && attSetting.Is_Student_Abs_SMS_ON)
                    //    {
                    //        var attRecordAbs = newAttendanceRecords.Where(a => a.Attendance == "Abs").ToList();
                    //        if (attRecordAbs.Count > 0)
                    //        {
                    //            var stuList = from a in attRecordAbs
                    //                          join s in stuAbsSetting
                    //                           on a.StudentID equals s.StudentID
                    //                          where s.Is_Abs_SMS && a.AttendanceDate.Date == today.Date
                    //                          select new Attendance_SMS
                    //                          {
                    //                              SchoolID = id,
                    //                              StudentID = a.StudentID,
                    //                              ScheduleTime = s.LateEntryTime, // Abs, so Late Enter time will start counting for sms sending time limit
                    //                              AttendanceDate = a.AttendanceDate,
                    //                              SMS_Text = attSetting.Is_English_SMS ? $"Respected guardian, {s.StudentsName} today({a.AttendanceDate:d MMM yy}) absent, please send to class regularly. {schoolName}"
                    //                                  : $"সম্মানিত অভিভাবক, {s.StudentsName} আজ({a.AttendanceDate:d MMM yy}) অনুপস্থিত, অনুগ্রহ করে নিয়মিত ক্লাসে পাঠান {schoolName}",
                    //                              MobileNo = s.SMSPhoneNo,
                    //                              AttendanceStatus = a.Attendance,
                    //                              SMS_TimeOut = attSetting.SMS_TimeOut_Minute
                    //                          };

                    //            smsList.AddRange(stuList);
                    //        }

                    //        var attRecordLatAbs = newAttendanceRecords.Where(a => !a.Is_OUT && a.Attendance == "Late Abs").ToList();
                    //        if (attRecordLatAbs.Count > 0)
                    //        {
                    //            var stuList = from a in attRecordLatAbs
                    //                          join s in stuAbsSetting
                    //                           on a.StudentID equals s.StudentID
                    //                          where s.Is_Abs_SMS && a.AttendanceDate.Date == today.Date
                    //                          select new Attendance_SMS
                    //                          {
                    //                              SchoolID = id,
                    //                              StudentID = a.StudentID,
                    //                              ScheduleTime = a.EntryTime ?? s.LateEntryTime,
                    //                              AttendanceDate = a.AttendanceDate,
                    //                              SMS_Text = attSetting.Is_English_SMS ? $"Respected guardian, {s.StudentsName} today({a.AttendanceDate:d MMM yy}) late absent. entry time {DateTime.Today.Add(a.EntryTime.GetValueOrDefault()):h:mm tt}. {schoolName}"
                    //                                  : $"সম্মানিত অভিভাবক, {s.StudentsName} আজ({a.AttendanceDate:d MMM yy}) বিলম্বে (অনুপস্থিত হিসাবে), প্রবেশ করেছে। প্রবেশ সময় {DateTime.Today.Add(a.EntryTime.GetValueOrDefault()):h:mm tt}. {schoolName}",
                    //                              MobileNo = s.SMSPhoneNo,
                    //                              AttendanceStatus = a.Attendance,
                    //                              SMS_TimeOut = attSetting.SMS_TimeOut_Minute
                    //                          };

                    //            smsList.AddRange(stuList);
                    //        }
                    //    }

                    //    //------Entry SMS
                    //    var stuEntrySetting = db.VW_Attendance_Stu_Settings.Where(a => a.SchoolID == id && a.Entry_Confirmation).ToList();
                    //    if (stuEntrySetting.Count > 0 && attSetting.Is_Student_Entry_SMS_ON)
                    //    {
                    //        var attRecordPre = newAttendanceRecords.Where(a => !a.Is_OUT && a.Attendance == "Pre").ToList();
                    //        if (attRecordPre.Count > 0)
                    //        {
                    //            var stuList = from a in attRecordPre
                    //                          join s in stuEntrySetting
                    //                          on a.StudentID equals s.StudentID
                    //                          where s.Entry_Confirmation && a.AttendanceDate.Date == today.Date
                    //                          select new Attendance_SMS
                    //                          {
                    //                              SchoolID = id,
                    //                              StudentID = a.StudentID,
                    //                              ScheduleTime = a.EntryTime ?? s.StartTime,
                    //                              AttendanceDate = a.AttendanceDate,
                    //                              SMS_Text = attSetting.Is_English_SMS ? $"Respected guardian, {s.StudentsName} has reached {schoolName} at {DateTime.Today.Add(a.EntryTime.GetValueOrDefault()):h:mm tt}"
                    //                                  : $"সম্মানিত অভিভাবক, {s.StudentsName} নিরাপদে {schoolName} এ ({DateTime.Today.Add(a.EntryTime.GetValueOrDefault()):h:mm tt}) প্রবেশ করেছে",
                    //                              MobileNo = s.SMSPhoneNo,
                    //                              AttendanceStatus = a.Attendance,
                    //                              SMS_TimeOut = attSetting.SMS_TimeOut_Minute
                    //                          };

                    //            smsList.AddRange(stuList);
                    //        }
                    //    }

                    //    ////------Late SMS
                    //    var stuLateSetting = db.VW_Attendance_Stu_Settings.Where(a => a.SchoolID == id && a.Is_Late_SMS).ToList();
                    //    if (stuLateSetting.Count > 0 && attSetting.Is_Student_Late_SMS_ON)
                    //    {
                    //        var attRecordLate = newAttendanceRecords.Where(a => !a.Is_OUT && a.Attendance == "Late").ToList();
                    //        if (attRecordLate.Count > 0)
                    //        {
                    //            var stuList = from a in attRecordLate
                    //                          join s in stuLateSetting
                    //                           on a.StudentID equals s.StudentID
                    //                          where s.Is_Late_SMS && a.AttendanceDate.Date == today.Date
                    //                          select new Attendance_SMS
                    //                          {
                    //                              SchoolID = id,
                    //                              StudentID = a.StudentID,
                    //                              ScheduleTime = a.EntryTime ?? s.LateEntryTime,
                    //                              AttendanceDate = a.AttendanceDate,
                    //                              SMS_Text = attSetting.Is_English_SMS ? $"Respected guardian, {s.StudentsName} today({a.AttendanceDate:d MMM yy}) late {(a.EntryTime.GetValueOrDefault() - s.StartTime).Minutes} min, entry time {DateTime.Today.Add(a.EntryTime.GetValueOrDefault()):h:mm tt}. {schoolName}"
                    //                                  : $"সম্মানিত অভিভাবক, {s.StudentsName} আজ({a.AttendanceDate:d MMM yy}) {(a.EntryTime.GetValueOrDefault() - s.StartTime).Minutes} মি: বিলম্বে, প্রবেশ করেছে। প্রবেশ সময় {DateTime.Today.Add(a.EntryTime.GetValueOrDefault()):h:mm tt}. {schoolName}",
                    //                              MobileNo = s.SMSPhoneNo,
                    //                              AttendanceStatus = a.Attendance,
                    //                              SMS_TimeOut = attSetting.SMS_TimeOut_Minute
                    //                          };

                    //            smsList.AddRange(stuList);
                    //        }
                    //    }

                    //    ////------Exit SMS
                    //    var stuExitSetting = db.VW_Attendance_Stu_Settings.Where(a => a.SchoolID == id && a.Exit_Confirmation).ToList();
                    //    if (stuExitSetting.Count > 0 && attSetting.Is_Student_Exit_SMS_ON)
                    //    {
                    //        var attRecordExit = newAttendanceRecords.Where(a => a.Is_OUT).ToList();
                    //        if (attRecordExit.Count > 0)
                    //        {
                    //            var stuList = from a in attRecordExit
                    //                          join s in stuExitSetting
                    //                              on a.StudentID equals s.StudentID
                    //                          where s.Exit_Confirmation && a.AttendanceDate.Date == today.Date
                    //                          select new Attendance_SMS
                    //                          {
                    //                              SchoolID = id,
                    //                              StudentID = a.StudentID,
                    //                              ScheduleTime = a.ExitTime ?? s.EndTime,
                    //                              AttendanceDate = a.AttendanceDate,
                    //                              SMS_Text = attSetting.Is_English_SMS ? $"Respected guardian, {s.StudentsName} has exited from {schoolName} at {DateTime.Today.Add(a.ExitTime.GetValueOrDefault()):h:mm tt}"
                    //                                  : $"সম্মানিত অভিভাবক, {s.StudentsName}, {schoolName} থেকে {DateTime.Today.Add(a.ExitTime.GetValueOrDefault()):h:mm tt} প্রস্থান করেছে",
                    //                              MobileNo = s.SMSPhoneNo,
                    //                              AttendanceStatus = a.Attendance,
                    //                              SMS_TimeOut = attSetting.SMS_TimeOut_Minute
                    //                          };
                    //            smsList.AddRange(stuList);
                    //        }
                    //    }
                    //}

                    //if (smsList.Count <= 0) return Ok();

                    //db.Attendance_sms.AddRange(smsList);


                    //db.SaveChanges();
                }
                return Ok();
            }
            catch (Exception e)
            {
                return Ok();
            }
        }

        [Route("api/Attendance/{id}/StudentsUpdate")]
        [HttpPost]
        public IHttpActionResult PutStudents(int id, [FromBody] List<AttendanceRecordAPI> logRecords)
        {
            try
            {
                if (logRecords == null) return NotFound();
                if (logRecords.Count < 1) return NotFound();

                var smsList = new List<Attendance_SMS>();
                using (var db = new EduContext())
                {
                    var today = DateTime.Now;

                    var attSetting = db.Attendance_Device_Settings.FirstOrDefault(s => s.SchoolID == id);
                    var schoolName = db.SchoolInfos.Find(id)?.SchoolName;

                    //var logForSms = new List<Attendance_Record>();
                    if (attSetting != null && attSetting.Is_Device_Attendance_Enable && attSetting.Is_Student_Attendance_Enable)
                    {
                        var studentList = db.VW_Attendance_Stus.Where(a => a.SchoolID == id).ToList();

                        var updatedRecords = from s in studentList
                                             join a in logRecords
                                             on s.DeviceID equals a.DeviceID
                                             where s.SchoolID == id
                                             select new Attendance_Record
                                             {
                                                 SchoolID = s.SchoolID,
                                                 ClassID = s.ClassID,
                                                 EducationYearID = s.EducationYearID,
                                                 StudentID = s.StudentID,
                                                 StudentClassID = s.StudentClassID,
                                                 Attendance = a.AttendanceStatus,
                                                 AttendanceDate = a.AttendanceDate,
                                                 EntryTime = a.EntryTime == _clearTime ? (TimeSpan?)null : a.EntryTime,
                                                 ExitStatus = a.ExitStatus,
                                                 ExitTime = a.ExitTime == _clearTime ? (TimeSpan?)null : a.ExitTime,
                                                 Is_OUT = a.Is_OUT
                                             };

                        foreach (var log in updatedRecords)
                        {
                            var updateLog = db.Attendance_Records.FirstOrDefault(u => u.SchoolID == log.SchoolID & u.StudentID == log.StudentID & u.AttendanceDate == log.AttendanceDate);

                            if (updateLog == null) continue;

                            updateLog.Attendance = log.Attendance;
                            updateLog.ExitStatus = log.ExitStatus;

                            updateLog.ExitTime = log.ExitTime;
                            updateLog.Is_OUT = log.Is_OUT;
                            updateLog.EntryTime = log.EntryTime;

                            db.Entry(updateLog).State = EntityState.Modified;
                            db.SaveChanges();
                            //  logForSms.Add(updateLog);
                        }
                    }


                    //if (logForSms.Count > 0)
                    //{
                    //    if (attSetting != null && attSetting.Is_All_SMS_On && attSetting.Is_Student_All_SMS_Active)
                    //    {
                    //        ////------LateAbs SMS
                    //        if (attSetting.Is_Student_Abs_SMS_ON)
                    //        {
                    //            var attRecordLatAbs =
                    //                logForSms.Where(a => !a.Is_OUT && a.Attendance == "Late Abs").ToList();

                    //            if (attRecordLatAbs.Count > 0)
                    //            {
                    //                var lateAbsEnableStuList = db.VW_Attendance_Stu_Settings
                    //                    .Where(a => a.SchoolID == id && a.Is_Abs_SMS).ToList();

                    //                var stuList = from a in attRecordLatAbs
                    //                              join s in lateAbsEnableStuList
                    //                                  on a.StudentID equals s.StudentID
                    //                              where s.Is_Late_SMS && a.AttendanceDate.Date == today.Date
                    //                              select new Attendance_SMS
                    //                              {
                    //                                  SchoolID = id,
                    //                                  StudentID = a.StudentID,
                    //                                  ScheduleTime = a.EntryTime ?? s.LateEntryTime,
                    //                                  AttendanceDate = a.AttendanceDate,
                    //                                  SMS_Text = attSetting.Is_English_SMS
                    //                                      ? $"Respected guardian, {s.StudentsName} today({a.AttendanceDate:d MMM yy}) late absent. Entry time {DateTime.Today.Add(a.EntryTime.GetValueOrDefault()):h:mm tt}. {schoolName}"
                    //                                      : $"সম্মানিত অভিভাবক, {s.StudentsName} আজ({a.AttendanceDate:d MMM yy}) বিলম্বে (অনুপস্থিত হিসাবে), প্রবেশ করেছে। প্রবেশ সময় {DateTime.Today.Add(a.EntryTime.GetValueOrDefault()):h:mm tt}. {schoolName}",
                    //                                  MobileNo = s.SMSPhoneNo,
                    //                                  AttendanceStatus = a.Attendance,
                    //                                  SMS_TimeOut = attSetting.SMS_TimeOut_Minute
                    //                              };

                    //                smsList.AddRange(stuList);
                    //            }
                    //        }


                    //        ////------Exit SMS
                    //        if (attSetting.Is_Student_Exit_SMS_ON)
                    //        {
                    //            var attRecordExit = logForSms.Where(a => a.Is_OUT).ToList();
                    //            if (attRecordExit.Count > 0)
                    //            {
                    //                var exitEnableStuList = db.VW_Attendance_Stu_Settings
                    //                    .Where(a => a.SchoolID == id && a.Exit_Confirmation).ToList();

                    //                var stuList = from a in attRecordExit
                    //                              join s in exitEnableStuList
                    //                                  on a.StudentID equals s.StudentID
                    //                              where s.Is_Late_SMS && a.AttendanceDate.Date == today.Date
                    //                              select new Attendance_SMS
                    //                              {
                    //                                  SchoolID = id,
                    //                                  StudentID = a.StudentID,
                    //                                  ScheduleTime = a.EntryTime ?? s.LateEntryTime,
                    //                                  AttendanceDate = a.AttendanceDate,
                    //                                  SMS_Text = attSetting.Is_English_SMS
                    //                                      ? $"Respected guardian, {s.StudentsName} has exited from {schoolName} at {DateTime.Today.Add(a.ExitTime.GetValueOrDefault()):h:mm tt}"
                    //                                      : $"সম্মানিত অভিভাবক, {s.StudentsName}, {schoolName} থেকে {DateTime.Today.Add(a.ExitTime.GetValueOrDefault()):h:mm tt} প্রস্থান করেছে",
                    //                                  MobileNo = s.SMSPhoneNo,
                    //                                  AttendanceStatus = a.Attendance,
                    //                                  SMS_TimeOut = attSetting.SMS_TimeOut_Minute
                    //                              };

                    //                smsList.AddRange(stuList);

                    //            }
                    //        }

                    //        if (smsList.Count > 0)
                    //        {
                    //            db.Attendance_sms.AddRange(smsList);
                    //            db.SaveChanges();
                    //        }
                    //    }
                    //}
                }

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Route("api/Attendance/{id}/Employees")]
        [HttpPost]
        public IHttpActionResult PostEmployees(int id, [FromBody] List<AttendanceRecordAPI> logRecord)
        {
            try
            {


                if (logRecord == null) return NotFound();
                if (logRecord.Count < 1) return NotFound();

                using (var db = new EduContext())
                {
                    var today = DateTime.Now;

                    var attSetting = db.Attendance_Device_Settings.FirstOrDefault(s => s.SchoolID == id);

                    if (attSetting == null) return Ok();
                    if (!attSetting.Is_Device_Attendance_Enable && !attSetting.Is_Employee_Attendance_Enable) return Ok();

                    var empList = db.VW_Emp_Infos.Where(a => a.SchoolID == id).ToList();

                    var newRecords = (from e in empList
                                      join a in logRecord
                                      on e.DeviceID equals a.DeviceID
                                      where e.SchoolID == id
                                      select new Employee_Attendance_Record
                                      {
                                          SchoolID = e.SchoolID,
                                          EmployeeID = e.EmployeeID,
                                          RegistrationID = 0,
                                          AttendanceStatus = a.AttendanceStatus,
                                          AttendanceDate = a.AttendanceDate,
                                          EntryTime = a.EntryTime == _clearTime ? (TimeSpan?)null : a.EntryTime,
                                          ExitStatus = a.ExitStatus,
                                          ExitTime = a.ExitTime == _clearTime ? (TimeSpan?)null : a.ExitTime,
                                          Is_OUT = a.Is_OUT
                                      }).ToList();


                    db.Employee_Attendance_Records.AddRange(newRecords);
                    db.SaveChanges();

                    //var newAttendanceRecord = newRecord.Where(na => !db.Employee_Attendance_Records.Any(sa => na.SchoolID == sa.SchoolID & na.EmployeeID == sa.EmployeeID & na.AttendanceDate == sa.AttendanceDate)).ToList();

                    //if (newAttendanceRecord.Count <= 0) return Ok();

                    //db.Employee_Attendance_Records.AddRange(newAttendanceRecord);
                    //db.SaveChanges();
                    //var smsList = new List<Attendance_SMS>();
                    //if (attSetting.Is_All_SMS_On && attSetting.Is_Employee_SMS_Active)
                    //{
                    //    ////------Abs & LateAbs SMS
                    //    var empAbsSetting = db.VW_Attendance_Emp_Settings.Where(a => a.SchoolID == id && a.Is_Abs_SMS).ToList();
                    //    if (empAbsSetting.Count > 0 && attSetting.Is_Employee_SMS_Active)
                    //    {
                    //        var attRecordAbs = newAttendanceRecord.Where(a => a.AttendanceStatus == "Abs").ToList();
                    //        if (attRecordAbs.Count > 0)
                    //        {
                    //            var Emp_List = from a in attRecordAbs
                    //                           join e in empAbsSetting
                    //                           on a.EmployeeID equals e.EmployeeID
                    //                           where e.Is_Abs_SMS && a.AttendanceDate.Date == today.Date
                    //                           select new Attendance_SMS
                    //                           {
                    //                               SchoolID = id,
                    //                               EmployeeID = a.EmployeeID,
                    //                               ScheduleTime = e.LateEntryTime,
                    //                               AttendanceDate = a.AttendanceDate,
                    //                               SMS_Text = attSetting.Is_English_SMS ? $"{e.Name} Today({a.AttendanceDate:d MMM yy}) absent"
                    //                                   : $"{e.Name} আজ({a.AttendanceDate:d MMM yy}) অনুপস্থিত",
                    //                               MobileNo = attSetting.Is_Employee_SMS_OwnNumber ? e.Phone : attSetting.Employee_SMS_Number,
                    //                               AttendanceStatus = a.AttendanceStatus,
                    //                               SMS_TimeOut = attSetting.SMS_TimeOut_Minute
                    //                           };

                    //            smsList.AddRange(Emp_List);
                    //        }

                    //        var attRecordLatAbs = newAttendanceRecord.Where(a => a.AttendanceStatus == "Late Abs").ToList();
                    //        if (attRecordLatAbs.Count > 0)
                    //        {
                    //            var stuList = from a in attRecordLatAbs
                    //                          join e in empAbsSetting
                    //                           on a.EmployeeID equals e.EmployeeID
                    //                          where e.Is_Abs_SMS && a.AttendanceDate.Date == today.Date
                    //                          select new Attendance_SMS
                    //                          {
                    //                              SchoolID = id,
                    //                              EmployeeID = a.EmployeeID,
                    //                              ScheduleTime = a.EntryTime ?? e.LateEntryTime,
                    //                              AttendanceDate = a.AttendanceDate,
                    //                              SMS_Text = attSetting.Is_English_SMS ? $"{e.Name} Today({a.AttendanceDate:d MMM yy}) late absent. Entry time {DateTime.Today.Add(a.EntryTime.GetValueOrDefault()):h:mm tt}"
                    //                                  : $"{e.Name} আজ({a.AttendanceDate:d MMM yy}) বিলম্বে (অনুপস্থিত হিসাবে), প্রবেশ করেছে। প্রবেশ সময় {DateTime.Today.Add(a.EntryTime.GetValueOrDefault()):h:mm tt}",
                    //                              MobileNo = attSetting.Is_Employee_SMS_OwnNumber ? e.Phone : attSetting.Employee_SMS_Number,
                    //                              AttendanceStatus = a.AttendanceStatus,
                    //                              SMS_TimeOut = attSetting.SMS_TimeOut_Minute
                    //                          };

                    //            smsList.AddRange(stuList);
                    //        }
                    //    }


                    //    ////------Late SMS
                    //    var empLateSetting = db.VW_Attendance_Emp_Settings.Where(a => a.SchoolID == id && a.Is_Late_SMS).ToList();
                    //    if (empLateSetting.Count > 0 && attSetting.Is_Employee_Late_SMS_ON)
                    //    {
                    //        var attRecordLate = newAttendanceRecord.Where(a => a.AttendanceStatus == "Late").ToList();
                    //        if (attRecordLate.Count > 0)
                    //        {
                    //            var stuList = from a in attRecordLate
                    //                          join e in empLateSetting
                    //                           on a.EmployeeID equals e.EmployeeID
                    //                          where e.Is_Late_SMS && a.AttendanceDate.Date == today.Date
                    //                          select new Attendance_SMS
                    //                          {
                    //                              SchoolID = id,
                    //                              EmployeeID = a.EmployeeID,
                    //                              ScheduleTime = a.EntryTime ?? e.LateEntryTime,
                    //                              AttendanceDate = a.AttendanceDate,
                    //                              SMS_Text = attSetting.Is_English_SMS ? $"{e.Name} Today({a.AttendanceDate:d MMM yy}) late {(a.EntryTime.GetValueOrDefault() - e.StartTime).Minutes} min. Entry time {DateTime.Today.Add(a.EntryTime.GetValueOrDefault()):h:mm tt}"
                    //                                  : $"{e.Name} আজ({a.AttendanceDate:d MMM yy}) {(a.EntryTime.GetValueOrDefault() - e.StartTime).Minutes} মি: বিলম্বে প্রবেশ করেছে। প্রবেশ সময় {DateTime.Today.Add(a.EntryTime.GetValueOrDefault()):h:mm tt}",
                    //                              MobileNo = attSetting.Is_Employee_SMS_OwnNumber ? e.Phone : attSetting.Employee_SMS_Number,
                    //                              AttendanceStatus = a.AttendanceStatus,
                    //                              SMS_TimeOut = attSetting.SMS_TimeOut_Minute
                    //                          };

                    //            smsList.AddRange(stuList);
                    //        }
                    //    }
                    //}

                    //if (smsList.Count <= 0) return Ok();
                    //db.Attendance_sms.AddRange(smsList);
                    //db.SaveChanges();
                }

                return Ok();
            }
            catch (Exception e)
            {
                return Ok();
            }
        }

        [Route("api/Attendance/{id}/EmployeesUpdate")]
        [HttpPost]
        public IHttpActionResult PutEmployees(int id, [FromBody] List<AttendanceRecordAPI> logRecord)
        {
            if (logRecord == null) return NotFound();
            if (logRecord.Count < 1) return NotFound();

            using (var db = new EduContext())
            {
                var today = DateTime.Now;

                var attSetting = db.Attendance_Device_Settings.FirstOrDefault(s => s.SchoolID == id);
                var schoolName = db.SchoolInfos.Find(id)?.SchoolName;

                if (attSetting != null && attSetting.Is_Device_Attendance_Enable && attSetting.Is_Employee_Attendance_Enable)
                {
                    var empList = db.VW_Emp_Infos.Where(a => a.SchoolID == id).ToList();

                    var updatedRecords = from e in empList
                                         join a in logRecord
                                         on e.DeviceID equals a.DeviceID
                                         where e.SchoolID == id
                                         select new Employee_Attendance_Record
                                         {
                                             SchoolID = e.SchoolID,
                                             EmployeeID = e.EmployeeID,
                                             RegistrationID = 0,
                                             AttendanceStatus = a.AttendanceStatus,
                                             AttendanceDate = a.AttendanceDate,
                                             EntryTime = a.EntryTime == _clearTime ? (TimeSpan?)null : a.EntryTime,
                                             ExitStatus = a.ExitStatus,
                                             ExitTime = a.ExitTime == _clearTime ? (TimeSpan?)null : a.ExitTime,
                                             Is_OUT = a.Is_OUT
                                         };

                    foreach (var log in updatedRecords)
                    {
                        var updateLog = db.Employee_Attendance_Records.FirstOrDefault(u => u.SchoolID == log.SchoolID & u.EmployeeID == log.EmployeeID & u.AttendanceDate == log.AttendanceDate);
                        if (updateLog == null) continue;

                        updateLog.AttendanceStatus = log.AttendanceStatus;
                        updateLog.ExitStatus = log.ExitStatus;

                        updateLog.ExitTime = log.ExitTime;
                        updateLog.Is_OUT = log.Is_OUT;
                        updateLog.EntryTime = log.EntryTime;

                        db.Entry(updateLog).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }

            return Ok();
        }


        [Route("api/Attendance/{id}/backup_data")]
        [HttpPost]
        public IHttpActionResult PostBackupData(int id, [FromBody] List<BackupDataAPI> backupData)
        {
            if (backupData == null) return NotFound();
            if (backupData.Count < 1) return NotFound();

            using (var db = new EduContext())
            {
                var attSetting = db.Attendance_Device_Settings.FirstOrDefault(s => s.SchoolID == id);

                // Device Attendance Disable
                if (attSetting.Is_Device_Attendance_Enable)
                {
                    // Student Attendance Enabl
                    if (attSetting.Is_Student_Attendance_Enable)
                    {
                        foreach (var data in backupData.Where(b => b.Is_Student).OrderBy(b => b.EntryDate).OrderBy(b => b.EntryTime))
                        {
                            var dt = data.EntryDate;
                            var time = data.EntryTime;

                            var sDate = dt.ToShortDateString();

                            var sInfo = db.VW_Attendance_Stus.FirstOrDefault(a =>
                                a.SchoolID == id && a.DeviceID == data.DeviceID);


                            var schedule = db.Attendance_Schedule_Days.FirstOrDefault(s =>
                                s.Day == data.EntryDay && s.ScheduleID == data.ScheduleID);

                            var isHoliday = db.Holidays.Any(h => h.SchoolID == id && h.HolidayDate == dt);
                            //student not found
                            if (sInfo == null)
                            {
                                continue;
                            }
                            //schedule not found
                            else if (schedule == null)
                            {
                                continue;
                            }
                            //Hodiday attendance disable
                            else if (isHoliday && attSetting.Is_Holiday_As_Offday)
                            {
                                continue;
                            }
                            //Schedule Off day
                            else if (!schedule.Is_OnDay)
                            {
                                continue;
                            }
                            // Insert or Update Attendance Records
                            else
                            {

                                var attRecord = db.Attendance_Records.FirstOrDefault(s =>
                                    s.SchoolID == id & s.StudentID == sInfo.StudentID & s.AttendanceDate == dt);
                                var sStartTime = schedule.StartTime;
                                var sLateTime = schedule.LateEntryTime;
                                var sEndTime = schedule.EndTime;

                                if (attRecord == null)
                                {
                                    attRecord = new Attendance_Record
                                    {
                                        SchoolID = sInfo.SchoolID,
                                        ClassID = sInfo.ClassID,
                                        EducationYearID = sInfo.EducationYearID,
                                        StudentID = sInfo.StudentID,
                                        StudentClassID = sInfo.StudentClassID,
                                        AttendanceDate = dt
                                    };

                                    if (time > sEndTime)
                                    {
                                        //Enroll after end time (as first enroll)
                                    }
                                    else
                                    {
                                        attRecord.EntryTime = time;

                                        if (time <= sStartTime)
                                        {
                                            attRecord.Attendance = "Pre";
                                        }
                                        else if (time <= sLateTime)
                                        {
                                            attRecord.Attendance = "Late";
                                        }
                                        else if (time <= sEndTime)
                                        {
                                            attRecord.Attendance = "Late Abs";
                                        }

                                        db.Attendance_Records.Add(attRecord);
                                        db.Entry(attRecord).State = EntityState.Added;
                                    }
                                }
                                else
                                {
                                    if (attRecord.Attendance == "Abs")
                                    {
                                        if (time < sEndTime)
                                        {
                                            attRecord.Attendance = "Late Abs";
                                        }

                                        attRecord.EntryTime = time;
                                    }
                                    else
                                    {
                                        if (time > sLateTime && time < sEndTime && !attRecord.Is_OUT &&
                                            attRecord.EntryTime.GetValueOrDefault().TotalMinutes + 10 <
                                            time.TotalMinutes)
                                        {
                                            attRecord.ExitStatus = "Early Leave";
                                            attRecord.Is_OUT = true;
                                            attRecord.ExitTime = time;
                                        }
                                        else if (time > sLateTime && time < sEndTime && attRecord.Is_OUT &&
                                                 attRecord.ExitTime.GetValueOrDefault().TotalMinutes + 10 <
                                                 time.TotalMinutes)
                                        {
                                            attRecord.ExitStatus = "Early Leave";
                                            attRecord.Is_OUT = true;
                                            attRecord.ExitTime = time;
                                        }
                                        else if (time > sEndTime)
                                        {
                                            attRecord.ExitStatus = "Out";
                                            attRecord.Is_OUT = true;
                                            attRecord.ExitTime = time;
                                        }
                                    }

                                    db.Entry(attRecord).State = EntityState.Modified;
                                }

                                try
                                {
                                    db.SaveChanges();
                                }
                                catch (Exception e)
                                {
                                    continue;
                                }

                            }

                        }
                    }

                    // Employee Attendance Enable
                    if (attSetting.Is_Employee_Attendance_Enable)
                    {
                        foreach (var data in backupData.Where(b => !b.Is_Student).OrderBy(b => b.EntryDate).OrderBy(b => b.EntryTime))
                        {

                            var dt = data.EntryDate;
                            var time = data.EntryTime;

                            var sDate = dt.ToShortDateString();

                            var eInfo = db.VW_Emp_Infos.FirstOrDefault(a => a.SchoolID == id && a.DeviceID == data.DeviceID);

                            var schedule = db.Attendance_Schedule_Days.FirstOrDefault(s =>
                                s.Day == data.EntryDay && s.ScheduleID == data.ScheduleID);

                            var isHoliday = db.Holidays.Any(h => h.SchoolID == id && h.HolidayDate == dt);

                            //student not found
                            if (eInfo == null)
                            {
                                continue;
                            }
                            //schedule not found
                            else if (schedule == null)
                            {
                                continue;
                            }
                            //Holiday attendance disable
                            if (isHoliday && attSetting.Is_Holiday_As_Offday)
                            {
                                continue;
                            }
                            //Schedule Off day
                            else if (!schedule.Is_OnDay)
                            {
                                continue;
                            }
                            // Insert or Update Attendance Records
                            else
                            {

                                var attRecord = db.Employee_Attendance_Records.FirstOrDefault(s =>
                                    s.SchoolID == id && s.EmployeeID == eInfo.EmployeeID && s.AttendanceDate == dt);
                                var sStartTime = schedule.StartTime;
                                var sLateTime = schedule.LateEntryTime;
                                var sEndTime = schedule.EndTime;

                                if (attRecord == null)
                                {
                                    attRecord = new Employee_Attendance_Record
                                    {
                                        SchoolID = eInfo.SchoolID,
                                        EmployeeID = eInfo.EmployeeID,
                                        RegistrationID = 0,
                                        AttendanceDate = dt
                                    };

                                    if (time > sEndTime)
                                    {
                                        //Enroll after end time (as first enroll)
                                    }
                                    else
                                    {
                                        attRecord.EntryTime = time;

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

                                        db.Employee_Attendance_Records.Add(attRecord);
                                        db.Entry(attRecord).State = EntityState.Added;
                                    }
                                }
                                else
                                {
                                    if (attRecord.AttendanceStatus == "Abs")
                                    {
                                        if (time < sEndTime)
                                        {
                                            attRecord.AttendanceStatus = "Late Abs";
                                        }

                                        attRecord.EntryTime = time;
                                    }
                                    else
                                    {
                                        if (time > sLateTime && time < sEndTime && !attRecord.Is_OUT &&
                                            attRecord.EntryTime.GetValueOrDefault().TotalMinutes + 10 <
                                            time.TotalMinutes)
                                        {
                                            attRecord.ExitStatus = "Early Leave";
                                            attRecord.Is_OUT = true;
                                            attRecord.ExitTime = time;
                                        }
                                        else if (time > sLateTime && time < sEndTime && attRecord.Is_OUT &&
                                                 attRecord.ExitTime.GetValueOrDefault().TotalMinutes + 10 <
                                                 time.TotalMinutes)
                                        {
                                            attRecord.ExitStatus = "Early Leave";
                                            attRecord.Is_OUT = true;
                                            attRecord.ExitTime = time;
                                        }
                                        else if (time > sEndTime)
                                        {
                                            attRecord.ExitStatus = "Out";
                                            attRecord.Is_OUT = true;
                                            attRecord.ExitTime = time;
                                        }
                                    }

                                    db.Entry(attRecord).State = EntityState.Modified;
                                }

                                try
                                {
                                    db.SaveChanges();
                                }
                                catch (Exception e)
                                {
                                    continue;
                                }
                            }


                        }
                    }
                }
            }
            return Ok();
        }

        [Route("api/Attendance/{id}/GetTodayAttendance")]
        [HttpGet]
        public async Task<IEnumerable<AttendanceRecordAPI>> GetTodayAttendance(int id)
        {
            var today = DateTime.Today;
            var attendanceRecords = new List<AttendanceRecordAPI>();
            try
            {

                using (var db = new EduContext())
                {
                    var sAttendances = await db.Attendance_Records
                        .Where(a => a.SchoolID == id && a.AttendanceDate == today).ToListAsync();

                    var sUsers = await db.VW_Attendance_Stus.Where(a => a.SchoolID == id).ToListAsync();

                    var sAttendanceRecords = (from a in sAttendances
                                              join u in sUsers
                                          on a.StudentID
                                          equals u.StudentID
                                              select new AttendanceRecordAPI
                                              {
                                                  DeviceID = u.DeviceID,
                                                  AttendanceDate = a.AttendanceDate,
                                                  AttendanceStatus = a.Attendance,
                                                  ExitStatus = a.ExitStatus,
                                                  Is_OUT = a.Is_OUT,
                                                  EntryTime = a.EntryTime,
                                                  ExitTime = a.ExitTime,
                                                  Is_Sent = true,
                                                  Is_Updated = true
                                              }).ToList();


                    attendanceRecords.AddRange(sAttendanceRecords);


                    var eAttendances = await db.Employee_Attendance_Records
                        .Where(a => a.SchoolID == id && a.AttendanceDate == today).ToListAsync();

                    var eUsers = await db.VW_Emp_Infos.Where(a => a.SchoolID == id).ToListAsync();
                    var eAttendanceRecords = (from a in eAttendances
                                              join u in eUsers
                                                  on a.EmployeeID
                                                  equals u.EmployeeID
                                              select new AttendanceRecordAPI
                                              {
                                                  DeviceID = u.DeviceID,
                                                  AttendanceDate = a.AttendanceDate,
                                                  AttendanceStatus = a.AttendanceStatus,
                                                  ExitStatus = a.ExitStatus,
                                                  Is_OUT = a.Is_OUT,
                                                  EntryTime = a.EntryTime,
                                                  ExitTime = a.ExitTime,
                                                  Is_Sent = true,
                                                  Is_Updated = true
                                              }).ToList();


                    attendanceRecords.AddRange(eAttendanceRecords);

                }
                return attendanceRecords;
            }
            catch (Exception e)
            {
                return attendanceRecords;
            }
        }

        //[Route("api/Attendance/{id}/SendSms")]
        //[HttpPost]
        //public async Task<IHttpActionResult> SendSms(int id)
        //{
        //    try
        //    {
        //        var sms = new SMS_Class(id.ToString());

        //        var totalSms = 0;

        //        var today = DateTime.Now;

        //        var currentTime = DateTime.Now.TimeOfDay;

        //        var smsBalance = sms.SMSBalance;

        //        #region Send SMS to All students

        //        var smsListDay = new List<Attendance_SMS>();
        //        var smsList = new List<Attendance_SMS>();

        //        using (var db = new EduContext())
        //        {
        //            smsListDay = await db.Attendance_sms.Where(s => s.SchoolID == id && s.AttendanceDate == today.Date)
        //                .ToListAsync();
        //            smsList = smsListDay
        //                .Where(s => s.ScheduleTime.TotalMinutes + s.SMS_TimeOut > currentTime.TotalMinutes).ToList();


        //            if (smsList.Any())
        //            {
        //                foreach (var item in smsList)
        //                {
        //                    var isValid = sms.SMS_Validation(item.MobileNo, item.SMS_Text);
        //                    if (isValid.Validation)
        //                    {
        //                        totalSms += sms.SMS_Conut(item.SMS_Text);
        //                    }
        //                }


        //                if (totalSms > 0 && smsBalance >= totalSms)
        //                {
        //                    //delete all sms pending records 


        //                    db.Attendance_sms.RemoveRange(smsListDay);
        //                    await db.SaveChangesAsync();


        //                    // if (sms.SMS_GetBalance() >= totalSms)
        //                    {
        //                        var smsRecords = new List<SMS_OtherInfo>();

        //                        var smsSendList = new List<SendSmsModel>();

        //                        foreach (var item in smsList)
        //                        {
        //                            //var isValid = sms.SMS_Validation(item.MobileNo, item.SMS_Text);
        //                            //if (isValid.Validation)
        //                            {
        //                                var isDuplicateSms = await db.SMS_Send_Record.AnyAsync(s =>
        //                                    s.PhoneNumber == item.MobileNo && s.TextSMS == item.SMS_Text);


        //                                if (isDuplicateSms) continue;

        //                                var smsSend = new SendSmsModel
        //                                {
        //                                    Number = item.MobileNo,
        //                                    Text = item.SMS_Text
        //                                };
        //                                smsSendList.Add(smsSend);

        //                                var smsSendRecord = new SMS_OtherInfo
        //                                {
        //                                    SMS_Send_ID = smsSend.Guid,
        //                                    SchoolID = item.SchoolID,
        //                                    StudentID = item.StudentID == 0 ? (int?)null : item.StudentID,
        //                                    TeacherID = item.EmployeeID == 0 ? (int?)null : item.EmployeeID,
        //                                };

        //                                smsRecords.Add(smsSendRecord);


        //                            }
        //                        }

        //                        var isSend = sms.SmsSendMultiple(smsSendList, "Device Attendance");
        //                        if (isSend.Validation)
        //                        {
        //                            db.SMS_OtherInfo.AddRange(smsRecords);
        //                            await db.SaveChangesAsync();
        //                        }
        //                        else
        //                        {
        //                            return BadRequest(isSend.Message);
        //                        }

        //                        //con.Close();
        //                    }
        //                }
        //            }
        //        }

        //        #endregion

        //        return Ok();
        //    }
        //    catch (Exception ex)
        //    {
        //        // ignored
        //        return BadRequest(ex.Message);
        //    }
        //}

    }
}
