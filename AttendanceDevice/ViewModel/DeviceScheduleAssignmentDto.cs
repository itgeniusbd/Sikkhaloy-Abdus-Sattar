using Newtonsoft.Json;

namespace AttendanceDevice.ViewModel
{
    public class DeviceScheduleAssignmentDto
    {
        [JsonProperty("deviceID")]
        public int DeviceID { get; set; }

        [JsonProperty("scheduleID")]
        public int ScheduleID { get; set; }

        [JsonProperty("isStudent")]
        public bool IsStudent { get; set; }
    }
}
