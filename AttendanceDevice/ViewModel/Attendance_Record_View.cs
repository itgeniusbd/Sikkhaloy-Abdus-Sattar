using System;

namespace AttendanceDevice.ViewModel
{
    public class Attendance_Record_View
    {
        public int DeviceID { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public string AttendanceDate { get; set; }
        public string AttendanceStatus { get; set; }
        public string ExitStatus { get; set; }
        public string EntryTime { get; set; }
        public string ExitTime { get; set; }
        public bool Is_Student { get; set; }
        public DateTime dtAttendanceDate { get { return Convert.ToDateTime(this.AttendanceDate); } }
    }
}
