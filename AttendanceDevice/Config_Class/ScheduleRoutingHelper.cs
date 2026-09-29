using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AttendanceDevice.Model;

namespace AttendanceDevice.Config_Class
{
    /// <summary>
    /// Guards multi-schedule routing so AM punches stay on morning shifts
    /// and PM punches stay on afternoon/evening shifts.
    /// </summary>
    internal static class ScheduleRoutingHelper
    {
        internal const int EarlyPreMinutesDefault = 45;

        /// <summary>
        /// Empty or 12:00 AM placeholder left by auto-Abs / failed sync — not a real punch.
        /// </summary>
        internal static bool IsPlaceholderEntryTime(string entryTimeRaw)
        {
            if (string.IsNullOrWhiteSpace(entryTimeRaw))
                return true;

            if (!ScheduleTimeHelper.TryParse(entryTimeRaw, out var entryTime))
                return true;

            return entryTime == TimeSpan.Zero;
        }

        internal static bool IsPresentLikeStatus(string attendanceStatus)
        {
            return string.Equals(attendanceStatus, "Pre", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(attendanceStatus, "Late", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(attendanceStatus, "Late Abs", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Pre/Late/Late Abs with 12:00 AM is an auto-row, not a real punch.
        /// </summary>
        internal static bool CoercePlaceholderPresentToAbs(Attendance_Record record)
        {
            if (record == null || record.Is_OUT)
                return false;

            if (!IsPresentLikeStatus(record.AttendanceStatus))
                return false;

            if (!IsPlaceholderEntryTime(record.EntryTime))
                return false;

            record.AttendanceStatus = "Abs";
            record.EntryTime = null;
            return true;
        }

        /// <summary>
        /// First real IN punch should overwrite Abs / Late Abs / midnight-Pre rows that have no real entry yet.
        /// </summary>
        internal static bool ShouldApplyAsFirstEntry(string attendanceStatus, string entryTimeRaw, bool isOut)
        {
            if (isOut)
                return false;

            if (string.Equals(attendanceStatus, "Leave", StringComparison.OrdinalIgnoreCase))
                return false;

            if (IsPlaceholderEntryTime(entryTimeRaw))
                return true;

            return string.Equals(attendanceStatus, "Abs", StringComparison.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Pre: on/before start. Late: after start through late entry. Late Abs: after late entry.
        /// If LateEntry is misconfigured before Start, Late floor is raised to Start.
        /// </summary>
        internal static string ClassifyEntryStatus(TimeSpan time, TimeSpan start, TimeSpan late, TimeSpan end)
        {
            if (late < start)
                late = start;

            if (time <= start)
                return "Pre";
            if (time <= late)
                return "Late";
            return "Late Abs";
        }

        internal static bool IsMorningPunch(TimeSpan punch) => punch.Hours < 12;

        internal static bool IsAfternoonSchedule(TimeSpan start, string startTimeRaw)
        {
            if (!string.IsNullOrWhiteSpace(startTimeRaw)
                && startTimeRaw.IndexOf("PM", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            return start.Hours >= 12;
        }

        /// <summary>
        /// Detects "6:50 AM – 8:00 PM" when a PM start was parsed as AM.
        /// A normal school day (7:15 AM – 2:30 PM) must stay valid.
        /// </summary>
        internal static bool LooksLikeMisParsedPmBlock(TimeSpan start, TimeSpan end)
        {
            if (start.Hours >= 12 || end.Hours < 18)
                return false;

            // >13h usually means PM start stored as AM; Hifz 08:30-20:50 (~12.3h) must stay allowed.
            return (end - start).TotalHours > 13;
        }

        internal static bool IsCandidateAllowedForPunch(TimeSpan punch, TimeSpan start, TimeSpan end, string startTimeRaw)
        {
            if (LooksLikeMisParsedPmBlock(start, end))
                return false;

            if (IsMorningPunch(punch) && IsAfternoonSchedule(start, startTimeRaw))
                return false;

            return true;
        }

        internal static bool IsValidExitPunch(TimeSpan punch, TimeSpan start, TimeSpan end, string startTimeRaw, string entryTimeRaw)
        {
            if (!IsCandidateAllowedForPunch(punch, start, end, startTimeRaw))
                return false;

            if (punch < start)
                return false;

            if (!string.IsNullOrWhiteSpace(entryTimeRaw)
                && ScheduleTimeHelper.TryParse(entryTimeRaw, out var entryTime)
                && punch < entryTime)
                return false;

            return true;
        }

        internal static bool CanMarkExit(TimeSpan punch, TimeSpan start, TimeSpan end, string startTimeRaw, string entryTimeRaw)
        {
            if (IsPlaceholderEntryTime(entryTimeRaw))
                return false;

            return IsValidExitPunch(punch, start, end, startTimeRaw, entryTimeRaw);
        }

        internal static bool IsWithinActiveWindow(TimeSpan punch, TimeSpan start, TimeSpan end)
        {
            return punch >= start && punch <= end;
        }

        internal static bool IsMidDayGap(TimeSpan punch, IEnumerable<(TimeSpan Start, TimeSpan End)> blocks, int earlyPreMinutes)
        {
            var ordered = blocks
                .OrderBy(x => x.Start)
                .ThenBy(x => x.End)
                .ToList();

            if (ordered.Count < 2)
                return false;

            for (var i = 0; i < ordered.Count - 1; i++)
            {
                var gapStart = ordered[i].End;
                var nextStart = ordered[i + 1].Start;
                var gapEnd = nextStart - TimeSpan.FromMinutes(earlyPreMinutes);

                // No real gap between these two schedules (overlap or early-pre touches).
                if (gapEnd <= gapStart)
                    continue;

                if (punch > gapStart && punch < gapEnd)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// A physical punch that already closed (ExitTime) or opened (EntryTime) another schedule today
        /// in the same minute must not also open this schedule. Stops one exit punch from becoming
        /// "Exit on Class + Pre on Coaching" when it is processed a second time.
        /// </summary>
        internal static bool IsPunchAlreadyAppliedToOtherSchedule(
            IEnumerable<Attendance_Record> todayDeviceRecords,
            int targetScheduleId,
            TimeSpan punch)
        {
            if (todayDeviceRecords == null)
                return false;

            var punchMinute = new TimeSpan(punch.Hours, punch.Minutes, 0);

            foreach (var record in todayDeviceRecords)
            {
                if (record == null || record.ScheduleID == targetScheduleId)
                    continue;

                if (record.Is_OUT &&
                    ScheduleTimeHelper.TryParse(record.ExitTime, out var exitTime) &&
                    new TimeSpan(exitTime.Hours, exitTime.Minutes, 0) == punchMinute)
                    return true;

                if (!IsPlaceholderEntryTime(record.EntryTime) &&
                    ScheduleTimeHelper.TryParse(record.EntryTime, out var entryTime) &&
                    new TimeSpan(entryTime.Hours, entryTime.Minutes, 0) == punchMinute)
                    return true;
            }

            return false;
        }
    }

    /// <summary>
    /// In-session ledger of physical punches (DeviceID + device timestamp incl. seconds).
    /// The same punch can reach the PC more than once (duplicate realtime event delivery,
    /// realtime + device log import, repeated startup imports); it must be applied at most once.
    /// </summary>
    internal static class PunchLedger
    {
        private static readonly ConcurrentDictionary<string, byte> Claimed =
            new ConcurrentDictionary<string, byte>();
        private static readonly object DayLock = new object();
        private static DateTime _day = DateTime.Today;

        /// <summary>Returns false when this exact punch was already taken for processing.</summary>
        internal static bool TryClaim(int deviceId, DateTime punch)
        {
            lock (DayLock)
            {
                if (_day != DateTime.Today)
                {
                    Claimed.Clear();
                    _day = DateTime.Today;
                }
            }

            return Claimed.TryAdd(deviceId + "|" + punch.ToString("yyyy-MM-dd HH:mm:ss"), 0);
        }
    }
}
