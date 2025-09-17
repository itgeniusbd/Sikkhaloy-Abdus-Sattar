using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Attendance_API.DB_Model
{
    [Table("Attendance_Record")]
    public class Attendance_Record
    {
        [Key]
        public int AttendanceRecordID { get; set; }
        public int StudentID { get; set; }
        public int RegistrationID { get; set; } = 0;
        public int SchoolID { get; set; }
        public int? ClassID { get; set; }
        public int? StudentClassID { get; set; }
        public int? EducationYearID { get; set; }
        public string Attendance { get; set; }
        public DateTime AttendanceDate { get; set; }
        public string Reason { get; set; }
        public TimeSpan? EntryTime { get; set; }
        public TimeSpan? ExitTime { get; set; }
        public string ExitStatus { get; set; }
        public bool Is_OUT { get; set; }
        public bool IsFromDevice { get; set; } = true;

    }
}