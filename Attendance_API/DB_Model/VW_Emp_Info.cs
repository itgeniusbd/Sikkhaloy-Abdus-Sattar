using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Attendance_API.DB_Model
{
    [Table("VW_Emp_Info")]
    public class VW_Emp_Info
    {
        [Key]
        public int EmployeeID { get; set; }
        public int DeviceID { get; set; }
        public int SchoolID { get; set; }
    }
}