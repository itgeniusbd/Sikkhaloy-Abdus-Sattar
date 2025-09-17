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
    //[Authorize]
    public class UsersController : ApiController
    {
        [Route("api/Users/{id}")]
        [HttpGet]
        public async Task<IEnumerable<VW_Attendance_Users>> Get(int id)
        {
            using (var db = new EduContext())
            {
                return await db.VW_Attendance_Users.Where(a => a.SchoolID == id && a.ScheduleID != null).ToListAsync();
            }
        }

        [Route("api/Users/{id}/schedule")]
        [HttpGet]
        public async Task<IEnumerable<Attendance_Schedule_Day>> GetSchedule(int id)
        {
            using (var db = new EduContext())
            {
                var day = DateTime.Now.ToString("dddd");
                return await db.Attendance_Schedule_Days.Where(a => a.SchoolID == id && a.Day == day).ToListAsync();
            }
        }

        [Route("api/Users/{id}/leave")]
        [HttpGet]
        public async Task<IEnumerable<LeaveVM>> AttendanceLeave(int id)
        {
            var today = DateTime.Today.ToShortDateString();
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

    }
}
