using System;

namespace Attendance_API.DB_Model
{
    public class SchoolShortVM
    {
        public int SchoolID { get; set; }
        public bool IsValid { get; set; }
        public string SettingKey { get; set; }
        public bool Is_Device_Attendance_Enable { get; set; }
        public bool Is_Student_Attendance_Enable { get; set; }
        public bool Is_Employee_Attendance_Enable { get; set; }
        public bool Is_Today_Holiday { get; set; }
        public bool Holiday_NotActive { get; set; }
        public string LastUpdateDate { get; set; }
        public DateTime Current_Datetime { get; set; }
    }
}