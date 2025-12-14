using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using ZKTeco.PushAPI.DataAccess;
using ZKTeco.PushAPI.Models;
using ZKTeco.PushAPI.Services;

namespace ZKTeco.PushAPI.Controllers
{
    /// <summary>
    /// ZKTeco PUSH Data Protocol Controller
    /// Supports both Push Protocol and ADMS Protocol
    /// </summary>
    [RoutePrefix("iclock")]
    public class IclockController : ApiController
    {
        private readonly AttendanceService _attendanceService;
        private readonly AttendanceRepository _repository;

        public IclockController()
        {
            _attendanceService = new AttendanceService();
            _repository = new AttendanceRepository();
        }

        /// <summary>
        /// Device Handshake - Device initial connection
        /// GET /iclock/cdata?SN=DEVICE_SERIAL&options=all
        /// Also handles ADMS protocol handshake
        /// </summary>
        [HttpGet]
        [Route("cdata")]
        public HttpResponseMessage GetDeviceInfo()
        {
            try
            {
                var queryParams = HttpContext.Current.Request.QueryString;
                var serialNumber = queryParams["SN"] ?? "";
                var options = queryParams["options"] ?? "";
                var info = queryParams["INFO"] ?? "";
                var pushver = queryParams["pushver"] ?? "";
                var language = queryParams["language"] ?? "";

                // Log device connection with more details
                LogDeviceActivity(serialNumber, "HANDSHAKE", 
                    $"Device connected. Options: {options}, Info: {info}, PushVer: {pushver}, Language: {language}");

                // Update LastPushTime in database
                _repository.UpdateDeviceLastPushTime(serialNumber);

                // Response format for ZKTeco device (ADMS compatible)
                var response = new StringBuilder();
                response.AppendLine("GET OPTION FROM: " + serialNumber);
                response.AppendLine("Stamp=9999");
                response.AppendLine("OpStamp=9999");
                response.AppendLine("PhotoStamp=9999");
                response.AppendLine("ErrorDelay=60");
                response.AppendLine("Delay=30");
                response.AppendLine("TransTimes=00:00;23:59");
                response.AppendLine("TransInterval=1");
                response.AppendLine("TransFlag=TransData AttLog OpLog EnrollUser ChgUser");
                response.AppendLine("TimeZone=6"); // Bangladesh GMT+6
                response.AppendLine("Realtime=1");
                response.AppendLine("Encrypt=0");
                response.AppendLine("ServerVer=3.0.0");
                response.AppendLine("PushProtVer=3.1"); // ADMS uses 3.1
                response.AppendLine("PushOptionsFlag=0");
                response.AppendLine("ATTLOGStamp=0");
                response.AppendLine("OPERLOGStamp=0");
                response.AppendLine("ATTPHOTOStamp=0");
                response.AppendLine("ErrorDelay=30");
                response.AppendLine("SupportPing=1"); // Enable ping support

                return CreateTextResponse(response.ToString());
            }
            catch (Exception ex)
            {
                LogError("GetDeviceInfo", ex);
                return CreateTextResponse("OK");
            }
        }

        /// <summary>
        /// ADMS Protocol Handler - For ADMS mode devices (iClock9000-G)
        /// GET /iclock/getrequest?SN=DEVICE_SERIAL
        /// This is the main communication endpoint for ADMS devices
        /// </summary>
        [HttpGet]
        [Route("getrequest")]
        public HttpResponseMessage GetRequest()
        {
            try
            {
                var queryParams = HttpContext.Current.Request.QueryString;
                var serialNumber = queryParams["SN"] ?? "";
                var info = queryParams["INFO"] ?? "";

                LogDeviceActivity(serialNumber, "ADMS_GETREQUEST", 
                    $"ADMS device requesting commands. Info: {info}");

                // Update LastPushTime
                _repository.UpdateDeviceLastPushTime(serialNumber);

                // Check if device is registered
                var deviceInfo = _repository.GetDeviceInfo(serialNumber);
                if (deviceInfo == null)
                {
                    LogDeviceActivity(serialNumber, "ADMS_ERROR", "Device not registered in database");
                    return CreateTextResponse("OK");
                }

                // Log successful connection
                LogDeviceActivity(serialNumber, "ADMS_CONNECTED", 
                    $"Device '{deviceInfo.DeviceName}' connected for {deviceInfo.SchoolName}");

                // Return OK or commands for device
                // Format: C:ID:COMMAND for sending commands to device
                // For now, just acknowledge
                return CreateTextResponse("OK");
            }
            catch (Exception ex)
            {
                LogError("GetRequest", ex);
                return CreateTextResponse("OK");
            }
        }

