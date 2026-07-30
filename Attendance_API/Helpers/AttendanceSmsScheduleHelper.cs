using System;

namespace Attendance_API.Helpers
{
    internal static class AttendanceSmsScheduleHelper
    {
        /// <summary>
        /// SmsSender sends only while current time is before ScheduleTime + SMS_TimeOut.
        /// Backfilled Abs/Late rows often keep an old schedule time and miss the window.
        /// </summary>
        public static TimeSpan EnsureSendableScheduleTime(TimeSpan scheduleTime, int smsTimeoutMinutes)
        {
            var now = DateTime.Now.TimeOfDay;
            if (scheduleTime.TotalMinutes + smsTimeoutMinutes <= now.TotalMinutes)
                return now;

            return scheduleTime;
        }
    }
}
