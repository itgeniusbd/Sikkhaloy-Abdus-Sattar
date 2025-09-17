using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceDevice.Model
{
    [Table("Institution_Info")]
    public class Institution
    {
        [Key]
        public int Id { get; set; }
        public int SchoolID { get; set; }
        public string InstitutionName { get; set; }
        public string Image_Link { get; set; }
        public byte[] Logo { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public bool IsValid { get; set; }
        public string SettingKey { get; set; }
        public bool Is_Device_Attendance_Enable { get; set; }
        public bool Is_Student_Attendance_Enable { get; set; }
        public bool Is_Employee_Attendance_Enable { get; set; }
        public bool Is_Today_Holiday { get; set; }
        public bool Holiday_NotActive { get; set; }
        public string LastUpdateDate { get; set; }
        public int PingTimeOut { get; set; }
        [NotMapped]
        public DateTime Current_Datetime { get; set; }
    }
}
