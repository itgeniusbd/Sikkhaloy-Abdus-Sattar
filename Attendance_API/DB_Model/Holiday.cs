using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Attendance_API.DB_Model
{
    [Table("Employee_Holiday")]
    public class Holiday
    {
        [Key]
        public int HolidayID { get; set; }
        public int SchoolID { get; set; }
        public DateTime HolidayDate { get; set; }
        public string HolidayName { get; set; }
    }
}