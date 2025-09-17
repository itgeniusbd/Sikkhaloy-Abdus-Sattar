using System;

namespace AttendanceDevice.ViewModel
{
    public class LogView
    {
        public int DeviceId { get; set; }
        public TimeSpan EntryTime { get; set; }
        public string EntryDate { get; set; }
        public string EntryDay { get; set; }
        public DateTime EntryDateTime { get; set; }
    }
}
