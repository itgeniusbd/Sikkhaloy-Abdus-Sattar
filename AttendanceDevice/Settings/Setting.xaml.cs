using AttendanceDevice.Config_Class;
using AttendanceDevice.Model;
using AttendanceDevice.Settings.Pages;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Navigation;

namespace AttendanceDevice.Settings
{
    /// <summary>
    /// Interaction logic for Setting.xaml
    /// </summary>
    public partial class Setting : Window
    {
        public Setting()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (LocalData.Current_Error.Type == Error_Type.DeviceInfoPage)
            {
                var deviceInfoPage = new DeviceInfoPage();
                FrameSetting.Content = deviceInfoPage;
            }
            else if (LocalData.Current_Error.Type == Error_Type.UserInfoPage)
            {
                var user = new UserInfoPage();
                FrameSetting.Content = user;
            }
            else
            {
                var deshboardPage = new Deshboard(this);
                FrameSetting.Content = deshboardPage;
            }
        }
        //humburger menu
        private void ButtonManuClose_Click(object sender, RoutedEventArgs e)
        {
            ButtonManuClose.Visibility = Visibility.Collapsed;
            ButtonManuOpen.Visibility = Visibility.Visible;
        }
        private void ButtonManuOpen_Click(object sender, RoutedEventArgs e)
        {
            ButtonManuClose.Visibility = Visibility.Visible;
            ButtonManuOpen.Visibility = Visibility.Collapsed;
        }

