using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Attendance_API.DB_Model
{
    [Table("SchoolInfo")]
    public class SchoolInfo
    {
        [Key]
        public int SchoolID { get; set; }
        public string SchoolName { get; set; }
        public byte[] SchoolLogo { get; set; }
        public string Phone { get; set; }
        public string UserName { get; set; }
        public string Validation { get; set; }
        public int? School_SN { get; set; }
    }
}