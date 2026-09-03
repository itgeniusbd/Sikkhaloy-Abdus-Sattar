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

            var commKeyText = CommKeyTextbox.Text.Trim();
            var commKey = 2015;
            if (commKeyText != "")
            {
                if (!int.TryParse(commKeyText, out commKey) || commKey < 0 || commKey > 999999)
                {
                    LoadingDH.IsOpen = false;

                    if (ErrorSnackbar.Message != null)
                        ErrorSnackbar.Message.Content = "Comm Key must be a number between 0 and 999999.";

                    ErrorSnackbar.IsActive = true;
                    return;
                }
            }

            var device = new Device()
            {
                DeviceName = DeviceNameTextbox.Text,
                DeviceIP = DeviceIPTextbox.Text.Trim(),
                CommKey = commKey,
                Port = 4370
            };

            var d1 = new DeviceConnection(device);

            var status = d1.ConnectDeviceWithoutEvent();

            LoadingDH.IsOpen = false;

            if (!status.IsSuccess)
            {
                if (ErrorSnackbar.Message != null)
                    ErrorSnackbar.Message.Content = status.Message;

                ErrorSnackbar.IsActive = true;
                return;
            }

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
            ErrorSnackbar.IsActive = false;

            var deviceContext = e.Row.DataContext as Device;
            if (deviceContext == null)
            {
                LoadingDH.IsOpen = false;
                return;
            }

            if (deviceContext.CommKey < 0 || deviceContext.CommKey > 999999)
            {
                LoadingDH.IsOpen = false;

                if (ErrorSnackbar.Message != null)
                    ErrorSnackbar.Message.Content = "Comm Key must be a number between 0 and 999999.";

                ErrorSnackbar.IsActive = true;
                return;
            }

            var checkIp = await Device_PingTest.PingHostAsync(deviceContext.DeviceIP);

            using (var db = new ModelContext())
            {
                var device = await db.Devices.FindAsync(deviceContext.Id);
                if (device == null)
                {
                    LoadingDH.IsOpen = false;
                    return;
                }

                if (db.Devices.Any(o => o.Id != device.Id && o.DeviceIP == deviceContext.DeviceIP))
                {
                    if (ErrorSnackbar.Message != null)
                        ErrorSnackbar.Message.Content = "Another device already uses this IP address.";

                    ErrorSnackbar.IsActive = true;
                    DeviceDtagrid.ItemsSource = db.Devices.ToList();
                    LoadingDH.IsOpen = false;
                    return;
                }

                device.DeviceName = deviceContext.DeviceName;
                device.DeviceIP = deviceContext.DeviceIP;
                device.Port = deviceContext.Port;
                device.CommKey = deviceContext.CommKey;
                device.IsConnected = Convert.ToInt32(checkIp);

                await db.SaveChangesAsync();

                DeviceDtagrid.ItemsSource = db.Devices.ToList();
            }

            LoadingDH.IsOpen = false;
        }
        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Refresh();
            ErrorSnackbar.IsActive = false;
        }
    }
}
