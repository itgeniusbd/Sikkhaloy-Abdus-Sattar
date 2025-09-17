using AttendanceDevice.APIClass;
using AttendanceDevice.Config_Class;
using AttendanceDevice.Model;
using AttendanceDevice.Settings;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace AttendanceDevice
{
    /// <summary>
    /// Interaction logic for Login_Window.xaml
    /// </summary>
    public partial class Login_Window : Window
    {
        public Login_Window()
        {
            InitializeComponent();
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            var queue = new SnackbarMessageQueue(TimeSpan.FromSeconds(5));
            MessageSnackBar.MessageQueue = queue;

            try
            {
                //Empty Error
                LocalData.Current_Error = new Setting_Error();

                LoadingPb.IsIndeterminate = true;
                LoginButton.IsEnabled = false;

                if (string.IsNullOrEmpty(UserNameTextBox.Text) && string.IsNullOrEmpty(PasswordPasswordBox.Password))
                {
                    queue.Enqueue("Username and password is required");
                    LoadingPb.IsIndeterminate = false;
                    LoginButton.IsEnabled = true;
                    return;
                }

                //Check Internet connection
                if (await ApiUrl.IsNoNetConnection())
                {
                    queue.Enqueue("No internet connection");
                    LoadingPb.IsIndeterminate = false;
                    LoginButton.IsEnabled = true;
                    return;
                }

                //Check Server connection
                if (await ApiUrl.IsServerUnavailable())
                {
                    queue.Enqueue("The server is not available right now");
                    LoadingPb.IsIndeterminate = false;
                    LoginButton.IsEnabled = true;
                    return;
                }

                //get user token
                var client = new RestClient(ApiUrl.EndPoint);
                var loginRequest = new RestRequest("token", Method.POST);
                var loginUsers = new LoginUser(UserNameTextBox.Text.Trim(), PasswordPasswordBox.Password);

                loginRequest.AddObject(loginUsers);

                //Login execute the request
                var loginResponse = await client.ExecuteTaskAsync<Token>(loginRequest);

                //API call for token
                if (loginResponse.StatusCode != HttpStatusCode.OK)
                {
                    queue.Enqueue(loginResponse.Data?.error_description ?? "Response not found");
                    LoadingPb.IsIndeterminate = false;
                    LoginButton.IsEnabled = true;
                    return;
                }

                //get institution info
                var token = loginResponse.Data.access_token;
                var schoolRequest = new RestRequest("api/school/{id}", Method.GET);

                schoolRequest.AddUrlSegment("id", UserNameTextBox.Text.Trim());
                schoolRequest.AddHeader("Authorization", "Bearer " + token);

                //School info execute the request
                var schoolResponse = await client.ExecuteTaskAsync(schoolRequest);

                if (schoolResponse.StatusCode != HttpStatusCode.OK)
                {
                    queue.Enqueue(schoolResponse.StatusDescription);
                    LoadingPb.IsIndeterminate = false;
                    LoginButton.IsEnabled = true;
                    return;
                }

                var schoolInfo = JsonConvert.DeserializeObject<Institution>(schoolResponse.Content);

                if (schoolInfo == null)
                {
                    queue.Enqueue("School information not found");
                    LoadingPb.IsIndeterminate = false;
                    LoginButton.IsEnabled = true;
                    return;
                }


                //Institution Deactivate By Authority
                if (!schoolInfo.IsValid)
                {
                    queue.Enqueue("Institution Deactivate By Authority!");
                    LoadingPb.IsIndeterminate = false;
                    LoginButton.IsEnabled = true;
                    return;
                }

                var ins = LocalData.Instance.institution;


                var isFirstSetup = ins == null;
                if (isFirstSetup)
                {
                    ins = schoolInfo;
                    ins.UserName = schoolInfo.UserName.Trim();
                    ins.Token = token.Trim();
                    ins.Password = PasswordPasswordBox.Password;

                    ins.PingTimeOut = 100; // default Value for device ping 
                }
                else
                {
                    ins.Token = token;
                    ins.UserName = schoolInfo.UserName.Trim();
                    ins.Password = PasswordPasswordBox.Password;
                    ins.IsValid = schoolInfo.IsValid;
                    ins.SettingKey = schoolInfo.SettingKey.Trim();
                    ins.Is_Device_Attendance_Enable = schoolInfo.Is_Device_Attendance_Enable;
                    ins.Is_Employee_Attendance_Enable = schoolInfo.Is_Employee_Attendance_Enable;
                    ins.Is_Student_Attendance_Enable = schoolInfo.Is_Student_Attendance_Enable;
                    ins.Is_Today_Holiday = schoolInfo.Is_Today_Holiday;
                    ins.Holiday_NotActive = schoolInfo.Holiday_NotActive;

                }
                await LocalData.Instance.InstitutionUpdate(ins);


                //No user in local database
                if (!LocalData.Instance.IsUserExist())
                {
                    LocalData.Current_Error.Message = "No User Found on PC!";
                    LocalData.Current_Error.Type = Error_Type.UserInfoPage;

                    var setting = new Setting();
                    setting.Show();
                    this.Close();
                    return;
                }


                //Leave request
                #region Leave data

                var leaveRequest = new RestRequest("api/Users/{id}/leave", Method.GET);
                leaveRequest.AddUrlSegment("id", ins.SchoolID);
                leaveRequest.AddHeader("Authorization", "Bearer " + token);

                //Leave execute the request
                var leaveResponse = await client.ExecuteTaskAsync<List<User_Leave_Record>>(leaveRequest);

                if (leaveResponse.StatusCode == HttpStatusCode.OK && leaveResponse.Data != null)
                {
                    await LocalData.Instance.LeaveDataHandling(leaveResponse.Data);
                }

                #endregion Leave data

                //Schedule Day Request
                #region Schedule data

                var scheduleDayRequest = new RestRequest("api/Users/{id}/schedule", Method.GET);
                scheduleDayRequest.AddUrlSegment("id", ins.SchoolID);
                scheduleDayRequest.AddHeader("Authorization", "Bearer " + token);

                var scheduleDayResponse =
                    await client.ExecuteTaskAsync<List<Attendance_Schedule_Day>>(scheduleDayRequest);

                if (scheduleDayResponse.StatusCode == HttpStatusCode.OK && scheduleDayResponse.Data != null && scheduleDayResponse.Data.Any())
                {
                    var isNoScheduleUserExist = await LocalData.Instance.ScheduleDataHandling(scheduleDayResponse.Data);
                    if (isNoScheduleUserExist)
                    {
                        LocalData.Current_Error.Message = "Not all User assigned in the schedule on PC, Update User from server!";
                        LocalData.Current_Error.Type = Error_Type.UserInfoPage;

                        var setting = new Setting();
                        setting.Show();
                        this.Close();
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

                //check device added or not
                if (!LocalData.Instance.IsDeviceExist())
                {
                    LocalData.Current_Error.Message = "No Device Added In PC!";
                    LocalData.Current_Error.Type = Error_Type.DeviceInfoPage;

                    var setting = new Setting();
                    setting.Show();
                    this.Close();
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
                    LocalData.Current_Error.Message = "Device IP Not Found";
                    LocalData.Current_Error.Type = Error_Type.DeviceInfoPage;

                    var setting = new Setting();
                    setting.Show();
                    this.Close();
                    return;
                }

                //set api date in local pc
                var serverDatetime = schoolInfo.Current_Datetime;
                //check pc date time
                if (!(serverDatetime.AddMinutes(1) > DateTime.Now && serverDatetime.AddMinutes(-1) < DateTime.Now))
                {
                    var errorObj = new Error("Invalid", "Invalid PC Date Time. \n Server Time: " + serverDatetime.ToString("d MMM yy (hh:mm tt)"));
                    var errorWindow = new Error_Window(errorObj);
                    errorWindow.Show();
                    this.Close();
                    return;
                }

                //try connection to device successfully
                #region Device data send to server
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


                    //Get Today attendance data form server
                    var todayAttendanceRequest = new RestRequest("api/Attendance/{id}/GetTodayAttendance", Method.GET);
                    todayAttendanceRequest.AddUrlSegment("id", ins.SchoolID);
                    todayAttendanceRequest.AddHeader("Authorization", "Bearer " + token);

                    var todayAttendanceResponse =
                        await client.ExecuteTaskAsync<List<Attendance_Record>>(todayAttendanceRequest);

                    if (todayAttendanceResponse.StatusCode == HttpStatusCode.OK && todayAttendanceResponse.Data != null &&
                        todayAttendanceResponse.Data.Any())
                    {
                        await LocalData.Instance.GetTodayAttendanceRecords(todayAttendanceResponse.Data);
                    }

                    //log to attendance
                    var prevLog = device.DownloadPrevLogs();
                    var todayLog = device.DownloadTodayLogs();


                    await Machine.SaveLogsOrAttendanceInPc(prevLog, todayLog, ins, device.Device);
                }
                #endregion Device data send to server

                if (!isDeviceConnected)
                {
                    LocalData.Current_Error.Message = "Device Unable to Connect";
                    LocalData.Current_Error.Type = Error_Type.DeviceInfoPage;

                    var setting = new Setting();
                    setting.Show();
                    this.Close();
                    return;
                }

                if (!isFirstSetup)
                {
                    //Get any update notification from server
                    var updateNotificationRequest = new RestRequest("api/Users/{id}/updateInfo", Method.GET);
                    updateNotificationRequest.AddUrlSegment("id", ins.SchoolID);
                    updateNotificationRequest.AddHeader("Authorization", "Bearer " + token);

                    var updateNotificationResponse =
                        await client.ExecuteTaskAsync<List<DataUpdateList>>(updateNotificationRequest)
                            .ConfigureAwait(false);

                    if (updateNotificationResponse.StatusCode == HttpStatusCode.OK &&
                        updateNotificationResponse.Data != null &&
                        updateNotificationResponse.Data.Any())
                    {
                        await LocalData.Instance.AddNotifications(updateNotificationResponse.Data)
                            .ConfigureAwait(false);

                        var setting = new Setting();
                        setting.Show();
                        this.Close();
                        return;

                    }
                }

                //show display
                var initDevice = new DeviceDisplay(deviceConnections);
                var display = new DisplayWindow(initDevice);
                display.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                queue.Enqueue(ex.Message + " Inner ex: " + ex.InnerException.Message);
                LoadingPb.IsIndeterminate = false;
                LoginButton.IsEnabled = true;
            }
        }

        private void Login_Window_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(LocalData.Current_Error.Message)) return;

            MessageSnackBar.Message.Content = LocalData.Current_Error.Message;
            MessageSnackBar.IsActive = true;
        }
    }
}



