using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Attendance_API.DB_Model
{
    [Table("VW_Attendance_Stu")]
    public class VW_Attendance_Stu
    {
        [Key]
        public int DeviceID { get; set; }
        public int SchoolID { get; set; }
        public int StudentID { get; set; }
        public int StudentClassID { get; set; }
        public int ClassID { get; set; }
        public int EducationYearID { get; set; }

        // Not present on the SQL view; schedule comes from device payload.
        [NotMapped]
        public int? ScheduleID { get; set; }

    }
}