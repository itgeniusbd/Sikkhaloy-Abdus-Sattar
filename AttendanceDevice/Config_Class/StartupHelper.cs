using AttendanceDevice.Model;
using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Pull today's punches from each device and apply them locally before any Abs mark.
        /// Device log time is used as-is; the device clock is synced only after logs are read.
        /// </summary>
        /// <param name="recoverDays">
        /// 0 = read previous days only from Last_Down_Log_Time (normal start). N &gt; 0 = also re-read the last
        /// N days from the device (Device Data "Sync Record") so punches lost from the PC are recovered.
        /// </param>
        public static async Task<bool> ImportTodayDeviceLogsAsync(
            IEnumerable<DeviceConnection> devices,
            Institution institution,
            int recoverDays = 0)
        {
            if (devices == null || institution == null)
                return false;

            var imported = 0;
            var connected = 0;

            try
            {
                LocalData.Instance.EnsureScheduleDataForPunch();
                await LocalData.Instance.EnsureScheduleBootstrapAsync(forceNetwork: true);
            }
            catch (Exception ex)
            {
                StartupLogger.LogFailure("device logs: schedule bootstrap", ex);
            }

            foreach (var device in devices)
            {
                try
                {
                    var status = await ConnectDeviceWithTimeoutAsync(device);
                    if (!status.IsSuccess)
                        continue;

                    connected++;

                    var todayLog = await Task.Run(() => device.DownloadTodayLogs());
                    DateTime? recoverFrom = recoverDays > 0
                        ? LocalData.Instance.GetAttendanceDate().Date.AddDays(-recoverDays)
                        : (DateTime?)null;
                    var prevLog = await Task.Run(() => device.DownloadPrevLogs(recoverFrom));
                    await Machine.SaveLogsOrAttendanceInPc(prevLog, todayLog, institution, device.Device);
                    imported++;

                    try
                    {
                        var deviceTime = device.GetDateTime();
                        if (deviceTime.ToString("dd-MM-yyyy hh:mm tt") != DateTime.Now.ToString("dd-MM-yyyy hh:mm tt"))
                            await Task.Run(() => device.SetDateTime());
                    }
                    catch
                    {
                        // Clock sync is optional; punches already use the device log time.
                    }
                }
                catch (Exception ex)
                {
                    StartupLogger.LogFailure(
                        $"device logs ({device.Device?.DeviceIP})",
                        ex);
                }
            }

            var ok = imported > 0 && imported == connected;
            if (ok)
            {
                try
                {
                    await Machine.ReplayRejectedLogsAsAttendance(institution);
                }
                catch (Exception ex)
                {
                    StartupLogger.LogFailure("device logs: replay rejected backups", ex);
                }
            }

            if (LocalData.Instance != null)
                LocalData.Instance.TodayDeviceLogsImportedFromDevice = ok;

            StartupLogger.LogStage(
                ok
                    ? $"device logs imported from {imported} device(s)"
                    : $"device logs incomplete (imported {imported}/{connected})");

            return ok;
        }
    }
}
