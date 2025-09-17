using System.ComponentModel.DataAnnotations;

namespace AttendanceDevice.Model
{
    class User_Leave_Record
    {
        [Key]
        public int Id { get; set; }
        public int DeviceID { get; set; }
        public string LeaveDate { get; set; }
    }
}
