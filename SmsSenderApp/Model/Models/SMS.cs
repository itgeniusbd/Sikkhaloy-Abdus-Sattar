using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmsSenderApp
{
    [Table("SMS")]
    public class SMS
    {
        [Key]
        public int SMSID { get; set; }
        public int SchoolID { get; set; }
        public int SMS_Balance { get; set; }
        public string Masking { get; set; }
        public DateTime Date { get; set; }
    }
}