using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmsSenderApp
{
    [Table("Attendance_SMS")]
    public class Attendance_SMS
    {
        [Key]
        public int Attendance_SMSID { get; set; }
        public int SchoolID { get; set; }
        public int StudentID { get; set; } = 0;
        public int EmployeeID { get; set; } = 0;
        public TimeSpan CreateTime { get; set; }
        public TimeSpan? SentTime { get; set; }
        public TimeSpan ScheduleTime { get; set; }
        public DateTime AttendanceDate { get; set; }
        public string SMS_Text { get; set; }
        public string MobileNo { get; set; }
        public string AttendanceStatus { get; set; }
        public int SMS_TimeOut { get; set; }
        public bool Is_Send { get; set; }
        public DateTime InsertDate { get; set; }

    }
}