        /// <summary>
        /// Device Registration/Info - ADMS specific
        /// GET /iclock/deviceinfo?SN=DEVICE_SERIAL
        /// Called when device first connects
        /// </summary>
        [HttpGet]
        [Route("deviceinfo")]
        public HttpResponseMessage DeviceInfo()
        {
            try
            {
                var queryParams = HttpContext.Current.Request.QueryString;
                var serialNumber = queryParams["SN"] ?? "";
                var info = queryParams["INFO"] ?? "";

                LogDeviceActivity(serialNumber, "DEVICE_INFO", $"Device info request. Info: {info}");

                _repository.UpdateDeviceLastPushTime(serialNumber);

                // Check if device exists in database
                var deviceInfo = _repository.GetDeviceInfo(serialNumber);
                if (deviceInfo != null)
                {
                    LogDeviceActivity(serialNumber, "DEVICE_INFO_SUCCESS", 
                        $"Device found: {deviceInfo.DeviceName} ({deviceInfo.SchoolName})");
                }
                else
                {
                    LogDeviceActivity(serialNumber, "DEVICE_INFO_WARNING", 
                        "Device not found in database. Please register device.");
                }

                return CreateTextResponse("OK");
            }
            catch (Exception ex)
            {
                LogError("DeviceInfo", ex);
                return CreateTextResponse("OK");
            }
        }

