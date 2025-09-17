using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceDevice.Model
{
    [Table("Schedule_Day")]
    public class Attendance_Schedule_Day
    {
        [Key]
        public int id { get; set; }
        public int ScheduleID { get; set; }
        public int SchoolID { get; set; }
        public string Day { get; set; }
        public string LateEntryTime { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public bool Is_OnDay { get; set; }
        public bool Is_Abs_Count { get; set; }
    }
}

