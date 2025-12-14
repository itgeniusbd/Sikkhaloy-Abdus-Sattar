using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using ZKTeco.PushAPI.DataAccess;

namespace ZKTeco.PushAPI.Controllers
{
    /// <summary>
    /// Dashboard API Controller
    /// Dashboard ?? ???? data provide ???
    /// </summary>
    [RoutePrefix("api/dashboard")]
    public class DashboardController : ApiController
    {
        private readonly DatabaseConnection _dbConnection;
        private readonly AttendanceRepository _repository;

        public DashboardController()
        {
            _dbConnection = new DatabaseConnection();
            _repository = new AttendanceRepository();
        }

        /// <summary>
        /// Get all devices with their status
        /// GET api/dashboard/devices
        /// </summary>
        [HttpGet]
        [Route("devices")]
        public IHttpActionResult GetDevices()
        {
            try {
                var devices = new List<object>();

                using (var conn = _dbConnection.GetConnection())
                {
                    conn.Open();

                    var query = @"
                        SELECT 
                            m.MappingID,
                            m.SchoolID,
                            m.DeviceSerialNumber,
                            m.DeviceName,
                            m.DeviceLocation,
                            m.IsActive,
                            m.LastPushTime,
                            m.CreatedDate,
                            m.Remarks,
                            s.SchoolName
                        FROM Device_Institution_Mapping m
                        INNER JOIN SchoolInfo s ON m.SchoolID = s.SchoolID
                        ORDER BY m.LastPushTime DESC, m.CreatedDate DESC";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var lastPushTime = reader["LastPushTime"] != DBNull.Value
                                    ? (DateTime?)reader["LastPushTime"]
                                    : null;

                                devices.Add(new
                                {
                                    mappingID = Convert.ToInt32(reader["MappingID"]),
                                    schoolID = Convert.ToInt32(reader["SchoolID"]),
                                    schoolName = reader["SchoolName"].ToString(),
                                    deviceSerial = reader["DeviceSerialNumber"].ToString(),
                                    deviceName = reader["DeviceName"] != DBNull.Value ? reader["DeviceName"].ToString() : null,
                                    deviceLocation = reader["DeviceLocation"] != DBNull.Value ? reader["DeviceLocation"].ToString() : null,
                                    isActive = Convert.ToBoolean(reader["IsActive"]),
                                    lastPushTime = lastPushTime?.ToString("yyyy-MM-dd HH:mm:ss"),
                                    createdDate = reader["CreatedDate"] != DBNull.Value
                                        ? Convert.ToDateTime(reader["CreatedDate"]).ToString("yyyy-MM-dd HH:mm:ss")
                                        : null,
                                    remarks = reader["Remarks"] != DBNull.Value ? reader["Remarks"].ToString() : null
                                });
                            }
                        }
                    }
                }

                return Ok(new { success = true, devices = devices });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get dashboard statistics
        /// GET api/dashboard/stats
        /// </summary>
        [HttpGet]
        [Route("stats")]
        public IHttpActionResult GetStats()
        {
            try
            {
                int totalDevices = 0;
                int connectedDevices = 0;
                int todayAttendanceCount = 0;

                using (var conn = _dbConnection.GetConnection())
                {
                    conn.Open();

                    // Total devices
                    var totalQuery = "SELECT COUNT(*) FROM Device_Institution_Mapping WHERE IsActive = 1";
                    using (var cmd = new SqlCommand(totalQuery, conn))
                    {
                        totalDevices = (int)cmd.ExecuteScalar();
                    }

                    // Connected devices (last push within 30 minutes - increased for better detection)
                    var connectedQuery = @"
                        SELECT COUNT(*) 
                        FROM Device_Institution_Mapping 
                        WHERE IsActive = 1 
                            AND LastPushTime IS NOT NULL 
                            AND DATEDIFF(MINUTE, LastPushTime, GETDATE()) <= 30";
                    using (var cmd = new SqlCommand(connectedQuery, conn))
                    {
                        connectedDevices = (int)cmd.ExecuteScalar();
                    }

                    // Today's attendance count
                    var attendanceQuery = @"
                        SELECT COUNT(*) 
                        FROM Employee_Attendance_Record 
                        WHERE CAST(AttendanceDate AS DATE) = CAST(GETDATE() AS DATE)";
                    using (var cmd = new SqlCommand(attendanceQuery, conn))
                    {
                        todayAttendanceCount = (int)cmd.ExecuteScalar();
                    }
                }

                return Ok(new
                {
                    success = true,
                    totalDevices = totalDevices,
                    connectedDevices = connectedDevices,
                    todayAttendanceCount = todayAttendanceCount,
                    serverTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get recent activity logs
        /// GET api/dashboard/recent-logs
        /// </summary>
        [HttpGet]
        [Route("recent-logs")]
        public IHttpActionResult GetRecentLogs()
        {
            try
            {
                var logs = new List<object>();

                // Try to read today's log file
                string logPath = null;
                try
                {
                    logPath = HttpContext.Current.Server.MapPath("~/App_Data/DeviceLogs/");
                }
                catch
                {
                    logPath = Path.Combine(Path.GetTempPath(), "PushAPI_DeviceLogs");
                }

                var todayLog = Path.Combine(logPath, $"{DateTime.Now:yyyy-MM-dd}.log");

                if (File.Exists(todayLog))
                {
                    var lines = File.ReadAllLines(todayLog);
                    var recentLines = lines.Length > 20 ? lines.Skip(lines.Length - 20).ToArray() : lines;

                    foreach (var line in recentLines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        // Parse log entry
                        // Format: [2024-12-11 10:30:45] [SERIAL] [TYPE] Message
                        var type = "info";
                        if (line.Contains("ERROR")) type = "error";
                        else if (line.Contains("HANDSHAKE") || line.Contains("DATA_RECEIVED")) type = "success";
                        else if (line.Contains("WARNING")) type = "warning";

                        // Extract timestamp
                        var timestampStart = line.IndexOf('[');
                        var timestampEnd = line.IndexOf(']');
                        var timestamp = timestampStart >= 0 && timestampEnd > timestampStart
                            ? line.Substring(timestampStart + 1, timestampEnd - timestampStart - 1)
                            : DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        // Extract message (after third bracket)
                        var thirdBracket = line.IndexOf(']', timestampEnd + 1);
                        var message = thirdBracket > 0 && thirdBracket + 1 < line.Length
                            ? line.Substring(thirdBracket + 2)
                            : line;

                        logs.Add(new
                        {
                            timestamp = timestamp,
                            type = type,
                            message = message
                        });
                    }
                }

                return Ok(new { success = true, logs = logs });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    success = false,
                    logs = new[]
                    {
                        new
                        {
                            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            type = "error",
                            message = $"Error loading logs: {ex.Message}"
                        }
                    }
                });
            }
        }

        /// <summary>
        /// Add new device mapping
        /// POST api/dashboard/add-device
        /// </summary>
        [HttpPost]
        [Route("add-device")]
        public IHttpActionResult AddDevice([FromBody] DeviceMappingModel model)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.DeviceSerialNumber) || model.SchoolID <= 0)
                {
                    return BadRequest("Invalid device data");
                }

