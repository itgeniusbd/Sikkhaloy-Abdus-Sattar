using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Attendance_API.DB_Model
{
    [Table("Attendance_Device_DataUpdateList")]
    public class Attendance_Device_DataUpdateList
    {
        [Key]
        public int DateUpdateID { get; set; }
        public int SchoolID { get; set; }
        public string UpdateType { get; set; }
        public string UpdateDescription { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}