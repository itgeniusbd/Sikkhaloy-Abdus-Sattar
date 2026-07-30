using AttendanceDevice.Model;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace AttendanceDevice.Config_Class
{
    internal static class ScheduleDisplayHelper
    {
        private static readonly Regex BengaliScriptRegex = new Regex(@"[\u0980-\u09FF]", RegexOptions.Compiled);

        public static bool ContainsBengaliScript(string text)
        {
            return !string.IsNullOrWhiteSpace(text) && BengaliScriptRegex.IsMatch(text);
        }

        public static string FormatLine(Attendance_Schedule_Day schedule)
        {
            if (schedule == null)
                return "Schedule: —";

            var name = string.IsNullOrWhiteSpace(schedule.ScheduleName)
                ? $"Schedule {schedule.ScheduleID}"
                : schedule.ScheduleName.Trim();

            var timeRange = FormatTimeRange(schedule.StartTime, schedule.EndTime);
            return string.IsNullOrWhiteSpace(timeRange)
                ? $"Schedule: {name}"
                : $"Schedule: {name} ({timeRange})";
        }

        public static void ApplyTo(UserView userView, Attendance_Schedule_Day schedule)
        {
            if (userView == null)
                return;

            if (schedule != null)
                userView.ScheduleID = schedule.ScheduleID;

            var name = schedule == null
                ? "—"
                : string.IsNullOrWhiteSpace(schedule.ScheduleName)
                    ? $"Schedule {schedule.ScheduleID}"
                    : schedule.ScheduleName.Trim();

            var timeRange = schedule == null ? string.Empty : FormatTimeRange(schedule.StartTime, schedule.EndTime);

            userView.ScheduleNameLine = $"Schedule: {name}";
            userView.ScheduleTimeLine = string.IsNullOrWhiteSpace(timeRange) ? "—" : timeRange;
            userView.ScheduleDisplay = FormatLine(schedule);
        }

        public static void ApplyTo(UserView userView, int scheduleId)
        {
            if (userView == null)
                return;

            var schedule = LocalData.Instance.GetTodayDisplaySchedules()
                .FirstOrDefault(s => s.ScheduleID == scheduleId)
                ?? LocalData.Instance.GetUserSchedule(scheduleId);

            ApplyTo(userView, schedule);
        }

        private static string FormatTimeRange(string startTime, string endTime)
        {
            if (!ScheduleTimeHelper.TryParse(startTime, out var start) ||
                !ScheduleTimeHelper.TryParse(endTime, out var end))
            {
                return string.Empty;
            }

            var startLabel = DateTime.Today.Add(start).ToString("h:mm tt", CultureInfo.InvariantCulture);
            var endLabel = DateTime.Today.Add(end).ToString("h:mm tt", CultureInfo.InvariantCulture);
            return $"{startLabel} - {endLabel}";
        }
    }
}
