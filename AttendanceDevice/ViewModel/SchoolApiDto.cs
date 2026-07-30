using System;
using Newtonsoft.Json;

namespace AttendanceDevice.ViewModel
{
    internal class SchoolApiDto
    {
        [JsonProperty("schoolID")]
        public int SchoolID { get; set; }

        [JsonProperty("institutionName")]
        public string InstitutionName { get; set; }

        [JsonProperty("image_Link")]
        public string Image_Link { get; set; }

        [JsonProperty("logo")]
        public byte[] Logo { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("isValid")]
        public bool IsValid { get; set; }

        [JsonProperty("settingKey")]
        public string SettingKey { get; set; }

        [JsonProperty("is_Device_Attendance_Enable")]
        public bool Is_Device_Attendance_Enable { get; set; }

        [JsonProperty("is_Student_Attendance_Enable")]
        public bool Is_Student_Attendance_Enable { get; set; }

        [JsonProperty("is_Employee_Attendance_Enable")]
        public bool Is_Employee_Attendance_Enable { get; set; }

        [JsonProperty("is_Today_Holiday")]
        public bool Is_Today_Holiday { get; set; }

        /// <summary>API name; same meaning as Institution.Holiday_NotActive (attendance allowed on holiday when true).</summary>
        [JsonProperty("holiday_Active")]
        public bool Holiday_Active { get; set; }

        [JsonProperty("lastUpdateDate")]
        public string LastUpdateDate { get; set; }

        [JsonProperty("current_Datetime")]
        public DateTime Current_Datetime { get; set; }
    }
}
