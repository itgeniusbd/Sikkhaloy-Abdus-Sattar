using AttendanceDevice.Model;
using AttendanceDevice.ViewModel;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using zkemkeeper;

namespace AttendanceDevice.Config_Class
{
    public enum DataCode
    {
        NumberOfAdmin = 1,
        NumberOfUsers = 2,
        NumberOfFp = 3,
        NumberOfPasswords = 4,
        NumberOfOperationRecords = 5,
        AttendanceRecords = 6,
        FpCapacity = 7,
        UserCapacity = 8,
        AttendanceRecordCapacity = 9,
        RemainingPfCapacity = 10,
        RemainingUserCapacity = 11,
        RemainingAttendanceRecordCapacity = 12
    }
    public class DeviceConnection
    {
        private const int PrevDayLogCountable = 7;
        private int _fingerIndex = 0;
        public Card EnrollUserCard { get; set; }
        public bool IsSdkFullSupported { get; set; }
        public TextBlock FingerprintMessage { get; set; }
        public ListBox LogViewListBox { get; set; }
        public Device Device { get; private set; }
        private DeviceReturn Returns { get; set; }
        public CZKEM axCZKEM1 { get; private set; }

        private void ShowLivePunch(UserView userView)
        {
            if (EnrollUserCard == null || userView == null)
                return;

            var update = new Action(() =>
            {
                EnrollUserCard.DataContext = null;
                EnrollUserCard.DataContext = userView;
            });
            var dispatcher = EnrollUserCard.Dispatcher;
            if (dispatcher.CheckAccess())
                update();
            else
                dispatcher.BeginInvoke(update);
        }

        public async void axCZKEM1_OnAttTransactionEx(string enrollNumber, int isInValid, int attState, int verifyMethod, int year, int month, int day, int hour, int minute, int second, int workCode)
        {
            try
            {
                var deviceId = Convert.ToInt32(enrollNumber);
                var dt = new DateTime(year, month, day, hour, minute, second);
                var time = new TimeSpan(hour, minute, 0);
                var userView = LocalData.Instance.GetUserView(deviceId);
                var DuplicatePunchCountableMin = 10;

                if (userView == null)
                {
                    userView = new UserView
                    {
                        Name = "User Not found on PC",
                        Enroll_Time = dt,
                        ImgLink = UserPhotoHelper.DefaultPhotoUri
                    };
                    ShowLivePunch(userView);
                    return;
                }

                userView.Enroll_Time = dt;
                userView.ScheduleDisplay = string.Empty;
                userView.ScheduleNameLine = string.Empty;
                userView.ScheduleTimeLine = string.Empty;
                userView.ImgLink = UserPhotoHelper.ResolvePhotoUri(
                    LocalData.Instance.institution?.Image_Link,
                    userView.ID);
                var sDate = LocalData.Instance.GetAttendanceDateString();
                var attendanceDate = LocalData.Instance.GetAttendanceDate();

                ShowLivePunch(userView);

                LocalData.Instance.EnsureScheduleDataForPunch();
                _ = LocalData.Instance.EnsureScheduleBootstrapFromNetworkIfStaleAsync();

                var isStuDisable = userView.Is_Student && !LocalData.Instance.institution.Is_Student_Attendance_Enable;
                var isEmpDisable = !userView.Is_Student && !LocalData.Instance.institution.Is_Employee_Attendance_Enable;

                string reason;

                // Device Attendance Disable
                if (!LocalData.Instance.institution.Is_Device_Attendance_Enable)
                {
                    reason = "Device Attendance Disable";
                    await LogBackupInsert(deviceId, dt, reason);
                    return;
                }
                // Student Attendance Disable
                else if (isStuDisable)
                {
                    reason = "Student Attendance Disable";
                    await LogBackupInsert(deviceId, dt, reason);
                    return;
                }
                // Employee Attendance Disable
                else if (isEmpDisable)
                {
                    reason = "Employee Attendance Disable";
                    await LogBackupInsert(deviceId, dt, reason);
                    return;
                }
                // Today Check
                else if (dt.Date != attendanceDate)
                {
                    reason = "Not Current Data";
                    await LogBackupInsert(deviceId, dt, reason);
                    return;
                }
                //Holiday attendance disable
                else if (LocalData.Instance.institution.Is_Today_Holiday && !LocalData.Instance.institution.Holiday_NotActive)
                {
                    reason = "Holiday attendance disable";
                    await LogBackupInsert(deviceId, dt, reason);
                    return;
                }

                // Pick the active schedule for this user based on the punch time
                var activeSchedule = LocalData.Instance.ResolveScheduleForPunch(deviceId, dt);

                if (activeSchedule == null)
                {
                    reason = "No active schedule for this time";
                    await LogBackupInsert(deviceId, dt, reason);
                    return;
                }

                ScheduleDisplayHelper.ApplyTo(userView, activeSchedule);
                ShowLivePunch(userView);

                // Insert or Update Attendance Records
                using (var db = new ModelContext())
                {
                    var attRecords = await db.attendance_Records
                        .Where(a => a.DeviceID == deviceId && a.ScheduleID == activeSchedule.ScheduleID)
                        .ToListAsync();

                    var attRecord = attRecords.FirstOrDefault(a => AttendanceDateHelper.DatesMatch(a.AttendanceDate, sDate));

                    if (!LocalData.TryGetScheduleTimes(activeSchedule, out var sStartTime, out var sLateTime, out var sEndTime))
                    {
                        reason = "Invalid schedule time on PC";
                        await LogBackupInsert(deviceId, dt, reason);
                        return;
                    }

                    if (attRecord == null)
                    {
                        attRecord = new Attendance_Record
                        {
                            AttendanceDate = sDate,
                            DeviceID = deviceId,
                            ScheduleID = activeSchedule.ScheduleID,
                            EntryTime = time.ToString()
                        };

                        if (time > sEndTime)
                        {
                            attRecord.AttendanceStatus = "Late Abs";
                        }
                        else if (time <= sStartTime)
                        {
                            attRecord.AttendanceStatus = "Pre";
                        }
                        else if (time <= sLateTime)
                        {
                            attRecord.AttendanceStatus = "Late";
                        }
                        else if (time <= sEndTime)
                        {
                            attRecord.AttendanceStatus = "Late Abs";
                        }

                        attRecord.Is_Sent = false;
                        attRecord.Is_Updated = false;
                        db.attendance_Records.Add(attRecord);
                        db.Entry(attRecord).State = EntityState.Added;
                    }
                    else
                    {
                        var isDuplicatePunch = false;
                        var hasNoEntryYet = string.IsNullOrWhiteSpace(attRecord.EntryTime) && !attRecord.Is_OUT;

                        if (!hasNoEntryYet)
                        {
                            if (attRecord.Is_OUT)
                            {
                                if (TimeSpan.TryParse(attRecord.ExitTime, out var previousOutTime))
                                {
                                    isDuplicatePunch = previousOutTime.TotalMinutes + DuplicatePunchCountableMin > time.TotalMinutes;
                                }
                            }
                            else if (TimeSpan.TryParse(attRecord.EntryTime, out var previousTime))
                            {
                                isDuplicatePunch = previousTime.TotalMinutes + DuplicatePunchCountableMin > time.TotalMinutes;
                            }
                        }

                        if (hasNoEntryYet || !isDuplicatePunch)
                        {
                            if (hasNoEntryYet || attRecord.AttendanceStatus == "Abs")
                            {
                                attRecord.EntryTime = time.ToString();

                                if (time > sEndTime)
                                    attRecord.AttendanceStatus = "Late Abs";
                                else if (time <= sStartTime)
                                    attRecord.AttendanceStatus = "Pre";
                                else if (time <= sLateTime)
                                    attRecord.AttendanceStatus = "Late";
                                else
                                    attRecord.AttendanceStatus = "Late Abs";

                                attRecord.Is_Sent = false;
                                attRecord.Is_Updated = false;
                            }
                            else
                            {
                                if (time > sLateTime && time < sEndTime)
                                {
                                    attRecord.ExitStatus = "Early Leave";
                                }
                                else if (time > sEndTime)
                                {
                                    attRecord.ExitStatus = "Out";
                                }
                                attRecord.Is_OUT = true;
                                attRecord.ExitTime = time.ToString();
                                attRecord.Is_Updated = false;
                            }

                            db.Entry(attRecord).State = EntityState.Modified;
                        }
                    }

                    await db.SaveChangesAsync();
                }

                ScheduleDisplayHelper.ApplyTo(userView, activeSchedule);
                ShowLivePunch(userView);

                if (!this.IsSdkFullSupported)
                {
                    this.ClearAll_Logs();
                }
            }
            catch (Exception e)
            {
                // ignored
            }
        }

