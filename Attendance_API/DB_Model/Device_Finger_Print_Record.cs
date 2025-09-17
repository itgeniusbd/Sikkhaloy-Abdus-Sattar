using System.ComponentModel.DataAnnotations;

namespace Attendance_API.DB_Model
{
    public class Device_Finger_Print_Record
    {
        [Key]
        public int Finger_PrintID { get; set; }
        public int SchoolID { get; set; }
        public int DeviceID { get; set; }
        public int Finger_Index { get; set; }
        public string Temp_Data { get; set; }
        public int Flag { get; set; }
    }
}