using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceDevice.Model
{
    [Table("Schedule_Day")]
    public class Attendance_Schedule_Day
    {
        [Key]
        [JsonProperty("id")]
        public int id { get; set; }

        [NotMapped]
        [JsonProperty("scheduleDayID")]
        public int ScheduleDayID
        {
            get => id;
            set
            {
                if (value > 0)
                    id = value;
            }
        }

        [JsonProperty("scheduleID")]
        public int ScheduleID { get; set; }

        [JsonProperty("schoolID")]
        public int SchoolID { get; set; }

        [JsonProperty("day")]
        public string Day { get; set; }

        [JsonProperty("lateEntryTime")]
        public string LateEntryTime { get; set; }

        [JsonProperty("startTime")]
        public string StartTime { get; set; }

        [JsonProperty("endTime")]
        public string EndTime { get; set; }

        [JsonProperty("is_OnDay")]
        public bool Is_OnDay { get; set; }

        public bool Is_Abs_Count { get; set; }

        [JsonProperty("scheduleName")]
        public string ScheduleName { get; set; }
    }
}

