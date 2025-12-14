using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using ZKTeco.PushAPI.DataAccess;

namespace ZKTeco.PushAPI.Controllers
{
    /// <summary>
    /// Test and diagnostic controller
    /// </summary>
    [RoutePrefix("iclock/test")]
    public class TestController : ApiController
    {
        private readonly AttendanceRepository _repository;
        private readonly DatabaseConnection _dbConnection;

        public TestController()
        {
            _repository = new AttendanceRepository();
            _dbConnection = new DatabaseConnection();
        }

        /// <summary>
        /// Test API health
        /// GET /iclock/test/ping
        /// </summary>
        [HttpGet]
        [Route("ping")]
        public IHttpActionResult Ping()
        {
            return Ok(new
            {
                status = "Push API is running",
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                server = Environment.MachineName,
                version = "1.0.5"
            });
        }

        /// <summary>
        /// View recent device logs
        /// GET /iclock/test/logs
        /// </summary>
        [HttpGet]
        [Route("logs")]
        public IHttpActionResult ViewLogs()
        {
            try
            {
                var logs = new List<object>();
                string logPath = null;

                try
                {
                    logPath = HttpContext.Current.Server.MapPath("~/App_Data/DeviceLogs/");
                }
                catch
                {
                    logPath = Path.Combine(Path.GetTempPath(), "PushAPI_DeviceLogs");
                }

                // Get today's log file
                var todayLog = Path.Combine(logPath, $"{DateTime.Now:yyyy-MM-dd}.log");
                
                // Also check yesterday's log
                var yesterdayLog = Path.Combine(logPath, $"{DateTime.Now.AddDays(-1):yyyy-MM-dd}.log");

                var allLines = new List<string>();

                if (File.Exists(yesterdayLog))
                {
                    var yesterdayLines = File.ReadAllLines(yesterdayLog);
                    var recentYesterday = yesterdayLines.Length > 50 
                        ? yesterdayLines.Skip(yesterdayLines.Length - 50).ToArray() 
                        : yesterdayLines;
                    allLines.AddRange(recentYesterday);
                }

                if (File.Exists(todayLog))
                {
                    allLines.AddRange(File.ReadAllLines(todayLog));
                }

                // Get last 100 lines
                var recentLines = allLines.Count > 100 
                    ? allLines.Skip(allLines.Count - 100).ToArray() 
                    : allLines.ToArray();

                foreach (var line in recentLines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    logs.Add(new
                    {
                        timestamp = ExtractTimestamp(line),
                        message = line
                    });
                }

                var html = GenerateLogsHtml(logs);
                
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(html, System.Text.Encoding.UTF8, "text/html")
                };
                
                return ResponseMessage(response);
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    error = "Failed to read logs",
                    message = ex.Message,
                    timestamp = DateTime.Now
                });
            }
        }

        /// <summary>
        /// View all registered devices
        /// GET /iclock/test/devices
        /// </summary>
        [HttpGet]
        [Route("devices")]
        public IHttpActionResult ViewDevices()
        {
            try
            {
                var devices = _repository.GetAllDevices();
                
                var html = GenerateDevicesHtml(devices);
                
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(html, System.Text.Encoding.UTF8, "text/html")
                };
                
                return ResponseMessage(response);
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    error = "Failed to read devices",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Test database connection
        /// GET /iclock/test/database
        /// </summary>
        [HttpGet]
        [Route("database")]
        public IHttpActionResult TestDatabase()
        {
            try
            {
                using (var conn = _dbConnection.GetConnection())
                {
                    conn.Open();
                    
                    var deviceCount = _repository.GetAllDevices().Count;
                    var schoolCount = 0;
                    
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM SchoolInfo";
                        schoolCount = (int)cmd.ExecuteScalar();
                    }
                    
                    return Ok(new
                    {
                        status = "Database connected successfully",
                        server = conn.DataSource,
                        database = conn.Database,
                        totalDevices = deviceCount,
                        totalSchools = schoolCount,
                        timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    status = "Database connection failed",
                    error = ex.Message,
                    timestamp = DateTime.Now
                });
            }
        }

        /// <summary>
        /// Simulate device handshake for testing
        /// GET /iclock/test/handshake?serial=SMRS25200106
        /// </summary>
        [HttpGet]
        [Route("handshake")]
        public IHttpActionResult TestHandshake(string serial = "SMRS25200106")
        {
            try
            {
                var url = $"/iclock/cdata?SN={serial}&options=all";
                var baseUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority);
                var fullUrl = baseUrl + url;
                
                return Ok(new
                {
                    message = "Test handshake URL",
                    deviceSerial = serial,
                    testUrl = fullUrl,
                    instructions = "Device should call this URL on first connection",
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Comprehensive device diagnostics
        /// GET /iclock/test/diagnose?serial=SMRS25200106
        /// </summary>
        [HttpGet]
        [Route("diagnose")]
        public IHttpActionResult Diagnose(string serial = "SMRS25200106")
        {
            try
            {
                var report = new System.Text.StringBuilder();
                report.AppendLine("=== DEVICE DIAGNOSTIC REPORT ===");
                report.AppendLine($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                report.AppendLine($"Serial: {serial}");
                report.AppendLine("");

                using (var conn = _dbConnection.GetConnection())
                {
                    conn.Open();

                    // 1. Check device registration
                    report.AppendLine("1. DEVICE REGISTRATION:");
                    var deviceQuery = @"
                        SELECT DeviceSerialNumber, SchoolID, DeviceName, IsActive, LastPushTime,
                               DATEDIFF(SECOND, LastPushTime, GETDATE()) AS SecondsSinceLastPush
                        FROM Device_Institution_Mapping 
                        WHERE DeviceSerialNumber = @Serial";
                    
                    using (var cmd = new System.Data.SqlClient.SqlCommand(deviceQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@Serial", serial);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                report.AppendLine($"   ? Device Found in Database");
                                report.AppendLine($"   SchoolID: {reader["SchoolID"]}");
                                report.AppendLine($"   DeviceName: {reader["DeviceName"]}");
                                report.AppendLine($"   IsActive: {reader["IsActive"]}");
                                report.AppendLine($"   LastPushTime: {(reader["LastPushTime"] != DBNull.Value ? reader["LastPushTime"].ToString() : "NEVER")}");
                                if (reader["LastPushTime"] != DBNull.Value)
                                {
                                    var seconds = Convert.ToInt32(reader["SecondsSinceLastPush"]);
                                    report.AppendLine($"   Time Since Last Push: {seconds} seconds ({seconds/60} minutes)");
                                    
                                    if (seconds <= 300) // 5 minutes
                                        report.AppendLine($"   Status: ?? CONNECTED (within 5 min)");
                                    else if (seconds <= 1800) // 30 minutes
                                        report.AppendLine($"   Status: ?? RECENT (within 30 min)");
                                    else
                                        report.AppendLine($"   Status: ?? DISCONNECTED (>30 min)");
                                }
                                else
                                {
                                    report.AppendLine($"   Status: ?? NEVER CONNECTED - Device registered but no data received yet");
                                }
                            }
                            else
                            {
                                report.AppendLine($"   ? Device NOT found in Device_Institution_Mapping table");
                                report.AppendLine($"   ACTION: Add device using /device-mapping.html");
                            }
                        }
                    }
                    report.AppendLine("");

                    // 2. Check recent API hits
                    report.AppendLine("2. RECENT API ACTIVITY (Logs):");
                    try
                    {
                        string logPath = null;
                        try
                        {
                            logPath = System.Web.HttpContext.Current.Server.MapPath("~/App_Data/DeviceLogs/");
                        }
                        catch
                        {
                            logPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "PushAPI_DeviceLogs");
                        }

                        var todayLog = System.IO.Path.Combine(logPath, $"{DateTime.Now:yyyy-MM-dd}.log");
                        if (System.IO.File.Exists(todayLog))
                        {
                            var allLines = System.IO.File.ReadAllLines(todayLog);
                            var deviceLines = allLines.Where(l => l.Contains(serial)).ToArray();
                            
                            if (deviceLines.Length > 0)
                            {
                                report.AppendLine($"   ? Found {deviceLines.Length} log entries for this device today");
                                report.AppendLine($"   Last 3 entries:");
                                var lastLines = deviceLines.Length > 3 
                                    ? deviceLines.Skip(deviceLines.Length - 3).ToArray() 
                                    : deviceLines;
                                foreach (var line in lastLines)
                                {
                                    report.AppendLine($"     {line}");
                                }
                            }
                            else
                            {
                                report.AppendLine($"   ?? NO log entries found for this device today");
                                report.AppendLine($"   This means device is NOT sending data to API");
                            }
                        }
                        else
                        {
                            report.AppendLine($"   ?? No log file for today");
                        }
                    }
                    catch (Exception ex)
                    {
                        report.AppendLine($"   ? Error reading logs: {ex.Message}");
                    }
                    report.AppendLine("");

                    // 3. Check employees with DeviceID
                    report.AppendLine("3. EMPLOYEE/STUDENT MAPPING:");
                    var employeeQuery = @"
                        SELECT TOP 5 EmployeeID, FirstName, LastName, DeviceID, SchoolID, Job_Status
                        FROM VW_Emp_Info
                        WHERE SchoolID = (SELECT TOP 1 SchoolID FROM Device_Institution_Mapping WHERE DeviceSerialNumber = @Serial)
                          AND DeviceID IS NOT NULL
                        ORDER BY EmployeeID";
                    
                    using (var cmd = new System.Data.SqlClient.SqlCommand(employeeQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@Serial", serial);
                        using (var reader = cmd.ExecuteReader())
                        {
                            var count = 0;
                            while (reader.Read())
                            {
                                count++;
                                if (count == 1)
                                    report.AppendLine($"   Sample employees with DeviceID:");
                                report.AppendLine($"   - DeviceID: {reader["DeviceID"]}, Name: {reader["FirstName"]} {reader["LastName"]}, EmployeeID: {reader["EmployeeID"]}");
                            }
                            if (count == 0)
                            {
                                report.AppendLine($"   ?? NO employees/students have DeviceID configured");
                                report.AppendLine($"   ACTION: Assign DeviceID to users in database");
                            }
                            else
                            {
                                report.AppendLine($"   ? {count} sample users found with DeviceID");
                            }
                        }
                    }
                    report.AppendLine("");

                    // 4. Check recent attendance
                    report.AppendLine("4. RECENT ATTENDANCE (Last 24 hours):");
                    var attendanceQuery = @"
                        SELECT TOP 5 
                            ear.EmployeeID,
                            emp.FirstName + ' ' + emp.LastName AS Name,
                            emp.DeviceID,
                            CAST(ear.AttendanceDate AS DATE) AS Date,
                            CAST(ear.EntryTime AS TIME) AS Time
                        FROM Employee_Attendance_Record ear
                        LEFT JOIN VW_Emp_Info emp ON ear.EmployeeID = emp.EmployeeID
                        WHERE ear.SchoolID = (SELECT TOP 1 SchoolID FROM Device_Institution_Mapping WHERE DeviceSerialNumber = @Serial)
                          AND ear.AttendanceDate >= DATEADD(HOUR, -24, GETDATE())
                        ORDER BY ear.AttendanceDate DESC, ear.EntryTime DESC";
                    
                    using (var cmd = new System.Data.SqlClient.SqlCommand(attendanceQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@Serial", serial);
                        using (var reader = cmd.ExecuteReader())
                        {
                            var count = 0;
                            while (reader.Read())
                            {
                                count++;
                                if (count == 1)
                                    report.AppendLine($"   Recent attendance records:");
                                report.AppendLine($"   - DeviceID: {reader["DeviceID"]}, Name: {reader["Name"]}, Time: {reader["Date"]} {reader["Time"]}");
                            }
                            if (count == 0)
                            {
                                report.AppendLine($"   ?? NO attendance records in last 24 hours");
                                report.AppendLine($"   This confirms device is NOT uploading data");
                            }
                            else
                            {
                                report.AppendLine($"   ? {count} recent attendance records found");
                            }
                        }
                    }
                    report.AppendLine("");

                    // 5. Diagnosis & Solutions
                    report.AppendLine("5. DIAGNOSIS & RECOMMENDED ACTIONS:");
                    report.AppendLine("");
                    report.AppendLine("DEVICE CONFIGURATION CHECKLIST:");
                    report.AppendLine("? Device Menu ? System ? Communications ? ADMS Settings");
                    report.AppendLine("  - Server: pushapi.sikkhaloy.com");
                    report.AppendLine("  - Port: 4370 (ADMS) or 80 (Push)");
                    report.AppendLine("  - Enable ADMS: YES");
                    report.AppendLine("  - Upload Interval: 30 seconds");
                    report.AppendLine("");
                    report.AppendLine("? Device Menu ? System ? Network");
                    report.AppendLine("  - Check IP Address is correct");
                    report.AppendLine("  - Ping Test: pushapi.sikkhaloy.com (should succeed)");
                    report.AppendLine("");
                    report.AppendLine("? Reboot device after configuration:");
                    report.AppendLine("  Menu ? System ? Power ? Reboot");
                    report.AppendLine("");
                    report.AppendLine("TEST STEPS:");
                    report.AppendLine("1. After configuration, wait 2-3 minutes");
                    report.AppendLine("2. Check /iclock/test/logs for handshake");
                    report.AppendLine("3. Punch on device (fingerprint/card)");
                    report.AppendLine("4. Wait 30 seconds");
                    report.AppendLine("5. Refresh dashboard - should show 'Connected'");
                    report.AppendLine("");
                    report.AppendLine("If still not working, check:");
                    report.AppendLine("- Firewall blocking port 4370 or 80/443?");
                    report.AppendLine("- Device time/date correct?");
                    report.AppendLine("- Network cable connected properly?");
                    report.AppendLine("");
                    report.AppendLine("=== END OF DIAGNOSTIC REPORT ===");
                }

                var html = $"<html><body><pre style='font-family:monospace;'>{report}</pre></body></html>";
                
                var response = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new System.Net.Http.StringContent(html, System.Text.Encoding.UTF8, "text/html")
                };
                
                return ResponseMessage(response);
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    error = "Diagnostic failed",
                    message = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        // Helper methods
        private string ExtractTimestamp(string logLine)
        {
            try
            {
                var start = logLine.IndexOf('[');
                var end = logLine.IndexOf(']');
                if (start >= 0 && end > start)
                {
                    return logLine.Substring(start + 1, end - start - 1);
                }
            }
            catch { }
            
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private string GenerateLogsHtml(List<object> logs)
        {
            var html = @"
<!DOCTYPE html>
<html>
<head>
    <title>Device Logs - Push API</title>
    <meta charset='utf-8'>
    <meta http-equiv='refresh' content='10'>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
            font-family: 'Consolas', 'Monaco', monospace;
            background: #1e1e1e;
            color: #d4d4d4;
            padding: 20px;
        }
        .header {
            background: #2d2d30;
            padding: 20px;
            border-radius: 8px;
            margin-bottom: 20px;
        }
        h1 { color: #4ec9b0; margin-bottom: 10px; }
        .info { color: #9cdcfe; font-size: 0.9em; }
        .logs-container {
            background: #252526;
            border: 1px solid #3e3e42;
            border-radius: 8px;
            padding: 20px;
            max-height: 80vh;
            overflow-y: auto;
        }
        .log-entry {
            padding: 8px 12px;
            border-left: 3px solid #007acc;
            margin-bottom: 8px;
            background: #1e1e1e;
            border-radius: 4px;
            font-size: 0.9em;
            line-height: 1.6;
        }
        .log-entry:hover { background: #2d2d30; }
        .timestamp { color: #4ec9b0; font-weight: bold; margin-right: 10px; }
        .error { border-left-color: #f48771; }
        .success { border-left-color: #4ec9b0; }
        .warning { border-left-color: #dcdcaa; }
        .no-logs {
            text-align: center;
            padding: 40px;
            color: #858585;
        }
        .refresh-info {
            position: fixed;
            top: 20px;
            right: 20px;
            background: #007acc;
            color: white;
            padding: 10px 20px;
            border-radius: 20px;
            font-size: 0.8em;
        }
    </style>
</head>
<body>
    <div class='refresh-info'>? Auto-refresh: 10s</div>
    <div class='header'>
        <h1>?? Device Activity Logs</h1>
        <div class='info'>
            Push API Monitoring | Real-time device communication logs<br>
            Last updated: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"
        </div>
    </div>
    <div class='logs-container'>";

            if (logs.Count == 0)
            {
                html += @"
        <div class='no-logs'>
            <h2>?? No Recent Activity</h2>
            <p>No device logs found. Waiting for device connections...</p>
        </div>";
            }
            else
            {
                html += $"<div class='info' style='margin-bottom: 20px;'>Showing {logs.Count} recent log entries</div>";
                
                foreach (dynamic log in logs)
                {
                    var message = (string)log.message;
                    var cssClass = "log-entry";
                    
                    if (message.Contains("ERROR")) cssClass += " error";
                    else if (message.Contains("SUCCESS") || message.Contains("SAVED")) cssClass += " success";
                    else if (message.Contains("WARNING") || message.Contains("SKIP")) cssClass += " warning";
                    
                    html += $@"
        <div class='{cssClass}'>
            <span class='timestamp'>[{log.timestamp}]</span>
            {System.Web.HttpUtility.HtmlEncode(message)}
        </div>";
                }
            }

            html += @"
    </div>
</body>
</html>";

            return html;
        }

        private string GenerateDevicesHtml(List<dynamic> devices)
        {
            var html = @"
<!DOCTYPE html>
<html>
<head>
    <title>Registered Devices - Push API</title>
    <meta charset='utf-8'>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            padding: 20px;
        }
        .container {
            max-width: 1200px;
            margin: 0 auto;
            background: white;
            border-radius: 15px;
            padding: 30px;
            box-shadow: 0 10px 30px rgba(0,0,0,0.2);
        }
        h1 { color: #667eea; margin-bottom: 20px; }
        table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 20px;
        }
        th {
            background: #667eea;
            color: white;
            padding: 12px;
            text-align: left;
        }
        td {
            padding: 12px;
            border-bottom: 1px solid #eee;
        }
        tr:hover { background: #f5f5f5; }
        .status-connected { color: #10b981; font-weight: bold; }
        .status-disconnected { color: #ef4444; font-weight: bold; }
    </style>
</head>
<body>
    <div class='container'>
        <h1>?? Registered Devices</h1>
        <p>Total devices: " + devices.Count + @"</p>
        <table>
            <thead>
                <tr>
                    <th>Serial Number</th>
                    <th>Device Name</th>
                    <th>School ID</th>
                    <th>Location</th>
                    <th>Last Push</th>
                    <th>Status</th>
                </tr>
            </thead>
            <tbody>";

            foreach (var device in devices)
            {
                var isConnected = device.lastPushTime != null &&
                    ((DateTime)device.lastPushTime).AddMinutes(5) > DateTime.Now;
                
                var statusClass = isConnected ? "status-connected" : "status-disconnected";
                var statusText = isConnected ? "? Connected" : "?? Disconnected";
                
                html += $@"
                <tr>
                    <td>{device.deviceSerial}</td>
                    <td>{device.deviceName ?? "N/A"}</td>
                    <td>{device.schoolID}</td>
                    <td>{device.deviceLocation ?? "N/A"}</td>
                    <td>{device.lastPushTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Never"}</td>
                    <td class='{statusClass}'>{statusText}</td>
                </tr>";
            }

            html += @"
            </tbody>
        </table>
    </div>
</body>
</html>";

            return html;
        }
    }
}
