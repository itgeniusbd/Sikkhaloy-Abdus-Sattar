using AttendanceDevice.Config_Class;
using System.Windows;
using System.Windows.Controls;

namespace AttendanceDevice.Settings.Pages
{
    /// <summary>
    /// Interaction logic for LeaveUser_Page.xaml
    /// </summary>
    public partial class LeaveUser_Page : Page
    {
        public LeaveUser_Page()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LeaveDG.ItemsSource = LocalData.Instance.Get_Leave();
        }
    }
}
