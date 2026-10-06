using AttendanceDevice.Config_Class;
using AttendanceDevice.Model;
using AttendanceDevice.ViewModel;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AttendanceDevice.Settings.Pages
{
    /// <summary>
    /// Interaction logic for BackUp_LogsPage.xaml
    /// </summary>
    public partial class BackUp_LogsPage : Page
    {
        private List<Log_Backups_View> _log_Backups = new List<Log_Backups_View>();
        private string user_type = "";
        private DateTime fdate = new DateTime(2000, 1, 1);
        private DateTime tdate = new DateTime(3000, 1, 1);
        public BackUp_LogsPage()
        {
            _log_Backups = LocalData.Instance.Get_Log_Backup();
            InitializeComponent();
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LogDG.ItemsSource = _log_Backups;
        }


        private List<Log_Backups_View> data_filter()
        {
            var sd = string.IsNullOrEmpty(FromDate.Text) ? "1/1/2000" : FromDate.Text;
            var td = string.IsNullOrEmpty(ToDate.Text) ? "1/1/3000" : ToDate.Text;

            DateTime.TryParse(sd, out fdate);
            DateTime.TryParse(td, out tdate);

            if (user_type == "Student")
            {
                return _log_Backups.Where(a => a.dtEntry_Date >= fdate && a.dtEntry_Date <= tdate && a.Is_Student).ToList();
            }
            else if (user_type == "Employee")
            {
                return _log_Backups.Where(a => a.dtEntry_Date >= fdate && a.dtEntry_Date <= tdate && !a.Is_Student).ToList();
            }
            else
            {
                return _log_Backups.Where(a => a.dtEntry_Date >= fdate && a.dtEntry_Date <= tdate).ToList();
            }
        }
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var UserRadioButton = (sender as RadioButton);

            user_type = UserRadioButton.Content.ToString();
            LogDG.ItemsSource = data_filter();


        }

        private void BtnFind_Click(object sender, RoutedEventArgs e)
        {
            LogDG.ItemsSource = data_filter();
        }

        private void DeleteRecord_Button_Click(object sender, RoutedEventArgs e)
        {
            LoadingDH.IsOpen = true;

            var IDs = data_filter().Select(l => l.DeviceID).ToList();

            if (IDs.Count > 0)
            {
                LocalData.Instance.Delete_Log_Backup(fdate, tdate, IDs);
            }

            LogDG.ItemsSource = LocalData.Instance.Get_Log_Backup();

            LoadingDH.IsOpen = false;
        }

        private async void UploadRecord_Button_Click(object sender, RoutedEventArgs e)
        {
            LoadingDH.IsOpen = true;

            try
            {
                var ins = LocalData.Instance.institution;
                void Progress(string msg)
                {
                    try
                    {
                        Dispatcher.Invoke(() =>
                        {
                            // Circular ProgressBar dialog has no label; keep stage in the window title briefly.
                            if (Window.GetWindow(this) is Window w)
                                w.Title = "Sync Record — " + msg;
                        });
                    }
                    catch { /* ignore UI progress failures */ }
                    StartupLogger.LogStage("sync-record: " + msg);
                }

                // 1) Re-read EVERY log still on the device (month chunks), back up locally, apply all dates.
                Progress("Reading full device history...");
                var deviceNote = await RecoverFullHistoryFromDevicesAsync(ins, Progress);

                // 2) Apply Device Data backups of every date (not only the 7-day window).
                Progress("Applying Device Logs...");
                var replay = await Machine.ReplayBackupLogsAsync(ins, refreshSchedulesFromNetwork: true, allDates: true);
                LocalData.Instance.FlagIncompleteRecordsForResync();
                LocalData.Instance.RepairTodayScheduleIdsForResync();

                // 3) Upload ALL unsynced attendance (any date) in batches; drop only website-confirmed backups.
                Progress("Uploading unsynced attendance...");
                var push = await AttendanceWebsiteSync.PushPendingAsync(ins, Progress);
                var cleared = LocalData.Instance.CleanupSyncedLogBackups();

                _log_Backups = LocalData.Instance.Get_Log_Backup();
                LogDG.ItemsSource = data_filter();

                MessageBox.Show(
                    $"{deviceNote}\n" +
                    $"Applied {replay.Applied} device log(s) to attendance{replay.AppliedByDateText}.\n" +
                    $"Removed {cleared} device log(s) already saved on the website.\n" +
                    $"Still in Device Data (backup): {_log_Backups.Count}.\n" +
                    $"Pending after upload: {LocalData.Instance.GetPendingAttendanceCount()}.\n\n{push.Message}",
                    "Sync Record",
                    MessageBoxButton.OK,
                    push.TotalPushed > 0 || replay.Applied > 0 ? MessageBoxImage.Information : MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Sync Record", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoadingDH.IsOpen = false;
                try
                {
                    if (Window.GetWindow(this) is Window w && w.Title != null && w.Title.StartsWith("Sync Record"))
                        w.Title = "SIKKHALOY Attendance Device — Settings";
                }
                catch { /* ignore */ }
            }
        }

        private static async Task<string> RecoverFullHistoryFromDevicesAsync(Institution ins, Action<string> progress)
        {
            try
            {
                var connections = new List<DeviceConnection>();
                using (var db = new ModelContext())
                {
                    foreach (var device in db.Devices.ToList())
                    {
                        if (await Device_PingTest.PingHostAsync(device.DeviceIP))
                            connections.Add(new DeviceConnection(device));
                    }
                }

                if (!connections.Any())
                    return "Device not reachable - used the logs already on this PC.";

                progress?.Invoke("Downloading all device logs (chunked)...");
                var ok = await StartupHelper.ImportFullDeviceHistoryAsync(connections, ins, progress);
                return ok
                    ? $"Re-read and backed up the full device history (~{LocalData.FullDeviceHistoryDays} days back) plus today."
                    : "Could not read every device - used the logs already on this PC.";
            }
            catch (Exception ex)
            {
                StartupLogger.LogFailure("sync-record: full device recovery", ex);
                return "Device read failed - used the logs already on this PC.";
            }
        }
    }
}
