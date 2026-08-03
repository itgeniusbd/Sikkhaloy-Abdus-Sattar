using AttendanceDevice.APIClass;
using AttendanceDevice.Model;
using AttendanceDevice.ViewModel;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AttendanceDevice.Config_Class
{
    class LocalData
    {
        private LocalData()
        {
            using (var db = new ModelContext())
            {
                Users = db.Users.ToList()
                    .GroupBy(u => u.DeviceID)
                    .Select(g => g.First())
                    .ToList();
                institution = db.Institutions.FirstOrDefault();

                try
                {
                    User_Schedules = db.User_Schedules.ToList();
                }
                catch
                {
                    User_Schedules = new List<User_Schedule>();
                }

                RebuildUserViewCache();
            }
        }

        private static readonly Lazy<LocalData> lazy = new Lazy<LocalData>(() => new LocalData());
        public static LocalData Instance { get { return lazy.Value; } }
        public static Setting_Error Current_Error { get; set; } = new Setting_Error();
        public Institution institution { get; set; }
        public List<User> Users { get; set; } = new List<User>();
        public List<User_Schedule> User_Schedules { get; set; } = new List<User_Schedule>();

        private List<Attendance_Schedule_Day> _schedulesCache;
        private readonly Dictionary<int, UserView> _userViewByDeviceId = new Dictionary<int, UserView>();
        private DateTime _lastNetworkBootstrapUtc = DateTime.MinValue;
        private readonly SemaphoreSlim _bootstrapLock = new SemaphoreSlim(1, 1);

        public void InvalidateScheduleCache()
        {
            _schedulesCache = null;
        }

        public void RebuildUserViewCache()
        {
            _userViewByDeviceId.Clear();

            if (!Users.Any())
                return;

            foreach (var u in Users)
            {
                if (u.DeviceID <= 0)
                    continue;

                _userViewByDeviceId[u.DeviceID] = new UserView
                {
                    DeviceID = u.DeviceID,
                    ID = u.ID,
                    RFID = u.RFID,
                    Name = u.Name,
                    Designation = u.Designation,
                    ImgLink = UserPhotoHelper.ResolvePhotoUri(institution?.Image_Link, u.ID),
                    Is_Student = u.Is_Student,
                    ScheduleID = u.ScheduleID
                };
            }
        }

        public List<UserView> UserViews
        {
            get
            {
                if (!_userViewByDeviceId.Any() && Users.Any())
                    RebuildUserViewCache();

                return _userViewByDeviceId.Values.ToList();
            }
        }

        public UserView GetUserView(int deviceID)
        {
            if (!_userViewByDeviceId.Any() && Users.Any())
                RebuildUserViewCache();

            _userViewByDeviceId.TryGetValue(deviceID, out var view);
            return view;
        }

        // public List<Attendance_Schedule_Day> Schedules { get; set; } = new List<Attendance_Schedule_Day>();
        public DateTime GetAttendanceDate()
        {
            if (!string.IsNullOrWhiteSpace(institution?.ServerTodayDate) &&
                DateTime.TryParse(institution.ServerTodayDate, out var stored))
                return stored.Date;

            if (institution != null && institution.Current_Datetime != default(DateTime))
                return institution.Current_Datetime.Date;

            return DateTime.Today;
        }

        public string GetAttendanceDateString()
        {
            return GetAttendanceDate().ToString("dd-MMM-yy");
        }

        public int GetEffectiveSchoolId()
        {
            if (institution == null)
                return 0;

            if (institution.SchoolID > 0)
                return institution.SchoolID;

            if (int.TryParse(institution.UserName?.Trim(), out var parsed) && parsed > 0)
                return parsed;

            return 0;
        }

        private static bool IsSameAttendanceDate(string left, string right)
        {
            return DatesMatch(left, right);
        }

        /// <summary>Delete today's local attendance rows (does not touch server). Close sync loop first.</summary>
        public int ClearTodayLocalAttendance()
        {
            return ClearLocalAttendanceForDateRange(DateTime.Today, DateTime.Today);
        }

        /// <summary>Delete local attendance (+ backup logs) for a date range. Does not touch server.</summary>
        public int ClearLocalAttendanceForDateRange(DateTime fromDate, DateTime toDate)
        {
            fromDate = fromDate.Date;
            toDate = toDate.Date;
            if (toDate < fromDate)
                return 0;

            using (var db = new ModelContext())
            {
                var rows = db.attendance_Records
                    .AsEnumerable()
                    .Where(a => TryParseAttendanceDate(a.AttendanceDate, out var d) && d >= fromDate && d <= toDate)
                    .ToList();

                var backups = db.attendanceLog_Backups
                    .AsEnumerable()
                    .Where(b => TryParseAttendanceDate(b.Entry_Date, out var d) && d >= fromDate && d <= toDate)
                    .ToList();

                if (!rows.Any() && !backups.Any())
                    return 0;

                if (rows.Any())
                    db.attendance_Records.RemoveRange(rows);
                if (backups.Any())
                    db.attendanceLog_Backups.RemoveRange(backups);

                if (fromDate <= DateTime.Today && DateTime.Today <= toDate)
                {
                    foreach (var row in db.attendance_Schedule_Days.ToList())
                        row.Is_Abs_Count = false;
                }

                db.SaveChanges();
                return rows.Count;
            }
        }

        private static bool TryParseAttendanceDate(string value, out DateTime date)
        {
            return AttendanceDateHelper.TryParse(value, out date);
        }

        /// <summary>Stop re-sending today's rows to server after you wiped server data.</summary>
        public int MarkTodayAttendanceSynced()
        {
            var today = GetAttendanceDateString();
            using (var db = new ModelContext())
            {
                var rows = db.attendance_Records
                    .AsEnumerable()
                    .Where(a => IsSameAttendanceDate(a.AttendanceDate, today))
                    .ToList();

                foreach (var row in rows)
                {
                    row.Is_Sent = true;
                    row.Is_Updated = true;
                }

                db.SaveChanges();
                return rows.Count;
            }
        }

        public void UpdateServerDateTime(DateTime serverDateTime)
        {
            if (institution == null)
                return;

            institution.Current_Datetime = serverDateTime;
            institution.ServerTodayDate = serverDateTime.ToString("dd-MMM-yy");
        }

        public void MergeSchoolApiIntoInstitution(Institution ins, SchoolApiDto api)
        {
            if (ins == null || api == null)
                return;

            ins.SchoolID = api.SchoolID;
            ins.InstitutionName = api.InstitutionName;
            if (string.IsNullOrWhiteSpace(ins.Image_Link) && !string.IsNullOrWhiteSpace(api.Image_Link))
                ins.Image_Link = api.Image_Link;
            ins.Logo = api.Logo;
            ins.UserName = api.UserName?.Trim();
            ins.IsValid = api.IsValid;
            ins.SettingKey = api.SettingKey?.Trim();
            ins.Is_Device_Attendance_Enable = api.Is_Device_Attendance_Enable;
            ins.Is_Student_Attendance_Enable = api.Is_Student_Attendance_Enable;
            ins.Is_Employee_Attendance_Enable = api.Is_Employee_Attendance_Enable;
            ins.Is_Today_Holiday = api.Is_Today_Holiday;
            ins.Holiday_NotActive = api.Holiday_Active;
            ins.LastUpdateDate = api.LastUpdateDate;
            UpdateServerDateTime(api.Current_Datetime);
        }

        public async Task ApplySchoolStatusFromApiAsync(SchoolApiDto api)
        {
            if (institution == null || api == null)
                return;

            institution.Is_Today_Holiday = api.Is_Today_Holiday;
            institution.Holiday_NotActive = api.Holiday_Active;
            institution.Is_Device_Attendance_Enable = api.Is_Device_Attendance_Enable;
            institution.Is_Student_Attendance_Enable = api.Is_Student_Attendance_Enable;
            institution.Is_Employee_Attendance_Enable = api.Is_Employee_Attendance_Enable;
            institution.IsValid = api.IsValid;

            if (!string.IsNullOrWhiteSpace(api.SettingKey))
                institution.SettingKey = api.SettingKey.Trim();

            if (!string.IsNullOrWhiteSpace(api.LastUpdateDate))
                institution.LastUpdateDate = api.LastUpdateDate;

            UpdateServerDateTime(api.Current_Datetime);

            using (var db = new ModelContext())
            {
                var row = await db.Institutions.FirstOrDefaultAsync();
                if (row == null)
                    return;

                row.Is_Today_Holiday = institution.Is_Today_Holiday;
                row.Holiday_NotActive = institution.Holiday_NotActive;
                row.Is_Device_Attendance_Enable = institution.Is_Device_Attendance_Enable;
                row.Is_Student_Attendance_Enable = institution.Is_Student_Attendance_Enable;
                row.Is_Employee_Attendance_Enable = institution.Is_Employee_Attendance_Enable;
                row.IsValid = institution.IsValid;
                row.SettingKey = institution.SettingKey;
                row.LastUpdateDate = institution.LastUpdateDate;
                row.ServerTodayDate = institution.ServerTodayDate;
                await db.SaveChangesAsync();
            }
        }

        public List<Attendance_Schedule_Day> Schedules_Get()
        {
            if (_schedulesCache != null)
                return _schedulesCache;

            using (var db = new ModelContext())
                _schedulesCache = db.attendance_Schedule_Days.ToList();

            return _schedulesCache;
            //return Schedules.Select(a => new Attendance_Schedule_Day
            //{
            //    id = a.id,
            //    Day = a.Day,
            //    Is_OnDay = a.Is_OnDay,
            //    ScheduleID = a.ScheduleID,
            //    SchoolID = a.SchoolID,
            //    StartTime = Convert.ToDateTime(a.StartTime).ToString("hh:mm tt"),
            //    EndTime = Convert.ToDateTime(a.EndTime).ToString("hh:mm tt"),
            //    LateEntryTime = Convert.ToDateTime(a.LateEntryTime).ToString("hh:mm tt")
            //}).ToList();
        }

        /// <summary>
        /// One row per schedule for today's weekday (UI display only).
        /// </summary>
        public List<Attendance_Schedule_Day> GetTodayDisplaySchedules()
        {
            var now = DateTime.Now;
            var all = Schedules_Get();

            return all
                .GroupBy(s => s.ScheduleID)
                .Select(g => PickTodayRowForSchedule(g, now))
                .Where(s => s != null)
                .Select(s =>
                {
                    if (string.IsNullOrWhiteSpace(s.ScheduleName))
                        s.ScheduleName = $"Schedule {s.ScheduleID}";
                    return s;
                })
                .OrderBy(s => s.StartTime ?? string.Empty)
                .ToList();
        }

        private static Attendance_Schedule_Day PickTodayRowForSchedule(
            IEnumerable<Attendance_Schedule_Day> scheduleRows,
            DateTime date)
        {
            var rows = scheduleRows?.ToList() ?? new List<Attendance_Schedule_Day>();
            if (!rows.Any())
                return null;

            var todayRows = rows
                .Where(s => ScheduleDayHelper.IsSameDay(s.Day, date))
                .ToList();

            if (todayRows.Any())
            {
                // Show today's row even when only that day has custom times or Is_OnDay differs.
                return todayRows.FirstOrDefault(s => s.Is_OnDay) ?? todayRows.First();
            }

            return rows.FirstOrDefault();
        }

        public Attendance_Schedule_Day GetUserSchedule(int scheduleID)
        {
            return Schedules_Get().FirstOrDefault(u => u.ScheduleID == scheduleID);
        }

        public Attendance_Schedule_Day GetActiveSchedule(int deviceID, DateTime punchDateTime)
        {
            const int earlyPreMinutes = 120;

            EnsureUserScheduleAssignmentsFromUsers();
            var time = punchDateTime.TimeOfDay;

            var user = Users?.FirstOrDefault(u => u.DeviceID == deviceID);
            if (user == null)
            {
                using (var db = new ModelContext())
                    user = db.Users.FirstOrDefault(u => u.DeviceID == deviceID);
            }

            if (user == null)
                return null;

            var userSchedules = User_Schedules ?? new List<User_Schedule>();
            var scheduleIds = userSchedules
                .Where(us => us.DeviceID == deviceID)
                .Select(us => us.ScheduleID)
                .Distinct()
                .ToList();

            if (!scheduleIds.Any() && user.ScheduleID > 0)
                scheduleIds.Add(user.ScheduleID);

            var allSchedules = Schedules_Get();
            var knownScheduleIds = allSchedules.Select(s => s.ScheduleID).Distinct().ToHashSet();
            scheduleIds = scheduleIds.Where(id => knownScheduleIds.Contains(id)).Distinct().ToList();

            if (!scheduleIds.Any() && user.ScheduleID > 0 && knownScheduleIds.Contains(user.ScheduleID))
                scheduleIds.Add(user.ScheduleID);

            if (!scheduleIds.Any())
                return null;

            var todayRows = FilterScheduleDaysForPunch(allSchedules, punchDateTime)
                .Where(s => scheduleIds.Contains(s.ScheduleID))
                .ToList();

            if (!todayRows.Any())
            {
                todayRows = allSchedules
                    .Where(s => scheduleIds.Contains(s.ScheduleID) && s.Is_OnDay)
                    .GroupBy(s => s.ScheduleID)
                    .Select(g => g.First())
                    .ToList();
            }

            if (!todayRows.Any())
            {
                todayRows = allSchedules
                    .Where(s => scheduleIds.Contains(s.ScheduleID))
                    .GroupBy(s => s.ScheduleID)
                    .Select(g => g.First())
                    .ToList();
            }

            if (!todayRows.Any())
                return null;

            var parsedCandidates = todayRows
                .Select(s => new
                {
                    Schedule = s,
                    Start = ScheduleTimeHelper.TryParse(s.StartTime, out var start) ? start : (TimeSpan?)null,
                    End = ScheduleTimeHelper.TryParse(s.EndTime, out var end) ? end : (TimeSpan?)null
                })
                .Where(x => x.Start.HasValue && x.End.HasValue)
                .ToList();

            if (!parsedCandidates.Any())
                return null;

            // Morning punches must not route to PM schedules (e.g. Coaching 2 PM at 7:52 AM).
            if (time.Hours < 12)
            {
                var morningSchedules = parsedCandidates
                    .Where(x => x.Start.Value.Hours < 13)
                    .ToList();

                if (morningSchedules.Any())
                    parsedCandidates = morningSchedules;
            }

            var windowMatch = parsedCandidates
                .Where(x => time >= x.Start.Value && time <= x.End.Value)
                .OrderBy(x => x.Start.Value)
                .Select(x => x.Schedule)
                .FirstOrDefault();

            if (windowMatch != null)
                return windowMatch;

            // Still inside a schedule that has not ended (e.g. 11 AM in class 8:00–12:30).
            var openSchedule = parsedCandidates
                .Where(x => time <= x.End.Value && time >= x.Start.Value)
                .OrderBy(x => x.Start.Value)
                .Select(x => x.Schedule)
                .FirstOrDefault();

            if (openSchedule != null)
                return openSchedule;

            var minStart = parsedCandidates.Min(x => x.Start.Value);
            var maxEnd = parsedCandidates.Max(x => x.End.Value);

            // Early Pre: within 2 hours before each schedule's own start (not only global minStart).
            if (time < minStart)
            {
                var earlyMatch = parsedCandidates
                    .Where(x => x.Start.HasValue
                                && (x.Start.Value - time).TotalMinutes <= earlyPreMinutes
                                && (x.Start.Value - time).TotalMinutes >= 0)
                    .OrderBy(x => x.Start.Value)
                    .Select(x => x.Schedule)
                    .FirstOrDefault();

                if (earlyMatch != null)
                    return earlyMatch;

                // Morning punch: prefer earliest AM schedule when still before first start.
                if (time.Hours < 13)
                {
                    var morningSchedule = parsedCandidates
                        .Where(x => x.Start.HasValue && x.Start.Value.Hours < 13)
                        .OrderBy(x => x.Start.Value)
                        .Select(x => x.Schedule)
                        .FirstOrDefault();

                    if (morningSchedule != null)
                        return morningSchedule;
                }

                return null;
            }

            // After the last schedule ended → Late Abs on last schedule.
            if (time > maxEnd)
            {
                return parsedCandidates
                    .OrderByDescending(x => x.End.Value)
                    .Select(x => x.Schedule)
                    .First();
            }

            // Gap between schedules: next shift only if within early-pre window.
            var nextUpcoming = parsedCandidates
                .Where(x => x.Start.Value > time)
                .OrderBy(x => x.Start.Value)
                .FirstOrDefault();

            if (nextUpcoming != null)
            {
                var minutesUntilStart = (nextUpcoming.Start.Value - time).TotalMinutes;
                if (minutesUntilStart <= earlyPreMinutes)
                    return nextUpcoming.Schedule;
            }

            // Prefer a schedule that is still open (between start and end).
            var stillRunning = parsedCandidates
                .Where(x => time <= x.End.Value)
                .OrderByDescending(x => x.Start.Value)
                .Select(x => x.Schedule)
                .FirstOrDefault();

            if (stillRunning != null)
                return stillRunning;

            return parsedCandidates
                .OrderByDescending(x => x.End.Value)
                .Select(x => x.Schedule)
                .FirstOrDefault();
        }

        /// <summary>
        /// If user still has an open IN on any schedule, the next punch must close that shift (OUT)
        /// before a new schedule can receive an entry.
        /// </summary>
        public Attendance_Schedule_Day ResolveScheduleForPunch(int deviceID, DateTime punchDateTime)
        {
            var openSchedule = TryGetOpenScheduleForExit(deviceID, punchDateTime);
            if (openSchedule != null)
                return openSchedule;

            return GetActiveSchedule(deviceID, punchDateTime);
        }

        private Attendance_Schedule_Day GetScheduleRowForPunch(int scheduleID, DateTime punchDateTime)
        {
            var rows = Schedules_Get().Where(s => s.ScheduleID == scheduleID);
            return PickTodayRowForSchedule(rows, punchDateTime);
        }

        private Attendance_Schedule_Day TryGetOpenScheduleForExit(int deviceID, DateTime punchDateTime)
        {
            var date = GetAttendanceDateString();
            var time = punchDateTime.TimeOfDay;

            using (var db = new ModelContext())
            {
                var openRecords = db.attendance_Records
                    .AsEnumerable()
                    .Where(a => a.DeviceID == deviceID
                                && DatesMatch(a.AttendanceDate, date)
                                && !a.Is_OUT
                                && !string.IsNullOrWhiteSpace(a.EntryTime)
                                && !string.Equals(a.AttendanceStatus, "Abs", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (!openRecords.Any())
                    return null;

                var candidates = new List<(Attendance_Schedule_Day Schedule, TimeSpan Start, TimeSpan End, bool Ended)>();

                foreach (var record in openRecords)
                {
                    var scheduleRow = GetScheduleRowForPunch(record.ScheduleID, punchDateTime);
                    if (scheduleRow == null || !TryGetScheduleTimes(scheduleRow, out var start, out _, out var end))
                        continue;

                    candidates.Add((scheduleRow, start, end, time >= end));
                }

                if (!candidates.Any())
                    return null;

                var endedShift = candidates
                    .Where(c => c.Ended)
                    .OrderByDescending(c => c.End)
                    .Select(c => c.Schedule)
                    .FirstOrDefault();

                if (endedShift != null)
                    return endedShift;

                return candidates
                    .OrderBy(c => c.Start)
                    .Select(c => c.Schedule)
                    .FirstOrDefault();
            }
        }

        internal static bool TryGetScheduleTimes(Attendance_Schedule_Day schedule, out TimeSpan start, out TimeSpan late, out TimeSpan end)
        {
            start = late = end = TimeSpan.Zero;
            if (schedule == null)
                return false;

            return ScheduleTimeHelper.TryParse(schedule.StartTime, out start)
                   && ScheduleTimeHelper.TryParse(schedule.LateEntryTime, out late)
                   && ScheduleTimeHelper.TryParse(schedule.EndTime, out end);
        }

        private static IEnumerable<Attendance_Schedule_Day> FilterScheduleDaysForPunch(
            IEnumerable<Attendance_Schedule_Day> rows,
            DateTime punchDateTime)
        {
            return rows
                .Where(s => ScheduleDayHelper.IsSameDay(s.Day, punchDateTime))
                .GroupBy(s => s.ScheduleID)
                .SelectMany(group =>
                {
                    var scheduleToday = group.ToList();
                    if (!scheduleToday.Any())
                        return Enumerable.Empty<Attendance_Schedule_Day>();

                    if (scheduleToday.Any(s => s.Is_OnDay))
                        return scheduleToday.Where(s => s.Is_OnDay);

                    return scheduleToday;
                });
        }

        public void EnsureScheduleDataForPunch()
        {
            RefreshUserSchedulesFromDb();
            EnsureUserScheduleAssignmentsFromUsers();
        }

        public async Task EnsureScheduleBootstrapFromNetworkIfStaleAsync()
        {
            if (DateTime.UtcNow - _lastNetworkBootstrapUtc < PerformanceSettings.ScheduleBootstrapInterval)
                return;

            await EnsureScheduleBootstrapAsync(forceNetwork: true).ConfigureAwait(false);
        }

        public async Task EnsureScheduleBootstrapAsync(bool forceNetwork = false)
        {
            RefreshUserSchedulesFromDb();

            var ins = institution;
            if (ins == null || string.IsNullOrWhiteSpace(ins.Token))
                return;

            if (!forceNetwork &&
                DateTime.UtcNow - _lastNetworkBootstrapUtc < PerformanceSettings.ScheduleBootstrapInterval)
            {
                EnsureUserScheduleAssignmentsFromUsers();
                return;
            }

            await _bootstrapLock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (!forceNetwork &&
                    DateTime.UtcNow - _lastNetworkBootstrapUtc < PerformanceSettings.ScheduleBootstrapInterval)
                {
                    EnsureUserScheduleAssignmentsFromUsers();
                    return;
                }

                var client = new RestClient(ApiUrl.EndPoint);
                var schoolId = GetEffectiveSchoolId();

                if (!Schedules_Get().Any())
                    await ScheduleAssignmentSync.SyncScheduleDaysFromServerAsync(client, schoolId, ins.Token);

                await ScheduleAssignmentSync.SyncAssignmentsFromServerAsync(client, schoolId, ins.Token);
                EnsureUserScheduleAssignmentsFromUsers();
                _lastNetworkBootstrapUtc = DateTime.UtcNow;
            }
            finally
            {
                _bootstrapLock.Release();
            }
        }

        public void ReconcileUserScheduleAssignments()
        {
            var knownScheduleIds = Schedules_Get()
                .Select(s => s.ScheduleID)
                .Distinct()
                .ToHashSet();

            if (!knownScheduleIds.Any())
                return;

            using (var db = new ModelContext())
            {
                var changed = false;

                foreach (var user in db.Users.Where(u => u.DeviceID > 0))
                {
                    var assignments = db.User_Schedules.Where(us => us.DeviceID == user.DeviceID).ToList();
                    var validAssignments = assignments.Where(a => knownScheduleIds.Contains(a.ScheduleID)).ToList();

                    if (validAssignments.Any())
                        continue;

                    if (user.ScheduleID <= 0 || !knownScheduleIds.Contains(user.ScheduleID))
                        continue;

                    if (assignments.Any())
                        db.User_Schedules.RemoveRange(assignments);

                    db.User_Schedules.Add(new User_Schedule
                    {
                        DeviceID = user.DeviceID,
                        ScheduleID = user.ScheduleID,
                        Is_Student = user.Is_Student
                    });
                    changed = true;
                }

                if (changed)
                    db.SaveChanges();
            }

            RefreshUserSchedulesFromDb();
        }

        public void EnsureUserScheduleAssignmentsFromUsers()
        {
            EnsureUserScheduleSeeded();
            ReconcileUserScheduleAssignments();
            using (var db = new ModelContext())
            {
                SeedMissingUserScheduleFallback(db);
                db.SaveChanges();
            }

            RefreshUserSchedulesFromDb();
        }

        public void EnsureUserScheduleSeeded()
        {
            using (var db = new ModelContext())
            {
                if (db.User_Schedules.Any())
                {
                    User_Schedules = db.User_Schedules.ToList();
                    return;
                }

                var users = db.Users.Where(u => u.ScheduleID > 0).ToList();
                foreach (var user in users)
                {
                    db.User_Schedules.Add(new User_Schedule
                    {
                        DeviceID = user.DeviceID,
                        ScheduleID = user.ScheduleID,
                        Is_Student = user.Is_Student
                    });
                }

                if (users.Any())
                    db.SaveChanges();

                User_Schedules = db.User_Schedules.ToList();
            }
        }

        public void RefreshUserSchedulesFromDb()
        {
            using (var db = new ModelContext())
                User_Schedules = db.User_Schedules.ToList();
        }

        /// <summary>Keep User_Schedule rows only for users present on this PC.</summary>
        public void PruneUserSchedulesToLocalUsers()
        {
            using (var db = new ModelContext())
            {
                var deviceIds = db.Users.Where(u => u.DeviceID > 0).Select(u => u.DeviceID).ToHashSet();
                if (!deviceIds.Any())
                    return;

                var orphans = db.User_Schedules.Where(us => !deviceIds.Contains(us.DeviceID)).ToList();
                if (!orphans.Any())
                    return;

                db.User_Schedules.RemoveRange(orphans);
                db.SaveChanges();
            }

            RefreshUserSchedulesFromDb();
        }

        public List<int> GetCurrentOndaySchduleIds()
        {
            RefreshUserSchedulesFromDb();
            var now = DateTime.Now;

            return FilterScheduleDaysForPunch(Schedules_Get(), now)
                .GroupBy(s => s.ScheduleID)
                .Select(g => g.First())
                .Where(s => ScheduleTimeHelper.TryParse(s.LateEntryTime, out var late)
                            && late < now.TimeOfDay
                            && !s.Is_Abs_Count)
                .Where(s => User_Schedules.Any(us => us.ScheduleID == s.ScheduleID))
                .OrderBy(s => ScheduleTimeHelper.TryParse(s.StartTime, out var start) ? start : TimeSpan.MaxValue)
                .Select(s => s.ScheduleID)
                .Distinct()
                .ToList();
        }

        public async Task<List<Device>> DeviceListAsync()
        {
            using (var db = new ModelContext())
            {
                return await db.Devices.ToListAsync();
            }
        }

        public async Task GetTodayAttendanceRecords(List<Attendance_Record> records)
        {
            if (records == null || !records.Any())
                return;

            try
            {
                var today = GetAttendanceDateString();
                using (var db = new ModelContext())
                {
                    var todayLocalRecords = db.attendance_Records
                        .AsEnumerable()
                        .Where(a => IsSameAttendanceDate(a.AttendanceDate, today))
                        .Select(a => new { a.DeviceID, a.ScheduleID })
                        .ToList();

                    var additionalServerAttendance = records.Where(u =>
                        !todayLocalRecords.Any(l => l.DeviceID == u.DeviceID && l.ScheduleID == u.ScheduleID)).ToList();

                    var attendanceData = additionalServerAttendance
                        .Select(a =>
                        {
                            a.AttendanceDate = AttendanceDateHelper.Normalize(a.AttendanceDate);
                            a.EntryTime = ScheduleTimeHelper.NormalizeForStorage(a.EntryTime);
                            a.ExitTime = ScheduleTimeHelper.NormalizeForStorage(a.ExitTime);
                            return a;
                        })
                        .Where(a => !string.IsNullOrWhiteSpace(a.AttendanceDate) && a.DeviceID > 0)
                        .ToList();

                    if (attendanceData.Any())
                    {
                        db.attendance_Records.AddRange(attendanceData);
                        await db.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                StartupLogger.LogFailure("GetTodayAttendanceRecords", ex);
            }
        }

        public async Task<List<Attendance_Record>> StudentLog_Post()
        {
            var today = GetAttendanceDateString();
            using (var db = new ModelContext())
            {
                var logs = from a in db.attendance_Records
                           join u in db.Users
                           on a.DeviceID equals u.DeviceID
                           where !a.Is_Sent && u.Is_Student
                           select a;

                return (await logs.ToListAsync())
                    .Where(a => IsSameAttendanceDate(a.AttendanceDate, today))
                    .ToList();
            }
        }
        public async Task<List<Attendance_Record>> StudentLog_Put()
        {
            var today = GetAttendanceDateString();
            using (var db = new ModelContext())
            {
                var logs = from a in db.attendance_Records
                           join u in db.Users
                           on a.DeviceID equals u.DeviceID
                           where a.Is_Sent && !a.Is_Updated && u.Is_Student
                           select a;

                return (await logs.ToListAsync())
                    .Where(a => IsSameAttendanceDate(a.AttendanceDate, today))
                    .ToList();
            }
        }
        public async Task<List<Attendance_Record>> EmpLog_Post()
        {
            var today = GetAttendanceDateString();
            using (var db = new ModelContext())
            {
                var logs = from a in db.attendance_Records
                           join u in db.Users
                           on a.DeviceID equals u.DeviceID
                           where !a.Is_Sent && !u.Is_Student
                           select a;

                return (await logs.ToListAsync())
                    .Where(a => IsSameAttendanceDate(a.AttendanceDate, today))
                    .ToList();
            }
        }
        public async Task<List<Attendance_Record>> EmpLog_Put()
        {
            var today = GetAttendanceDateString();
            using (var db = new ModelContext())
            {
                var logs = from a in db.attendance_Records
                           join u in db.Users
                           on a.DeviceID equals u.DeviceID
                           where a.Is_Sent && !a.Is_Updated && !u.Is_Student
                           select a;

                return (await logs.ToListAsync())
                    .Where(a => IsSameAttendanceDate(a.AttendanceDate, today))
                    .ToList();
            }
        }

        public void FlagIncompleteRecordsForResync()
        {
            var today = GetAttendanceDateString();
            using (var db = new ModelContext())
            {
                foreach (var record in db.attendance_Records.ToList())
                {
                    if (!IsSameAttendanceDate(record.AttendanceDate, today))
                        continue;

                    if (record.Is_OUT ||
                        string.Equals(record.AttendanceStatus, "Leave", StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Abs auto-mark synced earlier but later got a real entry on device — push again.
                    if (string.Equals(record.AttendanceStatus, "Abs", StringComparison.OrdinalIgnoreCase) &&
                        !string.IsNullOrWhiteSpace(record.EntryTime))
                    {
                        record.Is_Sent = false;
                        record.Is_Updated = false;
                        continue;
                    }

                    if (string.Equals(record.AttendanceStatus, "Abs", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (!record.Is_Updated && !string.IsNullOrWhiteSpace(record.EntryTime))
                    {
                        record.Is_Sent = false;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(record.EntryTime))
                    {
                        record.Is_Sent = false;
                        record.Is_Updated = false;
                    }
                }

                db.SaveChanges();
            }
        }

        /// <summary>
        /// Back-fill ScheduleID on today's rows and re-push to server when legacy sync saved without schedule.
        /// </summary>
        public void RepairTodayScheduleIdsForResync()
        {
            var today = GetAttendanceDateString();
            using (var db = new ModelContext())
            {
                var changed = false;

                foreach (var row in db.attendance_Records.AsEnumerable()
                    .Where(a => IsSameAttendanceDate(a.AttendanceDate, today)))
                {
                    var rowChanged = false;
                    var punchAt = GetAttendanceDate();
                    if (!string.IsNullOrWhiteSpace(row.EntryTime) &&
                        ScheduleTimeHelper.TryParse(row.EntryTime, out var entryTime))
                    {
                        punchAt = punchAt.Add(entryTime);
                    }

                    var schedule = ResolveScheduleForPunch(row.DeviceID, punchAt);
                    if (schedule != null && schedule.ScheduleID > 0 &&
                        row.ScheduleID != schedule.ScheduleID)
                    {
                        row.ScheduleID = schedule.ScheduleID;
                        rowChanged = true;
                    }
                    else if (row.ScheduleID <= 0 && schedule != null && schedule.ScheduleID > 0)
                    {
                        row.ScheduleID = schedule.ScheduleID;
                        rowChanged = true;
                    }

                    if (row.ScheduleID <= 0)
                        continue;

                    if (!string.IsNullOrWhiteSpace(row.EntryTime) && schedule != null &&
                        TryGetScheduleTimes(schedule, out var start, out var late, out var end) &&
                        ScheduleTimeHelper.TryParse(row.EntryTime, out var time))
                    {
                        string newStatus;
                        if (time > end)
                            newStatus = "Late Abs";
                        else if (time <= start)
                            newStatus = "Pre";
                        else if (time <= late)
                            newStatus = "Late";
                        else
                            newStatus = "Late Abs";

                        if (!string.Equals(newStatus, row.AttendanceStatus, StringComparison.OrdinalIgnoreCase))
                        {
                            row.AttendanceStatus = newStatus;
                            rowChanged = true;
                        }
                    }

                    if (rowChanged)
                    {
                        row.Is_Sent = false;
                        row.Is_Updated = false;
                    }

                    if (rowChanged)
                        changed = true;
                }

                if (changed)
                    db.SaveChanges();
            }
        }

        public static bool IsFullySynced(Attendance_Record record)
        {
            return record != null && record.Is_Sent && record.Is_Updated;
        }

        /// <summary>
        /// Past-date rows leave the pending list: synced rows are removed locally;
        /// unsynced rows are copied to Device Logs for manual sync.
        /// </summary>
        public int ArchiveExpiredAttendanceRecords()
        {
            var today = GetAttendanceDate().Date;
            using (var db = new ModelContext())
            {
                var expired = db.attendance_Records
                    .AsEnumerable()
                    .Where(a => TryParseAttendanceDate(a.AttendanceDate, out var d) && d.Date < today)
                    .ToList();

                if (!expired.Any())
                    return 0;

                var archived = 0;
                foreach (var row in expired)
                {
                    if (IsFullySynced(row))
                    {
                        db.attendance_Records.Remove(row);
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(row.EntryTime) ||
                        string.Equals(row.AttendanceStatus, "Abs", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(row.AttendanceStatus, "Late Abs", StringComparison.OrdinalIgnoreCase))
                    {
                        var entryDay = TryParseAttendanceDate(row.AttendanceDate, out var entryDate)
                            ? entryDate.ToString("dddd")
                            : string.Empty;

                        db.attendanceLog_Backups.Add(new AttendanceLog_Backup
                        {
                            DeviceID = row.DeviceID,
                            Entry_Time = row.EntryTime ?? string.Empty,
                            Entry_Date = row.AttendanceDate,
                            Entry_Day = entryDay,
                            Backup_Reason = string.IsNullOrWhiteSpace(row.EntryTime)
                                ? $"Expired: {row.AttendanceStatus}"
                                : "Expired pending sync"
                        });
                        archived++;
                    }

                    db.attendance_Records.Remove(row);
                }

                db.SaveChanges();
                return archived;
            }
        }

        public int GetPendingAttendanceCount()
        {
            var today = GetAttendanceDateString();
            using (var db = new ModelContext())
            {
                return db.attendance_Records
                    .AsEnumerable()
                    .Count(a => IsSameAttendanceDate(a.AttendanceDate, today) && !IsFullySynced(a));
            }
        }

        public List<Attendance_Record_View> Get_Pending_Attendance_Record()
        {
            var today = GetAttendanceDateString();
            using (var db = new ModelContext())
            {
                var logs = from a in db.attendance_Records
                           join u in db.Users
                           on a.DeviceID equals u.DeviceID
                           where !a.Is_Sent || !a.Is_Updated
                           select new Attendance_Record_View()
                           {
                               AttendanceDate = a.AttendanceDate,
                               AttendanceStatus = a.AttendanceStatus,
                               DeviceID = a.DeviceID,
                               ScheduleID = a.ScheduleID,
                               EntryTime = a.EntryTime,
                               ExitStatus = a.ExitStatus,
                               ExitTime = a.ExitTime,
                               ID = u.ID,
                               Name = u.Name,
                               Is_Student = u.Is_Student
                           };

                return logs
                    .AsEnumerable()
                    .Where(a => IsSameAttendanceDate(a.AttendanceDate, today))
                    .ToList();
            }
        }

        public void ResetAttendanceSyncFlags()
        {
            using (var db = new ModelContext())
            {
                foreach (var record in db.attendance_Records.ToList())
                {
                    record.Is_Sent = false;
                    record.Is_Updated = false;
                }

                db.SaveChanges();
            }
        }

        public List<Log_Backups_View> Get_Log_Backup()
        {
            var logs = new List<Log_Backups_View>();
            using (var db = new ModelContext())
            {
                logs = (from a in db.attendanceLog_Backups
                        join u in db.Users
                        on a.DeviceID equals u.DeviceID
                        select new Log_Backups_View()
                        {
                            DeviceID = a.DeviceID,
                            Entry_Date = a.Entry_Date,
                            Entry_Time = a.Entry_Time,
                            Backup_Reason = a.Backup_Reason,
                            ID = u.ID,
                            Name = u.Name,
                            Is_Student = u.Is_Student
                        }).Distinct().ToList();
            }

            return logs;
        }
        public List<LeaveView> Get_Leave()
        {
            using (var db = new ModelContext())
            {
                var logs = from l in db.user_Leave_Records
                           join u in db.Users
                           on l.DeviceID equals u.DeviceID
                           select new LeaveView()
                           {
                               ID = u.ID,
                               Name = u.Name,
                               LeaveDate = l.LeaveDate
                           };

                return logs.ToList();
            }
        }

        public List<UserFP_View> Get_AllUserFP()
        {
            var imageLink = institution?.Image_Link;
            using (var db = new ModelContext())
            {
                var userFp = (from f in db.user_FingerPrints
                             join u in db.Users
                             on f.DeviceID equals u.DeviceID
                             group u by u into g
                             select new
                             {
                                 User = g.Key,
                                 FingerCount = g.Count()
                             }).ToList();

                return userFp.Select(x => new UserFP_View
                {
                    DeviceID = x.User.DeviceID,
                    ID = x.User.ID,
                    Name = x.User.Name,
                    Designation = x.User.Designation,
                    ImgLink = UserPhotoHelper.ResolvePhotoUri(imageLink, x.User.ID),
                    Is_Student = x.User.Is_Student,
                    FingerCount = x.FingerCount
                }).ToList();
            }
        }

        public Finger Get_UserFP(int DeviceID)
        {
            var fingers = new Finger();

            using (var db = new ModelContext())
            {
                var Finger_Indexs = db.user_FingerPrints.Where(f => f.DeviceID == DeviceID).Select(f => f.Finger_Index).ToList();

                foreach (var item in Finger_Indexs)
                {
                    if (item == 3)
                    {
                        fingers.LeftIndex = Brushes.GreenYellow;
                    }
                    else if (item == 4)
                    {
                        fingers.LeftThamb = Brushes.GreenYellow;
                    }
                    else if (item == 5)
                    {
                        fingers.RightIndex = Brushes.GreenYellow;
                    }
                    else if (item == 6)
                    {
                        fingers.RightThamb = Brushes.GreenYellow;
                    }
                }




            }
            return fingers;
        }
        public void Delete_UserFP(int DeviceID, int Index)
        {
            using (var db = new ModelContext())
            {
                var finger = db.user_FingerPrints.Where(f => f.DeviceID == DeviceID && f.Finger_Index == Index).ToList();
                db.user_FingerPrints.RemoveRange(finger);
                db.SaveChanges();
            }
        }

        public async Task AddNotifications(IEnumerable<DataUpdateList> notifications)
        {
            using (var db = new ModelContext())
            {
                db.dataUpdateLists.AddRange(notifications);
                await db.SaveChangesAsync();
            }
        }
        public List<ErrorData_View> GetServerNotifications()
        {
            using (var db = new ModelContext())
            {
                var Errors = from d in db.dataUpdateLists
                             select new ErrorData_View()
                             {
                                 id = d.DateUpdateID,
                                 ErrorType = d.UpdateType,
                                 ErrorDescription = d.UpdateDescription,
                                 ErrorDate = d.UpdateDate
                             };

                return Errors.OrderByDescending(a => a.id).ToList();
            }
        }
        public void DeleteNotifications()
        {
            using (var db = new ModelContext())
            {
                db.dataUpdateLists.Clear();
                db.SaveChanges();
            }
        }
        public void Delete_Log_Backup(DateTime fdate, DateTime tdate, List<int> DeviceIDs)
        {
            using (var db = new ModelContext())
            {
                var logs = db.attendanceLog_Backups.ToList().Where(a =>
                    AttendanceDateHelper.TryParse(a.Entry_Date, out var entryDate) &&
                    entryDate >= fdate.Date &&
                    entryDate <= tdate.Date &&
                    (DeviceIDs == null || !DeviceIDs.Any() || DeviceIDs.Contains(a.DeviceID)));

                db.attendanceLog_Backups.RemoveRange(logs);
                db.SaveChanges();
            }
        }
        public void Abs_Insert(List<int> scheduleIDs, string date, Institution ins)
        {
            if (scheduleIDs == null || !scheduleIDs.Any())
                return;

            if (string.IsNullOrWhiteSpace(date))
                date = GetAttendanceDateString();

            var scheduleUsers = new List<User_Schedule>();

            if (ins.Is_Employee_Attendance_Enable && ins.Is_Student_Attendance_Enable)
                scheduleUsers = User_Schedules.Where(u => scheduleIDs.Contains(u.ScheduleID)).ToList();
            else if (ins.Is_Employee_Attendance_Enable)
                scheduleUsers = User_Schedules.Where(u => scheduleIDs.Contains(u.ScheduleID) && !u.Is_Student).ToList();
            else if (ins.Is_Student_Attendance_Enable)
                scheduleUsers = User_Schedules.Where(u => scheduleIDs.Contains(u.ScheduleID) && u.Is_Student).ToList();

            using (var db = new ModelContext())
            {
                var logs = db.attendance_Records.ToList();
                var attRecords = new List<Attendance_Record>();
                var completedScheduleIds = new List<int>();

                foreach (var scheduleId in scheduleIDs.Distinct())
                {
                    var usersForSchedule = scheduleUsers.Where(u => u.ScheduleID == scheduleId).ToList();
                    if (!usersForSchedule.Any())
                        continue;

                    var missing = usersForSchedule
                        .Where(us => !logs.Any(a =>
                            a.DeviceID == us.DeviceID &&
                            a.ScheduleID == us.ScheduleID &&
                            AttendanceDateHelper.DatesMatch(a.AttendanceDate, date)))
                        .ToList();

                    attRecords.AddRange(missing.Select(us => new Attendance_Record
                    {
                        AttendanceDate = date,
                        DeviceID = us.DeviceID,
                        ScheduleID = us.ScheduleID,
                        AttendanceStatus = "Abs"
                    }));

                    completedScheduleIds.Add(scheduleId);
                }

                if (completedScheduleIds.Any())
                {
                    var schs = db.attendance_Schedule_Days
                        .Where(s => completedScheduleIds.Contains(s.ScheduleID))
                        .ToList();
                    schs.ForEach(s => s.Is_Abs_Count = true);
                }

                if (attRecords.Any())
                    db.attendance_Records.AddRange(attRecords);

                db.SaveChanges();
            }
        }

        public bool IsUserExist()
        {
            using (var db = new ModelContext())
            {
                return db.Users.Any();
            }
        }

        public bool IsDeviceExist()
        {
            using (var db = new ModelContext())
            {
                return db.Devices.Any();
            }
        }

        public async Task InstitutionUpdate(Institution data)
        {
            if (data == null)
                return;

            var currentDateTime = data.Current_Datetime;

            using (var db = new ModelContext())
            {
                var existing = data.Id > 0
                    ? await db.Institutions.FindAsync(data.Id)
                    : null;

                if (existing == null)
                    existing = await db.Institutions.FirstOrDefaultAsync();

                if (existing == null)
                {
                    data.Id = 0;
                    db.Institutions.Add(data);
                }
                else
                {
                    data.Id = existing.Id;
                    db.Entry(existing).CurrentValues.SetValues(data);
                    data = existing;
                }

                await db.SaveChangesAsync();
            }

            institution = data;
            if (currentDateTime != default(DateTime))
                institution.Current_Datetime = currentDateTime;
        }

        public async Task LeaveDataHandling(List<User_Leave_Record> data)
        {
            using (var db = new ModelContext())
            {

                //For deleting all previous data
                db.user_Leave_Records.Clear();

                foreach (var item in data)
                {
                    // Insert attendance records if new record
                    if (!db.attendance_Records.Any(a =>
                        a.AttendanceDate == item.LeaveDate && a.DeviceID == item.DeviceID))
                    {
                        var attRecord = new Attendance_Record
                        {
                            AttendanceDate = item.LeaveDate,
                            DeviceID = item.DeviceID,
                            ScheduleID = 0,
                            AttendanceStatus = "Leave"
                        };

                        db.attendance_Records.Add(attRecord);
                    }

                    db.user_Leave_Records.Add(item);
                }

                await db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Replaces local schedule-day rows (main-branch behaviour).
        /// Returns true when a local user is assigned to a schedule not present in the download.
        /// </summary>
        public async Task<bool> ScheduleDataHandling(List<Attendance_Schedule_Day> data)
        {
            if (data == null || !data.Any())
                return false;

            SqliteMultiScheduleMigration.EnsureApplied();

            foreach (var item in data)
            {
                item.StartTime = ScheduleTimeHelper.NormalizeForStorage(item.StartTime);
                item.LateEntryTime = ScheduleTimeHelper.NormalizeForStorage(item.LateEntryTime);
                item.EndTime = ScheduleTimeHelper.NormalizeForStorage(item.EndTime);
                if (string.IsNullOrWhiteSpace(item.ScheduleName))
                    item.ScheduleName = $"Schedule {item.ScheduleID}";
            }

            using (var db = new ModelContext())
            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    db.attendance_Schedule_Days.Clear();

                    foreach (var item in data)
                        db.attendance_Schedule_Days.Add(item);

                    await db.SaveChangesAsync();
                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }

            InvalidateScheduleCache();

            var scheduleIds = data.Select(s => s.ScheduleID).Distinct().ToArray();

            using (var db = new ModelContext())
            {
                var legacyUserMismatch = await db.Users.AnyAsync(u =>
                    u.ScheduleID > 0 && !scheduleIds.Contains(u.ScheduleID));

                if (legacyUserMismatch)
                    return true;

                if (db.User_Schedules.Any())
                {
                    return await db.User_Schedules.AnyAsync(us =>
                        us.ScheduleID > 0 && !scheduleIds.Contains(us.ScheduleID));
                }
            }

            return false;
        }

        public async Task UserScheduleDataHandling(List<User_Schedule> data, bool fromServer = false)
        {
            using (var db = new ModelContext())
            {
                if (data != null && data.Any())
                {
                    await db.Database.ExecuteSqlCommandAsync("DELETE FROM User_Schedule");

                    foreach (var item in data.Where(d => d.DeviceID > 0 && d.ScheduleID > 0))
                    {
                        if (fromServer)
                        {
                            db.User_Schedules.Add(new User_Schedule
                            {
                                DeviceID = item.DeviceID,
                                ScheduleID = item.ScheduleID,
                                Is_Student = item.Is_Student
                            });
                            continue;
                        }

                        var user = db.Users.FirstOrDefault(u => u.DeviceID == item.DeviceID);
                        if (user == null)
                            continue;

                        item.Is_Student = user.Is_Student;
                        db.User_Schedules.Add(item);
                    }

                    if (fromServer)
                        ResetAbsCountForIncompleteSchedules(db, data.Select(d => d.ScheduleID));
                }
                else if (!db.User_Schedules.Any())
                {
                    EnsureUserScheduleSeeded();
                }

                await db.SaveChangesAsync();
                User_Schedules = db.User_Schedules.ToList();
                ApplyStudentFlagsFromUserSchedules(db);
                await db.SaveChangesAsync();
                Users = db.Users.ToList();
                RebuildUserViewCache();
            }
        }

        private void ResetAbsCountForIncompleteSchedules(ModelContext db, IEnumerable<int> scheduleIds)
        {
            var today = GetAttendanceDateString();
            var logs = db.attendance_Records.ToList();

            foreach (var scheduleId in scheduleIds.Distinct())
            {
                var assigned = db.User_Schedules.Where(u => u.ScheduleID == scheduleId).ToList();
                if (!assigned.Any())
                    continue;

                var allMarked = assigned.All(us => logs.Any(a =>
                    a.DeviceID == us.DeviceID &&
                    a.ScheduleID == us.ScheduleID &&
                    DatesMatch(a.AttendanceDate, today)));

                if (allMarked)
                    continue;

                foreach (var row in db.attendance_Schedule_Days.Where(s => s.ScheduleID == scheduleId))
                    row.Is_Abs_Count = false;
            }

            InvalidateScheduleCache();
        }

        private static bool DatesMatch(string left, string right)
        {
            return AttendanceDateHelper.DatesMatch(left, right);
        }

        private static void ApplyStudentFlagsFromUserSchedules(ModelContext db)
        {
            var studentScheduleDeviceIds = new HashSet<int>(
                db.User_Schedules.Where(s => s.Is_Student).Select(s => s.DeviceID));

            var employeeScheduleDeviceIds = new HashSet<int>(
                db.User_Schedules.Where(s => !s.Is_Student).Select(s => s.DeviceID));

            foreach (var user in db.Users)
            {
                var designatedStudent = string.Equals(user.Designation, "Student", StringComparison.OrdinalIgnoreCase);

                if (studentScheduleDeviceIds.Contains(user.DeviceID) || designatedStudent)
                    user.Is_Student = true;
                else if (employeeScheduleDeviceIds.Contains(user.DeviceID))
                    user.Is_Student = false;
                else
                    user.Is_Student = designatedStudent;
            }

            SeedMissingUserScheduleFallback(db);
        }

        /// <summary>
        /// Only back-fill User_Schedule when a user has none (legacy Users.ScheduleID).
        /// Never assign every schedule to every student.
        /// </summary>
        private static void SeedMissingUserScheduleFallback(ModelContext db)
        {
            foreach (var user in db.Users.Where(u => u.ScheduleID > 0))
            {
                if (db.User_Schedules.Any(us => us.DeviceID == user.DeviceID))
                    continue;

                db.User_Schedules.Add(new User_Schedule
                {
                    DeviceID = user.DeviceID,
                    ScheduleID = user.ScheduleID,
                    Is_Student = user.Is_Student
                });
            }
        }

        public List<User_FingerPrint> FingerPrintData()
        {
            var fpList = new List<User_FingerPrint>();
            using (var db = new ModelContext())
            {
                fpList = db.user_FingerPrints.ToList();
            }
            return fpList;
        }

        public async Task ResetApp()
        {
            using (var db = new ModelContext())
            {
                db.attendanceLog_Backups.Clear();
                db.attendance_Records.Clear();
                db.attendance_Schedule_Days.Clear();
                db.user_Leave_Records.Clear();
                db.dataUpdateLists.Clear();
                db.user_FingerPrints.Clear();
                db.Devices.Clear();
                db.Users.Clear();
                db.Institutions.Clear();

                if (db.User_Schedules.Any())
                    await db.Database.ExecuteSqlCommandAsync("DELETE FROM User_Schedule");

                await db.SaveChangesAsync();
            }

            institution = null;
            Users = new List<User>();
            User_Schedules = new List<User_Schedule>();
        }
    }

    public enum Error_Type
    {
        NoError,
        DeviceInfoPage,
        UserInfoPage,
        SchedulePage,
    }
    public class Setting_Error
    {
        public Error_Type Type { get; set; }
        public string Message { get; set; }
    }
    public class Finger
    {
        public SolidColorBrush LeftIndex { get; set; } = Brushes.White;
        public SolidColorBrush LeftThamb { get; set; } = Brushes.White;
        public SolidColorBrush RightIndex { get; set; } = Brushes.White;
        public SolidColorBrush RightThamb { get; set; } = Brushes.White;

    }
}
