using Attendance_API.DB_Model;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace Attendance_API.Controllers
{
    //  [Authorize]
    public class SchoolController : ApiController
    {
        public async Task<SchoolVM> Get(string id)
        {

            try
            {
                SchoolVM ins;
                using (var entities = new EduContext())
                {
                    var q = from i in entities.SchoolInfos
                            join a in entities.Attendance_Device_Settings
                                on i.SchoolID equals a.SchoolID
                            where i.UserName == id
                            select new SchoolVM
                            {
                                SchoolID = i.SchoolID,
                                InstitutionName = i.SchoolName,
                                Image_Link = a.Image_Link,
                                Logo = i.SchoolLogo,
                                UserName = i.UserName,
                                SettingKey = a.SettingKey,
                                Is_Device_Attendance_Enable = a.Is_Device_Attendance_Enable,
                                Is_Employee_Attendance_Enable = a.Is_Employee_Attendance_Enable,
                                Is_Student_Attendance_Enable = a.Is_Student_Attendance_Enable,
                                Holiday_Active = !a.Is_Holiday_As_Offday,
                                IsValid = a.IsActive
                            };


                    ins = await q.FirstOrDefaultAsync<SchoolVM>();

                    var isHoliday = from h in entities.Holidays
                                    join i in entities.SchoolInfos
                                        on h.SchoolID equals i.SchoolID
                                    where i.UserName == id && h.HolidayDate == DateTime.Today
                                    select h.HolidayID;

                    ins.Is_Today_Holiday = await isHoliday.AnyAsync();
                    ins.LastUpdateDate = DateTime.Now.ToShortDateString();
                    ins.Current_Datetime = DateTime.Now;
                }

                return ins;
            }
            catch (Exception e)
            {
                return null;
            }


        }

        [Route("api/School/{id}/short")]
        [HttpGet]
        public async Task<SchoolShortVM> Short(string id)
        {
            SchoolShortVM ins;
            using (var entities = new EduContext())
            {
                var q = from i in entities.SchoolInfos
                        join a in entities.Attendance_Device_Settings
                        on i.SchoolID equals a.SchoolID
                        where i.UserName == id
                        select new SchoolShortVM
                        {
                            SchoolID = i.SchoolID,
                            IsValid = a.IsActive,
                            SettingKey = a.SettingKey,
                            Is_Device_Attendance_Enable = a.Is_Device_Attendance_Enable,
                            Is_Employee_Attendance_Enable = a.Is_Employee_Attendance_Enable,
                            Is_Student_Attendance_Enable = a.Is_Student_Attendance_Enable,
                            Holiday_NotActive = !a.Is_Holiday_As_Offday
                        };

                ins = await q.FirstOrDefaultAsync<SchoolShortVM>();

                var isHoliday = from h in entities.Holidays
                                join i in entities.SchoolInfos
                                on h.SchoolID equals i.SchoolID
                                where i.UserName == id && h.HolidayDate == DateTime.Today
                                select h.HolidayID;

                ins.Is_Today_Holiday = await isHoliday.AnyAsync();
                ins.LastUpdateDate = DateTime.Now.ToShortDateString();
                ins.Current_Datetime = DateTime.Now;
            }

            return ins;
        }

        [Route("api/School/{id}/Attendance_Enable")]
        [HttpGet]
        public async Task<bool> IsDeviceAttendaceEnable(string id)
        {
            bool isEnable;
            using (var db = new EduContext())
            {
                isEnable = await db.Attendance_Device_Settings.Where(d => d.UserName == id).Select(d => d.Is_Device_Attendance_Enable).FirstOrDefaultAsync<bool>();
            }
            return isEnable;
        }

        [Route("api/School/{id}/Student_Att_Enable")]
        [HttpGet]
        public async Task<bool> IsStudentAttEnable(string id)
        {
            bool isEnable;
            using (var db = new EduContext())
            {
                isEnable = await db.Attendance_Device_Settings.Where(d => d.UserName == id).Select(d => d.Is_Student_Attendance_Enable).FirstOrDefaultAsync<bool>();
            }
            return isEnable;
        }

        [Route("api/School/{id}/Employee_Att_Enable")]
        [HttpGet]
        public async Task<bool> IsEmployeeAttEnable(string id)
        {
            bool isEnable;
            using (var db = new EduContext())
            {
                isEnable = await db.Attendance_Device_Settings.Where(d => d.UserName == id).Select(d => d.Is_Employee_Attendance_Enable).FirstOrDefaultAsync<bool>();
            }
            return isEnable;
        }

        [Route("api/School/{id}/Holiday_Active")]
        [HttpGet]
        public async Task<bool> HolidayActive(string id)
        {
            var isEnable = false;
            using (var db = new EduContext())
            {
                isEnable = await db.Attendance_Device_Settings.Where(d => d.UserName == id).Select(d => d.Is_Holiday_As_Offday).FirstOrDefaultAsync();
            }
            return isEnable;
        }

        [Route("api/School/{id}/Today_Is_Holiday")]
        [HttpGet]
        public async Task<bool> Today_Holiday(string id)
        {
            var isEnable = false;
            using (var db = new EduContext())
            {
                var q = from h in db.Holidays
                        join i in db.SchoolInfos
                        on h.SchoolID equals i.SchoolID
                        where i.UserName == id && h.HolidayDate == DateTime.Today
                        select h.HolidayID;
                isEnable = await q.AnyAsync();
            }
            return isEnable;
        }
    }
}
