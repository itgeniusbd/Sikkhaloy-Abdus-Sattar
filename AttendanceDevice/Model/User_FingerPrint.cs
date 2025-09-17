using System.ComponentModel.DataAnnotations;

namespace AttendanceDevice.Model
{
    public class User_FingerPrint
    {
        [Key]
        public int Id { get; set; }
        public int DeviceID { get; set; }
        public int Finger_Index { get; set; }
        public string Temp_Data { get; set; }
        public int Flag { get; set; }
    }
}
