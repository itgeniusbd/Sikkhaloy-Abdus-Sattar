using System;

namespace AttendanceDevice.Model
{
    public class UserView
    {
        public int DeviceID { get; set; }
        public string ID { get; set; }
        public string RFID { get; set; }
        public string Name { get; set; }
        public string Designation { get; set; }
        public string ImgLink { get; set; }
        public DateTime Enroll_Time { get; set; }
        public bool Is_Student { get; set; }

        public int ScheduleID { get; set; }
    }

    public class Attendance_view
    {
        public int DeviceID { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public string Designation { get; set; }
        public string ImgLink { get; set; }
        public string AttendanceStatus { get; set; }
        public string AttendanceDate { get; set; }
        public bool Is_Student { get; set; }
        public bool Is_OUT { get; set; }
        public string EntryTime { get; set; }
        public string ExitTime { get; set; }
    }
}
