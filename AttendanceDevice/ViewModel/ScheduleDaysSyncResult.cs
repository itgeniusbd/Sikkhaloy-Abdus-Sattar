namespace AttendanceDevice.ViewModel
{
    public sealed class ScheduleDaysSyncResult
    {
        public bool Success { get; set; }
        public bool UserScheduleMismatch { get; set; }

        public static ScheduleDaysSyncResult Failed()
        {
            return new ScheduleDaysSyncResult();
        }

        public static ScheduleDaysSyncResult Ok(bool userScheduleMismatch = false)
        {
            return new ScheduleDaysSyncResult
            {
                Success = true,
                UserScheduleMismatch = userScheduleMismatch
            };
        }
    }
}