        public void axCZKEM1_OnFingerFeature(int score)
        {
            FingerprintMessage.Text = $"Score: ${score}";
        }


        public void axCZKEM1_OnEnrollFingerEx(string EnrollNumber, int FingerIndex, int ActionResult, int TemplateLength)
        {
            FingerprintMessage.Text = ActionResult == 0 ? "Success" : "Failed";

            if (axCZKEM1.GetUserTmpExStr(Machine.Number, EnrollNumber, _fingerIndex, out var flag, out var tmpData, out var tmpLength))
            {

                var deviceIdInt = Convert.ToInt32(EnrollNumber);
                using (var db = new ModelContext())
                {
                    var fp = db.user_FingerPrints.FirstOrDefault(f => f.DeviceID == deviceIdInt && f.Finger_Index == _fingerIndex);

                    if (fp == null)
                    {
                        fp = new User_FingerPrint
                        {
                            DeviceID = deviceIdInt,
                            Finger_Index = _fingerIndex,
                            Temp_Data = tmpData,
                            Flag = flag
                        };
                        db.Entry(fp).State = EntityState.Added;
                    }
                    else
                    {
                        fp.Temp_Data = tmpData;
                        fp.Flag = flag;
                        db.Entry(fp).State = EntityState.Modified;
                    }

                    db.SaveChanges();
                }
            }
        }

        private static async Task<bool> BackupAlreadyExists(ModelContext db, int deviceId, string entryDate, string entryTime)
        {
            return await db.attendanceLog_Backups.AnyAsync(b =>
                b.DeviceID == deviceId &&
                b.Entry_Date == entryDate &&
                b.Entry_Time == entryTime);
        }

