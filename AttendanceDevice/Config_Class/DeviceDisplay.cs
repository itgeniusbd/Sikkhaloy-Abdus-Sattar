using System.Collections.Generic;
using System.Linq;

namespace AttendanceDevice.Config_Class
{
    public class DeviceDisplay
    {
        public List<DeviceConnection> Devices { get; }
        public DeviceDisplay(List<DeviceConnection> devices)
        {
            this.Devices = devices;
        }
        public int Total_Devices()
        {
            foreach (var device in Devices.ToList())
            {
                var connected = device.IsConnected();

                if (!connected)
                    Devices.Remove(device);
            }
            return Devices.Count();
        }
    }
}
