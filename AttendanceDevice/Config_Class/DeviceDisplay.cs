using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AttendanceDevice.Config_Class
{
    public class DeviceDisplay
    {
        public List<DeviceConnection> Devices { get; }

        private int _cachedConnectedCount = -1;
        private DateTime _cacheTime = DateTime.MinValue;

        public DeviceDisplay(List<DeviceConnection> devices)
        {
            Devices = devices ?? new List<DeviceConnection>();
        }

        public void InvalidateConnectionCache()
        {
            _cachedConnectedCount = -1;
            _cacheTime = DateTime.MinValue;
        }

        public async Task<int> Total_DevicesAsync(bool forceRefresh = false)
        {
            if (!forceRefresh &&
                _cachedConnectedCount >= 0 &&
                DateTime.Now - _cacheTime < PerformanceSettings.DevicePingCacheDuration)
            {
                return _cachedConnectedCount;
            }

            var disconnected = new List<DeviceConnection>();
            foreach (var device in Devices.ToList())
            {
                if (!await device.IsConnectedAsync().ConfigureAwait(false))
                    disconnected.Add(device);
            }

            foreach (var device in disconnected)
                Devices.Remove(device);

            _cachedConnectedCount = Devices.Count;
            _cacheTime = DateTime.Now;
            return _cachedConnectedCount;
        }

        public int Total_Devices()
        {
            if (_cachedConnectedCount >= 0 &&
                DateTime.Now - _cacheTime < PerformanceSettings.DevicePingCacheDuration)
            {
                return _cachedConnectedCount;
            }

            return Total_DevicesAsync(forceRefresh: true).GetAwaiter().GetResult();
        }
    }
}
