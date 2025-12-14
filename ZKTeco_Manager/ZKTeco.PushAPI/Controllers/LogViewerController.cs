using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Http;

namespace ZKTeco.PushAPI.Controllers
{
    /// <summary>
    /// Log Viewer Controller - View logs directly from browser
    /// </summary>
    [RoutePrefix("api/logs")]
    public class LogViewerController : ApiController
    {
        /// <summary>
        /// Get today's device logs
        /// GET /api/logs/device-today
        /// </summary>
        [HttpGet]
        [Route("device-today")]
        public IHttpActionResult GetTodayDeviceLogs()
        {
            try
            {
                var logs = new List<string>();
                var logPaths = new List<string>();

                // Try App_Data first
                try
                {
                    var appDataPath = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/DeviceLogs/");
                    if (Directory.Exists(appDataPath))
                    {
                        logPaths.Add(appDataPath);
                    }
                }
                catch { }

                // Try Temp directory
                var tempPath = Path.Combine(Path.GetTempPath(), "PushAPI_DeviceLogs");
                if (Directory.Exists(tempPath))
                {
                    logPaths.Add(tempPath);
                }

                // Try current directory
                try
                {
                    var currentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "DeviceLogs");
                    if (Directory.Exists(currentPath))
                    {
                        logPaths.Add(currentPath);
                    }
                }
                catch { }

                var todayLogFile = $"{DateTime.Now:yyyy-MM-dd}.log";
                string foundLogPath = null;
                string logContent = null;

                foreach (var logPath in logPaths.Distinct())
                {
                    var fullPath = Path.Combine(logPath, todayLogFile);
                    if (File.Exists(fullPath))
                    {
                        foundLogPath = fullPath;
                        logContent = File.ReadAllText(fullPath);
                        break;
                    }
                }

                if (logContent != null)
                {
                    var lines = logContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    return Ok(new
                    {
                        success = true,
                        logFile = todayLogFile,
                        logPath = foundLogPath,
                        totalLines = lines.Length,
                        logs = lines.Reverse().Take(100).Reverse().ToList(), // Last 100 lines
                        message = $"Found log file with {lines.Length} lines"
                    });
                }
                else
                {
                    return Ok(new
                    {
                        success = false,
                        message = "No log file found for today",
                        searchedPaths = logPaths,
                        fileName = todayLogFile,
                        hint = "Logs will be created when device sends data"
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    success = false,
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Get all available log files
        /// GET /api/logs/list
        /// </summary>
        [HttpGet]
        [Route("list")]
        public IHttpActionResult GetLogFilesList()
        {
            try
            {
                var logFiles = new List<object>();
                var logPaths = new List<string>();

                // Try App_Data
                try
                {
                    var appDataPath = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/DeviceLogs/");
                    if (Directory.Exists(appDataPath))
                    {
                        logPaths.Add(appDataPath);
                    }
                }
                catch { }

                // Try Temp
                var tempPath = Path.Combine(Path.GetTempPath(), "PushAPI_DeviceLogs");
                if (Directory.Exists(tempPath))
                {
                    logPaths.Add(tempPath);
                }

                // Try current directory
                try
                {
                    var currentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "DeviceLogs");
                    if (Directory.Exists(currentPath))
                    {
                        logPaths.Add(currentPath);
                    }
                }
                catch { }

                foreach (var logPath in logPaths.Distinct())
                {
                    if (Directory.Exists(logPath))
                    {
                        var files = Directory.GetFiles(logPath, "*.log")
                            .Select(f => new FileInfo(f))
                            .OrderByDescending(f => f.LastWriteTime)
                            .Take(10)
                            .Select(f => new
                            {
                                fileName = f.Name,
                                filePath = f.FullName,
                                size = f.Length,
                                lastModified = f.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                directory = logPath
                            });

                        logFiles.AddRange(files);
                    }
                }

                if (logFiles.Any())
                {
                    return Ok(new
                    {
                        success = true,
                        totalFiles = logFiles.Count,
                        files = logFiles
                    });
                }
                else
                {
                    return Ok(new
                    {
                        success = false,
                        message = "No log files found",
                        searchedDirectories = logPaths
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get specific log file content
        /// GET /api/logs/view?date=2025-12-13
        /// </summary>
        [HttpGet]
        [Route("view")]
        public IHttpActionResult GetLogFileContent()
        {
            try
            {
                var dateParam = System.Web.HttpContext.Current.Request.QueryString["date"];
                
                if (string.IsNullOrEmpty(dateParam))
                {
                    dateParam = DateTime.Now.ToString("yyyy-MM-dd");
                }

                var logFileName = $"{dateParam}.log";
                var logPaths = new List<string>();

                // Try App_Data
                try
                {
                    var appDataPath = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/DeviceLogs/");
                    if (Directory.Exists(appDataPath))
                    {
                        logPaths.Add(appDataPath);
                    }
                }
                catch { }

                // Try Temp
                var tempPath = Path.Combine(Path.GetTempPath(), "PushAPI_DeviceLogs");
                if (Directory.Exists(tempPath))
                {
                    logPaths.Add(tempPath);
                }

                string foundPath = null;
                string content = null;

                foreach (var logPath in logPaths)
                {
                    var fullPath = Path.Combine(logPath, logFileName);
                    if (File.Exists(fullPath))
                    {
                        foundPath = fullPath;
                        content = File.ReadAllText(fullPath);
                        break;
                    }
                }

                if (content != null)
                {
                    var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                    return Ok(new
                    {
                        success = true,
                        date = dateParam,
                        filePath = foundPath,
                        totalLines = lines.Length,
                        content = content,
                        lines = lines.ToList()
                    });
                }
                else
                {
                    return Ok(new
                    {
                        success = false,
                        message = $"Log file not found for date: {dateParam}",
                        searchedPaths = logPaths,
                        fileName = logFileName
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get live logs - last N lines
        /// GET /api/logs/live?lines=50
        /// </summary>
        [HttpGet]
        [Route("live")]
        public IHttpActionResult GetLiveLogs()
        {
            try
            {
                var linesParam = System.Web.HttpContext.Current.Request.QueryString["lines"];
                int lineCount = 50;
                
                if (!string.IsNullOrEmpty(linesParam))
                {
                    int.TryParse(linesParam, out lineCount);
                }

                var logFileName = $"{DateTime.Now:yyyy-MM-dd}.log";
                var logPaths = new List<string>();

                // Try App_Data
                try
                {
                    var appDataPath = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/DeviceLogs/");
                    if (Directory.Exists(appDataPath))
                    {
                        logPaths.Add(appDataPath);
                    }
                }
                catch { }

                // Try Temp
                var tempPath = Path.Combine(Path.GetTempPath(), "PushAPI_DeviceLogs");
                if (Directory.Exists(tempPath))
                {
                    logPaths.Add(tempPath);
                }

                string foundPath = null;
                List<string> lastLines = null;

                foreach (var logPath in logPaths)
                {
                    var fullPath = Path.Combine(logPath, logFileName);
                    if (File.Exists(fullPath))
                    {
                        foundPath = fullPath;
                        var allLines = File.ReadAllLines(fullPath);
                        lastLines = allLines.Reverse().Take(lineCount).Reverse().ToList();
                        break;
                    }
                }

                if (lastLines != null)
                {
                    return Ok(new
                    {
                        success = true,
                        filePath = foundPath,
                        requestedLines = lineCount,
                        returnedLines = lastLines.Count,
                        logs = lastLines,
                        timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    });
                }
                else
                {
                    return Ok(new
                    {
                        success = false,
                        message = "No log file found for today",
                        searchedPaths = logPaths,
                        fileName = logFileName
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get device-specific logs by serial number and date
        /// GET /api/logs/device?sn=SMRS2522A00106&date=2025-12-13
        /// </summary>
        [HttpGet]
        [Route("device")]
        public IHttpActionResult GetDeviceLogs()
        {
            try
            {
                var serialNumber = System.Web.HttpContext.Current.Request.QueryString["sn"];
                var dateParam = System.Web.HttpContext.Current.Request.QueryString["date"];

                if (string.IsNullOrEmpty(dateParam))
                {
                    dateParam = DateTime.Now.ToString("yyyy-MM-dd");
                }

                if (string.IsNullOrEmpty(serialNumber))
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Device serial number (sn) is required",
                        usage = "/api/logs/device?sn=DEVICE_SERIAL&date=YYYY-MM-DD"
                    });
                }

                var logFileName = $"{dateParam}.log";
                var logPaths = new List<string>();

                // Try App_Data
                try
                {
                    var appDataPath = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/DeviceLogs/");
                    if (Directory.Exists(appDataPath))
                    {
                        logPaths.Add(appDataPath);
                    }
                }
                catch { }

                // Try Temp
                var tempPath = Path.Combine(Path.GetTempPath(), "PushAPI_DeviceLogs");
                if (Directory.Exists(tempPath))
                {
                    logPaths.Add(tempPath);
                }

                // Try current directory
                try
                {
                    var currentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "DeviceLogs");
                    if (Directory.Exists(currentPath))
                    {
                        logPaths.Add(currentPath);
                    }
                }
                catch { }

                string foundPath = null;
                List<string> deviceLogs = new List<string>();

                foreach (var logPath in logPaths.Distinct())
                {
                    var fullPath = Path.Combine(logPath, logFileName);
                    if (File.Exists(fullPath))
                    {
                        foundPath = fullPath;
                        var allLines = File.ReadAllLines(fullPath);
                        
                        // Filter logs by device serial number
                        deviceLogs = allLines
                            .Where(line => line.Contains(serialNumber))
                            .ToList();
                        
                        break;
                    }
                }

                if (foundPath != null)
                {
                    return Ok(new
                    {
                        success = true,
                        deviceSerial = serialNumber,
                        date = dateParam,
                        filePath = foundPath,
                        totalLogs = deviceLogs.Count,
                        logs = deviceLogs,
                        timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        message = deviceLogs.Count > 0 
                            ? $"Found {deviceLogs.Count} log entries for device {serialNumber}"
                            : $"No logs found for device {serialNumber} on {dateParam}"
                    });
                }
                else
                {
                    return Ok(new
                    {
                        success = false,
                        message = $"Log file not found for date: {dateParam}",
                        deviceSerial = serialNumber,
                        searchedPaths = logPaths,
                        fileName = logFileName,
                        hint = "Logs are created when device sends data. Make sure device is connected and sending data."
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    success = false,
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
    }
}
