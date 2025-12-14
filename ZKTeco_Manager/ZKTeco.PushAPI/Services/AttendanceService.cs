using System;
using System.Threading.Tasks;
using ZKTeco.PushAPI.DataAccess;
using ZKTeco.PushAPI.Models;

namespace ZKTeco.PushAPI.Services
{
    /// <summary>
    /// Attendance processing service
    /// AttendanceDevice ?????????? ?? ??? logic
    /// </summary>
    public class AttendanceService
    {
        private readonly AttendanceRepository _repository;

        public AttendanceService()
        {
            _repository = new AttendanceRepository();
        }

        /// <summary>
        /// Process attendance log from device
        /// </summary>
        public async Task<bool> ProcessAttendanceLog(DeviceAttendanceLog log, int schoolID)
        {
            try
            {
                // Get device settings
                var settings = _repository.GetDeviceSettings(schoolID);
                if (settings == null || !settings.Is_Device_Attendance_Enable)
                {
                    return false;
                }

                // Check if holiday
                if (settings.Is_Holiday_As_Offday && _repository.IsHoliday(schoolID, log.AttendanceTime.Date))
                {
                    return false;
                }

                // Get user info
                var userInfo = _repository.GetDeviceUserInfo(log.DeviceID, schoolID);
                if (userInfo == null)
                {
                    return false;
                }

                // Get schedule info
                var dayName = log.AttendanceTime.ToString("dddd"); // Monday, Tuesday, etc.
                var schedule = _repository.GetScheduleInfo(userInfo.ScheduleID, dayName);
                
                if (schedule == null || !schedule.Is_OnDay)
                {
                    return false;
                }

                // Process based on user type
                if (userInfo.IsStudent && settings.Is_Student_Attendance_Enable)
                {
                    return await ProcessStudentAttendance(log, userInfo, schedule);
                }
                else if (!userInfo.IsStudent && settings.Is_Employee_Attendance_Enable)
                {
                    return await ProcessEmployeeAttendance(log, userInfo, schedule);
                }

                return false;
            }
            catch (Exception ex)
            {
                // Log error
                LogError("ProcessAttendanceLog", ex);
                return false;
            }
        }

        /// <summary>
        /// Process student attendance
        /// </summary>
        private async Task<bool> ProcessStudentAttendance(DeviceAttendanceLog log, DeviceUserInfo userInfo, ScheduleInfo schedule)
        {
            var attendanceTime = log.AttendanceTime.TimeOfDay;
            var attendanceDate = log.AttendanceTime.Date;

            var record = new StudentAttendanceRecord
            {
                SchoolID = userInfo.SchoolID,
                ClassID = userInfo.ClassID ?? 0,
                EducationYearID = userInfo.EducationYearID,
                StudentID = userInfo.UserID,
                StudentClassID = userInfo.StudentClassID ?? 0,
                AttendanceDate = attendanceDate
            };

            // Determine attendance status based on time
            if (attendanceTime <= schedule.StartTime)
            {
                // Present - On time
                record.Attendance = "Pre";
                record.EntryTime = attendanceTime;
            }
            else if (attendanceTime <= schedule.LateEntryTime)
            {
                // Late - Within grace period
                record.Attendance = "Late";
                record.EntryTime = attendanceTime;
            }
            else if (attendanceTime <= schedule.EndTime)
            {
                // Late Absent - After grace period but before end time
                record.Attendance = "Late Abs";
                record.EntryTime = attendanceTime;
            }
            else
            {
                // After end time - might be exit
                if (log.Status == 1) // Check-Out
                {
                    record.ExitTime = attendanceTime;
                    record.ExitStatus = "Out";
                    record.Is_OUT = true;
                }
                else
                {
                    // Too late for entry
                    return false;
                }
            }

            return await _repository.SaveStudentAttendance(record);
        }

        /// <summary>
        /// Process employee attendance
        /// </summary>
        private async Task<bool> ProcessEmployeeAttendance(DeviceAttendanceLog log, DeviceUserInfo userInfo, ScheduleInfo schedule)
        {
            var attendanceTime = log.AttendanceTime.TimeOfDay;
            var attendanceDate = log.AttendanceTime.Date;

            var record = new EmployeeAttendanceRecord
            {
                SchoolID = userInfo.SchoolID,
                EmployeeID = userInfo.UserID,
                RegistrationID = 0, // Set to 0 as per your existing code
                AttendanceDate = attendanceDate
            };

            // Determine attendance status based on time
            if (attendanceTime <= schedule.StartTime)
            {
                // Present - On time
                record.AttendanceStatus = "Pre";
                record.EntryTime = attendanceTime;
            }
            else if (attendanceTime <= schedule.LateEntryTime)
            {
                // Late - Within grace period
                record.AttendanceStatus = "Late";
                record.EntryTime = attendanceTime;
            }
            else if (attendanceTime <= schedule.EndTime)
            {
                // Late Absent - After grace period but before end time
                record.AttendanceStatus = "Late Abs";
                record.EntryTime = attendanceTime;
            }
            else
            {
                // After end time - might be exit
                if (log.Status == 1) // Check-Out
                {
                    record.ExitTime = attendanceTime;
                    record.ExitStatus = "Out";
                    record.Is_OUT = true;
                }
                else
                {
                    // Too late for entry
                    return false;
                }
            }

            return await _repository.SaveEmployeeAttendance(record);
        }

        /// <summary>
        /// Calculate attendance status
        /// </summary>
        private string CalculateAttendanceStatus(TimeSpan attendanceTime, ScheduleInfo schedule)
        {
            if (attendanceTime <= schedule.StartTime)
            {
                return "Pre";
            }
            else if (attendanceTime <= schedule.LateEntryTime)
            {
                return "Late";
            }
            else if (attendanceTime <= schedule.EndTime)
            {
                return "Late Abs";
            }
            else
            {
                return "Abs";
            }
        }

        /// <summary>
        /// Log error to file
        /// </summary>
        private void LogError(string method, Exception ex)
        {
            try
            {
                var logPath = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/ServiceLogs/");
                if (!System.IO.Directory.Exists(logPath))
                {
                    System.IO.Directory.CreateDirectory(logPath);
                }

                var logFile = System.IO.Path.Combine(logPath, $"service_errors_{DateTime.Now:yyyy-MM-dd}.log");
                var logEntry = $"[{DateTime.Now:HH:mm:ss}] [{method}] {ex.Message}\n{ex.StackTrace}";

                System.IO.File.AppendAllText(logFile, logEntry + Environment.NewLine);
            }
            catch
            {
                // Ignore logging errors
            }
        }
    }
}
