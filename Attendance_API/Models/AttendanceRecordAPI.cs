using System;

namespace Attendance_API.Models
{
    public class AttendanceRecordAPI
    {
        public int RecordID { get; set; }
        public int DeviceID { get; set; }
        public DateTime AttendanceDate { get; set; }
        public string AttendanceStatus { get; set; }
        public string ExitStatus { get; set; }
        public bool Is_OUT { get; set; }
        public TimeSpan? EntryTime { get; set; }
        public TimeSpan? ExitTime { get; set; }
        public bool Is_Sent { get; set; }
        public bool Is_Updated { get; set; }
    }
}