using System.ComponentModel.DataAnnotations;

namespace Attendance_API.DB_Model
{
    public class VW_Attendance_Stu
    {
        [Key]
        public int DeviceID { get; set; }
        public int SchoolID { get; set; }
        public int StudentID { get; set; }
        public int StudentClassID { get; set; }
        public int ClassID { get; set; }
        public int EducationYearID { get; set; }

    }
}