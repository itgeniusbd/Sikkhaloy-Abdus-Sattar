using System.Data.SqlClient;
using System.Linq;
using System.Web.Http;
using Attendance_API.DB_Model;

namespace Attendance_API.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("api/Test")]
    public class TestController : ApiController
    {
        // GET api/Test/ping
        [Route("ping")]
        [HttpGet]
        public IHttpActionResult Ping()
        {
            return Ok(new { status = "API is running", timestamp = System.DateTime.Now });
        }

        // GET api/Test/database
        [Route("database")]
        [HttpGet]
        public IHttpActionResult TestDatabase()
        {
            try
            {
                using (var db = new EduContext())
                {
                    // Try to execute a simple query
                    var count = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM SchoolInfo").FirstOrDefault();
                    
                    return Ok(new 
                    { 
                        status = "Database connected successfully",
                        totalSchools = count,
                        server = db.Database.Connection.DataSource,
                        database = db.Database.Connection.Database,
                        timestamp = System.DateTime.Now
                    });
                }
            }
            catch (SqlException ex)
            {
                return Ok(new 
                { 
                    status = "Database connection failed",
                    error = ex.Message,
                    errorNumber = ex.Number,
                    server = ex.Server,
                    timestamp = System.DateTime.Now
                });
            }
            catch (System.Exception ex)
            {
                return Ok(new 
                { 
                    status = "Error occurred",
                    error = ex.Message,
                    timestamp = System.DateTime.Now
                });
            }
        }

        // POST api/Test/inserttestsms
        [Route("inserttestsms")]
        [HttpPost]
        public IHttpActionResult InsertTestSms([FromBody] TestSmsRequest request)
        {
            try
            {
                if (request == null)
                {
                    request = new TestSmsRequest
                    {
                        SchoolId = 1,
                        MobileNo = "01700000000",
                        SmsText = "Test SMS from Sikkhaloy SMS Sender"
                    };
                }

                using (var db = new EduContext())
                {
                    var now = System.DateTime.Now;
                    
                    var testSms = new Attendance_SMS
                    {
                        SchoolID = request.SchoolId,
                        StudentID = 0,
                        EmployeeID = 0,
                        ScheduleTime = now.TimeOfDay,
                        AttendanceDate = now.Date,
                        SMS_Text = request.SmsText ?? "Test SMS from Sikkhaloy SMS Sender",
                        MobileNo = request.MobileNo ?? "01700000000",
                        AttendanceStatus = "Test",
                        SMS_TimeOut = 60 // 60 minutes timeout
                    };

                    db.Attendance_sms.Add(testSms);
                    db.SaveChanges();

                    return Ok(new
                    {
                        status = "Test SMS inserted successfully",
                        smsId = testSms.Attendance_SMSID,
                        schoolId = testSms.SchoolID,
                        mobileNo = testSms.MobileNo,
                        smsText = testSms.SMS_Text,
                        scheduleTime = testSms.ScheduleTime.ToString(),
                        attendanceDate = testSms.AttendanceDate.ToString("yyyy-MM-dd"),
                        timeout = testSms.SMS_TimeOut,
                        message = "SMS will be sent within the next timer interval",
                        timestamp = System.DateTime.Now
                    });
                }
            }
            catch (System.Exception ex)
            {
                return Ok(new
                {
                    status = "Failed to insert test SMS",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message,
                    timestamp = System.DateTime.Now
                });
            }
        }

        // GET api/Test/inserttestsms?schoolId=1&mobileNo=01712345678&smsText=Test
        [Route("inserttestsms")]
        [HttpGet]
        public IHttpActionResult InsertTestSmsGet(int schoolId = 1, string mobileNo = "01700000000", string smsText = "Test SMS from Sikkhaloy")
        {
            try
            {
                using (var db = new EduContext())
                {
                    var now = System.DateTime.Now;
                    
                    var testSms = new Attendance_SMS
                    {
                        SchoolID = schoolId,
                        StudentID = 0,
                        EmployeeID = 0,
                        ScheduleTime = now.TimeOfDay,
                        AttendanceDate = now.Date,
                        SMS_Text = smsText,
                        MobileNo = mobileNo,
                        AttendanceStatus = "Test",
                        SMS_TimeOut = 60
                    };

                    db.Attendance_sms.Add(testSms);
                    db.SaveChanges();

                    return Ok(new
                    {
                        status = "Test SMS inserted successfully",
                        smsId = testSms.Attendance_SMSID,
                        schoolId = testSms.SchoolID,
                        mobileNo = testSms.MobileNo,
                        smsText = testSms.SMS_Text,
                        scheduleTime = testSms.ScheduleTime.ToString(),
                        attendanceDate = testSms.AttendanceDate.ToString("yyyy-MM-dd"),
                        timeout = testSms.SMS_TimeOut,
                        message = "SMS will be sent within the next timer interval (check your SmsSenderApp)",
                        timestamp = System.DateTime.Now
                    });
                }
            }
            catch (System.Exception ex)
            {
                return Ok(new
                {
                    status = "Failed to insert test SMS",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message,
                    timestamp = System.DateTime.Now
                });
            }
        }
    }

    public class TestSmsRequest
    {
        public int SchoolId { get; set; }
        public string MobileNo { get; set; }
        public string SmsText { get; set; }
    }
}
