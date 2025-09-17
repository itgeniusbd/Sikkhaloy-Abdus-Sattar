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
using System.Windows.Threading;

namespace AttendanceDevice
{
    /// <summary>
    /// Interaction logic for No_Internet_Window.xaml
    /// </summary>
    public partial class No_Internet_Window : Window
    {
        private DispatcherTimer _tmr = new DispatcherTimer();
        private readonly DeviceDisplay _deviceDisplay;
        public No_Internet_Window(DeviceDisplay deviceDisplay)
        {
            _deviceDisplay = deviceDisplay;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Timer-setup
            _tmr.Interval = new TimeSpan(0, 2, 0);
            _tmr.Tick += Timer_Tick;
            _tmr.Start();
            this.Closing += Window_Closing;
        }


        private async void Timer_Tick(object sender, EventArgs e)
        {
            #region Check Internet
            if (await ApiUrl.IsNoNetConnection()) return;

            // Institution not Register in local database
            var ins = LocalData.Instance.institution;

            var deviceConnections = new List<DeviceConnection>();
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
                LocalData.Current_Error.Message = loginResponse.Data.error_description;
                var login = new Login_Window();
                login.Show();
                this.Close();
                return;
            }

            //Get Token
            var token = loginResponse.Data.access_token;

            //Update Local PC information if date not same
            if (Convert.ToDateTime(ins.LastUpdateDate) != DateTime.Today)
            {
                //get institution info
                var schoolRequest = new RestRequest("api/school/{id}", Method.GET);

                schoolRequest.AddUrlSegment("id", ins.UserName);
                schoolRequest.AddHeader("Authorization", "Bearer " + token);

                //School info execute the request
                var schoolResponse = await client.ExecuteTaskAsync(schoolRequest);
                var schoolInfo = JsonConvert.DeserializeObject<Institution>(schoolResponse.Content);

                if (schoolResponse.StatusCode != HttpStatusCode.OK && schoolInfo == null)
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
                ins.Token = token;
                ins.IsValid = schoolInfo.IsValid;
                ins.SettingKey = schoolInfo.SettingKey;
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

                var prevLog = device.DownloadPrevLogs();
                var todayLog = device.DownloadTodayLogs();

                await Machine.SaveLogsOrAttendanceInPc(prevLog, todayLog, ins, device.Device);
            }

            #endregion Device data send to server
            //show display
            var display = new DisplayWindow(_deviceDisplay);
            display.Show();
            this.Close();

            #endregion Check Internet
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Clean up.
            _tmr?.Stop();

            _tmr = null;
        }
    }
}