        //web page link
        private void Sikkhaloy_Button_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://sikkhaloy.com");
        }
        private void Loopsit_Button_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://loopsit.com");
        }
        private void Sikkhaloy_Facebook_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.facebook.com/Sikkhaloy24/");
        }

        //menu link
        private void FrameSetting_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            var da = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(TimeSpan.FromMilliseconds(900))
            };
            FrameSetting.BeginAnimation(OpacityProperty, da);
        }
        private async void DisplayButton_Click(object sender, RoutedEventArgs e)
        {
            PB.IsIndeterminate = true;
            btnDisplay.IsEnabled = false;

            //No user in local database
            if (!LocalData.Instance.IsUserExist())
            {
                btnDisplay.IsEnabled = true;
                PB.IsIndeterminate = false;

                LocalData.Current_Error.Message = "No User Found on PC!";
                LocalData.Current_Error.Type = Error_Type.UserInfoPage;

                var user = new UserInfoPage();
                FrameSetting.Content = user;
                return;
            }

            //check device added or not
            if (!LocalData.Instance.IsDeviceExist())
            {
                btnDisplay.IsEnabled = true;
                PB.IsIndeterminate = false;

                LocalData.Current_Error.Message = "No Device Added In PC!";
                LocalData.Current_Error.Type = Error_Type.DeviceInfoPage;

                var deviceInfoPage = new DeviceInfoPage();
                FrameSetting.Content = deviceInfoPage;
                return;
            }


            //create all device list
            var deviceList = await LocalData.Instance.DeviceListAsync();
            var deviceConnections = new List<DeviceConnection>();

            foreach (var device in deviceList)
            {
                var checkIp = await Device_PingTest.PingHostAsync(device.DeviceIP);
                if (checkIp)
                {
                    deviceConnections.Add(new DeviceConnection(device));
                }
            }

            //check device ip
            if (!deviceConnections.Any())
            {

                btnDisplay.IsEnabled = true;
                PB.IsIndeterminate = false;

                LocalData.Current_Error.Message = "Device IP Not Found";
                LocalData.Current_Error.Type = Error_Type.DeviceInfoPage;

                var deviceInfoPage = new DeviceInfoPage();
                FrameSetting.Content = deviceInfoPage;
                return;
            }

            var ins = LocalData.Instance.institution;
            //Update Local PC information if date not same
            //if (Convert.ToDateTime(ins.LastUpdateDate) != DateTime.Today)
            {
                var client = new RestClient(ApiUrl.EndPoint);
                //get institution info
                var schoolRequest = new RestRequest("api/school/{id}", Method.GET);

                schoolRequest.AddUrlSegment("id", ins.UserName);
                schoolRequest.AddHeader("Authorization", "Bearer " + ins.Token);

                //School info execute the request
                var schoolResponse = await client.ExecuteTaskAsync(schoolRequest);

                if (schoolResponse.StatusCode != HttpStatusCode.OK)
                {
                    LocalData.Current_Error.Message = schoolResponse.StatusDescription;
                    var login = new Login_Window();
                    login.Show();
                    this.Close();
                    return;
                }

                var schoolInfo = JsonConvert.DeserializeObject<Institution>(schoolResponse.Content);

                if (schoolInfo == null)
                {
                    LocalData.Current_Error.Message = "Institution Information Not Found in Server!";
                    var login = new Login_Window();
                    login.Show();
                    this.Close();
                    return;
                }

                //Institution Deactivate By Authority
                if (!schoolInfo.IsValid)
                {
                    LocalData.Current_Error.Message = "Institution Deactivate By Authority!";
                    var login = new Login_Window();
                    login.Show();
                    this.Close();
                    return;
                }

                var serverDatetime = schoolInfo.Current_Datetime;
                //check pc date time
                if (!(serverDatetime.AddMinutes(1) > DateTime.Now && serverDatetime.AddMinutes(-1) < DateTime.Now))
                {
                    var errorObj = new Error("Invalid",
                        "Invalid PC Date Time. \n Server Time: " + serverDatetime.ToString("d MMM yy (hh:mm tt)"));
                    var errorWindow = new Error_Window(errorObj);
                    errorWindow.Show();
                    this.Close();
                    return;
                }

                //Update Institution Information
                ins.IsValid = schoolInfo.IsValid;
                ins.SettingKey = schoolInfo.SettingKey.Trim();
                ins.Is_Device_Attendance_Enable = schoolInfo.Is_Device_Attendance_Enable;
                ins.Is_Employee_Attendance_Enable = schoolInfo.Is_Employee_Attendance_Enable;
                ins.Is_Student_Attendance_Enable = schoolInfo.Is_Student_Attendance_Enable;
                ins.Is_Today_Holiday = schoolInfo.Is_Today_Holiday;
                ins.Holiday_NotActive = schoolInfo.Holiday_NotActive;

                await LocalData.Instance.InstitutionUpdate(ins);

                //Leave request

                #region Leave request

                var leaveRequest = new RestRequest("api/Users/{id}/leave", Method.GET);
                leaveRequest.AddUrlSegment("id", ins.SchoolID);
                leaveRequest.AddHeader("Authorization", "Bearer " + ins.Token);
                //Leave execute the request
                var leaveResponse = await client.ExecuteTaskAsync<List<User_Leave_Record>>(leaveRequest);

                if (leaveResponse.StatusCode == HttpStatusCode.OK && leaveResponse.Data != null)
                {
                    await LocalData.Instance.LeaveDataHandling(leaveResponse.Data);
                }
                else
                {
                    var errorObj = new Error("Api Leave Error", leaveResponse.ErrorMessage);
                    var errorWindow = new Error_Window(errorObj);
                    errorWindow.Show();
                    this.Close();
                    return;
                }

                #endregion Leave request

                //Schedule Day Request

                #region Schedule data

                var scheduleDayRequest = new RestRequest("api/Users/{id}/schedule", Method.GET);
                scheduleDayRequest.AddUrlSegment("id", ins.SchoolID);
                scheduleDayRequest.AddHeader("Authorization", "Bearer " + ins.Token);

                var scheduleDayResponse =
                    await client.ExecuteTaskAsync<List<Attendance_Schedule_Day>>(scheduleDayRequest);

                if (scheduleDayResponse.StatusCode == HttpStatusCode.OK && scheduleDayResponse.Data != null &&
                    scheduleDayResponse.Data.Any())
                {
                    var isNoScheduleUserExist = await LocalData.Instance.ScheduleDataHandling(scheduleDayResponse.Data);
                    if (isNoScheduleUserExist)
                    {
                        LocalData.Current_Error.Message =
                            "Not all User assigned in the schedule on PC, Update User from server!";
                        LocalData.Current_Error.Type = Error_Type.UserInfoPage;


                        PB.IsIndeterminate = false;
                        btnDisplay.IsEnabled = true;

                        var user = new UserInfoPage();
                        FrameSetting.Content = user;
                        return;
                    }
                }
                else
                {
                    throw new InvalidDataException("No Schedule data found in Server");
                }

                #endregion Schedule data

                //Update Local PC information update time
                ins.LastUpdateDate = schoolInfo.LastUpdateDate;
                await LocalData.Instance.InstitutionUpdate(ins);


                //Get Today attendance data form server
                var todayAttendanceRequest = new RestRequest("api/Attendance/{id}/GetTodayAttendance", Method.GET);
                todayAttendanceRequest.AddUrlSegment("id", ins.SchoolID);
                todayAttendanceRequest.AddHeader("Authorization", "Bearer " + ins.Token);

                var todayAttendanceResponse =
                    await client.ExecuteTaskAsync<List<Attendance_Record>>(todayAttendanceRequest);

                if (todayAttendanceResponse.StatusCode == HttpStatusCode.OK && todayAttendanceResponse.Data != null &&
                    todayAttendanceResponse.Data.Any())
                {
                    await LocalData.Instance.GetTodayAttendanceRecords(todayAttendanceResponse.Data);
                }
            }

            //try connection to device successfully
            var isDeviceConnected = false;
            foreach (var device in deviceConnections)
            {
                var status = await Task.Run(() => device.ConnectDevice());
                if (!status.IsSuccess) continue;

                isDeviceConnected = true;

                var deviceTime = device.GetDateTime();
                if (deviceTime.ToString("dd-MM-yyyy hh:mm tt") != DateTime.Now.ToString("dd-MM-yyyy hh:mm tt"))
                {
                    //Set server time to device
                    await Task.Run(() => device.SetDateTime());
                }

                var prevLog = device.DownloadPrevLogs();
                var todayLog = device.DownloadTodayLogs();

                await Machine.SaveLogsOrAttendanceInPc(prevLog, todayLog, LocalData.Instance.institution, device.Device);
            }

            if (!isDeviceConnected)
            {
                btnDisplay.IsEnabled = true;
                PB.IsIndeterminate = false;

                LocalData.Current_Error.Message = "Device Unable to Connect";
                LocalData.Current_Error.Type = Error_Type.DeviceInfoPage;

                var deviceInfoPage = new DeviceInfoPage();
                FrameSetting.Content = deviceInfoPage;
                return;
            }

            var initDevice = new DeviceDisplay(deviceConnections);

            //Check Internet connection || Server connection
            if (await ApiUrl.IsNoNetConnection() || await ApiUrl.IsServerUnavailable())
            {
                //show  Offline display window
                var noInternetWindow = new No_Internet_Window(initDevice);
                noInternetWindow.Show();
                this.Close();
                return;
            }

            PB.IsIndeterminate = false;
            btnDisplay.IsEnabled = true;

            //Need to send data to server 

            var displayWindow = new DisplayWindow(initDevice);
            displayWindow.Show();
            this.Close();

        }
        private void UserButton_Click(object sender, RoutedEventArgs e)
        {
            var user = new UserInfoPage();
            FrameSetting.Content = user;
        }
        private void InstitutionInfo_Button_Click(object sender, RoutedEventArgs e)
        {
            var institution = new Institution_InfoPage();
            FrameSetting.Content = institution;
        }
        private void Device_Button_Click(object sender, RoutedEventArgs e)
        {
            var deviceInfoPage = new DeviceInfoPage();
            FrameSetting.Content = deviceInfoPage;
        }
        private void BtnSchedule_Click(object sender, RoutedEventArgs e)
        {
            var schedulePage = new SchedulePage();
            FrameSetting.Content = schedulePage;
        }
        private void AttendanceLog_Button_Click(object sender, RoutedEventArgs e)
        {
            var log = new Attendance_LogPage();
            FrameSetting.Content = log;
        }
        private void BackUpData_Button_Click(object sender, RoutedEventArgs e)
        {
            var backUp = new BackUp_LogsPage();
            FrameSetting.Content = backUp;
        }

        private void Dashboard_Button_Click(object sender, RoutedEventArgs e)
        {
            var dashboard = new Deshboard(this);
            FrameSetting.Content = dashboard;
        }

        private void FingerPrint_Button_Click(object sender, RoutedEventArgs e)
        {
            var fingerPrint = new FingerPrint_Page();
            FrameSetting.Content = fingerPrint;
        }

        private void LeaveUser_Button_Click(object sender, RoutedEventArgs e)
        {
            var leaveUser = new LeaveUser_Page();
            FrameSetting.Content = leaveUser;
        }
    }
}
