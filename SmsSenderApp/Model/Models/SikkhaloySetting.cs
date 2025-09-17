using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmsSenderApp
{
    [Table("SikkhaloySetting")]
    public class SikkhaloySetting
    {
        [Key]
        public int SikkhaloySettingId { get; set; }
        public string SmsProvider { get; set; }
        public string SmsProviderMultiple { get; set; }
        public int SmsSendInterval { get; set; }
        public int SmsProcessingUnit { get; set; }
    }
}