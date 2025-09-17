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

        private DispatcherTimer _dialogTimer = new DispatcherTimer();


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
                    userView = new UserView { Name = "User Not found on PC", Enroll_Time = dt };
                    EnrollUserCard.DataContext = userView;
                    return;
                }

                userView.Enroll_Time = dt;
                var sDate = dt.ToShortDateString();

                EnrollUserCard.DataContext = userView;


                var isStuDisable = userView.Is_Student && !LocalData.Instance.institution.Is_Student_Attendance_Enable;
                var isEmpDisable = !userView.Is_Student && !LocalData.Instance.institution.Is_Employee_Attendance_Enable;
                var schedule = LocalData.Instance.GetUserSchedule(userView.ScheduleID);

                string reason;
                //Schedule data not found
                if (schedule == null)
                {
                    reason = "Schedule data not found";
                    await LogBackupInsert(deviceId, dt, reason);
                    return;
                }
                // Device Attendance Disable
                if (!LocalData.Instance.institution.Is_Device_Attendance_Enable)
                {
                    reason = "Device Attendance Disable";
                    await LogBackupInsert(deviceId, dt, reason);
                }
                // Student Attendance Disable
                else if (isStuDisable)
                {
                    reason = "Student Attendance Disable";
                    await LogBackupInsert(deviceId, dt, reason);
                }
                // Employee Attendance Disable
                else if (isEmpDisable)
                {
                    reason = "Employee Attendance Disable";
                    await LogBackupInsert(deviceId, dt, reason);
                }
                // Today Check
                else if (sDate != DateTime.Today.ToShortDateString())
                {
                    reason = "Not Current Data";
                    await LogBackupInsert(deviceId, dt, reason);
                }
                //Holiday attendance disable
                else if (LocalData.Instance.institution.Is_Today_Holiday && !LocalData.Instance.institution.Holiday_NotActive)
                {
                    reason = "Holiday attendance disable";
                    await LogBackupInsert(deviceId, dt, reason);
                }
                //Schedule Off day
                else if (!schedule.Is_OnDay)
                {
                    reason = "Schedule Off Day";
                    await LogBackupInsert(deviceId, dt, reason);
                }
                // Insert or Update Attendance Records
                else
                {
                    using (var db = new ModelContext())
                    {
                        var attRecords = await db.attendance_Records.Where(a => a.DeviceID == deviceId).ToListAsync();

                        var attRecord = attRecords.FirstOrDefault(a => Convert.ToDateTime(a.AttendanceDate) == Convert.ToDateTime(sDate));

                        var sStartTime = TimeSpan.Parse(schedule.StartTime);
                        var sLateTime = TimeSpan.Parse(schedule.LateEntryTime);
                        var sEndTime = TimeSpan.Parse(schedule.EndTime);

                        if (attRecord == null)
                        {
                            attRecord = new Attendance_Record();
                            attRecord.AttendanceDate = sDate;
                            attRecord.DeviceID = deviceId;
                            attRecord.EntryTime = time.ToString();
                            if (time > sEndTime)
                            {
                                attRecord.AttendanceStatus = "Late Abs";
                            }
                            else
                            {
                                if (time <= sStartTime)
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
                            }
                            attRecord.Is_Sent = false;
                            attRecord.Is_Updated = false;
                            db.attendance_Records.Add(attRecord);
                            db.Entry(attRecord).State = EntityState.Added;
                        }
                        else
                        {
                            var isDuplicatePunch = false;

                            if (attRecord.Is_OUT)
                            {
                                if (TimeSpan.TryParse(attRecord.ExitStatus, out var previousTime))
                                {
                                    isDuplicatePunch = previousTime.TotalMinutes + DuplicatePunchCountableMin > time.TotalMinutes;
                                }
                            }
                            else
                            {

                                if (TimeSpan.TryParse(attRecord.EntryTime, out var previousTime))
                                {
                                    isDuplicatePunch = previousTime.TotalMinutes + DuplicatePunchCountableMin > time.TotalMinutes;
                                }
                            }

                            if (!isDuplicatePunch)
                            {
                                if (attRecord.AttendanceStatus == "Abs")
                                {
                                    attRecord.AttendanceStatus = "Late Abs";
                                    attRecord.EntryTime = time.ToString();
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
                }

                //string fromTime = DateTime.Today.ToString("yyyy-MM-dd 00:00:00");
                //string toTime = DateTime.Today.ToString("yyyy-MM-dd 23:00:00");

                //LogViewListBox.ItemsSource = Machine.GetDailyAttendanceRecords(AttType.All);

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

        private async Task LogBackupInsert(int deviceId, DateTime dt, string reason)
        {
            using (var db = new ModelContext())
            {
                var logBackup = new AttendanceLog_Backup()
                {
                    DeviceID = deviceId,
                    Entry_Date = dt.ToShortDateString(),
                    Entry_Time = dt.ToShortTimeString(),
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
            axCZKEM1 = new CZKEM();

            this.Device = device;
            Returns = new DeviceReturn();
        }

        public DeviceReturn ConnectDevice()
        {
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
                Returns.Message = "Device is already Connected!";
                Returns.Code = 0;
                Returns.IsSuccess = true;
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

                if (axCZKEM1.RegEvent(Machine.Number, 1))
                {
                    axCZKEM1.OnAttTransactionEx += axCZKEM1_OnAttTransactionEx;
                }

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
        public DeviceReturn ConnectDeviceWithoutEvent()
        {
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
            var pingCheck = Device_PingTest.PingHost(this.Device.DeviceIP);

            if (!pingCheck) return false;

            var a = -1;
            axCZKEM1.GetConnectStatus(ref a);
            return a == 0;
        }


        public async Task<bool> IsConnectedAsync()
        {
            var checkIp = await Device_PingTest.PingHostAsync(this.Device.DeviceIP);
            if (!checkIp) return false;

            var dwErrorCode = -1;
            axCZKEM1.GetConnectStatus(ref dwErrorCode);
            return dwErrorCode == 0;
        }

        public string DeviceSerialNumber()
        {
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

            axCZKEM1.GetDeviceTime(Machine.Number, ref idwYear, ref idwMonth, ref idwDay, ref idwHour, ref idwMinute, ref idwSecond);

            return new DateTime(idwYear, idwMonth, idwDay, idwHour, idwMinute, idwSecond);
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

        public void Upload_User(List<User> users)
        {
            axCZKEM1.EnableDevice(Machine.Number, false);
            try
            {
                var batchUpdate = axCZKEM1.BeginBatchUpdate(Machine.Number, 1);
                foreach (var user in users)
                {
                    axCZKEM1.SetStrCardNumber(user.RFID);
                    axCZKEM1.SSR_SetUserInfo(Machine.Number, user.DeviceID.ToString(), user.Name, "", 0, true);
                }

                if (!batchUpdate) return;
                axCZKEM1.BatchUpdate(Machine.Number);
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
                    axCZKEM1.GetStrCardNumber(out var rfId);

                    //RFID = "";
                    //if (axCZKEM1.GetStrCardNumber(out RFID))
                    //{
                    if (string.IsNullOrEmpty(rfId) || rfId == "0")
                        rfId = null;
                    //}
                    //if (!string.IsNullOrEmpty(name))
                    //{
                    //    int index = name.IndexOf("\0");
                    //    if (index > 0)
                    //    {
                    //        name = name.Substring(0, index);
                    //    }
                    //}

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
                        var sDate = dt.ToShortDateString();

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
                                var sDate = dt.ToShortDateString();

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
                        var sDate = dt.ToShortDateString();

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
                                var sDate = dt.ToShortDateString();

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
                        var sDate = dt.ToShortDateString();

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
