using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceDevice.Model
{
    [Table("DataUpdateList")]
    class DataUpdateList
    {
        [Key]
        public int DateUpdateID { get; set; }
        public string UpdateType { get; set; }
        public string UpdateDescription { get; set; }
        public string UpdateDate { get; set; }
    }
}
