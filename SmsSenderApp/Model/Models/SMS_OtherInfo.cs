using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmsSenderApp
{
    [Table("SMS_OtherInfo")]
    public class SMS_OtherInfo
    {
        [Key]
        public Guid SMS_Send_ID { get; set; }
        public int SchoolID { get; set; }
        public int? StudentID { get; set; }
        public int? TeacherID { get; set; }
        public int? EducationYearID { get; set; }
        public int? SMS_NumberID { get; set; }
        public int? CommitteeMemberId { get; set; }
    }
}