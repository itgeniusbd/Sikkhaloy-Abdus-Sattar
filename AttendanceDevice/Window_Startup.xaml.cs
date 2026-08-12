using AttendanceDevice.APIClass;
using AttendanceDevice.Config_Class;
using AttendanceDevice.Model;
using AttendanceDevice.Settings;
using AttendanceDevice.ViewModel;
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
            const string stagePrefix = "startup";
            try
            {
                StartupLogger.LogStage($"{stagePrefix}: begin");
                //Empty Error
                LocalData.Current_Error = new Setting_Error();


                // Institution not Register in local database
                var ins = LocalData.Instance.institution;

                if (ins == null)
                {
                    StartupLogger.LogStage($"{stagePrefix}: no institution -> login");
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

                var pingResults = await Task.WhenAll(deviceList.Select(async device =>
                {
                    var checkIp = await Device_PingTest.PingHostAsync(device.DeviceIP, 2000);
                    return checkIp ? device : null;
                }));

                foreach (var device in pingResults.Where(d => d != null))
                    deviceConnections.Add(new DeviceConnection(device));

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
                StartupLogger.LogStage($"{stagePrefix}: device connect");
                var connectResults = await Task.WhenAll(
                    deviceConnections.Select(StartupHelper.ConnectDeviceWithTimeoutAsync));
                var isDeviceConnected = connectResults.Any(r => r.IsSuccess);
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


                StartupLogger.LogStage($"{stagePrefix}: token request");
                //get user token
                var client = ApiRequestHelper.CreateClient();
                var loginRequest = ApiLoginHelper.CreateTokenRequest(ins.UserName, ins.Password);

                //Login execute the request
                var loginResponse = await ApiRequestHelper.ExecuteAsync(client, loginRequest);

                //API call for token
                if (loginResponse.StatusCode != HttpStatusCode.OK)
                {
                    //Invalid username and password
                    LocalData.Current_Error.Message = ApiLoginHelper.GetTokenErrorMessage(loginResponse);
                    var login = new Login_Window();
                    login.Show();
                    this.Close();
                    return;
                }

                //Get Token
                var token = ApiResponseHelper.GetAccessToken(loginResponse);
                if (string.IsNullOrWhiteSpace(token))
                {
                    LocalData.Current_Error.Message = "Login token missing in server response.";
                    var login = new Login_Window();
                    login.Show();
                    this.Close();
                    return;
                }


                StartupLogger.LogStage($"{stagePrefix}: school info");
                //get institution info
                var schoolRequest = new RestRequest("api/school/{id}", Method.GET);

                schoolRequest.AddUrlSegment("id", ins.UserName);
                schoolRequest.AddHeader("Authorization", "Bearer " + token);


                //School info execute the request
                var schoolResponse = await ApiRequestHelper.ExecuteAsync(client, schoolRequest); //response.data not work because of logo image data

                if (schoolResponse.StatusCode != HttpStatusCode.OK)
                {
                    LocalData.Current_Error.Message = schoolResponse.StatusDescription;
                    var login = new Login_Window();
                    login.Show();
                    this.Close();
                    return;
                }

                var schoolInfo = ApiResponseHelper.ParseSchoolApi(ApiResponseHelper.ReadContent(schoolResponse));

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
                LocalData.Instance.MergeSchoolApiIntoInstitution(ins, schoolInfo);

                await LocalData.Instance.InstitutionUpdate(ins);

                StartupLogger.LogStage($"{stagePrefix}: leave sync");
                //Leave request

                #region Leave request

                var leaveRequest = new RestRequest("api/Users/{id}/leave", Method.GET);
                leaveRequest.AddUrlSegment("id", ins.SchoolID);
                leaveRequest.AddHeader("Authorization", "Bearer " + token);
                //Leave execute the request
                var leaveResponse = await ApiRequestHelper.ExecuteAsync(client, leaveRequest);

                if (leaveResponse.StatusCode == HttpStatusCode.OK)
                {
                    var leaveRecords = ApiResponseHelper.ParseLeaveRecords(ApiResponseHelper.ReadContent(leaveResponse));
                    await LocalData.Instance.LeaveDataHandling(leaveRecords);
                }
                else if (leaveResponse.ResponseStatus == ResponseStatus.TimedOut ||
                         leaveResponse.ResponseStatus == ResponseStatus.Error)
                {
                    StartupLogger.LogStage(
                        $"{stagePrefix}: leave sync skipped ({leaveResponse.ResponseStatus})");
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

                StartupLogger.LogStage($"{stagePrefix}: schedule sync");
                //Schedule Day Request

                #region Schedule data

                var schoolId = LocalData.Instance.GetEffectiveSchoolId();
                var scheduleResult = await ScheduleAssignmentSync.EnsureScheduleBundleAsync(client, schoolId, token);
                if (!scheduleResult.Success && !LocalData.Instance.Schedules_Get().Any())
                {
                    StartupLogger.LogStage($"{stagePrefix}: schedule sync failed — no local schedule rows");
                    LocalData.Current_Error.Message =
                        "Schedule download failed. Open Settings → Schedule → Download from Server.";
                    LocalData.Current_Error.Type = Error_Type.SchedulePage;

                    var setting = new Setting();
                    setting.Show();
                    this.Close();
                    return;
                }

                StartupLogger.LogStage($"{stagePrefix}: schedule sync done");

                if (scheduleResult.UserScheduleMismatch)
                {
                    LocalData.Current_Error.Message =
                        "Not all User assigned in the schedule on PC, Update User from server!";
                    LocalData.Current_Error.Type = Error_Type.UserInfoPage;

                    var setting = new Setting();
                    setting.Show();
                    this.Close();
                    return;
                }

                #endregion Schedule data

                //Update Local PC information update time
                ins.LastUpdateDate = schoolInfo.LastUpdateDate;
                await LocalData.Instance.InstitutionUpdate(ins);

                StartupLogger.LogStage($"{stagePrefix}: local maintenance deferred");
                StartupLogger.LogStage($"{stagePrefix}: today attendance");
                try
                {
                    var todayAttendanceRequest = new RestRequest("api/Attendance/{id}/GetTodayAttendance", Method.GET);
                    todayAttendanceRequest.AddUrlSegment("id", ins.SchoolID);
                    todayAttendanceRequest.AddHeader("Authorization", "Bearer " + token);

                    var todayAttendanceResponse =
                        await ApiRequestHelper.ExecuteAsync(client, todayAttendanceRequest);

                    StartupLogger.LogStage(
                        $"{stagePrefix}: today attendance response {(int)todayAttendanceResponse.StatusCode}");

                    var todayContent = ApiResponseHelper.ReadContent(todayAttendanceResponse);
                    if (todayAttendanceResponse.StatusCode == HttpStatusCode.OK &&
                        !string.IsNullOrWhiteSpace(todayContent))
                    {
                        var todayRecords = AttendanceRecordMapper.FromTodayAttendanceJson(todayContent);
                        StartupLogger.LogStage($"{stagePrefix}: today attendance parsed {todayRecords.Count} rows");
                        if (todayRecords.Any())
                            await LocalData.Instance.GetTodayAttendanceRecords(todayRecords);
                    }
                }
                catch (Exception todayEx)
                {
                    StartupLogger.LogFailure($"{stagePrefix}: today attendance", todayEx);
                }


                StartupLogger.LogStage($"{stagePrefix}: update notifications");
                //Get any update notification from server
                var updateNotificationRequest = new RestRequest("api/Users/{id}/updateInfo", Method.GET);
                updateNotificationRequest.AddUrlSegment("id", ins.SchoolID);
                updateNotificationRequest.AddHeader("Authorization", "Bearer " + token);

                var updateNotificationResponse =
                    await ApiRequestHelper.ExecuteAsync(client, updateNotificationRequest);

                if (updateNotificationResponse.StatusCode == HttpStatusCode.OK)
                {
                    var notifications = ApiResponseHelper.ParseUpdateNotifications(
                        ApiResponseHelper.ReadContent(updateNotificationResponse));
                    if (notifications.Any())
                    {
                        await LocalData.Instance.AddNotifications(notifications);

                        var setting = new Setting();
                        setting.Show();
                        this.Close();
                        return;
                    }
                }

                StartupLogger.LogStage($"{stagePrefix}: open display");
                var displayWindow = new DisplayWindow(initDevice);
                displayWindow.Show();
                _ = StartupHelper.RunPostScheduleMaintenanceAsync();
                _ = StartupHelper.DownloadDeviceLogsInBackgroundAsync(deviceConnections, ins);
                this.Close();
            }
            catch (Exception ex)
            {
                StartupLogger.LogFailure($"{stagePrefix}: unhandled", ex);
                var errorObj = new Error("System Error", GetFullExceptionMessage(ex));
                var errorWindow = new Error_Window(errorObj);
                errorWindow.Show();
                this.Close();
            }
        }

        private static string GetFullExceptionMessage(Exception ex)
        {
            var parts = new List<string>();
            while (ex != null)
            {
                if (!string.IsNullOrWhiteSpace(ex.Message))
                    parts.Add(ex.Message.Trim());
                ex = ex.InnerException;
            }

            return string.Join(Environment.NewLine + Environment.NewLine, parts.Distinct());
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}
