using System;
using System.ComponentModel.DataAnnotations;

namespace Attendance_API.DB_Model
{
    public class Attendance_Schedule_Day
    {
        [Key]
        public int ScheduleDayID { get; set; }
        public int ScheduleID { get; set; }
        public int SchoolID { get; set; }
        public string Day { get; set; }
        public TimeSpan LateEntryTime { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool Is_OnDay { get; set; }
    }
}