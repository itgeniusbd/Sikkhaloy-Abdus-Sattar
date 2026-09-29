using AttendanceDevice.APIClass;
using AttendanceDevice.Config_Class;
using AttendanceDevice.Model;
using AttendanceDevice.Settings;
using AttendanceDevice.ViewModel;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace AttendanceDevice
{
    /// <summary>
    /// Interaction logic for Display_Window.xaml
    /// </summary>

    public partial class DisplayWindow : Window
    {
        private DispatcherTimer _tmr = new DispatcherTimer();
        private readonly DeviceDisplay _deviceDisplay;
        private bool _syncTimerStarted;
        private bool _timerBusy;
        private bool _webViewNavigationReady;
        private bool _webViewIsNavigating;
        private DateTime _lastScheduleSync = DateTime.MinValue;
        private DateTime _lastAssignmentSync = DateTime.MinValue;
        private DateTime _lastSchoolStatusSync = DateTime.MinValue;
        private bool _scheduleFilterUpdating;
        private bool _scheduleFilterApplyBusy;
        private DateTime _lastScheduleFilterApply = DateTime.MinValue;
        private DateTime _lastWebViewRefresh = DateTime.MinValue;
        private bool _displayDataDirty;

        // Day-rollover state for a display left running for days (startup already did today's work).
        private DateTime _cycleDay = DateTime.MinValue;
        private DateTime _leaveSyncDay = DateTime.MinValue;
        private DateTime _tokenRefreshDay = DateTime.Today;
        private DateTime _deviceClockSyncDay = DateTime.Today;

        private sealed class DisplayScheduleFilterItem
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        public DisplayWindow(DeviceDisplay deviceDisplay)
        {
            _deviceDisplay = deviceDisplay;
            InitializeComponent();
        }

        async Task InitializeAsync()
        {
            var env = await CoreWebView2Environment.CreateAsync(null, AppPaths.WebView2UserDataFolder);
            await webView.EnsureCoreWebView2Async(env);
            webView.CoreWebView2.NavigationCompleted += WebView_NavigationCompleted;
        }

        private void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            _webViewIsNavigating = false;
            _webViewNavigationReady = e.IsSuccess;

            if (!e.IsSuccess)
            {
                DeviceError.Text = $"Display load failed ({e.WebErrorStatus})";
                return;
            }

            if (DeviceError.Text.StartsWith("Display load failed", StringComparison.Ordinal))
                DeviceError.Text = "";

            _ = SyncScheduleFilterPanelAsync();
            StartSyncTimerIfNeeded();
        }

        private void StartSyncTimerIfNeeded()
        {
            if (_syncTimerStarted) return;

            _syncTimerStarted = true;
            _tmr.Interval = PerformanceSettings.DisplaySyncInterval;
            RemoveClickEvent(_tmr);
            _tmr.Tick += Timer_Tick;
            _tmr.Start();

            // First sync after page load — refresh sliders only if attendance data changed.
            _ = RunDisplaySyncCycleAsync(reloadAfterSync: false);
        }

        private void ApplySyncTimerInterval()
        {
            if (_tmr != null)
                _tmr.Interval = PerformanceSettings.DisplaySyncInterval;
        }

        private async Task RefreshWebViewDisplayAsync(bool forceFullReload = false)
        {
            if (webView?.CoreWebView2 == null || _webViewIsNavigating || !_webViewNavigationReady)
                return;

            if (!forceFullReload && PerformanceSettings.PreferPartialWebViewRefresh)
            {
                try
                {
                    await webView.CoreWebView2.ExecuteScriptAsync(
                        "if (typeof window.requestEmbedDisplayRefresh==='function') window.requestEmbedDisplayRefresh();");
                    await Task.Delay(500);
                    await ApplyWebScheduleFilterAsync();
                    _lastWebViewRefresh = DateTime.Now;
                    return;
                }
                catch
                {
                    // Fall back to full reload below.
                }
            }

            _webViewIsNavigating = true;
            webView.Reload();
        }

        private void RefreshWebViewDisplay()
        {
            _ = RefreshWebViewDisplayAsync(forceFullReload: false);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = LocalData.Instance.institution;
            FitInstitutionName();
            var schoolId = LocalData.Instance.institution.SchoolID;

            await InitializeAsync();

            BuildScheduleFilterPanelFromLocal(null);

            var lowPower = PerformanceSettings.LowPowerMode ? "&lowPower=1" : string.Empty;
            var marqueePerf =
                $"&scroll={PerformanceSettings.MarqueeScrollAmount}&delay={PerformanceSettings.MarqueeScrollDelay}";
            var url =
                $"{ApiUrl.WebUrl}/Attendances/Online_Display/DeviceDisplay.aspx?SchoolID={schoolId}&embed=1{lowPower}{marqueePerf}";
            _webViewIsNavigating = true;
            webView.CoreWebView2.Navigate(url);

            countDevice.Badge = await _deviceDisplay.Total_DevicesAsync();
            foreach (var device in _deviceDisplay.Devices)
            {
                //Data Show context pass to the device class
                device.EnrollUserCard = UserDataGrid;
            }

            ShowLatestTodayPunchOnCard();

            Closing += Window_Closing;
        }

        private DateTime _lastWebViewLayoutNotify = DateTime.MinValue;

        private void InstitutionHeaderGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FitInstitutionName();
            NotifyWebViewLayoutChanged();
        }

        private async void NotifyWebViewLayoutChanged()
        {
            if (webView?.CoreWebView2 == null || !_webViewNavigationReady)
                return;

            if (_lastWebViewLayoutNotify != DateTime.MinValue &&
                DateTime.Now - _lastWebViewLayoutNotify < TimeSpan.FromMilliseconds(500))
            {
                return;
            }

            _lastWebViewLayoutNotify = DateTime.Now;

            try
            {
                await webView.CoreWebView2.ExecuteScriptAsync(
                    "if (window.scheduleFitEmbedLayout) { window.scheduleFitEmbedLayout(); }");
            }
            catch
            {
                // WebView may be reloading; ignore transient script errors.
            }
        }

        private void FitInstitutionName()
        {
            if (InstitutionNameText == null || InstitutionHeaderGrid == null)
                return;

            var availableWidth = InstitutionHeaderGrid.ActualWidth - 64;
            if (availableWidth <= 0)
                return;

            InstitutionNameFontHelper.ApplyAndFit(
                InstitutionNameText,
                LocalData.Instance.institution?.InstitutionName,
                availableWidth,
                InstitutionHeaderGrid.ActualHeight - 4);
        }

        private void ShowLatestTodayPunchOnCard()
        {
            try
            {
                var today = LocalData.Instance.GetAttendanceDateString();
                using (var db = new ModelContext())
                {
                    var latest = db.attendance_Records
                        .AsEnumerable()
                        .Where(a => AttendanceDateHelper.DatesMatch(a.AttendanceDate, today)
                                    && !a.Is_OUT
                                    && !string.IsNullOrWhiteSpace(a.EntryTime))
                        .Select(a =>
                        {
                            ScheduleTimeHelper.TryParse(a.EntryTime, out var time);
                            return new { Record = a, Time = time };
                        })
                        .OrderByDescending(x => x.Time)
                        .FirstOrDefault();

                    if (latest == null)
                        return;

                    var userView = LocalData.Instance.GetUserView(latest.Record.DeviceID);
                    if (userView == null)
                        return;

                    userView.Enroll_Time = LocalData.Instance.GetAttendanceDate().Add(latest.Time);
                    userView.ImgLink = UserPhotoHelper.ResolvePhotoUri(
                        LocalData.Instance.institution?.Image_Link,
                        userView.ID);
                    ScheduleDisplayHelper.ApplyTo(userView, latest.Record.ScheduleID);
                    UserDataGrid.DataContext = userView;
                }
            }
            catch
            {
                // Keep empty card if lookup fails.
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (webView?.CoreWebView2 != null)
                webView.CoreWebView2.NavigationCompleted -= WebView_NavigationCompleted;

            //Clean up.
            _tmr.Stop();
            _tmr = null;
        }

        private void RemoveClickEvent(DispatcherTimer b)
        {
            var fieldInfo = b.GetType().GetField(
                "Tick", BindingFlags.Instance | BindingFlags.NonPublic);
            var eventDelegate = fieldInfo.GetValue(b) as MulticastDelegate;
            if (eventDelegate != null) // will be null if no subscribed event consumers
            {
                var delegates = eventDelegate.GetInvocationList();
                foreach (Delegate d in delegates)
                {
                    _tmr.Tick -= (EventHandler)d;
                }
            }
        }

        private async void Timer_Tick(object sender, EventArgs e)
        {
            await RunDisplaySyncCycleAsync(reloadAfterSync: false);
        }

        private bool ShouldRefreshWebViewDisplay()
        {
            return _displayDataDirty;
        }

        private async Task RefreshWebViewDisplayIfNeededAsync()
        {
            if (!ShouldRefreshWebViewDisplay())
                return;

            _displayDataDirty = false;
            await RefreshWebViewDisplayAsync(forceFullReload: true);
        }

        private async Task RunDisplaySyncCycleAsync(bool reloadAfterSync)
        {
            if (_timerBusy)
                return;

            if (reloadAfterSync)
                _displayDataDirty = true;

            _timerBusy = true;
            try
            {
                OnDayRolloverIfNeeded();

                var totalDeviceConnected = await _deviceDisplay.Total_DevicesAsync();

                if (totalDeviceConnected == 0)
                {
                    LocalData.Current_Error.Message = "No Device Connected!";
                    LocalData.Current_Error.Type = Error_Type.DeviceInfoPage;

                    var setting = new Setting();
                    setting.Show();
                    this.Close();
                    return;
                }

                using (var db = new ModelContext())
                {
                    LocalData.Instance.ArchiveExpiredAttendanceRecords();

                    var pendingCount = LocalData.Instance.GetPendingAttendanceCount();
                    if (pendingCount > 0)
                    {
                        LocalData.Instance.FlagIncompleteRecordsForResync();
                        LocalData.Instance.RepairTodayScheduleIdsForResync();
                    }

                    var ins = LocalData.Instance.institution;
                    var client = new RestClient(ApiUrl.EndPoint);
                    var allDevices = db.Devices.Count();
                    var attendanceReplayed = false;

                    if (ins != null && !string.IsNullOrWhiteSpace(ins.Token))
                    {
                        if (_lastScheduleSync == DateTime.MinValue ||
                            DateTime.Now - _lastScheduleSync >= PerformanceSettings.ScheduleDaysSyncInterval)
                        {
                            var schoolId = LocalData.Instance.GetEffectiveSchoolId();
                            await ScheduleAssignmentSync.SyncScheduleDaysFromServerAsync(client, schoolId, ins.Token);
                            _lastScheduleSync = DateTime.Now;
                            await Dispatcher.InvokeAsync(() => BuildScheduleFilterPanelFromLocal(null));
                        }

                        if (_lastAssignmentSync == DateTime.MinValue ||
                            DateTime.Now - _lastAssignmentSync >= PerformanceSettings.AssignmentSyncInterval)
                        {
                            var schoolId = LocalData.Instance.GetEffectiveSchoolId();
                            var assignmentsSynced = await ScheduleAssignmentSync.SyncAssignmentsFromServerAsync(client, schoolId, ins.Token);
                            _lastAssignmentSync = DateTime.Now;

                            if (assignmentsSynced &&
                                ins.Is_Device_Attendance_Enable &&
                                !(ins.Is_Today_Holiday && !ins.Holiday_NotActive))
                            {
                                // Re-route today's "No active schedule" punches with the fresh assignments
                                // before Abs_Insert below would mark those users absent.
                                try
                                {
                                    if (await Machine.ReplayRejectedLogsAsAttendance(ins, refreshSchedulesFromNetwork: false) > 0)
                                        attendanceReplayed = true;
                                }
                                catch (Exception replayEx)
                                {
                                    StartupLogger.LogFailure("replay-rejected-after-assignment-sync", replayEx);
                                }
                            }
                        }
                    }

                    if (ins != null &&
                        (_lastSchoolStatusSync == DateTime.MinValue ||
                         DateTime.Now - _lastSchoolStatusSync >= PerformanceSettings.SchoolStatusSyncInterval))
                    {
                        var schoolRequest = new RestRequest("api/school/{id}", Method.GET);
                        schoolRequest.AddUrlSegment("id", ins.UserName);
                        ApiRequestHelper.AddAuthorizedJsonHeaders(schoolRequest, ins.Token);
                        var schoolResponse = await client.ExecuteTaskAsync(schoolRequest);
                        if (schoolResponse.StatusCode == HttpStatusCode.OK && !string.IsNullOrWhiteSpace(schoolResponse.Content))
                        {
                            var schoolInfo = JsonConvert.DeserializeObject<SchoolApiDto>(schoolResponse.Content);
                            if (schoolInfo != null)
                                await LocalData.Instance.ApplySchoolStatusFromApiAsync(schoolInfo);
                        }

                        _lastSchoolStatusSync = DateTime.Now;
                        ins = LocalData.Instance.institution;
                    }
                    else if (ins != null)
                    {
                        ins = LocalData.Instance.institution;
                    }

                    var currentDateTime = DateTime.Now;
                    var currentDate = LocalData.Instance.GetAttendanceDateString();
                    var attendanceChanged = attendanceReplayed;
                    DeviceError.Text = "";

                    //Device Attendance Disabled
                    if (ins != null && !ins.Is_Device_Attendance_Enable)
                    {
                        DeviceError.Text = "Device Attendance Disabled";
                    }

                    //Holiday attendance disable
                    else if (ins != null && ins.Is_Today_Holiday && !ins.Holiday_NotActive)
                    {
                        DeviceError.Text = "Today is Holiday And attendance disable";
                    }

                    //All Device not Connected 
                    else if (allDevices != totalDeviceConnected)
                    {
                        DeviceError.Text = "All device are not connected";
                        countDevice.Badge = totalDeviceConnected;
                    }
                    else
                    {
                        if (!LocalData.Instance.TodayDeviceLogsImportedFromDevice)
                        {
                            await StartupHelper.ImportTodayDeviceLogsAsync(_deviceDisplay.Devices, ins);
                        }

                        if (LocalData.Instance.TodayDeviceLogsImportedFromDevice)
                        {
                            var schScheduleIDs = LocalData.Instance.GetCurrentOndaySchduleIds();
                            if (schScheduleIDs.Any())
                            {
                                LocalData.Instance.Abs_Insert(schScheduleIDs, currentDate, ins);
                                attendanceChanged = true;
                            }
                        }
                    }

                    if (ins != null && !ins.Is_Employee_Attendance_Enable)
                    {
                        DeviceError.Text = "Employee Attendance Disabled";
                    }

                    if (ins != null && !ins.Is_Student_Attendance_Enable)
                    {
                        DeviceError.Text = "Student Attendance Disabled";
                    }

                    await SyncDeviceClocksOncePerDayAsync();

                    //check internet
                    var internet = await ApiUrl.IsNoNetConnection();
                    if (internet) return;

                    //check server ok
                    var server = await ApiUrl.IsServerUnavailable();
                    if (server) return;

                    await RunDailyOnlineTasksAsync(LocalData.Instance.institution);
                    ins = LocalData.Instance.institution;

                    countRecord.Badge = LocalData.Instance.GetPendingAttendanceCount();

                    var push = await AttendanceWebsiteSync.PushPendingAsync(ins);
                    if (push.Changed)
                        attendanceChanged = true;
                    if (!string.IsNullOrWhiteSpace(push.Message) && push.TotalPushed > 0)
                        DeviceError.Text = push.Message;
                    else if (!string.IsNullOrWhiteSpace(push.Message) &&
                             !push.Message.StartsWith("No pending attendance", StringComparison.OrdinalIgnoreCase))
                        DeviceError.Text = push.Message;

                    countRecord.Badge = LocalData.Instance.GetPendingAttendanceCount();

                    if (attendanceChanged)
                        _displayDataDirty = true;
                }
            }
            catch (Exception exception)
            {
                // ignored
            }
            finally
            {
                _timerBusy = false;
                await RefreshWebViewDisplayIfNeededAsync();
            }
        }

        /// <summary>
        /// When the attendance date changes while the app keeps running, reset per-day state and force
        /// schedule / assignment / school-status syncs in this cycle (before Abs_Insert), rebuild today's
        /// schedule filter and reload the web display - the same work an app restart would do.
        /// </summary>
        private void OnDayRolloverIfNeeded()
        {
            LocalData.Instance.CheckDayRollover();

            var today = LocalData.Instance.GetAttendanceDate();
            if (_cycleDay == today)
                return;

            var firstCycle = _cycleDay == DateTime.MinValue;
            _cycleDay = today;
            if (firstCycle)
                return;

            _lastScheduleSync = DateTime.MinValue;
            _lastAssignmentSync = DateTime.MinValue;
            _lastSchoolStatusSync = DateTime.MinValue;
            _displayDataDirty = true;

            try
            {
                BuildScheduleFilterPanelFromLocal(null);
            }
            catch (Exception ex)
            {
                StartupLogger.LogFailure("display: new-day filter rebuild", ex);
            }

            StartupLogger.LogStage($"display: new day {today:dd-MMM-yy} - forcing schedule/assignment/school/leave sync + display reload");
        }

        /// <summary>Device clocks drift over days; re-sync them to the PC once per day (startup does it on day one).</summary>
        private async Task SyncDeviceClocksOncePerDayAsync()
        {
            var today = DateTime.Today;
            if (_deviceClockSyncDay == today)
                return;

            _deviceClockSyncDay = today;
            foreach (var device in _deviceDisplay.Devices)
            {
                try
                {
                    if (!device.IsConnected())
                        continue;

                    var deviceTime = await Task.Run(() => device.GetDateTime());
                    if (deviceTime.ToString("dd-MM-yyyy hh:mm tt") != DateTime.Now.ToString("dd-MM-yyyy hh:mm tt"))
                    {
                        await Task.Run(() => device.SetDateTime());
                        StartupLogger.LogStage($"display: daily device clock sync ({device.Device?.DeviceIP}) {deviceTime:yyyy-MM-dd HH:mm:ss} -> PC time");
                    }
                }
                catch (Exception ex)
                {
                    StartupLogger.LogFailure($"display: daily device clock sync ({device.Device?.DeviceIP})", ex);
                }
            }
        }

        /// <summary>
        /// Once per attendance day while online: refresh the bearer token (server tokens expire after 14 days)
        /// and download today's leaves (the API returns only today's; previously fetched only at startup).
        /// </summary>
        private async Task RunDailyOnlineTasksAsync(Institution ins)
        {
            if (ins == null)
                return;

            var today = LocalData.Instance.GetAttendanceDate();
            var client = ApiRequestHelper.CreateClient();

            if (_tokenRefreshDay != today &&
                !string.IsNullOrWhiteSpace(ins.UserName) &&
                !string.IsNullOrWhiteSpace(ins.Password))
            {
                try
                {
                    var loginResponse = await ApiRequestHelper.ExecuteAsync(
                        client, ApiLoginHelper.CreateTokenRequest(ins.UserName, ins.Password));
                    var token = ApiResponseHelper.GetAccessToken(loginResponse);
                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        ins.Token = token;
                        _tokenRefreshDay = today;
                        StartupLogger.LogStage("display: daily token refresh ok");
                    }
                    else if (loginResponse != null && loginResponse.StatusCode != 0)
                    {
                        // Server answered (e.g. password changed): keep the current token, retry tomorrow.
                        _tokenRefreshDay = today;
                        StartupLogger.LogStage($"display: daily token refresh failed ({(int)loginResponse.StatusCode})");
                    }
                }
                catch (Exception ex)
                {
                    StartupLogger.LogFailure("display: daily token refresh", ex);
                }
            }

            if (_leaveSyncDay != today && ins.SchoolID > 0 && !string.IsNullOrWhiteSpace(ins.Token))
            {
                try
                {
                    var leaveRequest = new RestRequest("api/Users/{id}/leave", Method.GET);
                    leaveRequest.AddUrlSegment("id", ins.SchoolID);
                    ApiRequestHelper.AddAuthorizedJsonHeaders(leaveRequest, ins.Token);
                    var leaveResponse = await ApiRequestHelper.ExecuteAsync(client, leaveRequest);
                    if (leaveResponse.StatusCode == HttpStatusCode.OK)
                    {
                        var leaveRecords = ApiResponseHelper.ParseLeaveRecords(ApiResponseHelper.ReadContent(leaveResponse));
                        await LocalData.Instance.LeaveDataHandling(leaveRecords);
                        _leaveSyncDay = today;
                        StartupLogger.LogStage($"display: leave sync for {today:dd-MMM-yy} ({leaveRecords.Count} record(s))");
                    }
                }
                catch (Exception ex)
                {
                    StartupLogger.LogFailure("display: daily leave sync", ex);
                }
            }
        }

        private void BuildScheduleFilterPanelFromLocal(HashSet<int> activeIdsOverride)
        {
            var schedules = LocalData.Instance.GetTodayDisplaySchedules()
                .Select(s => new DisplayScheduleFilterItem
                {
                    id = s.ScheduleID,
                    name = string.IsNullOrWhiteSpace(s.ScheduleName)
                        ? $"Schedule {s.ScheduleID}"
                        : s.ScheduleName
                })
                .GroupBy(s => s.id)
                .Select(g => g.First())
                .OrderBy(s => s.id)
                .ToList();

            RenderScheduleFilterPanel(schedules, activeIdsOverride);
        }

        private void RenderScheduleFilterPanel(List<DisplayScheduleFilterItem> schedules, HashSet<int> activeIdsOverride)
        {
            if (ScheduleFilterPanel == null)
                return;

            _scheduleFilterUpdating = true;
            try
            {
                var existingChecks = ScheduleFilterPanel.Children
                    .OfType<CheckBox>()
                    .ToDictionary(cb => (int)cb.Tag, cb => cb.IsChecked == true);

                ScheduleFilterPanel.Children.Clear();
                if (!schedules.Any())
                    return;

                HashSet<int> activeIds;
                if (activeIdsOverride != null)
                    activeIds = activeIdsOverride;
                else if (existingChecks.Any())
                {
                    activeIds = new HashSet<int>(existingChecks.Where(p => p.Value).Select(p => p.Key));
                    foreach (var schedule in schedules)
                    {
                        if (!existingChecks.ContainsKey(schedule.id))
                            activeIds.Add(schedule.id);
                    }
                }
                else
                    activeIds = new HashSet<int>(schedules.Select(s => s.id));

                foreach (var schedule in schedules)
                {
                    var cb = new CheckBox
                    {
                        Content = schedule.name,
                        Tag = schedule.id,
                        IsChecked = activeIds.Contains(schedule.id),
                        Margin = new Thickness(0, 0, 14, 0),
                        FontSize = 12,
                        FontWeight = FontWeights.SemiBold,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    cb.Checked += ScheduleFilter_Changed;
                    cb.Unchecked += ScheduleFilter_Changed;
                    ScheduleFilterPanel.Children.Add(cb);
                }
            }
            finally
            {
                _scheduleFilterUpdating = false;
            }
        }

        private async Task SyncScheduleFilterPanelAsync()
        {
            if (webView?.CoreWebView2 == null)
                return;

            try
            {
                var schedulesJson = await webView.CoreWebView2.ExecuteScriptAsync(
                    "JSON.stringify(typeof window.getDisplaySchedules==='function'?window.getDisplaySchedules():[])");
                var schedules = ParseScriptJsonArray<DisplayScheduleFilterItem>(schedulesJson);
                if (schedules.Count > 0)
                {
                    RenderScheduleFilterPanel(schedules, null);
                    await ApplyWebScheduleFilterAsync();
                }
            }
            catch
            {
                // Keep local fallback panel if web filter sync fails.
            }
        }

        private async Task ApplyWebScheduleFilterAsync()
        {
            if (webView?.CoreWebView2 == null || !_webViewNavigationReady || ScheduleFilterPanel == null)
                return;

            if (_scheduleFilterApplyBusy)
                return;

            if ((DateTime.Now - _lastScheduleFilterApply).TotalMilliseconds < 400)
                return;

            _scheduleFilterApplyBusy = true;
            _lastScheduleFilterApply = DateTime.Now;
            try
            {
                var activeIds = ScheduleFilterPanel.Children
                    .OfType<CheckBox>()
                    .Where(cb => cb.IsChecked == true)
                    .Select(cb => (int)cb.Tag)
                    .ToList();

                var json = JsonConvert.SerializeObject(activeIds);
                await webView.CoreWebView2.ExecuteScriptAsync($"window.setScheduleFilter({json})");
            }
            finally
            {
                _scheduleFilterApplyBusy = false;
            }
        }

        private async void ScheduleFilter_Changed(object sender, RoutedEventArgs e)
        {
            if (_scheduleFilterUpdating)
                return;

            await ApplyWebScheduleFilterAsync();
        }

        private static List<T> ParseScriptJsonArray<T>(string scriptResult)
        {
            if (string.IsNullOrWhiteSpace(scriptResult) || scriptResult == "null")
                return new List<T>();

            var inner = JsonConvert.DeserializeObject<string>(scriptResult);
            if (string.IsNullOrWhiteSpace(inner))
                return new List<T>();

            return JsonConvert.DeserializeObject<List<T>>(inner) ?? new List<T>();
        }

        //Re-connect device
        private async void BtnReConnect_Device_Click(object sender, RoutedEventArgs e)
        {
            btnReconnect.IsEnabled = false;
            btnSetting.IsEnabled = false;

            var deviceList = _deviceDisplay.Devices;
            deviceList.Clear();

            var devices = await LocalData.Instance.DeviceListAsync();
            var ins = LocalData.Instance.institution;

            //Device Check
            if (devices.Any())
            {
                foreach (var device in devices)
                {
                    var checkIp = await Device_PingTest.PingHostAsync(device.DeviceIP);
                    if (checkIp)
                    {
                        deviceList.Add(new DeviceConnection(device));
                    }
                }

                if (deviceList.Count > 0)
                {
                    var dCheck = await StartupHelper.ImportTodayDeviceLogsAsync(deviceList, ins)
                        || deviceList.Any(d => d.IsConnected());

                    if (dCheck)
                    {
                        var dDisplay = new DeviceDisplay(deviceList);

                        var displayWindow = new DisplayWindow(dDisplay);
                        displayWindow.Show();
                        this.Close();
                    }
                    else
                    {
                        var errorObj = new Error("Connect Device", "Device Not connected");
                        var errorWindow = new Error_Window(errorObj);
                        errorWindow.Show();
                    }
                }
                else
                {
                    var errorObj = new Error("Connect Device", "Device Not connected");
                    var errorWindow = new Error_Window(errorObj);
                    errorWindow.Show();
                }
            }
            else
            {
                var errorObj = new Error("Add Device", "Add Device Info");
                var errorWindow = new Error_Window(errorObj);
                errorWindow.Show();
            }

            btnReconnect.IsEnabled = true;
            btnSetting.IsEnabled = true;
        }

        //Setting Dialog
        private void Setting_Button_Click(object sender, RoutedEventArgs e)
        {
            var settingLogin = new SettingLogin();
            settingLogin.Show();
            Close();
        }


        //external page link
        private void Sikkhaloy_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://sikkhaloy.com/Attendances/Online_Display/Attendance_Slider.aspx");
        }

        private void LoopsIT_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://loopsit.com/");
        }
    }
}
