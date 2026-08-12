using AttendanceDevice.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AttendanceDevice.Config_Class
{
    internal static class StartupHelper
    {
        public static Task RunPostScheduleMaintenanceAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    StartupLogger.LogStage("startup-bg: user schedule reconcile");
                    LocalData.Instance.EnsureUserScheduleAssignmentsFromUsers();
                    StartupLogger.LogStage("startup-bg: archive expired attendance");
                    LocalData.Instance.ArchiveExpiredAttendanceRecords();
                    StartupLogger.LogStage("startup-bg: repair today schedule ids");
                    LocalData.Instance.RepairTodayScheduleIdsForResync();
                    StartupLogger.LogStage("startup-bg: post-schedule maintenance done");
                }
                catch (Exception ex)
                {
                    StartupLogger.LogFailure("startup-bg: post-schedule maintenance", ex);
                }
            });
        }

        public static async Task<DeviceReturn> ConnectDeviceWithTimeoutAsync(DeviceConnection device)
        {
            if (device == null)
            {
                return new DeviceReturn
                {
                    IsSuccess = false,
                    Code = -1,
                    Message = "Device is missing."
                };
            }

            var connectTask = Task.Run(() => device.ConnectDevice());
            var completed = await Task.WhenAny(
                connectTask,
                Task.Delay(PerformanceSettings.StartupDeviceConnectTimeoutMs));

            if (completed != connectTask)
            {
                StartupLogger.LogStage(
                    $"startup: device connect timeout ({device.Device?.DeviceIP})");
                return new DeviceReturn
                {
                    IsSuccess = false,
                    Code = -1,
                    Message = "Device connection timed out."
                };
            }

            return await connectTask;
        }

        public static async Task DownloadDeviceLogsInBackgroundAsync(
            IEnumerable<DeviceConnection> devices,
            Institution institution)
        {
            if (devices == null || institution == null)
                return;

            foreach (var device in devices)
            {
                try
                {
                    var status = await ConnectDeviceWithTimeoutAsync(device);
                    if (!status.IsSuccess)
                        continue;

                    var deviceTime = device.GetDateTime();
                    if (deviceTime.ToString("dd-MM-yyyy hh:mm tt") != DateTime.Now.ToString("dd-MM-yyyy hh:mm tt"))
                        await Task.Run(() => device.SetDateTime());

                    var todayLog = await Task.Run(() => device.DownloadTodayLogs());
                    var prevLog = await Task.Run(() => device.DownloadPrevLogs());
                    await Machine.SaveLogsOrAttendanceInPc(prevLog, todayLog, institution, device.Device);
                }
                catch (Exception ex)
                {
                    StartupLogger.LogFailure(
                        $"startup-bg: device logs ({device.Device?.DeviceIP})",
                        ex);
                }
            }
        }
    }
}