                using (var conn = _dbConnection.GetConnection())
                {
                    conn.Open();

                    // Check if device already exists
                    var checkQuery = @"
                        SELECT COUNT(*) 
                        FROM Device_Institution_Mapping 
                        WHERE DeviceSerialNumber = @DeviceSerial";

                    using (var checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@DeviceSerial", model.DeviceSerialNumber);
                        var exists = (int)checkCmd.ExecuteScalar() > 0;

                        if (exists)
                        {
                            return Ok(new
                            {
                                success = false,
                                message = "Device with this serial number already exists"
                            });
                        }
                    }

                    // Verify school exists
                    var schoolQuery = "SELECT COUNT(*) FROM SchoolInfo WHERE SchoolID = @SchoolID";
                    using (var schoolCmd = new SqlCommand(schoolQuery, conn))
                    {
                        schoolCmd.Parameters.AddWithValue("@SchoolID", model.SchoolID);
                        var schoolExists = (int)schoolCmd.ExecuteScalar() > 0;

                        if (!schoolExists)
                        {
                            return Ok(new
                            {
                                success = false,
                                message = "School/Institution ID not found in database"
                            });
                        }
                    }

                    // Insert new device mapping
                    var insertQuery = @"
                        INSERT INTO Device_Institution_Mapping 
                        (SchoolID, DeviceSerialNumber, DeviceName, DeviceLocation, IsActive, CreatedDate, Remarks)
                        VALUES 
                        (@SchoolID, @DeviceSerial, @DeviceName, @DeviceLocation, @IsActive, GETDATE(), @Remarks);
                        SELECT CAST(SCOPE_IDENTITY() as int)";

                    using (var insertCmd = new SqlCommand(insertQuery, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@SchoolID", model.SchoolID);
                        insertCmd.Parameters.AddWithValue("@DeviceSerial", model.DeviceSerialNumber);
                        insertCmd.Parameters.AddWithValue("@DeviceName", (object)model.DeviceName ?? DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@DeviceLocation", (object)model.DeviceLocation ?? DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@IsActive", model.IsActive);
                        insertCmd.Parameters.AddWithValue("@Remarks", (object)model.Remarks ?? DBNull.Value);

                        var mappingId = (int)insertCmd.ExecuteScalar();

                        return Ok(new
                        {
                            success = true,
                            message = "Device mapping added successfully",
                            mappingId = mappingId
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
        /// Delete device mapping
        /// DELETE api/dashboard/delete-device/{id}
        /// </summary>
        [HttpDelete]
        [Route("delete-device/{id}")]
        public IHttpActionResult DeleteDevice(int id)
        {
            try
            {
                using (var conn = _dbConnection.GetConnection())
                {
                    conn.Open();

                    var deleteQuery = "DELETE FROM Device_Institution_Mapping WHERE MappingID = @MappingID";

                    using (var deleteCmd = new SqlCommand(deleteQuery, conn))
                    {
                        deleteCmd.Parameters.AddWithValue("@MappingID", id);
                        var affected = deleteCmd.ExecuteNonQuery();

                        if (affected > 0)
                        {
                            return Ok(new
                            {
                                success = true,
                                message = "Device mapping deleted successfully"
                            });
                        }
                        else
                        {
                            return Ok(new
                            {
                                success = false,
                                message = "Device mapping not found"
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }

    /// <summary>
    /// Device mapping model
    /// </summary>
    public class DeviceMappingModel
    {
        public int SchoolID { get; set; }
        public string DeviceSerialNumber { get; set; }
        public string DeviceName { get; set; }
        public string DeviceLocation { get; set; }
        public bool IsActive { get; set; }
        public string Remarks { get; set; }
    }
}
