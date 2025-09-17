using System;
using System.ComponentModel.DataAnnotations;

namespace Attendance_API.DB_Model
{
    public class VW_Attendance_Stu_Setting
    {
        [Key]
        public int StudentID { get; set; }
        public int SchoolID { get; set; }
        public int ScheduleID { get; set; }
        public string StudentsName { get; set; }
        public string SMSPhoneNo { get; set; }
        public TimeSpan LateEntryTime { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool Entry_Confirmation { get; set; }
        public bool Exit_Confirmation { get; set; }
        public bool Is_Abs_SMS { get; set; }
        public bool Is_Late_SMS { get; set; }
    }
}