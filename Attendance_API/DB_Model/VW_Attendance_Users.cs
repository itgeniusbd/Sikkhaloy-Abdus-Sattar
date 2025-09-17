using System;

namespace Attendance_API.DB_Model
{
    public class VW_Attendance_Users
    {
        public int DeviceID { get; set; }
        public int SchoolID { get; set; }
        public Nullable<int> ScheduleID { get; set; }
        public string ID { get; set; }
        public string RFID { get; set; }
        public string Name { get; set; }
        public string Designation { get; set; }
        public Nullable<bool> Is_Student { get; set; }
    }
}