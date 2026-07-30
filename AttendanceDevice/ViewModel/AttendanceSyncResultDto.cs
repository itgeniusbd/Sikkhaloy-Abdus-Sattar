using Newtonsoft.Json;

namespace AttendanceDevice.ViewModel
{
    public class AttendanceSyncResultDto
    {
        [JsonProperty("matched")]
        public int Matched { get; set; }

        [JsonProperty("inserted")]
        public int Inserted { get; set; }

        [JsonProperty("smsQueued")]
        public int SmsQueued { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("matchedDeviceIds")]
        public int[] MatchedDeviceIds { get; set; }
    }
}
