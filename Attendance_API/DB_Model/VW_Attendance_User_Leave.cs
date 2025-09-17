using System;
using System.ComponentModel.DataAnnotations;

namespace Attendance_API.DB_Model
{
    public class VW_Attendance_User_Leave
    {
        [Key]
        public int DeviceID { get; set; }
        public int SchoolID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}