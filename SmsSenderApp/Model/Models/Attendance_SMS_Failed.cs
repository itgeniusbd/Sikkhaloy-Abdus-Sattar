using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmsSenderApp
{
    [Table("Attendance_SMS_Failed")]
    public class Attendance_SMS_Failed
    {
        [Key]
        public int AttendanceSmsFailedId { get; set; }
        public int SchoolID { get; set; }
        public TimeSpan ScheduleTime { get; set; }
        public TimeSpan CreateTime { get; set; }
        public TimeSpan? SentTime { get; set; }
        public DateTime AttendanceDate { get; set; }
        public string SMS_Text { get; set; }
        public string MobileNo { get; set; }
        public string AttendanceStatus { get; set; }
        public int SMS_TimeOut { get; set; }
        public int? EmployeeID { get; set; }
        public int? StudentID { get; set; }
        public string FailedReson { get; set; }
        //public DateTime InsertDate { get; set; }
    }
}