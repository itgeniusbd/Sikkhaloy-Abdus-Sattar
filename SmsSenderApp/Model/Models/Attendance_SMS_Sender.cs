using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmsSenderApp
{
    [Table("Attendance_SMS_Sender")]
    public class Attendance_SMS_Sender
    {
        [Key]
        public int AttendanceSmsSenderId { get; set; }
        public DateTime AppStartTime { get; set; }
        public DateTime? AppCloseTime { get; set; }
        public int TotalEventCall { get; set; }
        public int TotalSmsSend { get; set; }
        public int TotalSmsFailed { get; set; }
    }
}