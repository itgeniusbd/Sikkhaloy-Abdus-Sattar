using Attendance_API.DB_Model;
using Attendance_API.Helpers;
using Attendance_API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
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
                if (logRecord == null) return BadRequest("Attendance payload is empty.");
                if (logRecord.Count < 1) return BadRequest("Attendance payload is empty.");

                using (var db = new EduContext())
                {
                    var today = DateTime.Now;

                    var attSetting = db.Attendance_Device_Settings.FirstOrDefault(s => s.SchoolID == id);
                    var schoolName = db.SchoolInfos.Find(id)?.SchoolName;

                    if (attSetting == null)
                        return Ok(new AttendanceSyncResult { Message = "Attendance settings not found." });

                    if (!attSetting.Is_Device_Attendance_Enable && !attSetting.Is_Student_Attendance_Enable)
                        return Ok(new AttendanceSyncResult { Message = "Student attendance is disabled." });

                    var attendanceRecords = LoadStudentsForDeviceMatch(db, id);

                    var allRecords = (from s in attendanceRecords
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
                                          Attendance_ScheduleID = a.ScheduleID == 0 ? (int?)null : a.ScheduleID,
                                          Attendance = a.AttendanceStatus,
                                          AttendanceDate = a.AttendanceDate,
                                          EntryTime = a.EntryTime == _clearTime ? (TimeSpan?)null : a.EntryTime,
                                          ExitStatus = a.ExitStatus,
                                          ExitTime = a.ExitTime == _clearTime ? (TimeSpan?)null : a.ExitTime,
                                          Is_OUT = a.Is_OUT
                                      }).ToList();

                    SanitizeIncomingStudentRecords(allRecords);
                    foreach (var row in allRecords) ReclassifyStudentFromSchedule(db, row);

                    try
                    {
                        BackfillStudentScheduleRows(db, allRecords);
                    }
                    catch (Exception ex) when (IsIgnorableInsteadOfInsertException(ex))
                    {
                        DetachAllTrackedEntities(db);
                    }

                    var newRecords = FilterNewStudentRecords(db, id, allRecords);

                    // One duplicate row used to roll back the whole batch while the device was told it was saved.
                    var failedRecords = InsertRowsWithFallback(
                        db, db.Attendance_Records, newRecords, row => MergeStudentIntoExisting(db, row));
                    var savedNewRecords = newRecords.Except(failedRecords).ToList();
                    var insertedCount = savedNewRecords.Count;
                    var failedDeviceIds = new HashSet<int>(attendanceRecords
                        .Where(s => failedRecords.Any(f => f.StudentID == s.StudentID))
                        .Select(s => s.DeviceID));

                    var smsQueued = 0;
                    if (attSetting != null)
                    {
                        var smsRows = OnlyToday(savedNewRecords, r => r.AttendanceDate);
                        if (smsRows.Count > 0)
                        {
                            smsQueued += AttendanceStudentSmsService.QueueStudentSms(
                                db, id, attSetting, schoolName, smsRows);
                        }
                    }

                    var matchedDeviceIds = allRecords.Count > 0
                        ? logRecord
                            .Where(a => attendanceRecords.Any(s => s.DeviceID == a.DeviceID && s.SchoolID == id))
                            .Select(a => a.DeviceID)
                            .Where(d => !failedDeviceIds.Contains(d))
                            .Distinct()
                            .ToArray()
                        : new int[0];

                    return Ok(new AttendanceSyncResult
                    {
                        Matched = allRecords.Count - failedRecords.Count,
                        Inserted = insertedCount,
                        SmsQueued = smsQueued,
                        MatchedDeviceIds = matchedDeviceIds,
                        Message = allRecords.Count == 0
                            ? "No student DeviceID matched in VW_Attendance_Stus."
                            : matchedDeviceIds.Length < logRecord.Count
                                ? $"{logRecord.Count - matchedDeviceIds.Length} punch(es) skipped: DeviceID not found on server."
                                : smsQueued == 0 && allRecords.Count > 0
                                    ? "Attendance saved but no SMS queued (check Pre/Abs flags, phone, or SMS settings)."
                                    : null
                    });


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
                    //                                  : $"সম্মানিত আবিভাবক, {s.StudentsName} নিরাপদে {schoolName} এ ({DateTime.Today.Add(a.EntryTime.GetValueOrDefault()):h:mm tt}) প্রবেশ করেছে",
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


                }
            }
            catch (Exception e)
            {
                if (IsIgnorableInsteadOfInsertException(e))
                    return Ok(new AttendanceSyncResult { Message = "Attendance saved (trigger/EF refresh)." });

                return BadRequest(e.Message);
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
                var smsCandidates = new List<Attendance_Record>();
                var smsQueued = 0;
                using (var db = new EduContext())
                {
                    var today = DateTime.Now;

                    var attSetting = db.Attendance_Device_Settings.FirstOrDefault(s => s.SchoolID == id);
                    var schoolName = db.SchoolInfos.Find(id)?.SchoolName;

                    //var logForSms = new List<Attendance_Record>();
                    if (attSetting != null && attSetting.Is_Device_Attendance_Enable)
                    {
                        var studentList = LoadStudentsForDeviceMatch(db, id);

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
                                                 Attendance_ScheduleID = a.ScheduleID == 0 ? (int?)null : a.ScheduleID,
                                                 Attendance = a.AttendanceStatus,
                                                 AttendanceDate = a.AttendanceDate,
                                                 EntryTime = a.EntryTime == _clearTime ? (TimeSpan?)null : a.EntryTime,
                                                 ExitStatus = a.ExitStatus,
                                                 ExitTime = a.ExitTime == _clearTime ? (TimeSpan?)null : a.ExitTime,
                                                 Is_OUT = a.Is_OUT
                                             };

                        var updateList = updatedRecords.ToList();
                        SanitizeIncomingStudentRecords(updateList);
                        foreach (var row in updateList) ReclassifyStudentFromSchedule(db, row);

                        foreach (var log in updateList)
                        {
                            var updateLog = FindStudentUpdateRow(db, log);

                            if (updateLog == null) continue;

                            var previousAttendance = updateLog.Attendance;
                            var wasOut = updateLog.Is_OUT;

                            if (updateLog.Attendance_ScheduleID == null && log.Attendance_ScheduleID != null)
                                updateLog.Attendance_ScheduleID = log.Attendance_ScheduleID;

                            ApplyStudentUpdate(updateLog, log);
                            ReclassifyStudentFromSchedule(db, updateLog);

                            db.Entry(updateLog).State = EntityState.Modified;
                            try
                            {
                                db.SaveChanges();

                                if (attSetting != null)
                                {
                                    var statusChanged = previousAttendance != updateLog.Attendance || wasOut != updateLog.Is_OUT;
                                    if (statusChanged)
                                    {
                                        smsCandidates.Add(new Attendance_Record
                                        {
                                            SchoolID = updateLog.SchoolID,
                                            StudentID = updateLog.StudentID,
                                            Attendance_ScheduleID = updateLog.Attendance_ScheduleID,
                                            Attendance = updateLog.Attendance,
                                            AttendanceDate = updateLog.AttendanceDate,
                                            EntryTime = updateLog.EntryTime,
                                            ExitTime = updateLog.ExitTime,
                                            Is_OUT = updateLog.Is_OUT
                                        });
                                    }
                                }
                            }
                            catch (Exception ex) when (IsIgnorableInsteadOfInsertException(ex))
                            {
                                DetachAllTrackedEntities(db);
                            }
                            catch
                            {
                                // Skip rows that fail to update; do not fail the whole batch.
                            }
                        }
                    }

                    smsCandidates = OnlyToday(smsCandidates, r => r.AttendanceDate);
                    if (attSetting != null && smsCandidates.Count > 0)
                    {
                        smsQueued += AttendanceStudentSmsService.QueueStudentSms(
                            db, id, attSetting, schoolName, smsCandidates);
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

                return Ok(new AttendanceSyncResult { SmsQueued = smsQueued });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Route("api/Attendance/{id}/RequeueStudentSmsToday")]
        [HttpPost]
        public IHttpActionResult RequeueStudentSmsToday(int id)
        {
            try
            {
                using (var db = new EduContext())
                {
                    var attSetting = db.Attendance_Device_Settings.FirstOrDefault(s => s.SchoolID == id);
                    if (attSetting == null)
                        return Ok(new AttendanceSyncResult { Message = "Attendance settings not found." });

                    var schoolName = db.SchoolInfos.Find(id)?.SchoolName;
                    var queueResult = AttendanceStudentSmsService.QueueMissingStudentSmsForToday(
                        db, id, attSetting, schoolName);

                    var message = queueResult.Queued > 0
                        ? $"{queueResult.Queued} SMS queued for today."
                        : !string.IsNullOrWhiteSpace(queueResult.SaveError)
                            ? "SMS save failed: " + queueResult.SaveError
                            : queueResult.Eligible > 0
                                ? $"{queueResult.Eligible} eligible but not saved (check API logs)."
                                : "No SMS queued (flags off, missing phone, or no eligible records).";

                    return Ok(new AttendanceSyncResult
                    {
                        SmsQueued = queueResult.Queued,
                        Matched = queueResult.Eligible,
                        Message = message
                    });
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Route("api/Attendance/{id}/RequeueEmployeeSmsToday")]
        [HttpPost]
        public IHttpActionResult RequeueEmployeeSmsToday(int id)
        {
            try
            {
                using (var db = new EduContext())
                {
                    var attSetting = db.Attendance_Device_Settings.FirstOrDefault(s => s.SchoolID == id);
                    if (attSetting == null)
                        return Ok(new AttendanceSyncResult { Message = "Attendance settings not found." });

                    var schoolName = db.SchoolInfos.Find(id)?.SchoolName;
                    var queueResult = AttendanceEmployeeSmsService.QueueMissingEmployeeSmsForToday(
                        db, id, attSetting, schoolName);

                    var message = queueResult.Queued > 0
                        ? $"{queueResult.Queued} employee SMS queued for today."
                        : !string.IsNullOrWhiteSpace(queueResult.SaveError)
                            ? "Employee SMS save failed: " + queueResult.SaveError
                            : queueResult.Eligible > 0
                                ? $"{queueResult.Eligible} eligible but not saved."
                                : "No employee SMS queued (Pre not supported; check Abs/Late flags or phone).";

                    return Ok(new AttendanceSyncResult
                    {
                        SmsQueued = queueResult.Queued,
                        Matched = queueResult.Eligible,
                        Message = message
                    });
                }
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
                if (logRecord == null) return BadRequest("Attendance payload is empty.");
                if (logRecord.Count < 1) return BadRequest("Attendance payload is empty.");

                using (var db = new EduContext())
                {
                    var attSetting = db.Attendance_Device_Settings.FirstOrDefault(s => s.SchoolID == id);

                    if (attSetting == null)
                        return Ok(new AttendanceSyncResult { Message = "Attendance settings not found." });

                    if (!attSetting.Is_Device_Attendance_Enable && !attSetting.Is_Employee_Attendance_Enable)
                        return Ok(new AttendanceSyncResult { Message = "Employee attendance is disabled." });

                    var empList = db.VW_Emp_Infos.Where(a => a.SchoolID == id).ToList();

                    var allEmpRecords = (from e in empList
                                      join a in logRecord
                                      on e.DeviceID equals a.DeviceID
                                      where e.SchoolID == id
                                      select new Employee_Attendance_Record
                                      {
                                          SchoolID = e.SchoolID,
                                          EmployeeID = e.EmployeeID,
                                          RegistrationID = 0,
                                          Attendance_ScheduleID = a.ScheduleID == 0 ? (int?)null : a.ScheduleID,
                                          AttendanceStatus = a.AttendanceStatus,
                                          AttendanceDate = a.AttendanceDate,
                                          EntryTime = a.EntryTime == _clearTime ? (TimeSpan?)null : a.EntryTime,
                                          ExitStatus = a.ExitStatus,
                                          ExitTime = a.ExitTime == _clearTime ? (TimeSpan?)null : a.ExitTime,
                                          Is_OUT = a.Is_OUT
                                      }).ToList();

                    SanitizeIncomingEmployeeRecords(allEmpRecords);
                    foreach (var row in allEmpRecords) ReclassifyEmployeeFromSchedule(db, row);

                    try
                    {
                        BackfillEmployeeScheduleRows(db, allEmpRecords);
                    }
                    catch (Exception ex) when (IsIgnorableInsteadOfInsertException(ex))
                    {
                        DetachAllTrackedEntities(db);
                    }

                    var newRecords = FilterNewEmployeeRecords(db, id, allEmpRecords);

                    // One duplicate row used to roll back the whole batch while the device was told it was saved.
                    var failedRecords = InsertRowsWithFallback(
                        db, db.Employee_Attendance_Records, newRecords, row => MergeEmployeeIntoExisting(db, row));
                    var savedNewRecords = newRecords.Except(failedRecords).ToList();
                    var insertedCount = savedNewRecords.Count;
                    var failedEmpDeviceIds = new HashSet<int>(empList
                        .Where(e => failedRecords.Any(f => f.EmployeeID == e.EmployeeID))
                        .Select(e => e.DeviceID));

                    var matchedEmpDeviceIds = allEmpRecords.Count > 0
                        ? logRecord
                            .Where(a => empList.Any(e => e.DeviceID == a.DeviceID && e.SchoolID == id))
                            .Select(a => a.DeviceID)
                            .Where(d => !failedEmpDeviceIds.Contains(d))
                            .Distinct()
                            .ToArray()
                        : new int[0];

                    var smsQueued = 0;
                    if (attSetting != null)
                    {
                        var schoolName = db.SchoolInfos.Find(id)?.SchoolName;
                        var smsRows = OnlyToday(savedNewRecords, r => r.AttendanceDate);
                        if (smsRows.Count > 0)
                        {
                            smsQueued += AttendanceEmployeeSmsService.QueueEmployeeSms(
                                db, id, attSetting, schoolName, smsRows);
                        }
                    }

                    return Ok(new AttendanceSyncResult
                    {
                        Matched = allEmpRecords.Count - failedRecords.Count,
                        Inserted = insertedCount,
                        SmsQueued = smsQueued,
                        MatchedDeviceIds = matchedEmpDeviceIds,
                        Message = allEmpRecords.Count == 0
                            ? "No employee DeviceID matched in VW_Emp_Info."
                            : matchedEmpDeviceIds.Length < logRecord.Count
                                ? $"{logRecord.Count - matchedEmpDeviceIds.Length} punch(es) skipped: DeviceID not found on server."
                                : smsQueued == 0 && allEmpRecords.Count > 0
                                    ? "Attendance saved but no employee SMS queued (Pre not supported; check Abs/Late flags or phone)."
                                    : null
                    });

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
            }
            catch (Exception ex)
            {
                return Ok(new AttendanceSyncResult { Message = ex.GetBaseException().Message });
            }
        }

        [Route("api/Attendance/{id}/EmployeesUpdate")]
        [HttpPost]
        public IHttpActionResult PutEmployees(int id, [FromBody] List<AttendanceRecordAPI> logRecord)
        {
            if (logRecord == null) return NotFound();
            if (logRecord.Count < 1) return NotFound();

            var smsQueued = 0;
            using (var db = new EduContext())
            {
                var today = DateTime.Now;

                var attSetting = db.Attendance_Device_Settings.FirstOrDefault(s => s.SchoolID == id);
                var schoolName = db.SchoolInfos.Find(id)?.SchoolName;
                var smsCandidates = new List<Employee_Attendance_Record>();

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
                                             Attendance_ScheduleID = a.ScheduleID == 0 ? (int?)null : a.ScheduleID,
                                             AttendanceStatus = a.AttendanceStatus,
                                             AttendanceDate = a.AttendanceDate,
                                             EntryTime = a.EntryTime == _clearTime ? (TimeSpan?)null : a.EntryTime,
                                             ExitStatus = a.ExitStatus,
                                             ExitTime = a.ExitTime == _clearTime ? (TimeSpan?)null : a.ExitTime,
                                             Is_OUT = a.Is_OUT
                                         };

                    foreach (var log in updatedRecords)
                    {
                        var updateLog = FindEmployeeUpdateRow(db, log);

                        if (updateLog == null) continue;

                        var previousStatus = updateLog.AttendanceStatus;

                        if (updateLog.Attendance_ScheduleID == null && log.Attendance_ScheduleID != null)
                            updateLog.Attendance_ScheduleID = log.Attendance_ScheduleID;

                        ApplyEmployeeUpdate(updateLog, log);
                            ReclassifyEmployeeFromSchedule(db, updateLog);

                        db.Entry(updateLog).State = EntityState.Modified;
                        try
                        {
                            db.SaveChanges();

                            if (attSetting != null)
                            {
                                var statusChanged = previousStatus != updateLog.AttendanceStatus;
                                if (statusChanged)
                                {
                                    smsCandidates.Add(new Employee_Attendance_Record
                                    {
                                        SchoolID = updateLog.SchoolID,
                                        EmployeeID = updateLog.EmployeeID,
                                        Attendance_ScheduleID = updateLog.Attendance_ScheduleID,
                                        AttendanceStatus = updateLog.AttendanceStatus,
                                        AttendanceDate = updateLog.AttendanceDate,
                                        EntryTime = updateLog.EntryTime,
                                        ExitTime = updateLog.ExitTime,
                                        Is_OUT = updateLog.Is_OUT
                                    });
                                }
                            }
                        }
                        catch (Exception ex) when (IsIgnorableInsteadOfInsertException(ex))
                        {
                            DetachAllTrackedEntities(db);
                        }
                    }
                }

                smsCandidates = OnlyToday(smsCandidates, r => r.AttendanceDate);
                if (attSetting != null && smsCandidates.Count > 0)
                {
                    smsQueued += AttendanceEmployeeSmsService.QueueEmployeeSms(
                        db, id, attSetting, schoolName, smsCandidates);
                }
            }

            return Ok(new AttendanceSyncResult { SmsQueued = smsQueued });
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
                        var studentsByDevice = LoadStudentsForDeviceMatch(db, id)
                            .GroupBy(s => s.DeviceID)
                            .ToDictionary(g => g.Key, g => g.First());

                        foreach (var data in backupData.Where(b => b.Is_Student).OrderBy(b => b.EntryDate).OrderBy(b => b.EntryTime))
                        {
                            var dt = data.EntryDate;
                            var time = data.EntryTime;

                            var sDate = dt.ToShortDateString();

                            VW_Attendance_Stu sInfo;
                            studentsByDevice.TryGetValue(data.DeviceID, out sInfo);


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

                                int? scheduleId = data.ScheduleID == 0 ? (int?)null : data.ScheduleID;
                                var attRecord = db.Attendance_Records.FirstOrDefault(s =>
                                    s.SchoolID == id &&
                                    s.StudentID == sInfo.StudentID &&
                                    s.AttendanceDate == dt &&
                                    s.Attendance_ScheduleID == scheduleId);
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
                                        Attendance_ScheduleID = scheduleId,
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
                                    if (IsOverwritablePlaceholderRow(attRecord))
                                    {
                                        if (time <= sStartTime)
                                            attRecord.Attendance = "Pre";
                                        else if (time <= sLateTime)
                                            attRecord.Attendance = "Late";
                                        else if (time <= sEndTime)
                                            attRecord.Attendance = "Late Abs";
                                        else
                                            attRecord.Attendance = "Late Abs";

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

                                int? scheduleId = data.ScheduleID == 0 ? (int?)null : data.ScheduleID;
                                var attRecord = db.Employee_Attendance_Records.FirstOrDefault(s =>
                                    s.SchoolID == id &&
                                    s.EmployeeID == eInfo.EmployeeID &&
                                    s.AttendanceDate == dt &&
                                    s.Attendance_ScheduleID == scheduleId);
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
                                        Attendance_ScheduleID = scheduleId,
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
                                    if (IsOverwritablePlaceholderEmployeeRow(attRecord))
                                    {
                                        if (time <= sStartTime)
                                            attRecord.AttendanceStatus = "Pre";
                                        else if (time <= sLateTime)
                                            attRecord.AttendanceStatus = "Late";
                                        else if (time <= sEndTime)
                                            attRecord.AttendanceStatus = "Late Abs";
                                        else
                                            attRecord.AttendanceStatus = "Late Abs";

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

                    var sUsers = LoadStudentsForDeviceMatch(db, id);

                    var sAttendanceRecords = (from a in sAttendances
                                              join u in sUsers
                                          on a.StudentID
                                          equals u.StudentID
                                              select new AttendanceRecordAPI
                                              {
                                                  DeviceID = u.DeviceID,
                                                  ScheduleID = a.Attendance_ScheduleID ?? 0,
                                                  AttendanceDate = a.AttendanceDate,
                                                  AttendanceStatus = a.Attendance,
                                                  ExitStatus = a.ExitStatus,
                                                  Is_OUT = a.Is_OUT,
                                                  EntryTime = a.EntryTime == _clearTime ? (TimeSpan?)null : a.EntryTime,
                                                  ExitTime = a.ExitTime == _clearTime ? (TimeSpan?)null : a.ExitTime,
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
                                                  ScheduleID = a.Attendance_ScheduleID ?? 0,
                                                  AttendanceDate = a.AttendanceDate,
                                                  AttendanceStatus = a.AttendanceStatus,
                                                  ExitStatus = a.ExitStatus,
                                                  Is_OUT = a.Is_OUT,
                                                  EntryTime = a.EntryTime == _clearTime ? (TimeSpan?)null : a.EntryTime,
                                                  ExitTime = a.ExitTime == _clearTime ? (TimeSpan?)null : a.ExitTime,
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

        /// <summary>
        /// INSTEAD OF INSERT triggers commit rows but EF fails refreshing the object context.
        /// </summary>
        private static int SaveAfterInsteadOfInsertTrigger(EduContext db, int expectedInsertCount)
        {
            try
            {
                db.SaveChanges();
                return expectedInsertCount;
            }
            catch (Exception ex) when (IsIgnorableInsteadOfInsertException(ex))
            {
                DetachAllTrackedEntities(db);
                return expectedInsertCount;
            }
            catch (Exception ex) when (IsDuplicateKeyViolation(ex))
            {
                DetachAllTrackedEntities(db);
                return 0;
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

        private static bool IsInsteadOfTriggerCommitSuccess(Exception exception)
        {
            return IsIgnorableInsteadOfInsertException(exception);
        }

        private static bool IsDuplicateKeyViolation(Exception exception)
        {
            for (var ex = exception; ex != null; ex = ex.InnerException)
            {
                if (ex.Message.IndexOf("duplicate key", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
                if (ex.Message.IndexOf("UQ_Employee_Attendance_Record", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// One row per DeviceID. The SQL view joins every StudentsClass year, so EF
        /// [Key]=DeviceID throws (or drops students) when a pupil has more than one session.
        /// </summary>
        private static List<VW_Attendance_Stu> LoadStudentsForDeviceMatch(EduContext db, int schoolId)
        {
            const string sql = @"
SELECT SchoolID, DeviceID, StudentID, StudentClassID, ClassID, EducationYearID
FROM (
    SELECT v.SchoolID, v.DeviceID, v.StudentID, v.StudentClassID, v.ClassID, v.EducationYearID,
           ROW_NUMBER() OVER (
               PARTITION BY v.DeviceID
               ORDER BY CASE WHEN ey.Status = N'True' THEN 0 ELSE 1 END,
                        v.EducationYearID DESC
           ) AS rn
    FROM VW_Attendance_Stu v
    LEFT JOIN Education_Year ey
        ON ey.EducationYearID = v.EducationYearID AND ey.SchoolID = v.SchoolID
    WHERE v.SchoolID = @SchoolID AND ISNULL(v.DeviceID, 0) > 0
) d
WHERE rn = 1";

            return db.Database.SqlQuery<VW_Attendance_Stu>(
                    sql, new SqlParameter("@SchoolID", schoolId))
                .ToList();
        }

        private static List<Attendance_Record> FilterNewStudentRecords(
            EduContext db, int schoolId, List<Attendance_Record> incoming)
        {
            if (incoming == null || !incoming.Any())
                return new List<Attendance_Record>();

            var studentIds = incoming.Select(r => r.StudentID).Distinct().ToList();
            var dates = incoming.Select(r => r.AttendanceDate).Distinct().ToList();

            var existing = db.Attendance_Records
                .Where(sa => sa.SchoolID == schoolId
                             && studentIds.Contains(sa.StudentID)
                             && dates.Contains(sa.AttendanceDate))
                .Select(sa => new
                {
                    sa.StudentID,
                    sa.AttendanceDate,
                    ScheduleId = sa.Attendance_ScheduleID ?? 0
                })
                .ToList();

            return incoming.Where(na => !existing.Any(ek =>
                ek.StudentID == na.StudentID &&
                ek.AttendanceDate == na.AttendanceDate &&
                ek.ScheduleId == (na.Attendance_ScheduleID ?? 0))).ToList();
        }

        private static bool IsPlaceholderAttendanceStatus(string status)
        {
            return string.Equals(status, "Abs", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(status, "Late Abs", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsPresentLikeStatus(string status)
        {
            return string.Equals(status, "Pre", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(status, "Late", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(status, "Late Abs", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsPlaceholderTime(TimeSpan? time)
        {
            return !time.HasValue || time.Value == TimeSpan.Zero;
        }

        private static bool IsOverwritablePlaceholderRow(Attendance_Record row)
        {
            return row != null
                   && !string.Equals(row.Attendance, "Leave", StringComparison.OrdinalIgnoreCase)
                   && IsPlaceholderTime(row.EntryTime);
        }

        private static bool IsOverwritablePlaceholderEmployeeRow(Employee_Attendance_Record row)
        {
            return row != null
                   && !string.Equals(row.AttendanceStatus, "Leave", StringComparison.OrdinalIgnoreCase)
                   && IsPlaceholderTime(row.EntryTime);
        }

        // Rows inserted or edited on a manual web page carry the login RegistrationID; device/API writes use 0.
        // Such rows may only be overwritten by a real punch, never by a device Abs/placeholder push or backfill.
        private static bool IsManualRow(Attendance_Record row)
        {
            return row != null && row.RegistrationID != 0;
        }

        private static bool IsManualEmployeeRow(Employee_Attendance_Record row)
        {
            return row != null && row.RegistrationID != 0;
        }

        private static bool HasRealPunch(Attendance_Record row)
        {
            return row != null
                   && (!IsPlaceholderTime(row.EntryTime) || (row.Is_OUT && !IsPlaceholderTime(row.ExitTime)));
        }

        private static bool HasRealEmployeePunch(Employee_Attendance_Record row)
        {
            return row != null
                   && (!IsPlaceholderTime(row.EntryTime) || (row.Is_OUT && !IsPlaceholderTime(row.ExitTime)));
        }

        /// <summary>Placeholder row that <paramref name="incoming"/> may take over (manual rows only for a real punch).</summary>
        private static bool CanTakeOverPlaceholderRow(Attendance_Record row, Attendance_Record incoming)
        {
            return IsOverwritablePlaceholderRow(row) && (HasRealPunch(incoming) || !IsManualRow(row));
        }

        private static bool CanTakeOverPlaceholderEmployeeRow(Employee_Attendance_Record row, Employee_Attendance_Record incoming)
        {
            return IsOverwritablePlaceholderEmployeeRow(row) && (HasRealEmployeePunch(incoming) || !IsManualEmployeeRow(row));
        }


        private static string ClassifyEntryStatus(TimeSpan time, TimeSpan start, TimeSpan late, TimeSpan end)
        {
            if (late < start)
                late = start;
            if (time <= start)
                return "Pre";
            if (time <= late)
                return "Late";
            return "Late Abs";
        }

        private static void ReclassifyStudentFromSchedule(EduContext db, Attendance_Record row)
        {
            if (db == null || row == null || row.Is_OUT)
                return;
            if (IsPlaceholderTime(row.EntryTime) || !row.Attendance_ScheduleID.HasValue)
                return;
            if (string.Equals(row.Attendance, "Leave", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(row.Attendance, "Abs", StringComparison.OrdinalIgnoreCase))
                return;

            var dayName = row.AttendanceDate.ToString("dddd", System.Globalization.CultureInfo.InvariantCulture);
            var schedule = db.Attendance_Schedule_Days.FirstOrDefault(s =>
                s.SchoolID == row.SchoolID &&
                s.ScheduleID == row.Attendance_ScheduleID.Value &&
                s.Day == dayName);
            if (schedule == null)
            {
                schedule = db.Attendance_Schedule_Days.FirstOrDefault(s =>
                    s.SchoolID == row.SchoolID &&
                    s.ScheduleID == row.Attendance_ScheduleID.Value);
            }
            if (schedule == null)
                return;

            var time = row.EntryTime.GetValueOrDefault();
            row.Attendance = ClassifyEntryStatus(time, schedule.StartTime, schedule.LateEntryTime, schedule.EndTime);
        }

        private static void ReclassifyEmployeeFromSchedule(EduContext db, Employee_Attendance_Record row)
        {
            if (db == null || row == null || row.Is_OUT)
                return;
            if (IsPlaceholderTime(row.EntryTime) || !row.Attendance_ScheduleID.HasValue)
                return;
            if (string.Equals(row.AttendanceStatus, "Leave", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(row.AttendanceStatus, "Abs", StringComparison.OrdinalIgnoreCase))
                return;

            var dayName = row.AttendanceDate.ToString("dddd", System.Globalization.CultureInfo.InvariantCulture);
            var schedule = db.Attendance_Schedule_Days.FirstOrDefault(s =>
                s.SchoolID == row.SchoolID &&
                s.ScheduleID == row.Attendance_ScheduleID.Value &&
                s.Day == dayName);
            if (schedule == null)
            {
                schedule = db.Attendance_Schedule_Days.FirstOrDefault(s =>
                    s.SchoolID == row.SchoolID &&
                    s.ScheduleID == row.Attendance_ScheduleID.Value);
            }
            if (schedule == null)
                return;

            var time = row.EntryTime.GetValueOrDefault();
            row.AttendanceStatus = ClassifyEntryStatus(time, schedule.StartTime, schedule.LateEntryTime, schedule.EndTime);
        }

        private static void SanitizeIncomingStudentRecords(List<Attendance_Record> records)
        {
            if (records == null)
                return;

            foreach (var row in records)
            {
                if (row.Is_OUT)
                    continue;

                if (IsPlaceholderTime(row.EntryTime) && IsPresentLikeStatus(row.Attendance))
                {
                    row.Attendance = "Abs";
                    row.EntryTime = null;
                }
            }
        }

        private static void SanitizeIncomingEmployeeRecords(List<Employee_Attendance_Record> records)
        {
            if (records == null)
                return;

            foreach (var row in records)
            {
                if (row.Is_OUT)
                    continue;

                if (IsPlaceholderTime(row.EntryTime) && IsPresentLikeStatus(row.AttendanceStatus))
                {
                    row.AttendanceStatus = "Abs";
                    row.EntryTime = null;
                }
            }
        }

        /// <summary>
        /// Real punch wins over a placeholder / manual row, but merging is monotonic: the earliest real entry
        /// is kept and an existing real exit is never wiped or moved earlier. A re-sent or out-of-order device
        /// state (retry after a power cut, recovered previous-day punches) can therefore never lose data.
        /// </summary>
        private static void ApplyStudentUpdate(Attendance_Record target, Attendance_Record source)
        {
            if (target == null || source == null)
                return;

            var incomingHasRealTime = !IsPlaceholderTime(source.EntryTime);

            if (incomingHasRealTime)
            {
                if (ShouldTakeIncomingEntry(target.EntryTime, target.Attendance, source.EntryTime))
                {
                    target.Attendance = source.Attendance;
                    target.EntryTime = source.EntryTime;
                }

                if (source.Is_OUT && !IsPlaceholderTime(source.ExitTime) &&
                    ShouldTakeIncomingExit(target.Is_OUT, target.ExitTime, target.EntryTime, source.ExitTime))
                {
                    target.ExitStatus = source.ExitStatus;
                    target.ExitTime = source.ExitTime;
                    target.Is_OUT = true;
                }
                return;
            }

            if (IsOverwritablePlaceholderRow(target) && !IsManualRow(target) &&
                string.Equals(source.Attendance, "Abs", StringComparison.OrdinalIgnoreCase))
            {
                target.Attendance = "Abs";
                target.EntryTime = null;
            }

            if (source.Is_OUT && !IsPlaceholderTime(source.ExitTime))
            {
                target.ExitStatus = source.ExitStatus;
                target.ExitTime = source.ExitTime;
                target.Is_OUT = true;
            }
        }

        private static void ApplyEmployeeUpdate(Employee_Attendance_Record target, Employee_Attendance_Record source)
        {
            if (target == null || source == null)
                return;

            var incomingHasRealTime = !IsPlaceholderTime(source.EntryTime);

            if (incomingHasRealTime)
            {
                if (ShouldTakeIncomingEntry(target.EntryTime, target.AttendanceStatus, source.EntryTime))
                {
                    target.AttendanceStatus = source.AttendanceStatus;
                    target.EntryTime = source.EntryTime;
                }

                if (source.Is_OUT && !IsPlaceholderTime(source.ExitTime) &&
                    ShouldTakeIncomingExit(target.Is_OUT, target.ExitTime, target.EntryTime, source.ExitTime))
                {
                    target.ExitStatus = source.ExitStatus;
                    target.ExitTime = source.ExitTime;
                    target.Is_OUT = true;
                }
                return;
            }

            if (IsOverwritablePlaceholderEmployeeRow(target) && !IsManualEmployeeRow(target) &&
                string.Equals(source.AttendanceStatus, "Abs", StringComparison.OrdinalIgnoreCase))
            {
                target.AttendanceStatus = "Abs";
                target.EntryTime = null;
            }

            if (source.Is_OUT && !IsPlaceholderTime(source.ExitTime))
            {
                target.ExitStatus = source.ExitStatus;
                target.ExitTime = source.ExitTime;
                target.Is_OUT = true;
            }
        }

        private static bool ShouldTakeIncomingEntry(TimeSpan? currentEntry, string currentStatus, TimeSpan? incomingEntry)
        {
            if (IsPlaceholderTime(incomingEntry))
                return false;
            if (IsPlaceholderTime(currentEntry))
                return true;
            // Abs/Leave with a time is inconsistent - the punch decides.
            if (string.Equals(currentStatus, "Abs", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(currentStatus, "Leave", StringComparison.OrdinalIgnoreCase))
                return true;
            // Same entry: take the device status (schedule may have been re-classified); earlier entry wins.
            return incomingEntry.Value <= currentEntry.Value;
        }

        private static bool ShouldTakeIncomingExit(bool currentIsOut, TimeSpan? currentExit, TimeSpan? currentEntry, TimeSpan? incomingExit)
        {
            if (IsPlaceholderTime(incomingExit))
                return false;
            if (!IsPlaceholderTime(currentEntry) && incomingExit.Value < currentEntry.Value)
                return false;
            if (!currentIsOut || IsPlaceholderTime(currentExit))
                return true;
            return incomingExit.Value >= currentExit.Value;
        }

        private static List<T> OnlyToday<T>(IEnumerable<T> rows, Func<T, DateTime> dateOf)
        {
            // Recovered previous-day punches are saved but must not send "reached school at ..." SMS a day late.
            var today = DateTime.Today;
            return (rows ?? Enumerable.Empty<T>()).Where(r => dateOf(r).Date == today).ToList();
        }

        /// <summary>
        /// Inserts new rows. If the batch hits a unique-index duplicate (EF rolls back every row of the batch),
        /// retries row by row and merges each duplicate into the existing row. Returns rows NOT saved.
        /// </summary>
        private static List<T> InsertRowsWithFallback<T>(
            EduContext db, DbSet<T> set, List<T> rows, Func<T, bool> mergeIntoExisting) where T : class
        {
            var failed = new List<T>();
            if (rows == null || rows.Count == 0)
                return failed;

            set.AddRange(rows);
            try
            {
                db.SaveChanges();
                return failed;
            }
            catch (Exception ex) when (IsIgnorableInsteadOfInsertException(ex))
            {
                DetachAllTrackedEntities(db);
                return failed;
            }
            catch (Exception ex) when (IsDuplicateKeyViolation(ex))
            {
                DetachAllTrackedEntities(db);
            }

            foreach (var row in rows)
            {
                set.Add(row);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex) when (IsIgnorableInsteadOfInsertException(ex))
                {
                    DetachAllTrackedEntities(db);
                }
                catch (Exception ex) when (IsDuplicateKeyViolation(ex))
                {
                    DetachAllTrackedEntities(db);
                    if (!mergeIntoExisting(row))
                        failed.Add(row);
                }
                catch (Exception)
                {
                    DetachAllTrackedEntities(db);
                    failed.Add(row);
                }
            }

            return failed;
        }

        private bool MergeStudentIntoExisting(EduContext db, Attendance_Record row)
        {
            try
            {
                var existing = FindStudentUpdateRow(db, row);
                if (existing == null)
                    return false;
                if (IsManualRow(existing) && !HasRealPunch(row))
                    return true; // manual row protected; nothing to add
                if (existing.Attendance_ScheduleID == null && row.Attendance_ScheduleID != null)
                    existing.Attendance_ScheduleID = row.Attendance_ScheduleID;
                ApplyStudentUpdate(existing, row);
                ReclassifyStudentFromSchedule(db, existing);
                db.SaveChanges();
                return true;
            }
            catch (Exception ex) when (IsIgnorableInsteadOfInsertException(ex))
            {
                DetachAllTrackedEntities(db);
                return true;
            }
            catch (Exception)
            {
                DetachAllTrackedEntities(db);
                return false;
            }
        }

        private bool MergeEmployeeIntoExisting(EduContext db, Employee_Attendance_Record row)
        {
            try
            {
                var existing = FindEmployeeUpdateRow(db, row);
                if (existing == null)
                    return false;
                if (IsManualEmployeeRow(existing) && !HasRealEmployeePunch(row))
                    return true;
                if (existing.Attendance_ScheduleID == null && row.Attendance_ScheduleID != null)
                    existing.Attendance_ScheduleID = row.Attendance_ScheduleID;
                ApplyEmployeeUpdate(existing, row);
                ReclassifyEmployeeFromSchedule(db, existing);
                db.SaveChanges();
                return true;
            }
            catch (Exception ex) when (IsIgnorableInsteadOfInsertException(ex))
            {
                DetachAllTrackedEntities(db);
                return true;
            }
            catch (Exception)
            {
                DetachAllTrackedEntities(db);
                return false;
            }
        }

        private static bool SameAttendanceDay(DateTime left, DateTime right)
        {
            return left.Date == right.Date;
        }

        private Attendance_Record FindStudentUpdateRow(EduContext db, Attendance_Record log)
        {
            var day = log.AttendanceDate.Date;
            var next = day.AddDays(1);
            var rows = db.Attendance_Records
                .Where(u =>
                    u.SchoolID == log.SchoolID &&
                    u.StudentID == log.StudentID &&
                    u.AttendanceDate >= day &&
                    u.AttendanceDate < next)
                .ToList();

            var updateLog = rows.FirstOrDefault(u =>
                u.Attendance_ScheduleID == log.Attendance_ScheduleID ||
                (u.Attendance_ScheduleID == null && log.Attendance_ScheduleID != null));

            // Real punch OR Abs correction must overwrite Pre/Late Abs + 12:00 AM rows.
            if (updateLog == null)
                updateLog = rows.FirstOrDefault(r => CanTakeOverPlaceholderRow(r, log));

            return updateLog;
        }

        private Employee_Attendance_Record FindEmployeeUpdateRow(EduContext db, Employee_Attendance_Record log)
        {
            var day = log.AttendanceDate.Date;
            var next = day.AddDays(1);
            var rows = db.Employee_Attendance_Records
                .Where(u =>
                    u.SchoolID == log.SchoolID &&
                    u.EmployeeID == log.EmployeeID &&
                    u.AttendanceDate >= day &&
                    u.AttendanceDate < next)
                .ToList();

            var updateLog = rows.FirstOrDefault(u =>
                u.Attendance_ScheduleID == log.Attendance_ScheduleID ||
                (u.Attendance_ScheduleID == null && log.Attendance_ScheduleID != null));

            if (updateLog == null)
                updateLog = rows.FirstOrDefault(r => CanTakeOverPlaceholderEmployeeRow(r, log));

            return updateLog;
        }

        private static List<Employee_Attendance_Record> FilterNewEmployeeRecords(
            EduContext db, int schoolId, List<Employee_Attendance_Record> incoming)
        {
            if (incoming == null || !incoming.Any())
                return new List<Employee_Attendance_Record>();

            var employeeIds = incoming.Select(r => r.EmployeeID).Distinct().ToList();
            var dates = incoming.Select(r => r.AttendanceDate).Distinct().ToList();

            var existing = db.Employee_Attendance_Records
                .Where(sa => sa.SchoolID == schoolId
                             && employeeIds.Contains(sa.EmployeeID)
                             && dates.Contains(sa.AttendanceDate))
                .Select(sa => new
                {
                    sa.EmployeeID,
                    sa.AttendanceDate,
                    ScheduleId = sa.Attendance_ScheduleID ?? 0
                })
                .ToList();

            return incoming.Where(na => !existing.Any(ek =>
                ek.EmployeeID == na.EmployeeID &&
                ek.AttendanceDate == na.AttendanceDate &&
                ek.ScheduleId == (na.Attendance_ScheduleID ?? 0))).ToList();
        }

        private static void BackfillStudentScheduleRows(EduContext db, List<Attendance_Record> incoming)
        {
            var updated = false;

            foreach (var row in incoming.Where(r => r.Attendance_ScheduleID.HasValue))
            {
                var day = row.AttendanceDate.Date;
                var next = day.AddDays(1);
                var dayRows = db.Attendance_Records
                    .Where(sa =>
                        sa.SchoolID == row.SchoolID &&
                        sa.StudentID == row.StudentID &&
                        sa.AttendanceDate >= day &&
                        sa.AttendanceDate < next)
                    .ToList();

                var scheduled = dayRows.FirstOrDefault(sa =>
                    sa.Attendance_ScheduleID == row.Attendance_ScheduleID);

                if (scheduled != null)
                {
                    MergeStudentAttendanceRow(scheduled, row);
                    updated = true;

                    var legacyDuplicate = dayRows.FirstOrDefault(sa =>
                        sa.Attendance_ScheduleID == null && !IsManualRow(sa));

                    if (legacyDuplicate != null)
                    {
                        db.Attendance_Records.Remove(legacyDuplicate);
                        updated = true;
                    }

                    continue;
                }

                // Same student/day Pre + 12:00 AM with wrong/missing schedule — still overwrite.
                var placeholder = dayRows.FirstOrDefault(sa => CanTakeOverPlaceholderRow(sa, row));
                if (placeholder != null)
                {
                    placeholder.Attendance_ScheduleID = row.Attendance_ScheduleID;
                    MergeStudentAttendanceRow(placeholder, row);
                    updated = true;
                    continue;
                }

                var legacy = dayRows.FirstOrDefault(sa =>
                    sa.Attendance_ScheduleID == null && (HasRealPunch(row) || !IsManualRow(sa)));
                if (legacy == null)
                    continue;

                legacy.Attendance_ScheduleID = row.Attendance_ScheduleID;
                MergeStudentAttendanceRow(legacy, row);
                updated = true;
            }

            if (updated)
            {
                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex) when (IsIgnorableInsteadOfInsertException(ex))
                {
                    DetachAllTrackedEntities(db);
                }
            }
        }

        private static void MergeStudentAttendanceRow(Attendance_Record target, Attendance_Record source)
        {
            ApplyStudentUpdate(target, source);
        }

        private static void BackfillEmployeeScheduleRows(EduContext db, List<Employee_Attendance_Record> incoming)
        {
            var updated = false;

            foreach (var row in incoming.Where(r => r.Attendance_ScheduleID.HasValue))
            {
                // Day range (like the student backfill): legacy rows may carry a time part in AttendanceDate.
                var day = row.AttendanceDate.Date;
                var next = day.AddDays(1);
                var scheduled = db.Employee_Attendance_Records.FirstOrDefault(sa =>
                    sa.SchoolID == row.SchoolID &&
                    sa.EmployeeID == row.EmployeeID &&
                    sa.AttendanceDate >= day && sa.AttendanceDate < next &&
                    sa.Attendance_ScheduleID == row.Attendance_ScheduleID);

                if (scheduled != null)
                {
                    MergeEmployeeAttendanceRow(scheduled, row);
                    updated = true;

                    var legacyDuplicate = db.Employee_Attendance_Records.FirstOrDefault(sa =>
                        sa.SchoolID == row.SchoolID &&
                        sa.EmployeeID == row.EmployeeID &&
                        sa.AttendanceDate >= day && sa.AttendanceDate < next &&
                        sa.Attendance_ScheduleID == null &&
                        sa.RegistrationID == 0);

                    if (legacyDuplicate != null)
                    {
                        db.Employee_Attendance_Records.Remove(legacyDuplicate);
                        updated = true;
                    }

                    continue;
                }

                var legacy = db.Employee_Attendance_Records.FirstOrDefault(sa =>
                    sa.SchoolID == row.SchoolID &&
                    sa.EmployeeID == row.EmployeeID &&
                    sa.AttendanceDate >= day && sa.AttendanceDate < next &&
                    sa.Attendance_ScheduleID == null);

                if (legacy == null)
                    continue;
                if (IsManualEmployeeRow(legacy) && !HasRealEmployeePunch(row))
                    continue;

                legacy.Attendance_ScheduleID = row.Attendance_ScheduleID;
                MergeEmployeeAttendanceRow(legacy, row);
                updated = true;
            }

            if (updated)
            {
                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex) when (IsIgnorableInsteadOfInsertException(ex))
                {
                    DetachAllTrackedEntities(db);
                }
            }
        }

        private static void MergeEmployeeAttendanceRow(Employee_Attendance_Record target, Employee_Attendance_Record source)
        {
            ApplyEmployeeUpdate(target, source);
        }

        private static void DetachAllTrackedEntities(EduContext db)
        {
            foreach (var entry in db.ChangeTracker.Entries().ToList())
                entry.State = EntityState.Detached;
        }

    }
}
