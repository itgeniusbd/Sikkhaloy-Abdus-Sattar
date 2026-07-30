using Newtonsoft.Json;



namespace AttendanceDevice.ViewModel

{

    internal class ScheduleDayApiDto

    {

        [JsonProperty("scheduleDayID")]

        public int ScheduleDayID { get; set; }



        [JsonProperty("scheduleID")]

        public int ScheduleID { get; set; }



        [JsonProperty("schoolID")]

        public int SchoolID { get; set; }



        [JsonProperty("day")]

        public string Day { get; set; }



        [JsonProperty("startTime")]

        public string StartTime { get; set; }



        [JsonProperty("lateEntryTime")]

        public string LateEntryTime { get; set; }



        [JsonProperty("endTime")]

        public string EndTime { get; set; }



        [JsonProperty("isOnDay")]

        public bool IsOnDay { get; set; } = true;



        [JsonProperty("is_OnDay")]

        public bool? Is_OnDay { get; set; }



        [JsonProperty("scheduleName")]

        public string ScheduleName { get; set; }

    }

}

