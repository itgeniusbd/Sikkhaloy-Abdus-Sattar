using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceDevice.Model
{
    [Table("Devices")]
    public class Device
    {
        [Key]
        public int Id { get; set; }
        public string DeviceName { get; set; }
        public string DeviceIP { get; set; }
        public int Port { get; set; }
        public int IsConnected { get; set; }
        public string DeviceSN { get; set; }
        public int CommKey { get; set; }
        public string Last_Down_Log_Time { get; set; }
    }
}
