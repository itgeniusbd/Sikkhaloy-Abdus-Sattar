namespace Attendance_API.ViewModel
{
    /// <summary>Schedule day row for AttendanceDevice (string times + schedule name).</summary>
    public class ScheduleDayDeviceVM
    {
        public int ScheduleDayID { get; set; }
        public int ScheduleID { get; set; }
        public int SchoolID { get; set; }
        public string Day { get; set; }
        public string StartTime { get; set; }
        public string LateEntryTime { get; set; }
        public string EndTime { get; set; }
        public bool Is_OnDay { get; set; }
        public string ScheduleName { get; set; }
    }
}
