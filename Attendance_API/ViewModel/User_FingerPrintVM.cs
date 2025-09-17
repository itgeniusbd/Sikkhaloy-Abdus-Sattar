namespace Attendance_API.DB_Model
{
    public class User_FingerPrintVM
    {
        public int DeviceID { get; set; }
        public int Finger_Index { get; set; }
        public string Temp_Data { get; set; }
        public int Flag { get; set; }
    }
}