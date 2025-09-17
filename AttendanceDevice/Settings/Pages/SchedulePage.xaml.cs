using AttendanceDevice.Config_Class;
using AttendanceDevice.Model;
using System.Windows;
using System.Windows.Controls;

namespace AttendanceDevice.Settings.Pages
{
    /// <summary>
    /// Interaction logic for Schedule.xaml
    /// </summary>
    public partial class SchedulePage : Page
    {
        public SchedulePage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ScheduleDG.ItemsSource = LocalData.Instance.Schedules_Get();
        }
    }
}