        /// <summary>
        /// Receive Attendance Data - Both Push and ADMS protocol
        /// POST /iclock/cdata?SN=DEVICE_SERIAL&table=ATTLOG
        /// </summary>
        [HttpPost]
        [Route("cdata")]
        public async Task<HttpResponseMessage> ReceiveAttendanceData()
        {
            try
            {
                var queryParams = HttpContext.Current.Request.QueryString;
                var serialNumber = queryParams["SN"] ?? "";
                var table = queryParams["table"] ?? "";
                var stamp = queryParams["Stamp"] ?? "";

                // Read POST body
                var body = await Request.Content.ReadAsStringAsync();

                LogDeviceActivity(serialNumber, "DATA_RECEIVED", 
                    $"Table: {table}, Stamp: {stamp}, Body length: {body.Length} bytes");

                // Log first 500 chars of body for debugging
                if (!string.IsNullOrEmpty(body))
                {
                    var preview = body.Length > 500 ? body.Substring(0, 500) + "..." : body;
                    LogDeviceActivity(serialNumber, "DATA_PREVIEW", $"First 500 chars: {preview}");
                }

                // Update LastPushTime
                _repository.UpdateDeviceLastPushTime(serialNumber);

                // Parse attendance data
                if (table.Equals("ATTLOG", StringComparison.OrdinalIgnoreCase))
                {
                    // Get SchoolID from device serial
                    var schoolID = _repository.GetSchoolIDFromDeviceSerial(serialNumber);
                    if (!schoolID.HasValue)
                    {
                        LogDeviceActivity(serialNumber, "ERROR", "Device not found in database");
                        return CreateTextResponse("OK");
                    }

                    LogDeviceActivity(serialNumber, "PROCESSING", 
                        $"Starting to parse attendance logs for SchoolID: {schoolID.Value}");

                    var records = ParseAttendanceLogs(body, serialNumber, schoolID.Value);
                    
                    LogDeviceActivity(serialNumber, "PARSED", 
                        $"Parsed {records.Count} attendance records from device data");

                    int successCount = 0;
                    int failedCount = 0;

                    foreach (var record in records)
                    {
                        try
                        {
                            LogDeviceActivity(serialNumber, "PROCESSING_RECORD", 
                                $"DeviceID: {record.DeviceID}, UserID: {record.UserID}, Time: {record.AttendanceTime:yyyy-MM-dd HH:mm:ss}, ReceivedTime: {record.ReceivedTime:yyyy-MM-dd HH:mm:ss}");

                            // Process and save to database
                            var success = await _attendanceService.ProcessAttendanceLog(record, schoolID.Value);
                            
                            if (success)
                            {
                                successCount++;
                                LogDeviceActivity(serialNumber, "ATTENDANCE_SAVED", 
                                    $"✓ Saved - DeviceID: {record.DeviceID}, UserID: {record.UserID}, Time: {record.AttendanceTime:yyyy-MM-dd HH:mm:ss}");
                            }
                            else
                            {
                                failedCount++;
                                LogDeviceActivity(serialNumber, "ATTENDANCE_FAILED", 
                                    $"✗ Failed - DeviceID: {record.DeviceID}, UserID: {record.UserID}, Time: {record.AttendanceTime:yyyy-MM-dd HH:mm:ss}");
                            }
                        }
                        catch (Exception ex)
                        {
                            failedCount++;
                            LogError($"ProcessRecord_DeviceID{record.DeviceID}_UserID{record.UserID}", ex);
                            LogDeviceActivity(serialNumber, "RECORD_ERROR", 
                                $"Exception processing DeviceID: {record.DeviceID}, Error: {ex.Message}");
                        }
                    }

                    LogDeviceActivity(serialNumber, "BATCH_COMPLETE", 
                        $"Finished batch - Total: {records.Count}, Success: {successCount}, Failed: {failedCount}");

                    return CreateTextResponse($"OK: {successCount}");
                }
                else if (table.Equals("OPERLOG", StringComparison.OrdinalIgnoreCase))
                {
                    // Operation logs (device settings changes, etc.)
                    LogDeviceActivity(serialNumber, "OPERLOG", $"Operation log received: {body}");
                    return CreateTextResponse("OK");
                }

                return CreateTextResponse("OK");
            }
            catch (Exception ex)
            {
                LogError("ReceiveAttendanceData", ex);
                return CreateTextResponse("OK");
            }
        }

        /// <summary>
        /// Device heartbeat/ping - Works for both Push and ADMS
        /// GET /iclock/ping?SN=DEVICE_SERIAL
        /// </summary>
        [HttpGet]
        [Route("ping")]
        public HttpResponseMessage Ping()
        {
            try
            {
                var queryParams = HttpContext.Current.Request.QueryString;
                var serialNumber = queryParams["SN"] ?? "";

                if (!string.IsNullOrEmpty(serialNumber))
                {
                    _repository.UpdateDeviceLastPushTime(serialNumber);
                    LogDeviceActivity(serialNumber, "PING", "Device heartbeat received");
                }

                return CreateTextResponse("OK");
            }
            catch (Exception ex)
            {
                LogError("Ping", ex);
                return CreateTextResponse("OK");
            }
        }

