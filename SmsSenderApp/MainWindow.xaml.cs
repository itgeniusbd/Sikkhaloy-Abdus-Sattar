using Microsoft.Win32;
using Serilog;
using SmsService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using System.Reflection;
using System.IO;
using System.Windows.Media.Imaging;

namespace SmsSenderApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Attendance_SMS_Sender SmsSender { get; set; }
        private DispatcherTimer timer;

        public MainWindow()
        {
            try
            {
                InitializeComponent();

                // Set window icon
                try
                {
                    var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Sikkhaloy.ico");
                    if (File.Exists(iconPath))
                    {
                        this.Icon = new BitmapImage(new Uri(iconPath, UriKind.Absolute));
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to load window icon");
                }

                //SetStartup();

                // Check if Setting is available
                if (GlobalClass.Instance.Setting == null)
                {
                    Log.Error("Failed to load settings");
                    MessageBox.Show("Failed to load application settings. Please check database connection.",
                     "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMinutes(GlobalClass.Instance.Setting.SmsSendInterval)
                };

                timer.Tick += Timer_Tick;
                timer.Start();

                GlobalClass.Instance.SenderInsert();
                SmsSender = GlobalClass.Instance.SmsSender;

                if (SmsSender == null)
                {
                    SmsSender = new Attendance_SMS_Sender
                    {
                        AppStartTime = DateTime.Now
                    };
                    Log.Warning("SmsSender initialization failed, using temporary instance");
                }

                ShowAppInfo();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Failed to initialize MainWindow");
                MessageBox.Show($"Failed to initialize application: {ex.Message}",
                  "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                var today = DateTime.Now;

                var failedSmsList = new List<Attendance_SMS_Failed>();
                var smsList = new List<Attendance_SMS>();

                var currentTime = DateTime.Now.TimeOfDay;

                //Get the list from database
                var totalSmsList = await GlobalClass.Instance.GetAttendanceSmsListAndDeleteFromDbAsync();

                //Get the To-days SMS List
                var smsListOfToDay = totalSmsList.Where(s => s.AttendanceDate == today.Date).ToList();

                //Get the other days SMS List
                var smsListOfOtherDay = totalSmsList.Where(s => s.AttendanceDate != today.Date)
                    .Select(s => new Attendance_SMS_Failed
                    {
                        SchoolID = s.SchoolID,
                        ScheduleTime = s.ScheduleTime,
                        CreateTime = s.CreateTime,
                        SentTime = s.SentTime,
                        AttendanceDate = s.AttendanceDate,
                        SMS_Text = s.SMS_Text,
                        MobileNo = s.MobileNo,
                        AttendanceStatus = s.AttendanceStatus,
                        SMS_TimeOut = s.SMS_TimeOut,
                        EmployeeID = s.EmployeeID,
                        StudentID = s.StudentID,
                        FailedReson = "Not current date",
                    }).ToList();

                failedSmsList.AddRange(smsListOfOtherDay);

                if (smsListOfToDay.Any())
                {
                    //Get the List of time-up SMS
                    var timeupSmsList = smsListOfToDay.Where(s =>
                            s.ScheduleTime.TotalMinutes + s.SMS_TimeOut <= currentTime.TotalMinutes)
                        .Select(s => new Attendance_SMS_Failed
                        {
                            SchoolID = s.SchoolID,
                            ScheduleTime = s.ScheduleTime,
                            CreateTime = s.CreateTime,
                            SentTime = s.SentTime,
                            AttendanceDate = s.AttendanceDate,
                            SMS_Text = s.SMS_Text,
                            MobileNo = s.MobileNo,
                            AttendanceStatus = s.AttendanceStatus,
                            SMS_TimeOut = s.SMS_TimeOut,
                            EmployeeID = s.EmployeeID,
                            StudentID = s.StudentID,
                            FailedReson = "SMS sending time up",
                        }).ToList();

                    failedSmsList.AddRange(timeupSmsList);

                    //Get the SMS List of send-able SMS
                    smsList = smsListOfToDay
                       .Where(s => s.ScheduleTime.TotalMinutes + s.SMS_TimeOut > currentTime.TotalMinutes).ToList();

                    if (smsList.Any())
                    {
                        //get the School Ids
                        var schoolIds = smsList.Select(s => s.SchoolID).Distinct().ToList();
                        //get the SMS Balance
                        var noSmsBalanceSchoolIds = await GlobalClass.Instance.NoSmsBalanceSchoolIdsAsync(schoolIds);
                        //Get the smsList of School which have available balance
                        var noBalanceSmsList = new List<Attendance_SMS_Failed>();
                        if (noSmsBalanceSchoolIds.Any())
                        {
                            noBalanceSmsList = smsList.Where(s => noSmsBalanceSchoolIds.Contains(s.SchoolID)).Select(
                                s =>
                                    new Attendance_SMS_Failed
                                    {
                                        SchoolID = s.SchoolID,
                                        ScheduleTime = s.ScheduleTime,
                                        CreateTime = s.CreateTime,
                                        SentTime = s.SentTime,
                                        AttendanceDate = s.AttendanceDate,
                                        SMS_Text = s.SMS_Text,
                                        MobileNo = s.MobileNo,
                                        AttendanceStatus = s.AttendanceStatus,
                                        SMS_TimeOut = s.SMS_TimeOut,
                                        EmployeeID = s.EmployeeID,
                                        StudentID = s.StudentID,
                                        FailedReson = "Insufficient SMS Balance",
                                    }).ToList();

                            smsList = smsList.Where(s => !noSmsBalanceSchoolIds.Contains(s.SchoolID)).ToList();
                        }

                        failedSmsList.AddRange(noBalanceSmsList);
                        //check the Duplicate SMS (Check in database insert)

                        //Send the smsList
                        if (smsList.Any())
                        {
                            var smsRecords = new List<SMS_OtherInfo>();

                            var smsSendList = new List<SendSmsModel>();

                            foreach (var item in smsList)
                            {
                                var smsSend = new SendSmsModel
                                {
                                    Number = item.MobileNo,
                                    Text = item.SMS_Text
                                };
                                smsSendList.Add(smsSend);

                                var smsSendRecord = new SMS_OtherInfo
                                {
                                    SMS_Send_ID = smsSend.Guid,
                                    SchoolID = item.SchoolID,
                                    StudentID = item.StudentID == 0 ? (int?)null : item.StudentID,
                                    TeacherID = item.EmployeeID == 0 ? (int?)null : item.EmployeeID,
                                };

                                smsRecords.Add(smsSendRecord);
                            }

                            var sms = new SMS_Class();
                            var isSend = sms.SmsSendMultiple(smsSendList, "Device Attendance");
                            if (isSend.Validation)
                            {
                                await GlobalClass.Instance.SMS_OtherInfoAddAsync(smsRecords);
                            }
                            else
                            {
                                var smsSendFail = smsList.Select(s => new Attendance_SMS_Failed
                                {
                                    SchoolID = s.SchoolID,
                                    ScheduleTime = s.ScheduleTime,
                                    CreateTime = s.CreateTime,
                                    SentTime = s.SentTime,
                                    AttendanceDate = s.AttendanceDate,
                                    SMS_Text = s.SMS_Text,
                                    MobileNo = s.MobileNo,
                                    AttendanceStatus = s.AttendanceStatus,
                                    SMS_TimeOut = s.SMS_TimeOut,
                                    EmployeeID = s.EmployeeID,
                                    StudentID = s.StudentID,
                                    FailedReson = "SMS Send Failed",
                                }).ToList();

                                failedSmsList.AddRange(smsSendFail);

                                smsList.Clear();
                            }

                        }
                    }
                }

                //insert the fail SMS table
                if (failedSmsList.Any())
                {
                    await GlobalClass.Instance.Attendance_SMS_FailedAddAsync(failedSmsList);
                }

                //Update the total send and fail status

                GlobalClass.Instance.SmsSender.TotalSmsSend += smsList.Count;
                GlobalClass.Instance.SmsSender.TotalSmsFailed += failedSmsList.Count;

                // Perform your desired action here
                GlobalClass.Instance.SmsSender.TotalEventCall++;
                ShowAppInfo();
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void ShowAppInfo()
        {
            try
            {
                if (SmsSender != null && txtStatus != null)
                {
                    txtStatus.Text = $"App Started at {SmsSender.AppStartTime.ToString("dd MMM, yyyy (hh:mm tt)")}, total event called: {SmsSender.TotalEventCall}, SMS send: {SmsSender.TotalSmsSend} & SMS Failed: {SmsSender.TotalSmsFailed}";
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to update status");
            }
        }

        private void SetStartup()
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                Assembly curAssembly = Assembly.GetExecutingAssembly();
                key.SetValue(curAssembly.GetName().Name, curAssembly.Location);
            }
            catch { }
        }
    }
}
