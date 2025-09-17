using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceDevice.Model
{
    [Table("Attendance_Record")]
    class Attendance_Record
    {
        [Key]
        public int RecordID { get; set; }
        public int DeviceID { get; set; }
        public string AttendanceDate { get; set; }
        public string AttendanceStatus { get; set; }
        public string ExitStatus { get; set; }
        public bool Is_OUT { get; set; }
        public string EntryTime { get; set; }
        public string ExitTime { get; set; }
        public bool Is_Sent { get; set; }
        public bool Is_Updated { get; set; }
    }
}
