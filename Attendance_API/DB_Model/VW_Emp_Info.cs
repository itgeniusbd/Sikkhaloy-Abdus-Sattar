using System.ComponentModel.DataAnnotations;

namespace Attendance_API.DB_Model
{
    public class VW_Emp_Info
    {
        [Key]
        public int EmployeeID { get; set; }
        public int DeviceID { get; set; }
        public int SchoolID { get; set; }
    }
}