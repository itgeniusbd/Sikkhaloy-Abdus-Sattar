using System;

namespace ZKTeco.Core.Models
{
    /// <summary>
    /// ZKTeco Device Configuration Model
    /// </summary>
    public class DeviceConfig
    {
        public int Id { get; set; }
        public string DeviceName { get; set; }
        public string DeviceIP { get; set; }
        public int Port { get; set; } = 4370;
        public int CommKey { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public int SchoolID { get; set; }
        public string ApiBaseUrl { get; set; }
        public string ApiKey { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? LastSyncTime { get; set; }
        public string LastSyncStatus { get; set; }
        
        public override string ToString()
        {
            return $"{DeviceName} ({DeviceIP}:{Port})";
        }
    }
}
