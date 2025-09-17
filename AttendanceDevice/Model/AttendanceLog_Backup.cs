using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceDevice.Model
{
    [Table("Attendance_Log_Backup")]
    public class AttendanceLog_Backup
    {
        [Key]
        public int LogId { get; set; }
        public int DeviceID { get; set; }
        public string Entry_Time { get; set; }
        public string Entry_Date { get; set; }
        public string Entry_Day { get; set; }
        public string Backup_Reason { get; set; }

    }
}
