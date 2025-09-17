using System;
using System.ComponentModel.DataAnnotations;

namespace Attendance_API.DB_Model
{
    public class VW_Attendance_Emp_Setting
    {
        [Key]
        public int EmployeeID { get; set; }
        public int SchoolID { get; set; }
        public int ScheduleID { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public TimeSpan LateEntryTime { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool Is_Abs_SMS { get; set; }
        public bool Is_Late_SMS { get; set; }
    }
}