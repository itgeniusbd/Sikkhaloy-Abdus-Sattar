using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Http;
using ZKTeco.PushAPI.DataAccess;
using ZKTeco.PushAPI.Models;
using zkemkeeper;

namespace ZKTeco.PushAPI.Controllers
{
    /// <summary>
    /// Device Management API
    /// Device manage ???? ???? API
    /// </summary>
    [RoutePrefix("api/devices")]
    public class DeviceManagementController : ApiController
    {
        private readonly DatabaseConnection _dbConnection;
        private readonly AttendanceRepository _repository;

        public DeviceManagementController()
        {
            _dbConnection = new DatabaseConnection();
            _repository = new AttendanceRepository();
        }

        /// <summary>
        /// Get all devices for a school
        /// GET api/device/school/{schoolId}
        /// </summary>
        [HttpGet]
        [Route("school/{schoolId}")]
        public IHttpActionResult GetSchoolDevices(int schoolId)
        {
            try
            {
                var devices = new List<DeviceInfo>();

                using (var conn = _dbConnection.GetConnection())
                {
                    conn.Open();

                    var query = @"
                        SELECT 
                            DeviceID,
                            DeviceSerial,
                            DeviceName,
                            DeviceIP,
                            Port,
                            SchoolID,
                            DeviceStatus,
                            LastSyncTime
                        FROM Devices
                        WHERE SchoolID = @SchoolID
                        ORDER BY DeviceName";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SchoolID", schoolId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                devices.Add(new DeviceInfo
                                {
                                    DeviceID = Convert.ToInt32(reader["DeviceID"]),
                                    DeviceSerial = reader["DeviceSerial"].ToString(),
                                    DeviceName = reader["DeviceName"].ToString(),
                                    DeviceIP = reader["DeviceIP"].ToString(),
                                    Port = Convert.ToInt32(reader["Port"]),
                                    SchoolID = Convert.ToInt32(reader["SchoolID"]),
                                    DeviceStatus = reader["DeviceStatus"].ToString(),
                                    LastSyncTime = reader["LastSyncTime"] != DBNull.Value 
                                        ? Convert.ToDateTime(reader["LastSyncTime"]) 
                                        : (DateTime?)null
                                });
                            }
                        }
                    }
                }