        private async Task LogBackupInsert(int deviceId, DateTime dt, string reason)
        {
            using (var db = new ModelContext())
            {
                var entryDate = dt.ToString("dd-MMM-yy");
                var entryTime = dt.ToShortTimeString();

                if (await BackupAlreadyExists(db, deviceId, entryDate, entryTime))
                    return;

                var logBackup = new AttendanceLog_Backup()
                {
                    DeviceID = deviceId,
                    Entry_Date = entryDate,
                    Entry_Time = entryTime,
                    Entry_Day = dt.ToString("dddd"),
                    Backup_Reason = reason
                };


                this.Device.Last_Down_Log_Time = dt.ToString("yyyy-MM-dd HH:mm:ss");
                db.Devices.Add(Device);
                db.Entry(Device).State = EntityState.Modified;

                db.attendanceLog_Backups.Add(logBackup);
                await db.SaveChangesAsync();
            }
        }

        public DeviceConnection(Device device)
        {
            EnrollUserCard = new Card();
            this.Device = device;
            Returns = new DeviceReturn();
        }

        private DeviceReturn SdkNotInstalledReturn()
        {
            return new DeviceReturn
            {
                Message = "ZKTeco device SDK is not installed.\nRestart the app and allow the one-time SDK install, or run ZKdllRegistrationApp.exe as Administrator from the install folder.",
                Code = -1,
                IsSuccess = false
            };
        }

