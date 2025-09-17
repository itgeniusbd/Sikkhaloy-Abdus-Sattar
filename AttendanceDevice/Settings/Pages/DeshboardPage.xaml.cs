using AttendanceDevice.Config_Class;
using AttendanceDevice.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace AttendanceDevice.Settings.Pages
{
    /// <summary>
    /// Interaction logic for Deshboard.xaml
    /// </summary>
    public partial class Deshboard : Page
    {
        private readonly Setting _setting;

        public Deshboard()
        {
            InitializeComponent();
        }

        public Deshboard(Setting _setting)
        {
            this._setting = _setting;
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var data = LocalData.Instance.GetServerNotifications();

            if (data.Count > 0)
            {
                ActivityListView.ItemsSource = data;
            }
            else
            {
                ErrorPanel.Visibility = Visibility.Collapsed;
            }

            var settingDashboard = new Setting_Dashboard_View();
            DashboardData.DataContext = settingDashboard;

        }

        private void ErrorDelete_Button_Click(object sender, RoutedEventArgs e)
        {
            LocalData.Instance.DeleteNotifications();
            this.NavigationService?.Refresh();
        }

        private async void ResetButton_Click(object sender, RoutedEventArgs e)
        {

            await LocalData.Instance.ResetApp();


            var loginWindow = new Login_Window();
            loginWindow.Show();
            _setting.Close();
            //App.Current.Shutdown();
        }
    }
}
