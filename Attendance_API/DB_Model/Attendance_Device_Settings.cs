using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Attendance_API.DB_Model
{
    [Table("Attendance_Device_Setting")]
    public class Attendance_Device_Setting
    {
        [Key]
        public int SchoolID { get; set; }
        public string UserName { get; set; }
        public bool IsActive { get; set; }
        public string SettingKey { get; set; }
        public string Image_Link { get; set; }
        public bool Is_Device_Attendance_Enable { get; set; }
        public bool Is_Student_Attendance_Enable { get; set; }
        public bool Is_Employee_Attendance_Enable { get; set; }
        public bool Is_Holiday_As_Offday { get; set; }
        public bool Is_All_SMS_On { get; set; }
        public bool Is_English_SMS { get; set; }
        public bool Is_Student_All_SMS_Active { get; set; }
        public bool Is_Student_Entry_SMS_ON { get; set; }
        public bool Is_Student_Exit_SMS_ON { get; set; }
        public bool Is_Student_Abs_SMS_ON { get; set; }
        public bool Is_Student_Late_SMS_ON { get; set; }
        public bool Is_Employee_SMS_Active { get; set; }
        public bool Is_Employee_Abs_SMS_ON { get; set; }
        public bool Is_Employee_Late_SMS_ON { get; set; }
        public bool Is_Employee_SMS_OwnNumber { get; set; }
        public string Employee_SMS_Number { get; set; }
        public int SMS_TimeOut_Minute { get; set; }
    }
}