        /// <summary>
        /// Test endpoint to check ADMS device status
        /// GET /iclock/test-adms?SN=DEVICE_SERIAL
        /// </summary>
        [HttpGet]
        [Route("test-adms")]
        public HttpResponseMessage TestAdms()
        {
            try
            {
                var queryParams = HttpContext.Current.Request.QueryString;
                var serialNumber = queryParams["SN"] ?? "";

                if (string.IsNullOrEmpty(serialNumber))
                {
                    return CreateTextResponse("ERROR: SN parameter required. Use: /iclock/test-adms?SN=YOUR_SERIAL");
                }

                // Get device info
                var deviceInfo = _repository.GetDeviceInfo(serialNumber);

                if (deviceInfo == null)
                {
                    return CreateTextResponse($"ERROR: Device '{serialNumber}' not found in database.\n" +
                        "Please register device in Device_Institution_Mapping table.");
                }

                // Update last push time
                _repository.UpdateDeviceLastPushTime(serialNumber);

                // Log the test
                LogDeviceActivity(serialNumber, "TEST_ADMS", "ADMS test endpoint called");

                // Return detailed info
                var response = new StringBuilder();
                response.AppendLine("=== ADMS Device Test ===");
                response.AppendLine($"Device Serial: {deviceInfo.DeviceSerialNumber}");
                response.AppendLine($"Device Name: {deviceInfo.DeviceName}");
                response.AppendLine($"School: {deviceInfo.SchoolName} (ID: {deviceInfo.SchoolID})");
                response.AppendLine($"Location: {deviceInfo.DeviceLocation}");
                response.AppendLine($"Is Active: {deviceInfo.IsActive}");
                response.AppendLine($"Last Push Time: {deviceInfo.LastPushTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Never"}");
                response.AppendLine($"Server Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                response.AppendLine();
                response.AppendLine("=== ADMS Endpoints Available ===");
                response.AppendLine("1. Handshake: GET /iclock/cdata?SN=" + serialNumber);
                response.AppendLine("2. Get Request: GET /iclock/getrequest?SN=" + serialNumber);
                response.AppendLine("3. Device Info: GET /iclock/deviceinfo?SN=" + serialNumber);
                response.AppendLine("4. Post Data: POST /iclock/cdata?SN=" + serialNumber + "&table=ATTLOG");
                response.AppendLine("5. Ping: GET /iclock/ping?SN=" + serialNumber);
                response.AppendLine();
                response.AppendLine("STATUS: OK - Device is properly configured!");

                return CreateTextResponse(response.ToString());
            }
            catch (Exception ex)
            {
                LogError("TestAdms", ex);
                return CreateTextResponse($"ERROR: {ex.Message}");
            }
        }

        /// <summary>
        /// Send commands to device - ADMS Protocol
        /// POST /iclock/sendcommand?SN=DEVICE_SERIAL
        /// This endpoint sends configuration commands to the device
        /// </summary>
        [HttpPost]
        [Route("sendcommand")]
        public HttpResponseMessage SendDeviceCommand()
        {
            try
            {
                var queryParams = HttpContext.Current.Request.QueryString;
                var serialNumber = queryParams["SN"] ?? "";
                var command = queryParams["command"] ?? "";

                LogDeviceActivity(serialNumber, "SEND_COMMAND", 
                    $"Sending command: {command}");

                // Check if device is registered
                var deviceInfo = _repository.GetDeviceInfo(serialNumber);
                if (deviceInfo == null)
                {
                    LogDeviceActivity(serialNumber, "COMMAND_ERROR", "Device not registered in database");
                    return CreateTextResponse("ERROR: Device not registered");
                }

                // Store command for device to fetch on next getrequest
                _repository.StoreDeviceCommand(serialNumber, command);

                LogDeviceActivity(serialNumber, "COMMAND_QUEUED", 
                    $"Command queued: {command}");

                return CreateTextResponse("OK");
            }
            catch (Exception ex)
            {
                LogError("SendDeviceCommand", ex);
                return CreateTextResponse("ERROR");
            }
        }

        /// <summary>
        /// Configure Device Date/Time Format via API
        /// GET /iclock/configure-datetime?SN=DEVICE_SERIAL&format=DD-MM-YY&timezone=6
        /// This will queue a command for the device to set date/time format
        /// </summary>
        [HttpGet]
        [Route("configure-datetime")]
        public HttpResponseMessage ConfigureDeviceDateTime()
        {
            try
            {
                var queryParams = HttpContext.Current.Request.QueryString;
                var serialNumber = queryParams["SN"] ?? "";
                var dateFormat = queryParams["format"] ?? "DD-MM-YY";
                var timeZone = queryParams["timezone"] ?? "6";

                LogDeviceActivity(serialNumber, "CONFIG_DATETIME", 
                    $"Configuring DateTime - Format: {dateFormat}, TimeZone: {timeZone}");

                // Check if device is registered
                var deviceInfo = _repository.GetDeviceInfo(serialNumber);
                if (deviceInfo == null)
                {
                    LogDeviceActivity(serialNumber, "CONFIG_ERROR", "Device not registered in database");
                    return CreateTextResponse("ERROR: Device not registered. Use: /iclock/configure-datetime?SN=YOUR_SERIAL");
                }

                // Build configuration commands for ZKTeco device
                var commands = new StringBuilder();
                
                // Command format: C:ID:COMMAND
                // Set Date Format (DD-MM-YY format)
                commands.AppendLine($"C:1:DATA UPDATE ATTLOG Stamp={DateTime.Now.Ticks}");
                commands.AppendLine($"C:2:SET OPTION ~TimeZone={timeZone}");
                
                // Note: Most ZKTeco devices require configuration via web interface or LCD
                // These commands are for reference - actual implementation may vary by device model
                
                var commandString = commands.ToString();
                
                // Store command for device
                _repository.StoreDeviceCommand(serialNumber, commandString);

                LogDeviceActivity(serialNumber, "CONFIG_QUEUED", 
                    $"DateTime configuration queued. Device will fetch on next getrequest.");

                var response = new StringBuilder();
                response.AppendLine("=== Device DateTime Configuration ===");
                response.AppendLine($"Device Serial: {deviceInfo.DeviceSerialNumber}");
                response.AppendLine($"Device Name: {deviceInfo.DeviceName}");
                response.AppendLine($"School: {deviceInfo.SchoolName}");
                response.AppendLine($"Date Format: {dateFormat}");
                response.AppendLine($"TimeZone: GMT+{timeZone}");
                response.AppendLine();
                response.AppendLine("IMPORTANT NOTICE:");
                response.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                response.AppendLine("⚠️ Most ZKTeco devices (especially iClock9000-G) require");
                response.AppendLine("   manual Date/Time format configuration via:");
                response.AppendLine();
                response.AppendLine("Option 1: Device LCD Menu");
                response.AppendLine("   Menu → System → Date/Time");
                response.AppendLine("   - Date Format: DD-MM-YY");
                response.AppendLine("   - Time Format: 24H");
                response.AppendLine("   - Time Zone: GMT+6");
                response.AppendLine();
                response.AppendLine("Option 2: Web Interface");
                response.AppendLine("   http://DEVICE_IP → Login → System → Date/Time Settings");
                response.AppendLine();
                response.AppendLine("Option 3: NTP Server (Recommended)");
                response.AppendLine("   Menu → Comm → NTP Server");
                response.AppendLine("   - Enable NTP: Yes");
                response.AppendLine("   - NTP Server: time.google.com");
                response.AppendLine("   - Time Zone: +6");
                response.AppendLine();
                response.AppendLine("After configuration, restart device for changes to take effect.");
                response.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                response.AppendLine();
                response.AppendLine($"Status: Configuration instructions provided");
                response.AppendLine($"Server Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

                return CreateTextResponse(response.ToString());
            }
            catch (Exception ex)
            {
                LogError("ConfigureDeviceDateTime", ex);
                return CreateTextResponse($"ERROR: {ex.Message}");
            }
        }

        #region Helper Methods

        private List<DeviceAttendanceLog> ParseAttendanceLogs(string body, string deviceSerial, int schoolID)
        {
            var records = new List<DeviceAttendanceLog>();

            if (string.IsNullOrEmpty(body)) return records;

            // ATTLOG format: USER_ID\tATT_TIME\tSTATUS\tVERIFY_TYPE\tWORK_CODE\tRESERVED1\tRESERVED2
            // Example: 1	2024-12-10 09:30:45	0	1	0	0	0
            // Example with dd-MM-yy format: 1	11-12-24 09:30:45	0	1	0	0	0
            var lines = body.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                try
                {
                    var parts = line.Split('\t');
                    if (parts.Length >= 2)
                    {
                        var userID = parts[0].Trim();
                        var attendanceTime = ParseDateTime(parts[1]);
                        var status = parts.Length > 2 ? ParseInt(parts[2]) : 0;
                        var verifyType = parts.Length > 3 ? ParseInt(parts[3]) : 0;

                        // Get DeviceID from UserID (DeviceID in database)
                        var deviceID = ParseInt(userID);
                        if (deviceID == 0) continue;

                        var record = new DeviceAttendanceLog
                        {
                            DeviceSerial = deviceSerial,
                            DeviceID = deviceID,
                            UserID = userID,
                            AttendanceTime = attendanceTime,
                            Status = status,
                            VerifyType = verifyType,
                            ReceivedTime = DateTime.Now
                        };

                        records.Add(record);
                    }
                }
                catch (Exception ex)
                {
                    LogError($"ParseLine: {line}", ex);
                }
            }

            return records;
        }

        private DateTime ParseDateTime(string dateTimeStr)
        {
            if (string.IsNullOrEmpty(dateTimeStr))
                return DateTime.Now;

            dateTimeStr = dateTimeStr.Trim();

            // Try multiple date formats - MOST SPECIFIC FIRST
            string[] formats = new[]
            {
                // DD-MM-YY format (Device default format) - PRIORITY
                "dd-MM-yy HH:mm:ss",
                "dd-MM-yy H:mm:ss",
                "dd-MM-yyyy HH:mm:ss",
                "dd-MM-yyyy H:mm:ss",
                
                // DD/MM/YY format
                "dd/MM/yy HH:mm:ss",
                "dd/MM/yy H:mm:ss",
                "dd/MM/yyyy HH:mm:ss",
                "dd/MM/yyyy H:mm:ss",
                
                // ISO format (yyyy-MM-dd)
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-dd H:mm:ss",
                
                // MM/dd/yy format (US format)
                "MM/dd/yy HH:mm:ss",
                "MM/dd/yy H:mm:ss",
                "MM/dd/yyyy HH:mm:ss",
                "MM/dd/yyyy H:mm:ss"
            };

            // Try parsing with specified formats (with explicit culture)
            if (DateTime.TryParseExact(dateTimeStr, formats, 
                System.Globalization.CultureInfo.InvariantCulture, 
                System.Globalization.DateTimeStyles.None, 
                out DateTime result))
            {
                // Log successful parse
                LogDeviceActivity("PARSER", "DATE_PARSE_SUCCESS", 
                    $"Input: '{dateTimeStr}' → Parsed: {result:yyyy-MM-dd HH:mm:ss}");
                return result;
            }

            // If specific formats fail, try general parsing with Bangladesh culture
            var bdCulture = new System.Globalization.CultureInfo("en-GB"); // DD/MM/YYYY format
            if (DateTime.TryParse(dateTimeStr, bdCulture, 
                System.Globalization.DateTimeStyles.None, out result))
            {
                LogDeviceActivity("PARSER", "DATE_PARSE_GENERAL", 
                    $"Input: '{dateTimeStr}' → Parsed: {result:yyyy-MM-dd HH:mm:ss}");
                return result;
            }

            // Last resort: try US culture
            var usCulture = new System.Globalization.CultureInfo("en-US"); // MM/DD/YYYY format
            if (DateTime.TryParse(dateTimeStr, usCulture, 
                System.Globalization.DateTimeStyles.None, out result))
            {
                LogDeviceActivity("PARSER", "DATE_PARSE_US", 
                    $"Input: '{dateTimeStr}' → Parsed: {result:yyyy-MM-dd HH:mm:ss}");
                return result;
            }

            // If all fails, log error and return current time
            LogError($"ParseDateTime FAILED for: '{dateTimeStr}'", 
                new FormatException($"Unable to parse date: {dateTimeStr}"));
            
            // Return current server time as fallback
            return DateTime.Now;
        }

        private int ParseInt(string value)
        {
            if (int.TryParse(value, out int result))
            {
                return result;
            }
            return 0;
        }

        private HttpResponseMessage CreateTextResponse(string content)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(content, Encoding.UTF8, "text/plain");
            return response;
        }

