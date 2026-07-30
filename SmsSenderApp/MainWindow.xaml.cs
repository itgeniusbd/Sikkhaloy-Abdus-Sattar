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
using System.Windows.Controls;

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

                // Initialize UI with default values immediately
                InitializeUI();

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

                // Ensure minimum interval is 1 minute to prevent infinite loop
                var intervalMinutes = GlobalClass.Instance.Setting.SmsSendInterval;
                if (intervalMinutes < 1)
                {
                    intervalMinutes = 5; // Default to 5 minutes if setting is invalid
                    Log.Warning($"Invalid SmsSendInterval ({GlobalClass.Instance.Setting.SmsSendInterval}), using default 5 minutes");
                }

                timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMinutes(intervalMinutes)
                };

                timer.Tick += Timer_Tick;
                timer.Start();

                GlobalClass.Instance.SenderInsert();
                SmsSender = GlobalClass.Instance.SmsSender;

                if (SmsSender == null)
                {
                    SmsSender = new Attendance_SMS_Sender
                    {
                        AppStartTime = DateTime.Now,
                        TotalEventCall = 0,
                        TotalSmsSend = 0,
                        TotalSmsFailed = 0
                    };
                    Log.Warning("SmsSender initialization failed, using temporary instance");
                }
                else
                {
                    Log.Information($"SmsSender initialized - ID: {SmsSender.AttendanceSmsSenderId}, Start Time: {SmsSender.AppStartTime}");
                }

                // Update UI with loaded data
                UpdateUIWithData();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Failed to initialize MainWindow");
                MessageBox.Show($"Failed to initialize application: {ex.Message}",
                  "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeUI()
        {
            try
            {
                Log.Information("InitializeUI started");
                
                if (txtAppStartTime == null)
                {
                    Log.Error("txtAppStartTime is NULL in InitializeUI!");
                    return;
                }
                if (txtEventCount == null)
                {
                    Log.Error("txtEventCount is NULL in InitializeUI!");
                    return;
                }
                if (txtSmsSent == null)
                {
                    Log.Error("txtSmsSent is NULL in InitializeUI!");
                    return;
                }
                if (txtSmsFailed == null)
                {
                    Log.Error("txtSmsFailed is NULL in InitializeUI!");
                    return;
                }
                
                txtAppStartTime.Text = "TESTING 123";
                txtEventCount.Text = "999";
                txtSmsSent.Text = "888";
                txtSmsFailed.Text = "777";
                
                Log.Information($"InitializeUI completed - txtEventCount.Text = '{txtEventCount.Text}'");
                Log.Information($"InitializeUI completed - txtEventCount.Foreground = '{txtEventCount.Foreground}'");
                Log.Information($"InitializeUI completed - txtEventCount.FontSize = '{txtEventCount.FontSize}'");
                Log.Information($"InitializeUI completed - txtEventCount.Visibility = '{txtEventCount.Visibility}'");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to initialize UI");
            }
        }

        private void UpdateUIWithData()
        {
            try
            {
                if (SmsSender == null)
                {
                    Log.Warning("SmsSender is null in UpdateUIWithData");
                    return;
                }

                txtAppStartTime.Text = $"App Started at {SmsSender.AppStartTime:dd MMM, yyyy (hh:mm tt)}";
                txtEventCount.Text = SmsSender.TotalEventCall.ToString();
                txtSmsSent.Text = SmsSender.TotalSmsSend.ToString();
                txtSmsFailed.Text = SmsSender.TotalSmsFailed.ToString();

                Log.Information($"UI updated - Event: {SmsSender.TotalEventCall}, Sent: {SmsSender.TotalSmsSend}, Failed: {SmsSender.TotalSmsFailed}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to update UI with data");
            }
        }

        private async void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                // Stop timer during processing to prevent overlapping calls
                timer.Stop();

                GlobalClass.Instance.ReloadSettings();

                var intervalMinutes = GlobalClass.Instance.Setting?.SmsSendInterval ?? 2;
                if (intervalMinutes < 1)
                    intervalMinutes = 2;

                if (timer.Interval != TimeSpan.FromMinutes(intervalMinutes))
                {
                    timer.Interval = TimeSpan.FromMinutes(intervalMinutes);
                    Log.Information($"SMS send interval updated to {intervalMinutes} minute(s)");
                }

                Log.Information("Timer tick started - checking for SMS to send (Provider: {Provider}, Multiple: {ProviderMultiple})",
                    GlobalClass.Instance.Setting?.SmsProvider,
                    GlobalClass.Instance.Setting?.SmsProviderMultiple);

                var todayDate = DateTime.Today;

                var failedSmsList = new List<Attendance_SMS_Failed>();
                var smsList = new List<Attendance_SMS>();

                var currentTime = DateTime.Now.TimeOfDay;

                // Read queue first; delete only after send/failed is logged.
                var totalSmsList = await GlobalClass.Instance.GetAttendanceSmsListAsync();

                Log.Information($"Retrieved {totalSmsList.Count} SMS from database");

                //Get the To-days SMS List
                var smsListOfToDay = totalSmsList
                    .Where(s => s.AttendanceDate.Date == todayDate)
                    .ToList();

                Log.Information($"Found {smsListOfToDay.Count} SMS for today");

                if (totalSmsList.Count > 0 && smsListOfToDay.Count == 0)
                {
                    Log.Warning(
                        "All {Count} queued SMS were treated as non-today. Sample AttendanceDate={SampleDate}",
                        totalSmsList.Count,
                        totalSmsList[0].AttendanceDate.ToString("yyyy-MM-dd HH:mm:ss"));
                }

                //Get the other days SMS List
                var smsListOfOtherDay = totalSmsList
                    .Where(s => s.AttendanceDate.Date != todayDate)
                    .Select(s => ToFailedRecord(s, "Not current date"))
                    .ToList();

                failedSmsList.AddRange(smsListOfOtherDay);

                if (smsListOfOtherDay.Any())
                {
                    Log.Information($"Marked {smsListOfOtherDay.Count} SMS as failed (not current date)");
                }

                if (smsListOfToDay.Any())
                {
                    //Get the List of time-up SMS
                    var timeupSmsList = smsListOfToDay.Where(s =>
                            s.ScheduleTime.TotalMinutes + s.SMS_TimeOut <= currentTime.TotalMinutes)
                        .Select(s => ToFailedRecord(s, "SMS sending time up"))
                        .ToList();

                    failedSmsList.AddRange(timeupSmsList);

                    if (timeupSmsList.Any())
                    {
                        Log.Information($"Marked {timeupSmsList.Count} SMS as failed (time up)");
                    }

                    //Get the SMS List of send-able SMS
                    smsList = smsListOfToDay
                       .Where(s => s.ScheduleTime.TotalMinutes + s.SMS_TimeOut > currentTime.TotalMinutes).ToList();

                    Log.Information($"Found {smsList.Count} sendable SMS (within time window)");

                    if (smsList.Any())
                    {
                        //get the School Ids
                        var schoolIds = smsList.Select(s => s.SchoolID).Distinct().ToList();
                        Log.Information($"Checking SMS balance for {schoolIds.Count} schools");

                        //get the SMS Balance
                        var noSmsBalanceSchoolIds = await GlobalClass.Instance.NoSmsBalanceSchoolIdsAsync(schoolIds);
                        
                        if (noSmsBalanceSchoolIds.Any())
                        {
                            Log.Warning($"Found {noSmsBalanceSchoolIds.Count} schools with insufficient balance");
                        }

                        //Get the smsList of School which have available balance
                        var noBalanceSmsList = new List<Attendance_SMS_Failed>();
                        if (noSmsBalanceSchoolIds.Any())
                        {
                            noBalanceSmsList = smsList.Where(s => noSmsBalanceSchoolIds.Contains(s.SchoolID)).Select(
                                s => ToFailedRecord(s, "Insufficient SMS Balance")).ToList();

                            smsList = smsList.Where(s => !noSmsBalanceSchoolIds.Contains(s.SchoolID)).ToList();

                            Log.Information($"Marked {noBalanceSmsList.Count} SMS as failed (insufficient balance)");
                        }

                        failedSmsList.AddRange(noBalanceSmsList);

                        var sendableBeforeDedup = smsList.Count;
                        smsList = await GlobalClass.Instance.FilterAlreadySentAsync(smsList);
                        if (smsList.Count < sendableBeforeDedup)
                        {
                            Log.Information(
                                "Skipped {Count} duplicate SMS already sent today",
                                sendableBeforeDedup - smsList.Count);
                        }

                        //Send the smsList
                        if (smsList.Any())
                        {
                            Log.Information($"Attempting to send {smsList.Count} SMS");

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
                            
                            Log.Information($"SMS send result: {isSend.Validation}, Message: {isSend.Message}");

                            if (isSend.Validation)
                            {
                                await GlobalClass.Instance.SMS_OtherInfoAddAsync(smsRecords);
                                Log.Information($"Successfully sent {smsList.Count} SMS");
                            }
                            else
                            {
                                var smsSendFail = smsList.Select(s => ToFailedRecord(s, "SMS Send Failed")).ToList();

                                failedSmsList.AddRange(smsSendFail);

                                Log.Warning($"Failed to send {smsList.Count} SMS");

                                smsList.Clear();
                            }

                        }
                        else
                        {
                            Log.Information("No SMS to send after balance check");
                        }
                    }
                }
                else
                {
                    Log.Information("No SMS found for today");
                }

                //insert the fail SMS table
                var failedLogged = true;
                if (failedSmsList.Any())
                {
                    failedLogged = await GlobalClass.Instance.Attendance_SMS_FailedAddAsync(failedSmsList);
                    if (failedLogged)
                        Log.Information($"Recorded {failedSmsList.Count} failed SMS");
                    else
                        Log.Error("Failed to record {Count} failed SMS; queue rows will be kept", failedSmsList.Count);
                }

                if (totalSmsList.Any() && failedLogged)
                {
                    await GlobalClass.Instance.RemoveAttendanceSmsFromDbAsync(totalSmsList);
                }

                //Update the total send and fail status

                GlobalClass.Instance.SmsSender.TotalSmsSend += smsList.Count;
                GlobalClass.Instance.SmsSender.TotalSmsFailed += failedSmsList.Count;
                GlobalClass.Instance.SmsSender.TotalEventCall++;
                
                Log.Information($"Before SenderUpdate - Event: {GlobalClass.Instance.SmsSender.TotalEventCall}, Sent: {GlobalClass.Instance.SmsSender.TotalSmsSend}, Failed: {GlobalClass.Instance.SmsSender.TotalSmsFailed}");
                
                // Update in database
                GlobalClass.Instance.SenderUpdate();
                
                Log.Information($"After SenderUpdate - Event: {GlobalClass.Instance.SmsSender.TotalEventCall}, Sent: {GlobalClass.Instance.SmsSender.TotalSmsSend}, Failed: {GlobalClass.Instance.SmsSender.TotalSmsFailed}");
                
                ShowAppInfo();
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }
            finally
            {
                // Always restart timer, even if there was an error
                timer.Start();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Information("Window loaded, refreshing UI");
                
                // Refresh UI with current data
                UpdateUIWithData();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in Window_Loaded");
            }
        }

        private void ShowAppInfo()
        {
            try
            {
                // Check if we're on UI thread
                if (!Dispatcher.CheckAccess())
                {
                    Dispatcher.Invoke(() => ShowAppInfo());
                    return;
                }

                UpdateUIWithData();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to update status");
            }
        }

        private static Attendance_SMS_Failed ToFailedRecord(Attendance_SMS source, string reason)
        {
            return new Attendance_SMS_Failed
            {
                SchoolID = source.SchoolID,
                ScheduleTime = source.ScheduleTime,
                CreateTime = source.CreateTime,
                SentTime = source.SentTime,
                AttendanceDate = source.AttendanceDate.Date,
                SMS_Text = source.SMS_Text,
                MobileNo = source.MobileNo,
                AttendanceStatus = source.AttendanceStatus,
                SMS_TimeOut = source.SMS_TimeOut,
                EmployeeID = source.EmployeeID,
                StudentID = source.StudentID,
                FailedReson = reason,
                InsertDate = DateTime.Now
            };
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
