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
    /// Device SDK Management Controller
    /// ??????? ?????? ??????? ??? ??????? ???? ????
    /// </summary>
    [RoutePrefix("api/device")]
    public class DeviceController : ApiController
    {
        private readonly DatabaseConnection _dbConnection;
        private static readonly Dictionary<string, CZKEM> _deviceConnections = new Dictionary<string, CZKEM>();
        private const int MACHINE_NUMBER = 1;

        public DeviceController()
        {
            _dbConnection = new DatabaseConnection();
        }

        /// <summary>
        /// Test endpoint to check if API is working
        /// GET api/device/test
        /// </summary>
        [HttpGet]
        [Route("test")]
        public IHttpActionResult TestApi()
        {
            return Ok(new
            {
                success = true,
                message = "Device API is working",
                timestamp = DateTime.Now,
                version = "1.0.0"
            });
        }

        /// <summary>
        /// Connect to device
        /// POST api/device/connect
        /// </summary>
        [HttpPost]
        [Route("connect")]
        public IHttpActionResult ConnectDevice([FromBody] DeviceConnectionModel model)
        {
            try
            {
                // Basic validation
                if (model == null)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Request body is null or invalid"
                    });
                }

                if (string.IsNullOrEmpty(model.DeviceIP))
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Device IP is required"
                    });
                }

                // Log incoming request
                System.Diagnostics.Debug.WriteLine($"[DeviceController] Connect request - IP: {model.DeviceIP}, Port: {model.Port}, Serial: {model.DeviceSerial}");

                // SECURITY CHECK: Validate device is registered in database (if serial provided)
                if (!string.IsNullOrEmpty(model.DeviceSerial))
                {
                    try
                    {
                        // Verify device is registered in Device_Institution_Mapping
                        bool isAuthorized = false;
                        int mappedSchoolID = 0;
                        string mappedDeviceName = "";

                        using (var conn = _dbConnection.GetConnection())
                        {
                            conn.Open();
                            var query = @"
                                SELECT SchoolID, DeviceName, IsActive 
                                FROM Device_Institution_Mapping 
                                WHERE DeviceSerialNumber = @DeviceSerial";

                            using (var cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@DeviceSerial", model.DeviceSerial);
                                
                                using (var reader = cmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        mappedSchoolID = Convert.ToInt32(reader["SchoolID"]);
                                        mappedDeviceName = reader["DeviceName"].ToString();
                                        isAuthorized = Convert.ToBoolean(reader["IsActive"]);
                                    }
                                }
                            }
                        }

                        if (!isAuthorized)
                        {
                            return Ok(new
                            {
                                success = false,
                                message = $"Device with serial {model.DeviceSerial} is not registered or not active in the system."
                            });
                        }

                        // Verify school ID matches (if provided)
                        if (model.SchoolID > 0 && model.SchoolID != mappedSchoolID)
                        {
                            return Ok(new
                            {
                                success = false,
                                message = "School ID mismatch. This device belongs to a different school."
                            });
                        }

                        System.Diagnostics.Debug.WriteLine($"[DeviceController] Device authorized - {mappedDeviceName} for School {mappedSchoolID}");
                    }
                    catch (Exception dbEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"[DeviceController] Database error: {dbEx.Message}");
                        // Continue without database validation if DB is not available
                        // This allows direct IP connection for testing
                    }
                }

                var deviceKey = $"{model.DeviceIP}:{model.Port}";
                CZKEM device;

                // Check if already connected
                if (_deviceConnections.ContainsKey(deviceKey))
                {
                    device = _deviceConnections[deviceKey];
                    
                    // Test if still connected
                    int status = -1;
                    device.GetConnectStatus(ref status);
                    
                    if (status == 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"[DeviceController] Device {deviceKey} already connected");
                        
                        // Get serial to verify
                        string connectedSerial = "";
                        try
                        {
                            device.GetSerialNumber(MACHINE_NUMBER, out connectedSerial);
                        }
                        catch { }

                        return Ok(new
                        {
                            success = true,
                            message = "Device is already connected",
                            deviceIP = model.DeviceIP,
                            port = model.Port,
                            serialNumber = connectedSerial,
                            isAlreadyConnected = true
                        });
                    }
                    else
                    {
                        // Remove stale connection
                        System.Diagnostics.Debug.WriteLine($"[DeviceController] Removing stale connection for {deviceKey}");
                        _deviceConnections.Remove(deviceKey);
                    }
                }

                // Create new connection
                device = new CZKEM();

                // Set communication password if provided
                if (model.CommKey > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[DeviceController] Setting CommKey: {model.CommKey}");
                    if (!device.SetCommPassword(model.CommKey))
                    {
                        var errorCode = GetLastError(device);
                        System.Diagnostics.Debug.WriteLine($"[DeviceController] CommKey error: {errorCode}");
                        return Ok(new
                        {
                            success = false,
                            message = "Invalid communication key",
                            errorCode = errorCode
                        });
                    }
                }

                // Connect to device
                System.Diagnostics.Debug.WriteLine($"[DeviceController] Connecting to {model.DeviceIP}:{model.Port}");
                if (device.Connect_Net(model.DeviceIP, model.Port))
                {
                    System.Diagnostics.Debug.WriteLine($"[DeviceController] Successfully connected to {deviceKey}");
                    
                    // Store connection
                    _deviceConnections[deviceKey] = device;

                    // Get device info
                    string serialNumber = "";
                    try
                    {
                        device.GetSerialNumber(MACHINE_NUMBER, out serialNumber);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[DeviceController] Error getting serial: {ex.Message}");
                    }

                    // Verify serial if provided
                    if (!string.IsNullOrEmpty(model.DeviceSerial) && !string.IsNullOrEmpty(serialNumber))
                    {
                        if (serialNumber != model.DeviceSerial)
                        {
                            System.Diagnostics.Debug.WriteLine($"[DeviceController] Serial mismatch! Expected: {model.DeviceSerial}, Got: {serialNumber}");
                            device.Disconnect();
                            _deviceConnections.Remove(deviceKey);
                            
                            return Ok(new
                            {
                                success = false,
                                message = $"Serial number mismatch! Expected: {model.DeviceSerial}, but device has: {serialNumber}",
                                expectedSerial = model.DeviceSerial,
                                actualSerial = serialNumber
                            });
                        }
                    }

                    string firmwareVersion = "";
                    try
                    {
                        device.GetFirmwareVersion(MACHINE_NUMBER, ref firmwareVersion);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[DeviceController] Error getting firmware: {ex.Message}");
                    }

                    System.Diagnostics.Debug.WriteLine($"[DeviceController] Device SN: {serialNumber}, FW: {firmwareVersion}");

                    // Update last connection time in database (if serial provided)
                    if (!string.IsNullOrEmpty(model.DeviceSerial))
                    {
                        try
                        {
                            UpdateDeviceConnectionTime(model.DeviceSerial, model.DeviceIP);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[DeviceController] Error updating connection time: {ex.Message}");
                        }
                    }

                    return Ok(new
                    {
                        success = true,
                        message = "Successfully connected to device",
                        deviceIP = model.DeviceIP,
                        port = model.Port,
                        serialNumber = serialNumber,
                        firmwareVersion = firmwareVersion,
                        deviceKey = deviceKey
                    });
                }
                else
                {
                    var errorCode = GetLastError(device);
                    System.Diagnostics.Debug.WriteLine($"[DeviceController] Connection failed with error code: {errorCode}");
                    
                    return Ok(new
                    {
                        success = false,
                        message = GetErrorMessage(errorCode),
                        deviceIP = model.DeviceIP,
                        port = model.Port,
                        errorCode = errorCode
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DeviceController] Exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[DeviceController] Stack trace: {ex.StackTrace}");
                
                return Ok(new
                {
                    success = false,
                    message = $"Server error: {ex.Message}",
                    errorType = ex.GetType().Name,
                    stackTrace = ex.StackTrace
                });
            }
        }

        private void UpdateDeviceConnectionTime(string deviceSerial, string deviceIP)
        {
            try
            {
                using (var conn = _dbConnection.GetConnection())
                {
                    conn.Open();
                    var query = @"
                        UPDATE Device_Institution_Mapping 
                        SET LastPushTime = GETDATE(),
                            DeviceIP = @DeviceIP
                        WHERE DeviceSerialNumber = @DeviceSerial";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@DeviceSerial", deviceSerial);
                        cmd.Parameters.AddWithValue("@DeviceIP", deviceIP);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DeviceController] Error updating connection time: {ex.Message}");
            }
        }
        /// <summary>
        /// Disconnect from device
        /// POST api/device/disconnect
        /// </summary>
        [HttpPost]
        [Route("disconnect")]
        public IHttpActionResult DisconnectDevice([FromBody] DeviceConnectionModel model)
        {
            try
            {
                var deviceKey = $"{model.DeviceIP}:{model.Port}";

                if (_deviceConnections.ContainsKey(deviceKey))
                {
                    var device = _deviceConnections[deviceKey];
                    device.Disconnect();
                    _deviceConnections.Remove(deviceKey);

                    return Ok(new
                    {
                        success = true,
                        message = "Device disconnected successfully"
                    });
                }

                return Ok(new
                {
                    success = false,
                    message = "Device is not connected"
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get device status and info
        /// GET api/device/status?ip=192.168.1.201&port=4370
        /// </summary>
        [HttpGet]
        [Route("status")]
        public IHttpActionResult GetDeviceStatus()
        {
            try
            {
                var ip = System.Web.HttpContext.Current.Request.QueryString["ip"];
                var portStr = System.Web.HttpContext.Current.Request.QueryString["port"];

                if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(portStr))
                {
                    return BadRequest("IP and Port are required");
                }

                int port = int.Parse(portStr);
                var deviceKey = $"{ip}:{port}";

                if (!_deviceConnections.ContainsKey(deviceKey))
                {
                    return Ok(new
                    {
                        success = false,
                        isConnected = false,
                        message = "Device is not connected. Please connect first."
                    });
                }

                var device = _deviceConnections[deviceKey];

                // Check connection status
                int status = -1;
                device.GetConnectStatus(ref status);

                if (status != 0)
                {
                    _deviceConnections.Remove(deviceKey);
                    return Ok(new
                    {
                        success = false,
                        isConnected = false,
                        message = "Device connection lost"
                    });
                }

                // Get device details
                int userCapacity = 0, userCount = 0, fpCount = 0, fpCapacity = 0;
                int recordCount = 0, recordCapacity = 0;

                device.GetDeviceStatus(MACHINE_NUMBER, 8, ref userCapacity);
                device.GetDeviceStatus(MACHINE_NUMBER, 2, ref userCount);
                device.GetDeviceStatus(MACHINE_NUMBER, 3, ref fpCount);
                device.GetDeviceStatus(MACHINE_NUMBER, 7, ref fpCapacity);
                device.GetDeviceStatus(MACHINE_NUMBER, 6, ref recordCount);
                device.GetDeviceStatus(MACHINE_NUMBER, 9, ref recordCapacity);

                string serialNumber = "";
                device.GetSerialNumber(MACHINE_NUMBER, out serialNumber);

                string firmwareVersion = "";
                device.GetFirmwareVersion(MACHINE_NUMBER, ref firmwareVersion);

                return Ok(new
                {
                    success = true,
                    isConnected = true,
                    deviceIP = ip,
                    port = port,
                    serialNumber = serialNumber,
                    firmwareVersion = firmwareVersion,
                    userCapacity = userCapacity,
                    userCount = userCount,
                    fingerprintCount = fpCount,
                    fingerprintCapacity = fpCapacity,
                    recordCount = recordCount,
                    recordCapacity = recordCapacity,
                    usedStorage = new
                    {
                        users = $"{userCount}/{userCapacity}",
                        fingerprints = $"{fpCount}/{fpCapacity}",
                        records = $"{recordCount}/{recordCapacity}"
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get users from device
        /// GET api/device/users?ip=192.168.1.201&port=4370
        /// </summary>
        [HttpGet]
        [Route("users")]
        public IHttpActionResult GetDeviceUsers()
        {
            try
            {
                var ip = System.Web.HttpContext.Current.Request.QueryString["ip"];
                var portStr = System.Web.HttpContext.Current.Request.QueryString["port"];

                if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(portStr))
                {
                    return BadRequest("IP and Port are required");
                }

                int port = int.Parse(portStr);
                var deviceKey = $"{ip}:{port}";

                if (!_deviceConnections.ContainsKey(deviceKey))
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Device is not connected. Please connect first."
                    });
                }

                var device = _deviceConnections[deviceKey];
                var users = new List<object>();

                device.EnableDevice(MACHINE_NUMBER, false);
                try
                {
                    device.ReadAllUserID(MACHINE_NUMBER);

                    string enrollNumber, name, password;
                    int privilege;
                    bool enabled;

                    while (device.SSR_GetAllUserInfo(MACHINE_NUMBER, out enrollNumber, out name, out password, out privilege, out enabled))
                    {
                        string cardNumber = "";
                        device.GetStrCardNumber(out cardNumber);

                        users.Add(new
                        {
                            enrollNumber = enrollNumber,
                            name = name,
                            cardNumber = !string.IsNullOrEmpty(cardNumber) && cardNumber != "0" ? cardNumber : null,
                            privilege = privilege,
                            enabled = enabled
                        });
                    }
                }
                finally
                {
                    device.EnableDevice(MACHINE_NUMBER, true);
                }

                return Ok(new
                {
                    success = true,
                    totalUsers = users.Count,
                    users = users
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get attendance logs from device
        /// GET api/device/logs?ip=192.168.1.201&port=4370&days=7
        /// </summary>
        [HttpGet]
        [Route("logs")]
        public IHttpActionResult GetDeviceLogs()
        {
            try
            {
                var ip = System.Web.HttpContext.Current.Request.QueryString["ip"];
                var portStr = System.Web.HttpContext.Current.Request.QueryString["port"];
                var daysStr = System.Web.HttpContext.Current.Request.QueryString["days"] ?? "7";

                if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(portStr))
                {
                    return BadRequest("IP and Port are required");
                }

                int port = int.Parse(portStr);
                int days = int.Parse(daysStr);
                var deviceKey = $"{ip}:{port}";

                if (!_deviceConnections.ContainsKey(deviceKey))
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Device is not connected. Please connect first."
                    });
                }

                var device = _deviceConnections[deviceKey];
                var logs = new List<object>();

                device.EnableDevice(MACHINE_NUMBER, false);
                try
                {
                    var fromTime = DateTime.Today.AddDays(-days).ToString("yyyy-MM-dd 00:00:00");
                    var toTime = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd 00:00:00");

                    string enrollNumber;
                    int verifyMode, inOutMode, year, month, day, hour, minute, second, workCode = 0;

                    // Try ReadTimeGLogData first (if supported)
                    bool readSuccess = device.ReadTimeGLogData(MACHINE_NUMBER, fromTime, toTime);

                    if (!readSuccess)
                    {
                        // Fallback to ReadGeneralLogData
                        readSuccess = device.ReadGeneralLogData(MACHINE_NUMBER);
                    }

                    if (readSuccess)
                    {
                        while (device.SSR_GetGeneralLogData(MACHINE_NUMBER, out enrollNumber, out verifyMode, 
                            out inOutMode, out year, out month, out day, out hour, out minute, out second, ref workCode))
                        {
                            var logTime = new DateTime(year, month, day, hour, minute, second);

                            // Filter by date range if ReadGeneralLogData was used
                            if (logTime < DateTime.Today.AddDays(-days) || logTime > DateTime.Today.AddDays(1))
                            {
                                continue;
                            }

                            logs.Add(new
                            {
                                enrollNumber = enrollNumber,
                                verifyMode = verifyMode,
                                inOutMode = inOutMode,
                                logTime = logTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                date = logTime.ToString("yyyy-MM-dd"),
                                time = logTime.ToString("HH:mm:ss")
                            });
                        }
                    }
                }
                finally
                {
                    device.EnableDevice(MACHINE_NUMBER, true);
                }

                return Ok(new
                {
                    success = true,
                    totalLogs = logs.Count,
                    fromDate = DateTime.Today.AddDays(-days).ToString("yyyy-MM-dd"),
                    toDate = DateTime.Today.ToString("yyyy-MM-dd"),
                    logs = logs.OrderByDescending(l => ((dynamic)l).logTime).ToList()
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Upload users to device from database
        /// POST api/device/upload-users
        /// </summary>
        [HttpPost]
        [Route("upload-users")]
        public IHttpActionResult UploadUsersToDevice([FromBody] DeviceUploadModel model)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.DeviceIP))
                {
                    return BadRequest("Device IP is required");
                }

                // SECURITY CHECK: Validate device serial
                if (string.IsNullOrEmpty(model.DeviceSerial))
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Device serial number is required for security."
                    });
                }

                var deviceKey = $"{model.DeviceIP}:{model.Port}";

                if (!_deviceConnections.ContainsKey(deviceKey))
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Device is not connected. Please connect first."
                    });
                }

                var device = _deviceConnections[deviceKey];
                var uploadedCount = 0;
                var failedCount = 0;

                // Get users from database for this device's school
                var users = new List<dynamic>();
                int schoolId = model.SchoolID;

                // If SchoolID not provided, get from Device_Institution_Mapping
                if (schoolId == 0)
                {
                    using (var conn = _dbConnection.GetConnection())
                    {
                        conn.Open();
                        var deviceQuery = "SELECT SchoolID FROM Device_Institution_Mapping WHERE DeviceSerialNumber = @DeviceSerial";
                        using (var cmd = new SqlCommand(deviceQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@DeviceSerial", model.DeviceSerial);
                            var result = cmd.ExecuteScalar();
                            
                            if (result != null)
                            {
                                schoolId = Convert.ToInt32(result);
                            }
                        }
                    }
                }

                if (schoolId == 0)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Device is not mapped to any school"
                    });
                }

                using (var conn = _dbConnection.GetConnection())
                {
                    conn.Open();

                    // Get students
                    var studentQuery = @"
                        SELECT 
                            s.DeviceID as UserDeviceID,
                            s.StudentsName as UserName,
                            s.RFID as CardNumber
                        FROM Student s
                        INNER JOIN Student_Class sc ON s.StudentID = sc.StudentID
                        INNER JOIN Education_Year ey ON sc.EducationYearID = ey.EducationYearID
                        WHERE s.SchoolID = @SchoolID 
                            AND s.Status = 'Active'
                            AND ey.Status = 'True'
                            AND s.DeviceID IS NOT NULL";

                    using (var cmd = new SqlCommand(studentQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@SchoolID", schoolId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                users.Add(new
                                {
                                    DeviceID = Convert.ToInt32(reader["UserDeviceID"]),
                                    Name = reader["UserName"].ToString(),
                                    CardNumber = reader["CardNumber"]?.ToString()
                                });
                            }
                        }
                    }

                    // Get employees
                    var employeeQuery = @"
                        SELECT 
                            vw.DeviceID as UserDeviceID,
                            vw.FirstName + ' ' + vw.LastName as UserName,
                            vw.RFID as CardNumber
                        FROM VW_Emp_Info vw
                        WHERE vw.SchoolID = @SchoolID 
                            AND vw.Job_Status = 'Active'
                            AND vw.DeviceID IS NOT NULL";

                    using (var cmd = new SqlCommand(employeeQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@SchoolID", schoolId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                users.Add(new
                                {
                                    DeviceID = Convert.ToInt32(reader["UserDeviceID"]),
                                    Name = reader["UserName"].ToString(),
                                    CardNumber = reader["CardNumber"]?.ToString()
                                });
                            }
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[DeviceController] Uploading {users.Count} users to device SN: {model.DeviceSerial}");

                // Upload users to device
                device.EnableDevice(MACHINE_NUMBER, false);
                try
                {
                    var batchStarted = device.BeginBatchUpdate(MACHINE_NUMBER, 1); // 1 = force overwrite

                    foreach (var user in users)
                    {
                        try
                        {
                            // Set card number if available
                            if (!string.IsNullOrEmpty(user.CardNumber))
                            {
                                device.SetStrCardNumber(user.CardNumber);
                            }

                            // Upload user info
                            if (device.SSR_SetUserInfo(MACHINE_NUMBER, user.DeviceID.ToString(), user.Name, "", 0, true))
                            {
                                uploadedCount++;
                            }
                            else
                            {
                                failedCount++;
                            }
                        }
                        catch
                        {
                            failedCount++;
                        }
                    }

                    if (batchStarted)
                    {
                        device.BatchUpdate(MACHINE_NUMBER);
                    }

                    device.RefreshData(MACHINE_NUMBER);
                }
                finally
                {
                    device.EnableDevice(MACHINE_NUMBER, true);
                }

                System.Diagnostics.Debug.WriteLine($"[DeviceController] Upload complete. Success: {uploadedCount}, Failed: {failedCount}");

                return Ok(new
                {
                    success = true,
                    message = $"Users uploaded successfully for School ID {schoolId}",
                    totalUsers = users.Count,
                    uploadedCount = uploadedCount,
                    failedCount = failedCount,
                    schoolID = schoolId
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Clear all attendance logs from device
        /// POST api/device/clear-logs
        /// </summary>
        [HttpPost]
        [Route("clear-logs")]
        public IHttpActionResult ClearDeviceLogs([FromBody] DeviceConnectionModel model)
        {
            try
            {
                var deviceKey = $"{model.DeviceIP}:{model.Port}";

                if (!_deviceConnections.ContainsKey(deviceKey))
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Device is not connected. Please connect first."
                    });
                }

                var device = _deviceConnections[deviceKey];

                device.EnableDevice(MACHINE_NUMBER, false);
                bool cleared = device.ClearGLog(MACHINE_NUMBER);
                device.EnableDevice(MACHINE_NUMBER, true);

                return Ok(new
                {
                    success = cleared,
                    message = cleared ? "Logs cleared successfully" : "Failed to clear logs"
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Restart device
        /// POST api/device/restart
        /// </summary>
        [HttpPost]
        [Route("restart")]
        public IHttpActionResult RestartDevice([FromBody] DeviceConnectionModel model)
        {
            try
            {
                var deviceKey = $"{model.DeviceIP}:{model.Port}";

                if (!_deviceConnections.ContainsKey(deviceKey))
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Device is not connected. Please connect first."
                    });
                }

                var device = _deviceConnections[deviceKey];
                bool restarted = device.RestartDevice(MACHINE_NUMBER);

                if (restarted)
                {
                    // Remove connection as device will restart
                    _deviceConnections.Remove(deviceKey);
                }

                return Ok(new
                {
                    success = restarted,
                    message = restarted ? "Device restart command sent" : "Failed to restart device"
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private int GetLastError(CZKEM device)
        {
            int errorCode = 0;
            device.GetLastError(ref errorCode);
            return errorCode;
        }

        private string GetErrorMessage(int errorCode)
        {
            switch (errorCode)
            {
                case 0: return "Success";
                case -1: return "Failed to connect to device. Check IP and Port.";
                case -2: return "Device not found or unreachable.";
                case -3: return "Communication timeout.";
                case -4: return "Invalid communication key.";
                case -5: return "Network error.";
                case -100: return "Device is busy.";
                default: return $"Connection failed. Error code: {errorCode}";
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Disconnect all devices
                foreach (var connection in _deviceConnections.Values)
                {
                    try
                    {
                        connection.Disconnect();
                    }
                    catch { }
                }
                _deviceConnections.Clear();
            }
            base.Dispose(disposing);
        }
    }
}
