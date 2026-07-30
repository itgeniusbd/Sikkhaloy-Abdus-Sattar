using Newtonsoft.Json;

namespace AttendanceDevice.ViewModel
{
    internal class UserApiDto
    {
        [JsonProperty("deviceID")]
        public int DeviceID { get; set; }

        [JsonProperty("schoolID")]
        public int SchoolID { get; set; }

        [JsonProperty("scheduleID")]
        public int? ScheduleID { get; set; }

        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("rfid")]
        public string RFID { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("designation")]
        public string Designation { get; set; }

        [JsonProperty("isStudent")]
        public bool? IsStudent { get; set; }
    }
}