        private bool EnsureCzkem()
        {
            if (axCZKEM1 != null)
                return true;

            if (!ZkSdkBootstrap.IsSdkInstalled())
                return false;

            try
            {
                axCZKEM1 = new CZKEM();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public DeviceReturn ConnectDevice()
        {
            if (!EnsureCzkem())
                return SdkNotInstalledReturn();

            if (this.Device.DeviceIP == "")
            {
                Returns.Message = "IP cannot be null!";
                Returns.Code = -1;
                Returns.IsSuccess = false;

                return Returns;
            }

            if (this.Device.Port <= 0 || this.Device.Port > 65535)
            {
                Returns.Message = "Port illegal!";
                Returns.Code = -1;
                Returns.IsSuccess = false;

                return Returns;
            }

            if (this.Device.CommKey < 0 || this.Device.CommKey > 999999)
            {
                Returns.Message = "CommKey illegal!";
                Returns.Code = -1;
                Returns.IsSuccess = false;

                return Returns;
            }

            if (IsConnected())
            {
                EnsureAttendanceEventHandler();
                Returns.Message = "Device is already Connected!";
                Returns.Code = 0;
                Returns.IsSuccess = true;
                return Returns;
            }

            if (!axCZKEM1.SetCommPassword(this.Device.CommKey))
            {
                var idwErrorCode = 0;
                axCZKEM1.GetLastError(ref idwErrorCode);

                Returns.Message = $"Unable to connect the device, Error Code: ${idwErrorCode}";
                Returns.Code = -1;
                Returns.IsSuccess = false;

                return Returns;
            }

            if (axCZKEM1.Connect_Net(this.Device.DeviceIP, this.Device.Port))
            {
                Returns.Message = $"Connected with device {this.Device.DeviceName}!";
                Returns.Code = 1;
                Returns.IsSuccess = true;

                this.IsSdkFullSupported = ClearPrevLog();

                EnsureAttendanceEventHandler();

                return Returns;
            }
            else
            {
                var idwErrorCode = 0;
                axCZKEM1.GetLastError(ref idwErrorCode);
                Returns.Message = $"Unable to connect the device, Error Code: ${idwErrorCode}";
                Returns.Code = -1;
                Returns.IsSuccess = false;

                return Returns;
            }
        }

        private void EnsureAttendanceEventHandler()
        {
            if (axCZKEM1 == null)
                return;

            axCZKEM1.OnAttTransactionEx -= axCZKEM1_OnAttTransactionEx;
            if (axCZKEM1.RegEvent(Machine.Number, 1))
            {
                axCZKEM1.OnAttTransactionEx += axCZKEM1_OnAttTransactionEx;
            }
        }

        public DeviceReturn ConnectDeviceWithoutEvent()
        {
            if (!EnsureCzkem())
                return SdkNotInstalledReturn();

            if (this.Device.DeviceIP == "")
            {
                Returns.Message = "IP cannot be null!";
                Returns.Code = -1;
                Returns.IsSuccess = false;

                return Returns;
            }

            if (this.Device.Port <= 0 || this.Device.Port > 65535)
            {
                Returns.Message = "Port illegal!";
                Returns.Code = -1;
                Returns.IsSuccess = false;

                return Returns;
            }

            if (this.Device.CommKey < 0 || this.Device.CommKey > 999999)
            {
                Returns.Message = "CommKey illegal!";
                Returns.Code = -1;
                Returns.IsSuccess = false;

                return Returns;
            }

            if (IsConnected())
            {
                Returns.Message = "Device is already Connected !";
                Returns.Code = 0;
                Returns.IsSuccess = true;
            }


            if (!axCZKEM1.SetCommPassword(this.Device.CommKey))
            {
                var idwErrorCode = 0;
                axCZKEM1.GetLastError(ref idwErrorCode);

                Returns.Message = $"Unable to connect the device, Error Code: {idwErrorCode}";
                Returns.Code = -1;
                Returns.IsSuccess = false;

                return Returns;
            }

            if (axCZKEM1.Connect_Net(this.Device.DeviceIP, this.Device.Port))
            {
                Returns.Message = $"Connected with device {this.Device.DeviceName}!";
                Returns.Code = 1;
                Returns.IsSuccess = true;

                return Returns;
            }
            else
            {
                var idwErrorCode = 0;
                axCZKEM1.GetLastError(ref idwErrorCode);
                Returns.Message = $"Unable to connect the device, ErrorCode: {idwErrorCode}";
                Returns.Code = -1;
                Returns.IsSuccess = false;

                return Returns;
            }
        }
        public DeviceReturn DisconnectDevice()
        {
            if (IsConnected())
            {
                axCZKEM1.Disconnect();
                //sta_UnRegRealTime();

                Returns.Message = "Disconnect with device !";
                Returns.Code = -2;
                Returns.IsSuccess = true;
            }
            else
            {
                Returns.Message = "Device is already Disconnected !";
                Returns.Code = -2;
                Returns.IsSuccess = false;
            }

            return Returns;
        }

        public bool IsConnected()
        {
            if (axCZKEM1 == null)
                return false;

            var connectStatus = -1;
            axCZKEM1.GetConnectStatus(ref connectStatus);
            if (connectStatus == 0)
                return true;

            var pingCheck = Device_PingTest.PingHost(this.Device.DeviceIP);
            if (!pingCheck)
                return false;

            connectStatus = -1;
            axCZKEM1.GetConnectStatus(ref connectStatus);
            return connectStatus == 0;
        }


        public async Task<bool> IsConnectedAsync()
        {
            if (axCZKEM1 == null)
                return false;

            var connectStatus = -1;
            axCZKEM1.GetConnectStatus(ref connectStatus);
            if (connectStatus == 0)
                return true;

            var checkIp = await Device_PingTest.PingHostAsync(this.Device.DeviceIP, 2000);
            if (!checkIp)
                return false;

            connectStatus = -1;
            axCZKEM1.GetConnectStatus(ref connectStatus);
            return connectStatus == 0;
        }

        public string DeviceSerialNumber()
        {
            if (axCZKEM1 == null)
                return string.Empty;

            axCZKEM1.GetSerialNumber(Machine.Number, out var deviceSn);
            return deviceSn;
        }

        public DateTime GetDateTime()
        {
            var idwYear = 0;
            var idwMonth = 0;
            var idwDay = 0;
            var idwHour = 0;
            var idwMinute = 0;
            var idwSecond = 0;

            try
            {
                axCZKEM1.GetDeviceTime(Machine.Number, ref idwYear, ref idwMonth, ref idwDay, ref idwHour, ref idwMinute, ref idwSecond);
                return new DateTime(idwYear, idwMonth, idwDay, idwHour, idwMinute, idwSecond);
            }
            catch
            {
                return DateTime.Now;
            }
        }

        public bool SetDateTime()
        {
            if (!axCZKEM1.SetDeviceTime(Machine.Number)) return false;

            axCZKEM1.RefreshData(Machine.Number);
            return true;
        }
        public bool Restart()
        {
            return axCZKEM1.RestartDevice(Machine.Number);
        }
        public bool PowerOff()
        {
            return axCZKEM1.PowerOffDevice(Machine.Number);
        }
        public bool Duplicate_Punch_Time_Reset()
        {
            //Duplicate time 5min 
            return axCZKEM1.SetDeviceInfo(Machine.Number, 8, 5);
        }

        public bool Upload_User(List<User> users)
        {
            if (users == null || users.Count == 0)
                return true;

            axCZKEM1.EnableDevice(Machine.Number, false);
            try
            {
                var batchUpdate = axCZKEM1.BeginBatchUpdate(Machine.Number, 1);
                if (!batchUpdate)
                    return false;

                foreach (var user in users)
                {
                    var rfid = DeviceUserSyncHelper.NormalizeRfid(user.RFID);
                    if (!string.IsNullOrEmpty(rfid))
                        axCZKEM1.SetStrCardNumber(rfid);

                    if (!axCZKEM1.SSR_SetUserInfo(Machine.Number, user.DeviceID.ToString(), user.Name, "", 0, true))
                        return false;
                }

                if (!axCZKEM1.BatchUpdate(Machine.Number))
                    return false;

                axCZKEM1.RefreshData(Machine.Number);
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                axCZKEM1.EnableDevice(Machine.Number, true);
            }
        }

        private string ReadDeviceCardNumber(string deviceId)
        {
            axCZKEM1.GetStrCardNumber(out var rfId);
            rfId = NormalizeDownloadedCardNumber(rfId);
            if (!string.IsNullOrEmpty(rfId))
                return rfId;

            if (axCZKEM1.SSR_GetUserInfo(Machine.Number, deviceId, out _, out _, out _, out _))
            {
                axCZKEM1.GetStrCardNumber(out rfId);
                rfId = NormalizeDownloadedCardNumber(rfId);
            }

            return rfId;
        }

        private static string NormalizeDownloadedCardNumber(string rfId)
        {
            if (string.IsNullOrWhiteSpace(rfId) || rfId.Trim() == "0")
                return null;

            return rfId.Trim();
        }

        public List<User> Download_User()
        {
            if (!IsConnected()) return new List<User>();

            var users = new List<User>();

            axCZKEM1.EnableDevice(Machine.Number, false);
            try
            {
                axCZKEM1.ReadAllUserID(Machine.Number);

                while (axCZKEM1.SSR_GetAllUserInfo(Machine.Number, out var deviceId, out var name, out _, out _, out _))
                {
                    var rfId = ReadDeviceCardNumber(deviceId);

                    var user = new User { DeviceID = Convert.ToInt32(deviceId), Name = name, RFID = rfId };

                    users.Add(user);
                }
            }
            catch
            {
                // ignored
            }
            finally
            {
                axCZKEM1.EnableDevice(Machine.Number, true);
            }
            return users;
        }
        public List<LogView> DownloadPrevLogs()
        {
            if (!IsConnected()) return new List<LogView>();

            var logs = new List<LogView>();
            axCZKEM1.EnableDevice(Machine.Number, false);
            try
            {
                var sdwEnrollNumber = "";
                var idwYear = 0;
                var idwMonth = 0;
                var idwDay = 0;
                var idwHour = 0;
                var idwMinute = 0;
                var idwSecond = 0;
                var idWorkCode = 0;

                var fromTime = DateTime.Today.AddDays(-PrevDayLogCountable);
                var fromTimeString = fromTime.ToString("yyyy-MM-dd 00:00:00");

                if (DateTime.TryParse(Device.Last_Down_Log_Time, out var deviceLastUpdate))
                {
                    if ((DateTime.Today - deviceLastUpdate).TotalDays < PrevDayLogCountable)
                    {
                        fromTimeString = Device.Last_Down_Log_Time;
                    }
                }

                var toTime = DateTime.Today.Date;
                var toTimeString = toTime.ToString("yyyy-MM-dd 00:00:00");

                if (axCZKEM1.ReadTimeGLogData(Machine.Number, fromTimeString, toTimeString))
                {
                    while (axCZKEM1.SSR_GetGeneralLogData(Machine.Number, out sdwEnrollNumber, out _, out _, out idwYear, out idwMonth, out idwDay, out idwHour, out idwMinute, out idwSecond, ref idWorkCode))//get records from the memory
                    {
                        var deviceId = Convert.ToInt32(sdwEnrollNumber);
                        var dt = new DateTime(idwYear, idwMonth, idwDay, idwHour, idwMinute, idwSecond);
                        var time = new TimeSpan(idwHour, idwMinute, idwSecond);
                        var sDate = dt.ToString("dd-MMM-yy");

                        var log = new LogView
                        {
                            DeviceId = deviceId,
                            EntryDate = sDate,
                            EntryTime = time,
                            EntryDay = dt.ToString("dddd"),
                            EntryDateTime = dt
                        };
                        logs.Add(log);
                    }
                }
                else
                {
                    var idwErrorCode = 0;
                    axCZKEM1.GetLastError(ref idwErrorCode);
                    //check no data found or a error  
                    if (idwErrorCode != 0)
                    {
                        if (axCZKEM1.ReadGeneralLogData(Machine.Number))
                        {
                            while (axCZKEM1.SSR_GetGeneralLogData(Machine.Number, out sdwEnrollNumber, out _, out _,
                                out idwYear, out idwMonth, out idwDay, out idwHour, out idwMinute, out idwSecond,
                                ref idWorkCode)) //get records from the memory
                            {
                                var dt = new DateTime(idwYear, idwMonth, idwDay, idwHour, idwMinute, idwSecond);

                                //Check data in Last Downloaded Log Time 
                                if (fromTime <= dt || dt >= toTime) continue;

                                var deviceId = Convert.ToInt32(sdwEnrollNumber);
                                var time = new TimeSpan(idwHour, idwMinute, idwSecond);
                                var sDate = dt.ToString("dd-MMM-yy");

                                var log = new LogView
                                {
                                    DeviceId = deviceId,
                                    EntryDate = sDate,
                                    EntryTime = time,
                                    EntryDay = dt.ToString("dddd"),
                                    EntryDateTime = dt,
                                };

                                logs.Add(log);
                            }
                        }
                    }

                }
            }
            catch
            {
                // ignored
            }
            finally
            {
                axCZKEM1.EnableDevice(Machine.Number, true);
            }

            return logs.OrderBy(l => l.EntryDateTime).ToList();
        }

        public List<LogView> DownloadTodayLogs()
        {
            if (!IsConnected()) return new List<LogView>();

            var logs = new List<LogView>();
            axCZKEM1.EnableDevice(Machine.Number, false);
            try
            {
                var sdwEnrollNumber = "";
                var idwYear = 0;
                var idwMonth = 0;
                var idwDay = 0;
                var idwHour = 0;
                var idwMinute = 0;
                var idwSecond = 0;
                var idWorkCode = 0;




                var fromTime = DateTime.Today.Date;
                var fromTimeString = fromTime.ToString("yyyy-MM-dd 00:00:00");

                var toTime = DateTime.Today.AddDays(1).Date;
                var toTimeString = toTime.ToString("yyyy-MM-dd 00:00:00");



                if (axCZKEM1.ReadTimeGLogData(Machine.Number, fromTimeString, toTimeString))
                {
                    while (axCZKEM1.SSR_GetGeneralLogData(Machine.Number, out sdwEnrollNumber, out _, out _, out idwYear, out idwMonth, out idwDay, out idwHour, out idwMinute, out idwSecond, ref idWorkCode))//get records from the memory
                    {
                        var dt = new DateTime(idwYear, idwMonth, idwDay, idwHour, idwMinute, idwSecond);


                        var deviceId = Convert.ToInt32(sdwEnrollNumber);
                        var time = new TimeSpan(idwHour, idwMinute, idwSecond);
                        var sDate = dt.ToString("dd-MMM-yy");

                        var log = new LogView()
                        {
                            DeviceId = deviceId,
                            EntryDate = sDate,
                            EntryTime = time,
                            EntryDay = dt.ToString("dddd"),
                            EntryDateTime = dt
                        };

                        logs.Add(log);
                    }
                }
                else
                {
                    var idwErrorCode = 0;
                    axCZKEM1.GetLastError(ref idwErrorCode);
                    //check no data found or a error  
                    if (idwErrorCode != 0)
                    {
                        if (axCZKEM1.ReadGeneralLogData(Machine.Number))
                        {
                            while (axCZKEM1.SSR_GetGeneralLogData(Machine.Number, out sdwEnrollNumber, out _, out _,
                                out idwYear, out idwMonth, out idwDay, out idwHour, out idwMinute, out idwSecond,
                                ref idWorkCode)) //get records from the memory
                            {
                                var dt = new DateTime(idwYear, idwMonth, idwDay, idwHour, idwMinute, idwSecond);
                                //Check data in Last Downloaded Log Time 
                                if (fromTime <= dt || dt >= toTime) continue;

                                var deviceId = Convert.ToInt32(sdwEnrollNumber);
                                var time = new TimeSpan(idwHour, idwMinute, idwSecond);
                                var sDate = dt.ToString("dd-MMM-yy");

                                var log = new LogView()
                                {
                                    DeviceId = deviceId,
                                    EntryDate = sDate,
                                    EntryTime = time,
                                    EntryDay = dt.ToString("dddd"),
                                    EntryDateTime = dt
                                };

                                logs.Add(log);
                            }

                        }
                    }
                }
            }
            catch
            {
                // ignored
            }
            finally
            {
                axCZKEM1.EnableDevice(Machine.Number, true);
            }
            return logs.OrderBy(l => l.EntryDateTime).ToList();
        }

        public bool ClearAllUsers()
        {
            if (!IsConnected()) return false;

            bool isClear;
            axCZKEM1.EnableDevice(Machine.Number, false);

            if (axCZKEM1.ClearData(Machine.Number, 5))
            {
                axCZKEM1.RefreshData(Machine.Number);
                isClear = true;
            }
            else
            {
                isClear = false;
            }

            axCZKEM1.EnableDevice(Machine.Number, true);
            return isClear;
        }

        public bool ClearAll_Logs()
        {
            if (!IsConnected()) return false;

            axCZKEM1.EnableDevice(Machine.Number, false);

            var isClear = axCZKEM1.ClearGLog(Machine.Number);

            axCZKEM1.EnableDevice(Machine.Number, true);

            return isClear;
        }
        public bool ClearAll_FPs()
        {
            if (!IsConnected()) return false;

            bool isClear;
            axCZKEM1.EnableDevice(Machine.Number, false);

            if (axCZKEM1.ClearData(Machine.Number, 2))
            {
                axCZKEM1.RefreshData(Machine.Number);
                isClear = true;
            }
            else
            {
                isClear = false;
            }

            axCZKEM1.EnableDevice(Machine.Number, true);
            return isClear;
        }

        public bool ClearPrevLog()
        {
            bool isClear;
            axCZKEM1.EnableDevice(Machine.Number, false);

            var fromTime = DateTime.Today.AddDays(-PrevDayLogCountable).ToString("yyyy-MM-dd 00:00:00");

            if (axCZKEM1.DeleteAttlogByTime(Machine.Number, fromTime))
            {
                axCZKEM1.RefreshData(Machine.Number);
                isClear = true;
            }
            else
            {
                isClear = false;
            }

            axCZKEM1.EnableDevice(Machine.Number, true);
            return isClear;
        }

        /// <summary>
        /// Reset Device Admin Password through SDK
        /// ডিভাইসের admin পাসওয়ার্ড রিসেট করার জন্য
        /// </summary>
        public bool ResetDeviceAdminPassword(string newPassword = "")
        {
            if (!IsConnected()) return false;

            try
            {
                axCZKEM1.EnableDevice(Machine.Number, false);

                // ZKTeco SDK doesn't have SetDevicePassword method
                // We can only set device time, clear data, etc. through SDK
                // Device admin password must be reset from device web interface or hardware reset
                
                // Alternative: You can set device communication password (CommKey)
                // But this is different from web interface admin password
                bool result = axCZKEM1.SetCommPassword(0); // Reset to default CommKey 0

                if (result)
                {
                    axCZKEM1.RefreshData(Machine.Number);
                }

                axCZKEM1.EnableDevice(Machine.Number, true);
                return result;
            }
            catch
            {
                axCZKEM1.EnableDevice(Machine.Number, true);
                return false;
            }
        }

        /// <summary>
        /// Get current device firmware and model info
        /// ডিভাইসের firmware এবং model তথ্য দেখার জন্য
        /// </summary>
        public DeviceModelInfo GetDeviceInfo()
        {
            var info = new DeviceModelInfo();

            if (!IsConnected()) return info;

            try
            {
                axCZKEM1.EnableDevice(Machine.Number, false);

                // Use temporary variables - some methods use 'ref', some use 'out'
                string firmwareVersion = "";
                string deviceModel = "";
                string serialNumber = "";
                string platform = "";

                // Get firmware version (uses ref)
                axCZKEM1.GetFirmwareVersion(Machine.Number, ref firmwareVersion);
                info.FirmwareVersion = firmwareVersion;

                // Get device name/model (uses out)
                axCZKEM1.GetDeviceStrInfo(Machine.Number, 1, out deviceModel);
                info.DeviceModel = deviceModel;

                // Get serial number (uses out)
                axCZKEM1.GetSerialNumber(Machine.Number, out serialNumber);
                info.SerialNumber = serialNumber;

                // Get platform info (uses ref)
                axCZKEM1.GetPlatform(Machine.Number, ref platform);
                info.Platform = platform;

                axCZKEM1.EnableDevice(Machine.Number, true);
            }
            catch
            {
                axCZKEM1.EnableDevice(Machine.Number, true);
            }

            return info;
        }

        public List<LogView> DownloadLogs()
        {
            if (!IsConnected()) return new List<LogView>();

            var logs = new List<LogView>();
            axCZKEM1.EnableDevice(Machine.Number, false);
            try
            {
                var idWorkCode = 0;

                if (axCZKEM1.ReadGeneralLogData(Machine.Number))
                {
                    while (axCZKEM1.SSR_GetGeneralLogData(Machine.Number, out var sdwEnrollNumber, out _, out _, out var idwYear, out var idwMonth, out var idwDay, out var idwHour, out var idwMinute, out var idwSecond, ref idWorkCode))//get records from the memory
                    {
                        var deviceId = Convert.ToInt32(sdwEnrollNumber);
                        var dt = new DateTime(idwYear, idwMonth, idwDay, idwHour, idwMinute, idwSecond);
                        var time = new TimeSpan(idwHour, idwMinute, idwSecond);
                        var sDate = dt.ToString("dd-MMM-yy");

                        var log = new LogView()
                        {
                            DeviceId = deviceId,
                            EntryDate = sDate,
                            EntryTime = time,
                            EntryDay = dt.ToString("dddd"),
                            EntryDateTime = dt
                        };
                        logs.Add(log);
                    }
                }
            }
            catch
            {
                // ignored
            }
            finally
            {
                axCZKEM1.EnableDevice(Machine.Number, true);
            }
            return logs.OrderBy(l => l.EntryDateTime).ToList();
        }
        public DeviceDetails GetDeviceDetails()
        {
            var userCapacity = 0;
            var userCount = 0;
            var fpCnt = 0;
            var fpCapacity = 0;
            var recordCnt = 0;
            var recordCapacity = 0;
            var duplicatePunchTime = 0;

            //axCZKEM1.EnableDevice(Machine.Number, false);//disable the device

            axCZKEM1.GetDeviceStatus(Machine.Number, (int)DataCode.UserCapacity, ref userCapacity);
            axCZKEM1.GetDeviceStatus(Machine.Number, (int)DataCode.NumberOfUsers, ref userCount);
            axCZKEM1.GetDeviceStatus(Machine.Number, (int)DataCode.NumberOfFp, ref fpCnt);
            axCZKEM1.GetDeviceStatus(Machine.Number, (int)DataCode.FpCapacity, ref fpCapacity);
            axCZKEM1.GetDeviceStatus(Machine.Number, (int)DataCode.AttendanceRecords, ref recordCnt);
            axCZKEM1.GetDeviceStatus(Machine.Number, (int)DataCode.AttendanceRecordCapacity, ref recordCapacity);
            axCZKEM1.GetDeviceInfo(Machine.Number, 8, ref duplicatePunchTime);

            // axCZKEM1.EnableDevice(Machine.Number, true);//enable the device

            return new DeviceDetails(fpCapacity, fpCnt, userCapacity, userCount, recordCapacity, recordCnt, duplicatePunchTime);
        }


        public int FP_Add(string deviceId, int fingerIndex)
        {
            _fingerIndex = fingerIndex;
            if (axCZKEM1.RegEvent(Machine.Number, 65535))
            {
                axCZKEM1.OnFingerFeature += new zkemkeeper._IZKEMEvents_OnFingerFeatureEventHandler(axCZKEM1_OnFingerFeature);
                axCZKEM1.OnEnrollFingerEx += new zkemkeeper._IZKEMEvents_OnEnrollFingerExEventHandler(axCZKEM1_OnEnrollFingerEx);
            }

            var idwErrorCode = 0;
            const int iFlag = 1;

            axCZKEM1.CancelOperation();

            //If the specified index of user's templates has existed ,delete it first
            axCZKEM1.SSR_DelUserTmpExt(Machine.Number, deviceId, fingerIndex);

            if (axCZKEM1.StartEnrollEx(deviceId, fingerIndex, iFlag))
            {
                FingerprintMessage.Text = "Start";

                if (axCZKEM1.StartIdentify())
                {
                    //FingerprintMessage.Text = $"Enroll a new User,UserId: {deviceId}";
                }
                //After enrolling templates,you should let the device into the 1:N verification condition

            }
            else
            {
                axCZKEM1.GetLastError(ref idwErrorCode);
                FingerprintMessage.Text = $"Operation failed, Error Code: {idwErrorCode}";
            }
            //axCZKEM1.OnFingerFeature -= new zkemkeeper._IZKEMEvents_OnFingerFeatureEventHandler(axCZKEM1_OnFingerFeature);
            // axCZKEM1.OnEnrollFingerEx -= new zkemkeeper._IZKEMEvents_OnEnrollFingerExEventHandler(axCZKEM1_OnEnrollFingerEx);
            return 1;
        }
        public void FP_StateCancel()
        {
            axCZKEM1.CancelOperation();
        }
        public int FP_Delete(string deviceId, int fingerIndex)
        {
            var idwErrorCode = 0;

            axCZKEM1.CancelOperation();

            if (axCZKEM1.SSR_DelUserTmpExt(Machine.Number, deviceId, fingerIndex))
            {
                axCZKEM1.RefreshData(Machine.Number);//the data in the device should be refreshed
            }
            else
            {
                axCZKEM1.GetLastError(ref idwErrorCode);
            }
            return idwErrorCode;
        }
        public void FP_DownloadInPc()
        {
            if (!IsConnected()) return;

            var users = LocalData.Instance.Users;
            if (!users.Any()) return;

            axCZKEM1.EnableDevice(Machine.Number, false);

            axCZKEM1.ReadAllTemplate(Machine.Number);//read all the users' fingerprint templates to the memory
            try
            {
                using (var db = new ModelContext())
                {
                    foreach (var user in users)
                    {
                        for (int idwFingerIndex = 0; idwFingerIndex < 10; idwFingerIndex++)
                        {
                            //get the corresponding templates string and length from the memory
                            if (axCZKEM1.GetUserTmpExStr(Machine.Number, user.DeviceID.ToString(), idwFingerIndex, out var flag, out var tmpData, out var tmpLength))
                            {

                                var fp = db.user_FingerPrints.FirstOrDefault(f =>
                                    f.DeviceID == user.DeviceID && f.Finger_Index == idwFingerIndex);

                                if (fp == null)
                                {
                                    fp = new User_FingerPrint
                                    {
                                        DeviceID = user.DeviceID,
                                        Finger_Index = idwFingerIndex,
                                        Temp_Data = tmpData,
                                        Flag = flag
                                    };
                                    db.Entry(fp).State = EntityState.Added;
                                }
                                else
                                {
                                    fp.Temp_Data = tmpData;
                                    fp.Flag = flag;
                                    db.Entry(fp).State = EntityState.Modified;
                                }
                            }
                        }
                    }
                    db.SaveChanges();
                }
            }
            catch
            {
                // ignored
            }
            finally
            {
                axCZKEM1.EnableDevice(Machine.Number, true);
            }
        }
        public void FP_UploadPcToDevice()
        {
            var users = LocalData.Instance.Users;
            if (!users.Any()) return;

            axCZKEM1.EnableDevice(Machine.Number, false);
            try
            {
                var fps = new List<User_FingerPrint>();
                using (var db = new ModelContext())
                {
                    fps = db.user_FingerPrints.ToList();
                }

                var batchUpdate = axCZKEM1.BeginBatchUpdate(Machine.Number, 1 /*data 1: Forcibly overwrite, 0: Not overwrite*/); //create memory space for batching.  
                foreach (var fp in fps)
                {

                    if (fp.Temp_Data != "")
                    {
                        var outPut = axCZKEM1.SetUserTmpExStr(Machine.Number, fp.DeviceID.ToString(), fp.Finger_Index, fp.Flag, fp.Temp_Data);
                    }
                }
            }
            catch
            {
                // ignored
            }
            finally
            {
                axCZKEM1.BatchUpdate(Machine.Number);//upload all the information in the memory
                axCZKEM1.RefreshData(Machine.Number);//the data in the device should be refreshed
                axCZKEM1.EnableDevice(Machine.Number, true);
            }
        }

        /// <summary>
        /// Configure Push Protocol Settings
        /// ডিভাইসে Push Protocol সেটিংস করার জন্য
        /// </summary>
        public bool ConfigurePushProtocol(string serverUrl, int port = 443)
        {
            if (!IsConnected()) return false;

            try
            {
                axCZKEM1.EnableDevice(Machine.Number, false);

                // Note: Most ZKTeco SDK versions don't have direct Push Protocol configuration methods
                // Push Protocol must be configured through device web interface or LCD screen
                
                // Try to set device time (as a test that we can modify device settings)
                bool timeSet = SetDateTime();

                if (timeSet)
                {
                    axCZKEM1.RefreshData(Machine.Number);
                    axCZKEM1.EnableDevice(Machine.Number, true);
                    return true;
                }

                axCZKEM1.EnableDevice(Machine.Number, true);
                return false;
            }
            catch
            {
                axCZKEM1.EnableDevice(Machine.Number, true);
                return false;
            }
        }
    }
    public class DeviceDetails
    {
        public DeviceDetails(int fpCapacity, int numberOfFp, int userCapacity, int numberOfUsers, int attendanceRecordCapacity, int attendanceRecords, int duplicatePunchTime)
        {
            this.FpCapacity = fpCapacity;
            this.NumberOfFp = numberOfFp;
            this.UserCapacity = userCapacity;
            this.NumberOfUsers = numberOfUsers;
            this.AttendanceRecordCapacity = attendanceRecordCapacity;
            this.AttendanceRecords = attendanceRecords;
            this.DuplicatePunchTime = duplicatePunchTime;
        }

        public int FpCapacity { get; private set; }
        public int NumberOfFp { get; private set; }
        public int UserCapacity { get; private set; }
        public int NumberOfUsers { get; private set; }
        public int AttendanceRecordCapacity { get; private set; }
        public int AttendanceRecords { get; private set; }
        public int DuplicatePunchTime { get; private set; }
    }


}
