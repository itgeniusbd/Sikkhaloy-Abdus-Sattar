using AttendanceDevice.APIClass;
using AttendanceDevice.Config_Class;
using AttendanceDevice.Model;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AttendanceDevice.Settings.Pages
{
    public partial class SchedulePage : Page
    {
        private bool _isLoading;

        public SchedulePage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadSchedulesAsync(forceSync: false);
        }

        private async void SyncButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadSchedulesAsync(forceSync: true);
        }

        private async Task LoadSchedulesAsync(bool forceSync)
        {
            if (_isLoading)
                return;

            _isLoading = true;
            SyncButton.IsEnabled = false;
            EmptyMessage.Visibility = Visibility.Collapsed;

            try
            {
                var syncFailed = false;

                if (forceSync || !LocalData.Instance.Schedules_Get().Any())
                {
                    StatusMessage.Text = "Downloading schedule from server...";
                    StatusMessage.Visibility = Visibility.Visible;

                    if (await ApiUrl.IsNoNetConnection())
                    {
                        syncFailed = true;
                        EmptyMessage.Text = "No internet connection. Connect and press \"Download from Server\" again.";
                    }
                    else if (await ApiUrl.IsServerUnavailable())
                    {
                        syncFailed = true;
                        EmptyMessage.Text = "Server is unavailable. Try again later.";
                    }
                    else
                    {
                        var ins = LocalData.Instance.institution;
                        if (ins == null || string.IsNullOrWhiteSpace(ins.Token))
                        {
                            syncFailed = true;
                            EmptyMessage.Text = "Not logged in. Close Settings, login again, then download schedule.";
                        }
                        else
                        {
                            var client = new RestSharp.RestClient(ApiUrl.EndPoint);
                            var schoolId = LocalData.Instance.GetEffectiveSchoolId();
                            var result = await ScheduleAssignmentSync.EnsureScheduleBundleAsync(
                                client, schoolId, ins.Token.Trim());

                            if (!result.Success && !LocalData.Instance.Schedules_Get().Any())
                            {
                                syncFailed = true;
                                EmptyMessage.Text =
                                    "Server returned schedule data but this PC could not save it. " +
                                    "Check Logs folder, then login again and retry.";
                            }
                        }
                    }
                }

                BindGrid(syncFailed);
            }
            finally
            {
                StatusMessage.Visibility = Visibility.Collapsed;
                SyncButton.IsEnabled = true;
                _isLoading = false;
            }
        }

        private void BindGrid(bool syncFailed = false)
        {
            var schedules = LocalData.Instance.GetTodayDisplaySchedules();
            ScheduleDG.ItemsSource = schedules;

            if (schedules.Any())
            {
                EmptyMessage.Visibility = Visibility.Collapsed;
                StatusMessage.Text = $"{schedules.Count} schedule(s) for today";
                StatusMessage.Visibility = Visibility.Visible;
                return;
            }

            if (!syncFailed)
            {
                EmptyMessage.Text =
                    "No schedule on this PC. Check internet, login again, then press \"Download from Server\".";
            }

            EmptyMessage.Visibility = Visibility.Visible;
        }
    }
}
