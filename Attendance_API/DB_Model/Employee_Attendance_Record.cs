using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Attendance_API.DB_Model
{
    [Table("Employee_Attendance_Record")]
    public class Employee_Attendance_Record
    {
        [Key]
        public int Employee_Attendance_RecordID { get; set; }
        public int SchoolID { get; set; }
        public int RegistrationID { get; set; }
        public int EmployeeID { get; set; }
        public string AttendanceStatus { get; set; }
        public DateTime AttendanceDate { get; set; }
        public TimeSpan? EntryTime { get; set; }
        public TimeSpan? ExitTime { get; set; }
        public string ExitStatus { get; set; }
        public bool Is_OUT { get; set; }
        public bool IsFromDevice { get; set; } = true;

    }
}