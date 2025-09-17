namespace AttendanceDevice.ViewModel
{
    public class LogDataSendModel
    {
        public int DeviceID { get; set; }
        public string EntryTime { get; set; }
        public string EntryDate { get; set; }
        public string EntryDay { get; set; }
        public bool Is_Student { get; set; }
        public int ScheduleID { get; set; }
    }
}