using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceDevice.Model
{
    [Table("User_Schedule")]
    public class User_Schedule
    {
        [Key]
        public int UserScheduleID { get; set; }
        public int DeviceID { get; set; }
        public int ScheduleID { get; set; }
        public bool Is_Student { get; set; }
    }
}
