using AttendanceDevice.Config_Class;
using AttendanceDevice.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AttendanceDevice.Settings.Pages
{
    /// <summary>
    /// Interaction logic for Attendance_LogPage.xaml
    /// </summary>
    public partial class Attendance_LogPage : Page
    {
        private List<Attendance_Record_View> _attendance_Record = new List<Attendance_Record_View>();

        public Attendance_LogPage()
        {
            _attendance_Record = LocalData.Instance.Get_Pending_Attendance_Record();
            InitializeComponent();

        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LogDG.ItemsSource = _attendance_Record;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var UserRadioButton = (sender as RadioButton);

            if (UserRadioButton.Content.ToString() == "Student")
            {
                LogDG.ItemsSource = _attendance_Record.Where(a => a.Is_Student).ToList();
            }
            else if (UserRadioButton.Content.ToString() == "Employee")
            {
                LogDG.ItemsSource = _attendance_Record.Where(a => !a.Is_Student).ToList();
            }
            else
            {
                LogDG.ItemsSource = _attendance_Record;
            }
        }

        private void BtnFind_Click(object sender, RoutedEventArgs e)
        {
            var fd = string.IsNullOrEmpty(FromDate.Text) ? "1/1/2000" : FromDate.Text;
            var td = string.IsNullOrEmpty(ToDate.Text) ? "1/1/3000" : ToDate.Text;

            DateTime fdate, tdate;
            DateTime.TryParse(fd, out fdate);
            DateTime.TryParse(td, out tdate);

            var IdName = IdNameTextBox.Text;

            if (string.IsNullOrEmpty(IdName))
            {
                LogDG.ItemsSource = _attendance_Record.Where(a => a.dtAttendanceDate >= fdate && a.dtAttendanceDate <= tdate).ToList();
            }
            else
            {
                LogDG.ItemsSource = _attendance_Record.Where(a => a.dtAttendanceDate >= fdate && a.dtAttendanceDate <= tdate && (a.ID.Contains(IdName) || a.Name.ToLower().Contains(IdName.ToLower()))).ToList();
            }
        }
    }
}
