namespace Attendance_API.Models
{
    public class AttendanceSyncResult
    {
        public int Matched { get; set; }
        public int Inserted { get; set; }
        public int SmsQueued { get; set; }
        public string Message { get; set; }
        public int[] MatchedDeviceIds { get; set; }
    }
}
