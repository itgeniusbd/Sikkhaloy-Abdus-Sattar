using AttendanceDevice.Config_Class;
using AttendanceDevice.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace AttendanceDevice.Settings.Pages
{
    /// <summary>
    /// Interaction logic for DeviceDetailsPage.xaml
    /// </summary>
    public partial class DeviceDetailsPage : Page
    {
        private readonly DeviceConnection _deviceCon;
        private string _deviceId = "";
        public DeviceDetailsPage(DeviceConnection d1)
        {
            _deviceCon = d1;
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_deviceCon.IsConnected()) return;

            DeviceName.Text = _deviceCon.Device.DeviceName;
            DeviceIP.Text = _deviceCon.Device.DeviceIP;
            DeviceTime.Text = _deviceCon.GetDateTime().ToString("d MMM yy (hh:mm.tt)");

            var deDetails = _deviceCon.GetDeviceDetails();

            CapacityTB.Text = deDetails.UserCapacity + "/" + deDetails.NumberOfUsers;
            LogCapacity.Text = deDetails.AttendanceRecordCapacity + "/" + deDetails.AttendanceRecords;
            FP_Capacity.Text = deDetails.FpCapacity + "/" + deDetails.NumberOfFp;

            //duplicate time
            btnDuplicateTime.Content = "Set Duplicate Punch Time (" + deDetails.DuplicatePunchTime + " min)";

            //PC new user
            PcNewUser();
        }

        private async void BtnSyncPcTimetoDevice_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!await _deviceCon.IsConnectedAsync()) return;

            await Task.Run(() => _deviceCon.SetDateTime());
            DeviceTime.Text = _deviceCon.GetDateTime().ToString("d MMM yyyy (hh:mm tt)");
        }

        private void BtnRestartDevice_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!_deviceCon.IsConnected()) return;

            if (!_deviceCon.Restart()) return;

            var deviceInfo = new DeviceInfoPage();
            NavigationService?.Navigate(deviceInfo);
        }
        private void BtnPowerOffDevice_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!_deviceCon.IsConnected()) return;

            if (!_deviceCon.PowerOff()) return;

            var deviceInfo = new DeviceInfoPage();
            NavigationService?.Navigate(deviceInfo);
        }

        private async void BtnUploadUserDevice_Click(object sender, RoutedEventArgs e)
        {
            if (!await _deviceCon.IsConnectedAsync()) return;

            LoadingDH.IsOpen = true;
            btnUploadUsers.IsEnabled = false;

            using (var db = new ModelContext())
            {
                var user = db.Users.ToList();
                if (user.Count > 0)
                {
                    await Task.Run(() => _deviceCon.Upload_User(user));
                }
            }

            LoadingDH.IsOpen = false;
            btnUploadUsers.IsEnabled = true;
            NavigationService?.Refresh();
        }
        private async void BtnDownloadUsersDevice_Click(object sender, RoutedEventArgs e)
        {
            LoadingDH.IsOpen = true;
            btnDownloadUsers.IsEnabled = false;

            var result = await Task.Run(() => _deviceCon.Download_User().OrderBy(x => x.RFID));

            if (result != null)
            {
                DeviceUserDG.ItemsSource = result;
            }

            btnDownloadUsers.IsEnabled = true;
            LoadingDH.IsOpen = false;

            NavigationService?.Refresh();
        }
        private async void PcNewUser()
        {
            LoadingDH.IsOpen = true;
            var deviceUsers = await Task.Run(() => _deviceCon.Download_User().OrderBy(x => x.RFID));
            var dUsers = deviceUsers.Select(x => new { x.DeviceID, x.RFID }).ToList();

            List<User> pcUsers;
            using (var db = new ModelContext())
            {
                pcUsers = db.Users.OrderBy(x => x.RFID).ToList();
            }

            var newUser = pcUsers.Where(pu => !dUsers.Any(du => du.RFID.ToInt(0) == pu.RFID.ToInt(0) && du.DeviceID == pu.DeviceID)).ToList();

            if (newUser.Count > 0)
            {
                PCNewUserDG.ItemsSource = newUser;
            }
            else
            {
                PCNewUserDG.Visibility = Visibility.Collapsed;
                Utb.Text = "No new User Found!";
                btnUploadUsers.IsEnabled = false;
            }

            LoadingDH.IsOpen = false;
        }

        //Device Setting
        private async void BtnClearAllUsera_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            LoadingDH.IsOpen = true;
            var deleted = await Task.Run(() => _deviceCon.ClearAllUsers());

            if (deleted)
            {
                LoadingDH.IsOpen = false;
            }
            else
            {
                LoadingDH.IsOpen = false;
                MessageBox.Show("System error", "Error!");
            }
            NavigationService?.Refresh();
        }
        private async void BtnAdditionalUserDevice_Click(object sender, RoutedEventArgs e)
        {
            LoadingDH.IsOpen = true;

            var deviceUsers = await Task.Run(() => _deviceCon.Download_User().OrderBy(x => x.DeviceID));

            List<User> pcUsers;
            using (var db = new ModelContext())
            {
                pcUsers = db.Users.OrderBy(x => x.RFID).ToList();
            }

            var newUser = deviceUsers.Where(pu => pcUsers.All(du => du.DeviceID != pu.DeviceID)).ToList();

            DeviceAddiUserDG.ItemsSource = newUser;

            LoadingDH.IsOpen = false;
        }
        private async void BtnClearAllfingerprint_Click(object sender, RoutedEventArgs e)
        {
            LoadingDH.IsOpen = true;
            var deleted = await Task.Run(() => _deviceCon.ClearAll_FPs());

            if (deleted)
            {
                LoadingDH.IsOpen = false;
            }
            else
            {
                LoadingDH.IsOpen = false;
                MessageBox.Show("Fingerprint not deleted", "Error!");
            }
            NavigationService?.Refresh();
        }

        private async void BtnClearAllLogs_Click(object sender, RoutedEventArgs e)
        {
            LoadingDH.IsOpen = true;
            var deleted = await Task.Run(() => _deviceCon.ClearAll_Logs());
            if (deleted)
            {
                LoadingDH.IsOpen = false;
            }
            else
            {
                LoadingDH.IsOpen = false;
                MessageBox.Show("System error", "Error!");
            }
            NavigationService?.Refresh();
        }


        private void BtnFindUser_Click(object sender, RoutedEventArgs e)
        {
            var filterText = findDeviceidTextBox.Text;
            var cv = CollectionViewSource.GetDefaultView(DeviceUserDG.ItemsSource);

            if (!string.IsNullOrEmpty(filterText))
            {
                cv.Filter = o =>
                {
                    var user = o as User;
                    return user.DeviceID.ToString() == filterText || user.Name.ToUpper().StartsWith(filterText.ToUpper());
                };
            }
        }

        private async void BtnDuplicateTime_Click(object sender, RoutedEventArgs e)
        {
            LoadingDH.IsOpen = true;
            var dReset = await Task.Run(() => _deviceCon.Duplicate_Punch_Time_Reset());

            if (dReset)
            {
                LoadingDH.IsOpen = false;
            }
            else
            {
                LoadingDH.IsOpen = false;
                MessageBox.Show("Duplicate time not set", "Error!");
            }
            NavigationService?.Refresh();
        }

        private void FindUserButton_Click(object sender, RoutedEventArgs e)
        {
            _deviceCon.FP_StateCancel();
            if (string.IsNullOrWhiteSpace(UserIDTextbox.Text))
            {
                _deviceId = "";
                Userpanel.Visibility = Visibility.Collapsed;
                return;
            };

            var info = LocalData.Instance.UserViews.FirstOrDefault(u => u.ID == UserIDTextbox.Text.Trim());

            if (info != null)
            {
                UserInfoGrid.DataContext = info;
                _deviceId = info.DeviceID.ToString();
                Userpanel.Visibility = Visibility.Visible;
                UserInfoError.Text = "";

                var fp = LocalData.Instance.Get_UserFP(info.DeviceID);

                LI.Background = fp.LeftIndex;
                LT.Background = fp.LeftThamb;
                RI.Background = fp.RightIndex;
                RT.Background = fp.RightThamb;
            }
            else
            {
                _deviceId = "";
                Userpanel.Visibility = Visibility.Collapsed;
                UserInfoError.Text = "User Not Found!";
            }
        }

        private void FingerButton_Click(object sender, RoutedEventArgs e)
        {
            MessageTB.Text = "";
            if (string.IsNullOrWhiteSpace(_deviceId)) return;

            var button = sender as Button;
            _deviceCon.FingerprintMessage = MessageTB;
            var index = Convert.ToInt32(button?.CommandParameter);

            if (button?.Background != Brushes.GreenYellow)
            {
                _deviceCon.FP_Add(_deviceId, index);
            }
            else
            {
                var messageBoxResult = MessageBox.Show("Delete Fingerprint?", "Finger Delete", MessageBoxButton.YesNo);
                if (messageBoxResult != MessageBoxResult.Yes) return;

                var errorCode = _deviceCon.FP_Delete(_deviceId, index);
                if (errorCode == 0)
                {
                    LocalData.Instance.Delete_UserFP(Convert.ToInt32(_deviceId), index);
                    button.Background = Brushes.White;
                }
                else
                {
                    //"Operation failed,ErrorCode=" + ErrorCode.ToString();
                }
            }
        }

        //download/upload finger print 
        private async void BtnDownloadFP_OnClick(object sender, RoutedEventArgs e)
        {
            LoadingDH.IsOpen = true;
            btnDownloadUsers.IsEnabled = false;

            await Task.Run(() => _deviceCon.FP_DownloadInPc());

            btnDownloadUsers.IsEnabled = true;
            LoadingDH.IsOpen = false;

            NavigationService?.Refresh();
        }

        private async void BtnUploadFP_OnClick(object sender, RoutedEventArgs e)
        {
            if (!await _deviceCon.IsConnectedAsync()) return;

            LoadingDH.IsOpen = true;
            btnUploadUsers.IsEnabled = false;

            using (var db = new ModelContext())
            {
                var fingerPrints = db.user_FingerPrints.ToList();
                if (fingerPrints.Count > 0)
                {
                    await Task.Run(() => _deviceCon.FP_UploadPcToDevice());
                }
            }

            LoadingDH.IsOpen = false;
            btnUploadUsers.IsEnabled = true;
            NavigationService?.Refresh();
        }

        private async void BtnCancelFinger_OnClick(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => _deviceCon.FP_StateCancel());
        }
    }
}
