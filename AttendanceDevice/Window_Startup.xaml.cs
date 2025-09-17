using AttendanceDevice.APIClass;
using AttendanceDevice.Config_Class;
using AttendanceDevice.Model;
using AttendanceDevice.Settings;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AttendanceDevice
{
    public partial class Window_Startup : Window
    {
        public Window_Startup()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //Empty Error
                LocalData.Current_Error = new Setting_Error();


                // Institution not Register in local database
                var ins = LocalData.Instance.institution;

                if (ins == null)
                {
                    LocalData.Current_Error.Message = "No institutional information found in your local Machine";
                    var login = new Login_Window();
                    login.Show();
                    this.Close();
                    return;
                }

                //check institution is valid
                if (!ins.IsValid)
                {
                    LocalData.Current_Error.Message =
                        $"{ins.InstitutionName} has currently deactivated by the Software Authority";
                    var login = new Login_Window();
                    login.Show();
                    this.Close();
                    return;
                }

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

                //try connection to device successfully & Device data send to server
                var isDeviceConnected = false;
                foreach (var device in deviceConnections)
                {
                    var status = await Task.Run(() => device.ConnectDevice());
                    if (!status.IsSuccess) continue;

                    isDeviceConnected = true;
                }


                if (!isDeviceConnected)
                {
                    LocalData.Current_Error.Message = "Device Unable to Connect";
                    LocalData.Current_Error.Type = Error_Type.DeviceInfoPage;

                    var setting = new Setting();
                    setting.Show();
                    this.Close();
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




                //Date update to Local Machine 


                //get user token
                var client = new RestClient(ApiUrl.EndPoint);
                var loginRequest = new RestRequest("token", Method.POST);
                var loginUsers = new LoginUser(ins.UserName, ins.Password);

                loginRequest.AddObject(loginUsers);

                //Login execute the request
                var loginResponse = await client.ExecuteTaskAsync<Token>(loginRequest);

                //API call for token
                if (loginResponse.StatusCode != HttpStatusCode.OK)
                {
                    //Invalid username and password
                    LocalData.Current_Error.Message = loginResponse.Data?.error_description ?? "Response not found";
                    var login = new Login_Window();
                    login.Show();
                    this.Close();
                    return;
                }

                //Get Token
                var token = loginResponse.Data.access_token;


                //get institution info
                var schoolRequest = new RestRequest("api/school/{id}", Method.GET);

                schoolRequest.AddUrlSegment("id", ins.UserName);
                schoolRequest.AddHeader("Authorization", "Bearer " + token);


                //School info execute the request
                var schoolResponse = await client.ExecuteTaskAsync(schoolRequest); //response.data not work because of logo image data

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
                ins.Token = token.Trim();
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
                leaveRequest.AddHeader("Authorization", "Bearer " + token);
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
                scheduleDayRequest.AddHeader("Authorization", "Bearer " + token);

                var scheduleDayResponse =
                    await client.ExecuteTaskAsync<List<Attendance_Schedule_Day>>(scheduleDayRequest);

                if (scheduleDayResponse.StatusCode == HttpStatusCode.OK && scheduleDayResponse.Data != null &&
                    scheduleDayResponse.Data.Any())
                {
                    var isNoScheduleUserExist =
                        await LocalData.Instance.ScheduleDataHandling(scheduleDayResponse.Data);
                    if (isNoScheduleUserExist)
                    {
                        LocalData.Current_Error.Message =
                            "Not all User assigned in the schedule on PC, Update User from server!";
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


                //Device data send to server
                #region Device data send to server

                foreach (var device in deviceConnections)
                {
                    var status = await Task.Run(() => device.ConnectDevice());
                    if (!status.IsSuccess) continue;

                    var deviceTime = device.GetDateTime();
                    if (deviceTime.ToString("dd-MM-yyyy hh:mm tt") != DateTime.Now.ToString("dd-MM-yyyy hh:mm tt"))
                    {
                        //Set server time to device
                        await Task.Run(() => device.SetDateTime());
                    }


                    var todayLog = device.DownloadTodayLogs();
                    var prevLog = device.DownloadPrevLogs();

                    await Machine.SaveLogsOrAttendanceInPc(prevLog, todayLog, ins, device.Device);
                }

                #endregion Device data send to server



                //Get any update notification from server
                var updateNotificationRequest = new RestRequest("api/Users/{id}/updateInfo", Method.GET);
                updateNotificationRequest.AddUrlSegment("id", ins.SchoolID);
                updateNotificationRequest.AddHeader("Authorization", "Bearer " + token);

                var updateNotificationResponse =
                    await client.ExecuteTaskAsync<List<DataUpdateList>>(updateNotificationRequest);

                if (updateNotificationResponse.StatusCode == HttpStatusCode.OK && updateNotificationResponse.Data != null &&
                    updateNotificationResponse.Data.Any())
                {
                    await LocalData.Instance.AddNotifications(updateNotificationResponse.Data);

                    var setting = new Setting();
                    setting.Show();
                    this.Close();
                    return;

                }


                //show display
                var display = new DisplayWindow(initDevice);
                display.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                var errorObj = new Error("System Error", ex.Message);
                var errorWindow = new Error_Window(errorObj);
                errorWindow.Show();
                this.Close();
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}