        private void LogDeviceActivity(string serialNumber, string activityType, string details)
        {
            try
            {
                // Try multiple log paths
                string logPath = null;
                bool usingTempPath = false;
                
                try
                {
                    logPath = HttpContext.Current.Server.MapPath("~/App_Data/DeviceLogs/");
                }
                catch
                {
                    // Fallback to temp directory if App_Data is not accessible
                    logPath = Path.Combine(Path.GetTempPath(), "PushAPI_DeviceLogs");
                    usingTempPath = true;
                }

                if (!Directory.Exists(logPath))
                {
                    Directory.CreateDirectory(logPath);
                }

                var logFile = Path.Combine(logPath, $"{DateTime.Now:yyyy-MM-dd}.log");
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{serialNumber}] [{activityType}] {details}";

                // Use FileStream for better control
                using (var fs = new FileStream(logFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                using (var sw = new StreamWriter(fs))
                {
                    sw.WriteLine(logEntry);
                    sw.Flush();
                }

                // Also write to console/debug output
                System.Diagnostics.Debug.WriteLine(logEntry);

                // Log critical events to Event Viewer
                if (activityType.Contains("ERROR") || activityType.Contains("HANDSHAKE") || 
                    activityType.Contains("CONNECTED") || activityType.Contains("DATA_RECEIVED"))
                {
                    try
                    {
                        System.Diagnostics.EventLog.WriteEntry("ZKTeco.PushAPI", 
                            logEntry, 
                            activityType.Contains("ERROR") ? 
                                System.Diagnostics.EventLogEntryType.Error : 
                                System.Diagnostics.EventLogEntryType.Information);
                    }
                    catch
                    {
                        // Silent fail
                    }
                }
            }
            catch (Exception ex)
            {
                // Log to Event Viewer if file logging fails
                try
                {
                    System.Diagnostics.EventLog.WriteEntry("ZKTeco.PushAPI", 
                        $"Failed to write log: {ex.Message}\nSerial: {serialNumber}, Activity: {activityType}", 
                        System.Diagnostics.EventLogEntryType.Error);
                }
                catch
                {
                    // Absolutely silent fail - don't break API
                }
            }
        }

        private void LogError(string method, Exception ex)
        {
            try
            {
                // Try App_Data first, fallback to Temp directory
                string logPath = null;
                
                try
                {
                    logPath = HttpContext.Current.Server.MapPath("~/App_Data/ErrorLogs/");
                }
                catch
                {
                    // Fallback to temp directory if App_Data is not accessible
                    logPath = Path.Combine(Path.GetTempPath(), "PushAPI_ErrorLogs");
                }

                if (!Directory.Exists(logPath))
                {
                    Directory.CreateDirectory(logPath);
                }

                var logFile = Path.Combine(logPath, $"errors_{DateTime.Now:yyyy-MM-dd}.log");
                var logEntry = $"[{DateTime.Now:HH:mm:ss}] [{method}] {ex.Message}\n{ex.StackTrace}";

                File.AppendAllText(logFile, logEntry + Environment.NewLine);
            }
            catch
            {
                // Silently fail - don't break API functionality due to logging issues
            }
        }

        #endregion
    }
}
