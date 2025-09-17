using AttendanceDevice.Config_Class;
using AttendanceDevice.Model;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AttendanceDevice.Settings.Pages
{
    /// <summary>
    /// Interaction logic for DeviceInfoPage.xaml
    /// </summary>
    public partial class DeviceInfoPage : Page
    {
        public DeviceInfoPage()
        {
            InitializeComponent();
        }
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadingDH.IsOpen = true;
            var isAnyDeviceConnected = false;
            using (var db = new ModelContext())
            {
                var devices = db.Devices.ToList();

                if (devices.Any())
                {
                    foreach (var device in devices)
                    {
                        var checkIp = await Device_PingTest.PingHostAsync(device.DeviceIP);
                        device.IsConnected = Convert.ToInt32(checkIp);
                        db.Entry(device).State = EntityState.Modified;

                        if (checkIp) isAnyDeviceConnected = true;
                    }

                    await db.SaveChangesAsync();

                    DeviceDtagrid.ItemsSource = devices;
                }
            }

            if (LocalData.Current_Error.Type == Error_Type.DeviceInfoPage)
            {
                if (!isAnyDeviceConnected)
                {
                    ErrorSnackbar.Message.Content = LocalData.Current_Error.Message;
                    ErrorSnackbar.IsActive = true;
                }
                else
                {
                    ErrorSnackbar.Message.Content = "";
                    ErrorSnackbar.IsActive = false;
                }
            }

            LoadingDH.IsOpen = false;
        }
        private async void AddDevice_Button_Click(object sender, RoutedEventArgs e)
        {
            if (DeviceNameTextbox.Text.Trim() == "" && DeviceIPTextbox.Text.Trim() == "") return;

            LoadingDH.IsOpen = true;
            ErrorSnackbar.IsActive = false;

            var checkIp = await Device_PingTest.PingHostAsync(DeviceIPTextbox.Text.Trim());

            if (!checkIp)
            {
                LoadingDH.IsOpen = false;

                if (ErrorSnackbar.Message != null)
                    ErrorSnackbar.Message.Content = $"Device is not connected to this {DeviceIPTextbox.Text} IP!";

                ErrorSnackbar.IsActive = true;
                return;
            }

            var device = new Device()
            {
                DeviceName = DeviceNameTextbox.Text,
                DeviceIP = DeviceIPTextbox.Text.Trim(),
                CommKey = 2015,
                Port = 4370
            };

            var d1 = new DeviceConnection(device);

            var status = d1.ConnectDeviceWithoutEvent();

            LoadingDH.IsOpen = false;

            if (!status.IsSuccess) return;

            device.DeviceSN = d1.DeviceSerialNumber();
            device.IsConnected = 1;

            using (var db = new ModelContext())
            {
                db.Devices.Add(device);
                await db.SaveChangesAsync();

                DeviceDtagrid.ItemsSource = db.Devices.ToList();
            }

            DeviceNameTextbox.Text = "";
            DeviceIPTextbox.Text = "";
        }
        private async void Connect_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as Button) is Button btnConnect)) return;

            btnConnect.IsEnabled = false;
            ErrorSnackbar.IsActive = false;
            btnConnect.Content = "Connecting...";

            var device = ((Button)sender).DataContext as Device;

            var checkIp = device != null && await Device_PingTest.PingHostAsync(device.DeviceIP);
            if (!checkIp)
            {
                btnConnect.IsEnabled = true;
                btnConnect.Content = "Connect";
                DeviceDtagrid.UpdateLayout();

                if (ErrorSnackbar.Message != null) ErrorSnackbar.Message.Content = "Unable to connect device";
                ErrorSnackbar.IsActive = true;
                return;
            }

            var d1 = new DeviceConnection(device);
            var status = await Task.Run(() => d1.ConnectDeviceWithoutEvent());

            if (status.IsSuccess)
            {
                var details = new DeviceDetailsPage(d1);
                NavigationService?.Navigate(details);
            }
            else
            {
                btnConnect.Content = "Connect";
            }
        }
        private async void DeviceDtagrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit) return;

            LoadingDH.IsOpen = true;

            var deviceContext = e.Row.DataContext as Device;

            using (var db = new ModelContext())
            {
                if (db.Devices.Any(o => o.DeviceIP == deviceContext.DeviceIP))
                {
                    DeviceDtagrid.ItemsSource = db.Devices.ToList();
                    LoadingDH.IsOpen = false;
                    return;
                }

                var checkIp = deviceContext != null && await Device_PingTest.PingHostAsync(deviceContext.DeviceIP);

                if (deviceContext != null)
                {
                    deviceContext.IsConnected = Convert.ToInt32(checkIp);
                    db.Entry(deviceContext).State = EntityState.Modified;
                }

                await db.SaveChangesAsync();

                DeviceDtagrid.ItemsSource = db.Devices.ToList();
                LoadingDH.IsOpen = false;
            }
        }
        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Refresh();
            ErrorSnackbar.IsActive = false;
        }
    }
}