                return Ok(devices);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get device by serial number
        /// GET api/device/serial/{serialNumber}
        /// </summary>
        [HttpGet]
        [Route("serial/{serialNumber}")]
        public IHttpActionResult GetDeviceBySerial(string serialNumber)
        {
            try
            {
                using (var conn = _dbConnection.GetConnection())
                {
                    conn.Open();

                    var query = @"
                        SELECT 
                            d.DeviceID,
                            d.DeviceSerial,
                            d.DeviceName,
                            d.DeviceIP,
                            d.Port,
                            d.SchoolID,
                            d.DeviceStatus,
                            d.LastSyncTime,
                            s.SchoolName
                        FROM Devices d
                        INNER JOIN SchoolInfo s ON d.SchoolID = s.SchoolID
                        WHERE d.DeviceSerial = @DeviceSerial";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@DeviceSerial", serialNumber);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var device = new DeviceInfo
                                {
                                    DeviceID = Convert.ToInt32(reader["DeviceID"]),
                                    DeviceSerial = reader["DeviceSerial"].ToString(),
                                    DeviceName = reader["DeviceName"].ToString(),
                                    DeviceIP = reader["DeviceIP"].ToString(),
                                    Port = Convert.ToInt32(reader["Port"]),
                                    SchoolID = Convert.ToInt32(reader["SchoolID"]),
                                    DeviceStatus = reader["DeviceStatus"].ToString(),
                                    SchoolName = reader["SchoolName"].ToString(),
                                    LastSyncTime = reader["LastSyncTime"] != DBNull.Value 
                                        ? Convert.ToDateTime(reader["LastSyncTime"]) 
                                        : (DateTime?)null
                                };

                                return Ok(device);
                            }
                        }
                    }
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Register a new device
        /// POST api/device/register
        /// </summary>
        [HttpPost]
        [Route("register")]
        public IHttpActionResult RegisterDevice([FromBody] DeviceRegistrationModel model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest("Invalid device data");
                }

                using (var conn = _dbConnection.GetConnection())
                {
                    conn.Open();

                    // Check if device already exists
                    var checkQuery = "SELECT COUNT(*) FROM Devices WHERE DeviceSerial = @DeviceSerial";
                    using (var checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@DeviceSerial", model.DeviceSerial);
                        var exists = (int)checkCmd.ExecuteScalar() > 0;

                        if (exists)
                        {
                            return BadRequest("Device with this serial number already exists");
                        }
                    }

                    // Insert new device
                    var insertQuery = @"
                        INSERT INTO Devices 
                        (DeviceSerial, DeviceName, DeviceIP, Port, SchoolID, DeviceStatus, CommKey)
                        VALUES 
                        (@DeviceSerial, @DeviceName, @DeviceIP, @Port, @SchoolID, @DeviceStatus, @CommKey);
                        SELECT CAST(SCOPE_IDENTITY() as int)";

                    using (var insertCmd = new SqlCommand(insertQuery, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@DeviceSerial", model.DeviceSerial);
                        insertCmd.Parameters.AddWithValue("@DeviceName", model.DeviceName);
                        insertCmd.Parameters.AddWithValue("@DeviceIP", model.DeviceIP);
                        insertCmd.Parameters.AddWithValue("@Port", model.Port);
                        insertCmd.Parameters.AddWithValue("@SchoolID", model.SchoolID);
                        insertCmd.Parameters.AddWithValue("@DeviceStatus", "Active");
                        insertCmd.Parameters.AddWithValue("@CommKey", model.CommKey);

                        var deviceId = (int)insertCmd.ExecuteScalar();

                        return Ok(new { 
                            Success = true, 
                            DeviceID = deviceId, 
                            Message = "Device registered successfully" 
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Update device sync time
        /// POST api/device/sync/{deviceId}
        /// </summary>
        [HttpPost]
        [Route("sync/{deviceId}")]
        public IHttpActionResult UpdateSyncTime(int deviceId)
        {
            try
            {
                using (var conn = _dbConnection.GetConnection())
                {
                    conn.Open();

                    var query = "UPDATE Devices SET LastSyncTime = @SyncTime WHERE DeviceID = @DeviceID";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SyncTime", DateTime.Now);
                        cmd.Parameters.AddWithValue("@DeviceID", deviceId);

                        var affected = cmd.ExecuteNonQuery();

                        if (affected > 0)
                        {
                            return Ok(new { Success = true, Message = "Sync time updated" });
                        }
                    }
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get users for device synchronization
        /// GET api/device/{deviceId}/users
        /// </summary>
        [HttpGet]
        [Route("{deviceId}/users")]
        public IHttpActionResult GetDeviceUsers(int deviceId)
        {
            try
            {
                var users = new List<DeviceUserData>();

                using (var conn = _dbConnection.GetConnection())
                {
                    conn.Open();

                    // Get device info first
                    var deviceQuery = "SELECT SchoolID FROM Devices WHERE DeviceID = @DeviceID";
                    int schoolId = 0;

                    using (var deviceCmd = new SqlCommand(deviceQuery, conn))
                    {
                        deviceCmd.Parameters.AddWithValue("@DeviceID", deviceId);
                        var result = deviceCmd.ExecuteScalar();
                        
                        if (result == null)
                        {
                            return NotFound();
                        }
                        
                        schoolId = Convert.ToInt32(result);
                    }

                    // Get students
                    var studentQuery = @"
                        SELECT 
                            s.DeviceID as UserDeviceID,
                            s.StudentsName as UserName,
                            s.ID as UserCode,
                            'Student' as UserType,
                            s.Status
                        FROM Student s
                        INNER JOIN Student_Class sc ON s.StudentID = sc.StudentID
                        INNER JOIN Education_Year ey ON sc.EducationYearID = ey.EducationYearID
                        WHERE s.SchoolID = @SchoolID 
                            AND s.Status = 'Active'
                            AND ey.Status = 'True'
                            AND s.DeviceID IS NOT NULL
                        ORDER BY s.DeviceID";

                    using (var cmd = new SqlCommand(studentQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@SchoolID", schoolId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                users.Add(new DeviceUserData
                                {
                                    UserDeviceID = Convert.ToInt32(reader["UserDeviceID"]),
                                    UserName = reader["UserName"].ToString(),
                                    UserCode = reader["UserCode"].ToString(),
                                    UserType = reader["UserType"].ToString(),
                                    Status = reader["Status"].ToString()
                                });
                            }
                        }
                    }

                    // Get employees
                    var employeeQuery = @"
                        SELECT 
                            vw.DeviceID as UserDeviceID,
                            vw.FirstName + ' ' + vw.LastName as UserName,
                            vw.ID as UserCode,
                            'Employee' as UserType,
                            vw.Job_Status as Status
                        FROM VW_Emp_Info vw
                        WHERE vw.SchoolID = @SchoolID 
                            AND vw.Job_Status = 'Active'
                            AND vw.DeviceID IS NOT NULL
                        ORDER BY vw.DeviceID";

                    using (var cmd = new SqlCommand(employeeQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@SchoolID", schoolId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                users.Add(new DeviceUserData
                                {
                                    UserDeviceID = Convert.ToInt32(reader["UserDeviceID"]),
                                    UserName = reader["UserName"].ToString(),
                                    UserCode = reader["UserCode"].ToString(),
                                    UserType = reader["UserType"].ToString(),
                                    Status = reader["Status"].ToString()
                                });
                            }
                        }
                    }
                }

                return Ok(new
                {
                    TotalUsers = users.Count,
                    Students = users.Count(u => u.UserType == "Student"),
                    Employees = users.Count(u => u.UserType == "Employee"),
                    Users = users
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get fingerprint templates for device
        /// GET api/device/{deviceId}/fingerprints
        /// </summary>
        [HttpGet]
        [Route("{deviceId}/fingerprints")]
        public IHttpActionResult GetDeviceFingerprints(int deviceId)
        {
            try
            {
                var fingerprints = new List<DeviceFingerprintData>();

                using (var conn = _dbConnection.GetConnection())
                {
                    conn.Open();

                    // Get device school ID
                    var deviceQuery = "SELECT SchoolID FROM Devices WHERE DeviceID = @DeviceID";
                    int schoolId = 0;

                    using (var deviceCmd = new SqlCommand(deviceQuery, conn))
                    {
                        deviceCmd.Parameters.AddWithValue("@DeviceID", deviceId);
                        var result = deviceCmd.ExecuteScalar();
                        
                        if (result == null)
                        {
                            return NotFound();
                        }
                        
                        schoolId = Convert.ToInt32(result);
                    }

                    // Get student fingerprints
                    var fpQuery = @"
                        SELECT 
                            s.DeviceID as UserDeviceID,
                            ufp.Finger_Index as FingerIndex,
                            ufp.Temp_Data as TemplateData,
                            ufp.Flag,
                            'Student' as UserType
                        FROM User_FingerPrint ufp
                        INNER JOIN Student s ON ufp.StudentID = s.StudentID
                        WHERE s.SchoolID = @SchoolID 
                            AND s.Status = 'Active'
                            AND s.DeviceID IS NOT NULL
                            AND ufp.Temp_Data IS NOT NULL";

                    using (var cmd = new SqlCommand(fpQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@SchoolID", schoolId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                fingerprints.Add(new DeviceFingerprintData
                                {
                                    UserDeviceID = Convert.ToInt32(reader["UserDeviceID"]),
                                    FingerIndex = Convert.ToInt32(reader["FingerIndex"]),
                                    TemplateData = reader["TemplateData"].ToString(),
                                    Flag = Convert.ToInt32(reader["Flag"]),
                                    UserType = reader["UserType"].ToString()
                                });
                            }
                        }
                    }

                    // Get employee fingerprints (if you have employee fingerprint table)
                    // Add similar query here if needed
                }

                return Ok(new
                {
                    TotalFingerprints = fingerprints.Count,
                    Fingerprints = fingerprints
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Test device connection and get device info
        /// GET api/devices/test?sn=DEVICE_SERIAL
        /// </summary>
        [HttpGet]
        [Route("test")]
        public IHttpActionResult TestDevice()
        {
            try
            {
                var serialNumber = System.Web.HttpContext.Current.Request.QueryString["sn"];

                if (string.IsNullOrEmpty(serialNumber))
                {
                    return BadRequest("Device serial number (sn) is required");
                }

                // Get device info from Device_Institution_Mapping
                var device = _repository.GetDeviceInfo(serialNumber);

                if (device == null)
                {
                    return Ok(new
                    {
                        success = false,
                        deviceSerial = serialNumber,
                        message = "Device not registered",
                        instructions = "Please register this device in Device_Institution_Mapping table"
                    });
                }

                return Ok(new
                {
                    success = true,
                    deviceSerial = serialNumber,
                    schoolID = device.SchoolID,
                    schoolName = device.SchoolName,
                    deviceName = device.DeviceName,
                    location = device.DeviceLocation,
                    lastPushTime = device.LastPushTime,
                    isActive = device.IsActive,
                    connectionStatus = "OK",
                    message = $"Device '{device.DeviceName}' is registered for {device.SchoolName}"
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get API version for debugging
        /// GET api/devices/version
        /// </summary>
        [HttpGet]
        [Route("version")]
        public IHttpActionResult GetVersion()
        {
            return Ok(new
            {
                version = "1.0.4",
                buildDate = "2024-12-11 18:30:00",
                message = "ZKTeco Push API - Date format changed to dd-MM-yyyy",
                serverTime = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")
            });
        }

        /// <summary>
        /// Get today's attendance records for monitoring
        /// GET api/devices/attendance/today?schoolID=1012
        /// </summary>
        [HttpGet]
        [Route("attendance/today")]
        public IHttpActionResult GetTodayAttendance()
        {
            try
            {
                var schoolIDParam = System.Web.HttpContext.Current.Request.QueryString["schoolID"];
                
                if (string.IsNullOrEmpty(schoolIDParam))
                {
                    return BadRequest("School ID is required. Use: ?schoolID=1012");
                }

                int schoolID = int.Parse(schoolIDParam);

                var attendanceRecords = new List<object>();
                
                using (var conn = _dbConnection.GetConnection())
                {
                    conn.Open();

                    // Get today's employee attendance with detailed info
                    var query = @"
                        SELECT 
                            ear.Employee_Attendance_RecordID,
                            ear.EmployeeID,
                            CASE 
                                WHEN vw.FirstName IS NOT NULL 
                                THEN vw.FirstName + ' ' + ISNULL(vw.LastName, '')
                                ELSE 'Employee ID: ' + CAST(ear.EmployeeID AS VARCHAR(10))
                            END AS EmployeeName,
                            vw.ID AS EmployeeCode,
                            vw.DeviceID AS EmployeeDeviceID,
                            ear.AttendanceDate,
                            FORMAT(ear.AttendanceDate, 'dd-MM-yyyy') AS FormattedDate,
                            CAST(ear.EntryTime AS VARCHAR(8)) AS EntryTime,
                            CAST(ear.ExitTime AS VARCHAR(8)) AS ExitTime,
                            ear.AttendanceStatus,
                            ear.ExitStatus,
                            ear.Is_OUT,
                            s.SchoolName,
                            DATEDIFF(day, CAST(ear.AttendanceDate AS DATE), CAST(GETDATE() AS DATE)) AS DaysAgo
                        FROM Employee_Attendance_Record ear
                        LEFT JOIN VW_Emp_Info vw ON ear.EmployeeID = vw.EmployeeID
                        LEFT JOIN SchoolInfo s ON ear.SchoolID = s.SchoolID
                        WHERE CAST(ear.AttendanceDate AS DATE) = CAST(GETDATE() AS DATE)
                            AND ear.SchoolID = @SchoolID
                        ORDER BY ear.AttendanceDate DESC, ear.EntryTime DESC";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SchoolID", schoolID);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                attendanceRecords.Add(new
                                {
                                    recordID = reader["Employee_Attendance_RecordID"],
                                    employeeID = reader["EmployeeID"],
                                    employeeName = reader["EmployeeName"]?.ToString(),
                                    employeeCode = reader["EmployeeCode"]?.ToString(),
                                    deviceID = reader["EmployeeDeviceID"] != DBNull.Value ? (int?)reader["EmployeeDeviceID"] : null,
                                    attendanceDate = reader["AttendanceDate"] != DBNull.Value ? Convert.ToDateTime(reader["AttendanceDate"]) : (DateTime?)null,
                                    formattedDate = reader["FormattedDate"]?.ToString(),
                                    entryTime = reader["EntryTime"]?.ToString(),
                                    exitTime = reader["ExitTime"]?.ToString(),
                                    attendanceStatus = reader["AttendanceStatus"]?.ToString(),
                                    exitStatus = reader["ExitStatus"]?.ToString(),
                                    isOut = reader["Is_OUT"] != DBNull.Value && Convert.ToBoolean(reader["Is_OUT"]),
                                    schoolName = reader["SchoolName"]?.ToString(),
                                    daysAgo = reader["DaysAgo"] != DBNull.Value ? Convert.ToInt32(reader["DaysAgo"]) : 0
                                });
                            }
                        }
                    }

                    // Get summary statistics
                    var summaryQuery = @"
                        SELECT 
                            COUNT(*) AS TotalRecords,
                            COUNT(DISTINCT ear.EmployeeID) AS UniqueEmployees,
                            SUM(CASE WHEN ISNULL(ear.Is_OUT, 0) = 0 THEN 1 ELSE 0 END) AS EntryRecords,
                            SUM(CASE WHEN ISNULL(ear.Is_OUT, 0) = 1 THEN 1 ELSE 0 END) AS ExitRecords,
                            SUM(CASE WHEN ear.AttendanceStatus = 'Pre' THEN 1 ELSE 0 END) AS PresentCount,
                            SUM(CASE WHEN ear.AttendanceStatus = 'Late' THEN 1 ELSE 0 END) AS LateCount,
                            SUM(CASE WHEN ear.AttendanceStatus = 'Abs' THEN 1 ELSE 0 END) AS AbsentCount
                        FROM Employee_Attendance_Record ear
                        WHERE CAST(ear.AttendanceDate AS DATE) = CAST(GETDATE() AS DATE)
                            AND ear.SchoolID = @SchoolID";

                    using (var cmd = new SqlCommand(summaryQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@SchoolID", schoolID);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return Ok(new
                                {
                                    success = true,
                                    schoolID = schoolID,
                                    date = DateTime.Today.ToString("dd-MM-yyyy"),
                                    serverDate = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
                                    summary = new
                                    {
                                        totalRecords = reader["TotalRecords"] != DBNull.Value ? Convert.ToInt32(reader["TotalRecords"]) : 0,
                                        uniqueEmployees = reader["UniqueEmployees"] != DBNull.Value ? Convert.ToInt32(reader["UniqueEmployees"]) : 0,
                                        entryRecords = reader["EntryRecords"] != DBNull.Value ? Convert.ToInt32(reader["EntryRecords"]) : 0,
                                        exitRecords = reader["ExitRecords"] != DBNull.Value ? Convert.ToInt32(reader["ExitRecords"]) : 0,
                                        presentCount = reader["PresentCount"] != DBNull.Value ? Convert.ToInt32(reader["PresentCount"]) : 0,
                                        lateCount = reader["LateCount"] != DBNull.Value ? Convert.ToInt32(reader["LateCount"]) : 0,
                                        absentCount = reader["AbsentCount"] != DBNull.Value ? Convert.ToInt32(reader["AbsentCount"]) : 0
                                    },
                                    records = attendanceRecords
                                });
                            }
                        }
                    }
                }

                return Ok(new
                {
                    success = false,
                    message = "No data found"
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Debug endpoint - Get all attendance records (last 7 days)
        /// GET api/devices/attendance/debug?schoolID=1012
        /// </summary>
        [HttpGet]
        [Route("attendance/debug")]
        public IHttpActionResult GetAttendanceDebug()
        {
            try
            {
                var schoolIDParam = System.Web.HttpContext.Current.Request.QueryString["schoolID"];
                
                if (string.IsNullOrEmpty(schoolIDParam))
                {
                    return BadRequest("School ID is required. Use: ?schoolID=1012");
                }

                int schoolID = int.Parse(schoolIDParam);

                var result = new
                {
                    serverTime = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
                    serverDate = DateTime.Today.ToString("dd-MM-yyyy"),
                    schoolID = schoolID,
                    last7Days = new List<object>(),
                    recentRecords = new List<object>()
                };

                using (var conn = _dbConnection.GetConnection())
                {
                    conn.Open();

                    // Get last 7 days summary
                    var summaryQuery = @"
                        SELECT 
                            CAST(AttendanceDate AS DATE) AS Date,
                            FORMAT(CAST(AttendanceDate AS DATE), 'dd-MM-yyyy') AS FormattedDate,
                            COUNT(*) AS RecordCount,
                            COUNT(DISTINCT EmployeeID) AS EmployeeCount,
                            DATEDIFF(day, CAST(AttendanceDate AS DATE), CAST(GETDATE() AS DATE)) AS DaysAgo
                        FROM Employee_Attendance_Record
                        WHERE SchoolID = @SchoolID
                            AND AttendanceDate >= DATEADD(day, -7, GETDATE())
                        GROUP BY CAST(AttendanceDate AS DATE)
                        ORDER BY Date DESC";

                    using (var cmd = new SqlCommand(summaryQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@SchoolID", schoolID);

                        using (var reader = cmd.ExecuteReader())
                        {
                            var days = new List<object>();
                            while (reader.Read())
                            {
                                days.Add(new
                                {
                                    date = Convert.ToDateTime(reader["Date"]).ToString("dd-MM-yyyy"),
                                    formattedDate = reader["FormattedDate"]?.ToString(),
                                    recordCount = Convert.ToInt32(reader["RecordCount"]),
                                    employeeCount = Convert.ToInt32(reader["EmployeeCount"]),
                                    daysAgo = Convert.ToInt32(reader["DaysAgo"])
                                });
                            }
                            result = new
                            {
                                serverTime = result.serverTime,
                                serverDate = result.serverDate,
                                schoolID = result.schoolID,
                                last7Days = days,
                                recentRecords = result.recentRecords
                            };
                        }
                    }

                    // Get recent records
                    var recentQuery = @"
                        SELECT TOP 50
                            ear.Employee_Attendance_RecordID,
                            ear.EmployeeID,
                            CASE 
                                WHEN vw.FirstName IS NOT NULL 
                                THEN vw.FirstName + ' ' + ISNULL(vw.LastName, '')
                                ELSE 'Employee ID: ' + CAST(ear.EmployeeID AS VARCHAR(10))
                            END AS EmployeeName,
                            vw.DeviceID AS EmployeeDeviceID,
                            ear.AttendanceDate,
                            FORMAT(ear.AttendanceDate, 'dd-MM-yyyy') AS FormattedDate,
                            CAST(ear.EntryTime AS VARCHAR(8)) AS EntryTime,
                            ear.AttendanceStatus,
                            DATEDIFF(day, CAST(ear.AttendanceDate AS DATE), CAST(GETDATE() AS DATE)) AS DaysAgo
                        FROM Employee_Attendance_Record ear
                        LEFT JOIN VW_Emp_Info vw ON ear.EmployeeID = vw.EmployeeID
                        WHERE ear.SchoolID = @SchoolID
                        ORDER BY ear.AttendanceDate DESC, ear.EntryTime DESC";

                    using (var cmd = new SqlCommand(recentQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@SchoolID", schoolID);

                        using (var reader = cmd.ExecuteReader())
                        {
                            var records = new List<object>();
                            while (reader.Read())
                            {
                                records.Add(new
                                {
                                    recordID = Convert.ToInt32(reader["Employee_Attendance_RecordID"]),
                                    employeeID = Convert.ToInt32(reader["EmployeeID"]),
                                    employeeName = reader["EmployeeName"]?.ToString(),
                                    deviceID = reader["EmployeeDeviceID"] != DBNull.Value ? (int?)reader["EmployeeDeviceID"] : null,
                                    attendanceDate = reader["AttendanceDate"] != DBNull.Value ? Convert.ToDateTime(reader["AttendanceDate"]) : (DateTime?)null,
                                    formattedDate = reader["FormattedDate"]?.ToString(),
                                    entryTime = reader["EntryTime"]?.ToString(),
                                    status = reader["AttendanceStatus"]?.ToString(),
                                    daysAgo = Convert.ToInt32(reader["DaysAgo"])
                                });
                            }
                            result = new
                            {
                                serverTime = result.serverTime,
                                serverDate = result.serverDate,
                                schoolID = result.schoolID,
                                last7Days = result.last7Days,
                                recentRecords = records
                            };
                        }
                    }
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
