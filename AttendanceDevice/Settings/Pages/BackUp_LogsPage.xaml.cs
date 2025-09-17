using AttendanceDevice.Config_Class;
using AttendanceDevice.Model;
using AttendanceDevice.ViewModel;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

            var logs = new List<LogDataSendModel>();
            using (var db = new ModelContext())
            {
                logs = (from b in db.attendanceLog_Backups
                        join u in db.Users
                            on b.DeviceID equals u.DeviceID
                        select new LogDataSendModel
                        {
                            DeviceID = b.DeviceID,
                            EntryTime = b.Entry_Time,
                            EntryDate = b.Entry_Date,
                            EntryDay = b.Entry_Day,
                            Is_Student = u.Is_Student,
                            ScheduleID = u.ScheduleID
                        }).Distinct().ToList().Where(b => Convert.ToDateTime(b.EntryDate) >= fdate && Convert.ToDateTime(b.EntryDate) <= tdate).ToList();
            }

            if (logs.Any())
            {
                var client = new RestClient(ApiUrl.EndPoint);

                var backupData = new RestRequest("api/Attendance/{id}/backup_data", Method.POST);
                backupData.AddUrlSegment("id", LocalData.Instance.institution.SchoolID);
                backupData.AddHeader("Authorization", "Bearer " + LocalData.Instance.institution.Token);
                backupData.AddJsonBody(logs);
                //Leave execute the request
                var response = await client.ExecutePostTaskAsync(backupData);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (var db = new ModelContext())
                    {
                        var bLogs = db.attendanceLog_Backups
                            .ToList().Where(b =>
                                Convert.ToDateTime(b.Entry_Date) >= fdate && Convert.ToDateTime(b.Entry_Date) <= tdate)
                            .ToList();
                        db.attendanceLog_Backups.RemoveRange(bLogs);
                        await db.SaveChangesAsync();
                    }

                    LogDG.ItemsSource = LocalData.Instance.Get_Log_Backup();
                }
                else
                {

                }


            }

            LoadingDH.IsOpen = false;
        }
    }
}
