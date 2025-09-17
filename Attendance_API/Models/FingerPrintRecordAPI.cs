namespace Attendance_API.Models
{
    public class FingerPrintRecordAPI
    {
        public int DeviceID { get; set; }
        public int Finger_Index { get; set; }
        public string Temp_Data { get; set; }
        public int Flag { get; set; }
    }
}