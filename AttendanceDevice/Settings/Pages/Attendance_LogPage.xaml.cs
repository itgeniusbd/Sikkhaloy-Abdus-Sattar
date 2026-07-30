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
            InitializeComponent();
        }

        private void RefreshPendingList()
        {
            LocalData.Instance.ArchiveExpiredAttendanceRecords();
            LocalData.Instance.FlagIncompleteRecordsForResync();
            _attendance_Record = LocalData.Instance.Get_Pending_Attendance_Record();
            LogDG.ItemsSource = _attendance_Record;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshPendingList();
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

        private void BtnResetSync_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show(
                "Reset sync status for all local attendance records?\n\nThis will re-send them to the server.",
                "Mark For Resend",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            LocalData.Instance.ResetAttendanceSyncFlags();
            RefreshPendingList();
        }

        private void BtnMarkSynced_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show(
                "Mark today's local attendance as already synced?\n\nUse this after wiping server data to stop re-sending old punches.",
                "Mark Synced",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            var count = LocalData.Instance.MarkTodayAttendanceSynced();
            RefreshPendingList();
            MessageBox.Show($"{count} record(s) marked synced for today.", "Mark Synced");
        }

        private void BtnClearDateRange_Click(object sender, RoutedEventArgs e)
        {
            if (!FromDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Select From Date (e.g. 17-Jul).", "Clear Date Range");
                return;
            }

            var from = FromDate.SelectedDate.Value.Date;
            var to = ToDate.SelectedDate?.Date ?? from;

            var confirm = MessageBox.Show(
                $"Delete local attendance from {from:dd-MMM-yyyy} to {to:dd-MMM-yyyy}?\n\nServer data is not changed.",
                "Clear Date Range (Local)",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            var count = LocalData.Instance.ClearLocalAttendanceForDateRange(from, to);
            RefreshPendingList();
            MessageBox.Show($"{count} local attendance record(s) deleted.", "Clear Date Range");
        }

        private void BtnClearToday_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show(
                "Delete today's attendance from this PC only?\n\nServer data is not changed. Also clear device logs if punches keep coming back.",
                "Clear Today (Local)",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            var count = LocalData.Instance.ClearTodayLocalAttendance();
            RefreshPendingList();
            MessageBox.Show($"{count} local record(s) deleted for today.", "Clear Today");
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
