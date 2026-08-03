using AttendanceDevice.Model;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AttendanceDevice.Config_Class
{
    /// <summary>
    /// CPU/network tuning for display PCs. Low power mode lengthens sync intervals and slows marquee animation.
    /// </summary>
    internal static class PerformanceSettings
    {
        public static bool LowPowerMode =>
            LocalData.Instance?.institution?.Is_Low_Power_Mode ?? false;

        public static TimeSpan DisplaySyncInterval =>
            LowPowerMode ? TimeSpan.FromMinutes(8) : TimeSpan.FromMinutes(5);

        public static TimeSpan AssignmentSyncInterval =>
            LowPowerMode ? TimeSpan.FromMinutes(15) : TimeSpan.FromMinutes(10);

        public static TimeSpan ScheduleDaysSyncInterval =>
            LowPowerMode ? TimeSpan.FromMinutes(15) : TimeSpan.FromMinutes(10);

        public static TimeSpan ScheduleBootstrapInterval =>
            LowPowerMode ? TimeSpan.FromMinutes(20) : TimeSpan.FromMinutes(10);

        public static TimeSpan SchoolStatusSyncInterval =>
            LowPowerMode ? TimeSpan.FromMinutes(15) : TimeSpan.FromMinutes(10);

        public static TimeSpan DevicePingCacheDuration =>
            LowPowerMode ? TimeSpan.FromSeconds(90) : TimeSpan.FromSeconds(45);

        /// <summary>Minimum time between WebView attendance slider refreshes.</summary>
        public static TimeSpan WebViewRefreshMinInterval =>
            LowPowerMode ? TimeSpan.FromMinutes(8) : TimeSpan.FromMinutes(4);

        public static int MarqueeScrollAmount => LowPowerMode ? 8 : 18;

        public static int MarqueeScrollDelay => LowPowerMode ? 100 : 40;

        public static bool PreferPartialWebViewRefresh => true;

        public static async Task SetLowPowerModeAsync(bool enabled)
        {
            using (var db = new ModelContext())
            {
                var row = db.Institutions.FirstOrDefault();
                if (row == null)
                    return;

                row.Is_Low_Power_Mode = enabled;
                await db.SaveChangesAsync();

                if (LocalData.Instance?.institution != null)
                    LocalData.Instance.institution.Is_Low_Power_Mode = enabled;
            }
        }
    }
}
