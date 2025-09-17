using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceDevice.Model
{
    [Table("User_Info")]
    public class User
    {
        [Key]
        public int UserId { get; set; }
        public int DeviceID { get; set; }
        public string ID { get; set; }
        public string RFID { get; set; }
        public string Name { get; set; }
        public string Designation { get; set; }
        public bool Is_Student { get; set; }
        public int ScheduleID { get; set; }
    }
}
