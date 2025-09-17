using System;

namespace Attendance_API.Models
{
    public class BackupDataAPI
    {
        public int DeviceID { get; set; }
        public TimeSpan EntryTime { get; set; }
        public DateTime EntryDate { get; set; }
        public string EntryDay { get; set; }
        public bool Is_Student { get; set; }
        public int ScheduleID { get; set; }
    }
}