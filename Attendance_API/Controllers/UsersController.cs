using Attendance_API.DB_Model;
using Attendance_API.Models;
using Attendance_API.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace Attendance_API.Controllers
{
    //[Authorize]
    public class UsersController : ApiController
    {
        [Route("api/Users/{id}")]
        [HttpGet]
        public async Task<IEnumerable<VW_Attendance_Users>> Get(int id)
        {
            using (var db = new EduContext())
            {
                // Multi-schedule: students may have ScheduleID only in assign tables (or User_Schedule sync).
                // Match web CSV (VW_Attendance_Users) for students; employees still need a schedule.
                const string sql = @"
                    SELECT
                        u.DeviceID,
                        u.SchoolID,
                        COALESCE(NULLIF(u.ScheduleID, 0), stuAss.ScheduleID, empAss.ScheduleID) AS ScheduleID,
                        u.ID,
                        u.RFID,
                        u.Name,
                        u.Designation,
                        CAST(CASE
                            WHEN ISNULL(u.Is_Student, 0) = 1 THEN 1
                            WHEN stuAss.ScheduleID IS NOT NULL THEN 1
                            WHEN EXISTS (
                                SELECT 1
                                FROM Student s
                                WHERE s.SchoolID = @SchoolID
                                  AND s.DeviceID = u.DeviceID
                                  AND s.Status = 'Active'
                            ) THEN 1
                            ELSE 0
                        END AS bit) AS Is_Student
                    FROM VW_Attendance_Users u
                    OUTER APPLY (
                        SELECT TOP 1 ass.ScheduleID
                        FROM Student s
                        INNER JOIN Attendance_Schedule_AssignStudent ass ON s.StudentID = ass.StudentID
                        INNER JOIN Attendance_Schedule sch ON ass.ScheduleID = sch.ScheduleID AND ass.SchoolID = sch.SchoolID
                        WHERE s.SchoolID = @SchoolID
                          AND s.DeviceID = u.DeviceID
                          AND s.Status = 'Active'
                          AND ass.SchoolID = @SchoolID
                        ORDER BY sch.StartTime, ass.ScheduleID
                    ) stuAss
                    OUTER APPLY (
                        SELECT TOP 1 eas.ScheduleID
                        FROM Employee_Info e
                        INNER JOIN Employee_Attendance_Schedule_Assign eas ON e.EmployeeID = eas.EmployeeID
                        WHERE e.SchoolID = @SchoolID
                          AND e.DeviceID = u.DeviceID
                          AND e.Job_Status = 'Active'
                          AND eas.SchoolID = @SchoolID
                        ORDER BY eas.ScheduleID
                    ) empAss
                    WHERE u.SchoolID = @SchoolID
                      AND u.DeviceID > 0
                      AND COALESCE(stuAss.ScheduleID, empAss.ScheduleID, NULLIF(u.ScheduleID, 0)) IS NOT NULL";

                var users = await db.Database.SqlQuery<VW_Attendance_Users>(sql,
                    new System.Data.SqlClient.SqlParameter("@SchoolID", id)).ToListAsync();

                return users
                    .GroupBy(a => a.DeviceID)
                    .Select(g => g.First())
                    .ToList();
            }
        }

        [Route("api/Users/{id}/schedules")]
        [HttpGet]
        public IHttpActionResult GetDeviceScheduleAssignments(int id)
        {
            using (var db = new EduContext())
            {
                var sql = @"
                    SELECT s.DeviceID, ass.ScheduleID, 1 AS IsStudent
                    FROM Student s
                    INNER JOIN Attendance_Schedule_AssignStudent ass ON s.StudentID = ass.StudentID
                    WHERE s.SchoolID = @SchoolID AND s.Status = 'Active' AND ass.SchoolID = @SchoolID

                    UNION ALL

                    SELECT e.DeviceID, eas.ScheduleID, 0 AS IsStudent
                    FROM Employee_Info e
                    INNER JOIN Employee_Attendance_Schedule_Assign eas ON e.EmployeeID = eas.EmployeeID
                    WHERE e.SchoolID = @SchoolID AND e.Job_Status = 'Active' AND eas.SchoolID = @SchoolID";

                var result = db.Database.SqlQuery<DeviceScheduleAssignmentVM>(sql,
                    new System.Data.SqlClient.SqlParameter("@SchoolID", id)).ToList();

                return Ok(result);
            }
        }

        [Route("api/Users/{id}/schedule")]
        [HttpGet]
        public async Task<IEnumerable<ScheduleDayDeviceVM>> GetSchedule(int id)
        {
            using (var db = new EduContext())
            {
                const string sql = @"
                    SELECT sd.ScheduleDayID,
                           sd.ScheduleID,
                           sd.SchoolID,
                           sd.Day,
                           CONVERT(varchar(8), sd.StartTime, 108) AS StartTime,
                           CONVERT(varchar(8), sd.LateEntryTime, 108) AS LateEntryTime,
                           CONVERT(varchar(8), sd.EndTime, 108) AS EndTime,
                           sd.Is_OnDay,
                           ISNULL(NULLIF(LTRIM(RTRIM(sch.ScheduleName)), N''),
                               N'Schedule ' + CAST(sd.ScheduleID AS nvarchar(20))) AS ScheduleName
                    FROM Attendance_Schedule_Day sd
                    LEFT JOIN Attendance_Schedule sch
                        ON sd.ScheduleID = sch.ScheduleID AND sd.SchoolID = sch.SchoolID
                    WHERE sd.SchoolID = @SchoolID";

                return await db.Database.SqlQuery<ScheduleDayDeviceVM>(sql,
                    new System.Data.SqlClient.SqlParameter("@SchoolID", id)).ToListAsync();
            }
        }

        [Route("api/Users/{id}/leave")]
        [HttpGet]
        public async Task<IEnumerable<LeaveVM>> AttendanceLeave(int id)
        {
            var today = DateTime.Today.ToString("dd-MMM-yy");
            using (var db = new EduContext())
            {
                return await db.Attendance_User_Leaves.Where(a => a.SchoolID == id && a.StartDate <= DateTime.Today && a.EndDate >= DateTime.Today)
                    .Select(a => new LeaveVM() { DeviceID = a.DeviceID, LeaveDate = today }).Distinct().ToListAsync();
            }
        }


        [Route("api/Users/{id}/updateInfo")]
        [HttpGet]
        public async Task<IEnumerable<DataUpdateList_VM>> UpdateInfo(int id)
        {
            using (var db = new EduContext())
            {
                var dataUpdateLists = await db.Attendance_Device_DataUpdateLists.Where(a => a.SchoolID == id).ToListAsync();

                db.Attendance_Device_DataUpdateLists.RemoveRange(dataUpdateLists);
                await db.SaveChangesAsync();

                return dataUpdateLists.Select(d => new DataUpdateList_VM()
                {
                    UpdateType = d.UpdateType,
                    UpdateDescription = d.UpdateDescription,
                    UpdateDate = d.UpdateDate.ToShortDateString()
                });
            }
        }

        [Route("api/Users/{id}/FingerPrint")]
        [HttpGet]
        public async Task<IEnumerable<User_FingerPrintVM>> GetFP(int id)
        {
            using (var db = new EduContext())
            {
                return await db.Device_Finger_Print_Records
                .Where(a => a.SchoolID == id)
                .Select(a => new User_FingerPrintVM
                {
                    DeviceID = a.DeviceID,
                    Finger_Index = a.Finger_Index,
                    Flag = a.Flag,
                    Temp_Data = a.Temp_Data
                }).ToListAsync();
            }
        }


        [Route("api/Users/{id}/FingerPrintPost")]
        [HttpPost]
        public IHttpActionResult PostStudents(int id, [FromBody] List<FingerPrintRecordAPI> fingerPrintRecords)
        {
            if (fingerPrintRecords == null) return NotFound();
            if (fingerPrintRecords.Count < 1) return NotFound();

            var deviceIds = fingerPrintRecords.Select(f => f.DeviceID).ToList();

            using (var db = new EduContext())
            {
                var oldFingerPrints = db.Device_Finger_Print_Records.Where(f => deviceIds.Contains(f.DeviceID)).ToList();
                if (oldFingerPrints.Any())
                {
                    db.Device_Finger_Print_Records.RemoveRange(oldFingerPrints);
                    db.SaveChanges();
                }

                var newFingerPrints = fingerPrintRecords.Select(f => new Device_Finger_Print_Record
                {
                    SchoolID = id,
                    DeviceID = f.DeviceID,
                    Finger_Index = f.Finger_Index,
                    Temp_Data = f.Temp_Data,
                    Flag = f.Flag
                }).ToList();

                db.Device_Finger_Print_Records.AddRange(newFingerPrints);
                db.SaveChanges();
            }

            return Ok();
        }

        [Route("api/Users/{id}/photos")]
        [HttpGet]
        public IHttpActionResult GetUserPhotos(int id)
        {
            const string sql = @"
                SELECT ID, Image
                FROM VW_Attendance_Users_Image
                WHERE SchoolID = @SchoolID
                  AND Image IS NOT NULL
                  AND DATALENGTH(Image) > 0";

            using (var db = new EduContext())
            {
                var photos = db.Database.SqlQuery<UserPhotoVM>(sql,
                    new System.Data.SqlClient.SqlParameter("@SchoolID", id))
                    .GroupBy(p => p.ID)
                    .Select(g => g.First())
                    .ToList();

                return Ok(photos);
            }
        }

    }
}
