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
            if (!_displayDataDirty)
                return false;

            if (_lastWebViewRefresh != DateTime.MinValue &&
                DateTime.Now - _lastWebViewRefresh < PerformanceSettings.WebViewRefreshMinInterval)
            {
                return false;
            }

            return true;
        }

        private async Task RefreshWebViewDisplayIfNeededAsync()
        {
            if (!ShouldRefreshWebViewDisplay())
                return;

            _displayDataDirty = false;
            await RefreshWebViewDisplayAsync(forceFullReload: false);
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
                            await ScheduleAssignmentSync.SyncAssignmentsFromServerAsync(client, schoolId, ins.Token);
                            _lastAssignmentSync = DateTime.Now;
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
                    var attendanceChanged = false;
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
                        //get only Late time exceed and Abs not Count schedules
                        var schScheduleIDs = LocalData.Instance.GetCurrentOndaySchduleIds();
                        if (schScheduleIDs.Any())
                        {
                            LocalData.Instance.Abs_Insert(schScheduleIDs, currentDate, ins);
                            attendanceChanged = true;
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

                    //check internet
                    var internet = await ApiUrl.IsNoNetConnection();
                    if (internet) return;

                    //check server ok
                    var server = await ApiUrl.IsServerUnavailable();
                    if (server) return;

                    countRecord.Badge = LocalData.Instance.GetPendingAttendanceCount();

                    #region Student Post
                    var studentLog = await LocalData.Instance.StudentLog_Post();

                    if (studentLog.Count > 0)
                    {
                        var request = new RestRequest("api/Attendance/{id}/Students", Method.POST);

                        request.AddUrlSegment("id", ins.SchoolID);
                        ApiRequestHelper.AddAuthorizedJsonHeaders(request, ins.Token);
                        ApiRequestHelper.AddCamelCaseJsonBody(request, AttendanceRecordMapper.ToApiPayload(studentLog));

                        var response = await client.ExecutePostTaskAsync(request);
                        var syncResult = AttendanceSyncResponseParser.Parse(response);

                        if (response.StatusCode == HttpStatusCode.OK &&
                            (AttendanceSyncResponseParser.IsLegacyEmptyOk(response) ||
                             (syncResult != null && (syncResult.Matched > 0 || syncResult.Inserted > 0))))
                        {
                            var synced = FilterSyncedLogs(studentLog, syncResult);
                            await MarkAttendanceRecordsAsync(db, synced, markSent: true, markUpdated: true);

                            DeviceError.Text = BuildPartialSyncMessage("Student", studentLog.Count, syncResult);
                            attendanceChanged = true;
                        }
                        else if (AttendanceSyncResponseParser.TryGetStudentPostFailureMessage(
                            response, syncResult, out var failureMessage))
                        {
                            DeviceError.Text = failureMessage;
                        }
                    }

                    #endregion Student Post

                    #region Student Update
                    var studentLogUpdate = await LocalData.Instance.StudentLog_Put();

                    if (studentLogUpdate.Count > 0)
                    {
                        var request = new RestRequest("api/Attendance/{id}/StudentsUpdate", Method.POST);

                        request.AddUrlSegment("id", ins.SchoolID);
                        ApiRequestHelper.AddAuthorizedJsonHeaders(request, ins.Token);
                        ApiRequestHelper.AddCamelCaseJsonBody(request, AttendanceRecordMapper.ToApiPayload(studentLogUpdate));

                        var response = await client.ExecutePostTaskAsync(request);

                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            await MarkAttendanceRecordsAsync(db, studentLogUpdate, markSent: false, markUpdated: true);
                            attendanceChanged = true;
                        }
                        else
                        {
                            var detail = string.IsNullOrWhiteSpace(response.Content)
                                ? $"Student update failed ({(int)response.StatusCode})"
                                : $"Student update failed ({(int)response.StatusCode}): {response.Content}";
                            DeviceError.Text = detail;
                        }
                    }
                    #endregion Student Update

                    #region Employee Post
                    var empLog = await LocalData.Instance.EmpLog_Post();

                    if (empLog.Count > 0)
                    {
                        var request = new RestRequest("api/Attendance/{id}/Employees", Method.POST);

                        if (ins != null)
                        {
                            request.AddUrlSegment("id", ins.SchoolID);
                            ApiRequestHelper.AddAuthorizedJsonHeaders(request, ins.Token);
                        }

                        ApiRequestHelper.AddCamelCaseJsonBody(request, AttendanceRecordMapper.ToApiPayload(empLog));

                        var response = await client.ExecutePostTaskAsync(request);
                        var syncResult = AttendanceSyncResponseParser.Parse(response);

                        if (response.StatusCode == HttpStatusCode.OK &&
                            (AttendanceSyncResponseParser.IsLegacyEmptyOk(response) ||
                             (syncResult != null && (syncResult.Matched > 0 || syncResult.Inserted > 0))))
                        {
                            var synced = FilterSyncedLogs(empLog, syncResult);
                            await MarkAttendanceRecordsAsync(db, synced, markSent: true, markUpdated: true);

                            DeviceError.Text = BuildPartialSyncMessage("Employee", empLog.Count, syncResult);
                            attendanceChanged = true;
                        }
                        else if (AttendanceSyncResponseParser.TryGetEmployeePostFailureMessage(
                            response, syncResult, out var failureMessage))
                        {
                            DeviceError.Text = failureMessage;
                        }
                    }
                    #endregion Employee Post

                    #region Employees Update
                    var empLogUpdate = await LocalData.Instance.EmpLog_Put();

                    if (empLogUpdate.Count > 0)
                    {
                        var request = new RestRequest("api/Attendance/{id}/EmployeesUpdate", Method.POST);

                        request.AddUrlSegment("id", ins.SchoolID);
                        ApiRequestHelper.AddAuthorizedJsonHeaders(request, ins.Token);
                        ApiRequestHelper.AddCamelCaseJsonBody(request, AttendanceRecordMapper.ToApiPayload(empLogUpdate));

                        var response = await client.ExecutePostTaskAsync(request);

                        if (response.StatusCode != HttpStatusCode.OK) return;

                        await MarkAttendanceRecordsAsync(db, empLogUpdate, markSent: false, markUpdated: true);
                        attendanceChanged = true;
                    }
                    #endregion Employees Update

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

        private static async Task MarkAttendanceRecordsAsync(
            ModelContext db,
            System.Collections.Generic.IEnumerable<Attendance_Record> logs,
            bool markSent,
            bool markUpdated)
        {
            foreach (var log in logs)
            {
                if (!log.Is_OUT &&
                    !string.Equals(log.AttendanceStatus, "Abs", StringComparison.OrdinalIgnoreCase) &&
                    string.IsNullOrWhiteSpace(log.EntryTime))
                    continue;

                var record = log.RecordID > 0
                    ? await db.attendance_Records.SingleOrDefaultAsync(r => r.RecordID == log.RecordID)
                    : null;

                if (record == null)
                {
                    record = await db.attendance_Records.FirstOrDefaultAsync(r =>
                        r.DeviceID == log.DeviceID &&
                        r.ScheduleID == log.ScheduleID &&
                        r.AttendanceDate == log.AttendanceDate);
                }

                if (record == null) continue;

                if (markSent) record.Is_Sent = true;
                if (markUpdated) record.Is_Updated = true;
            }

            await db.SaveChangesAsync();
        }

        private static System.Collections.Generic.List<Attendance_Record> FilterSyncedLogs(
            System.Collections.Generic.List<Attendance_Record> logs,
            AttendanceSyncResultDto syncResult)
        {
            if (logs == null || logs.Count == 0)
                return logs ?? new System.Collections.Generic.List<Attendance_Record>();

            if (syncResult?.MatchedDeviceIds != null && syncResult.MatchedDeviceIds.Length > 0)
            {
                var matched = new System.Collections.Generic.HashSet<int>(syncResult.MatchedDeviceIds);
                return logs.Where(l => matched.Contains(l.DeviceID)).ToList();
            }

            return logs;
        }

        private static string BuildPartialSyncMessage(
            string entityLabel,
            int sentCount,
            AttendanceSyncResultDto syncResult)
        {
            if (syncResult == null || string.IsNullOrWhiteSpace(syncResult.Message))
                return string.Empty;

            if (syncResult.MatchedDeviceIds != null &&
                syncResult.MatchedDeviceIds.Length >= sentCount)
                return string.Empty;

            return $"{entityLabel}: {syncResult.Message}";
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
                    var dCheck = false;
                    foreach (var item in deviceList)
                    {
                        var status = await Task.Run(() => item.ConnectDevice());
                        if (status.IsSuccess)
                        {
                            dCheck = true;
                            var prevLog = item.DownloadPrevLogs();
                            var todayLog = item.DownloadTodayLogs();

                            await Machine.SaveLogsOrAttendanceInPc(prevLog, todayLog, ins, item.Device);
                        }
                    }


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
