using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using ZKTeco.Core.Models;

namespace ZKTeco.Core.Services
{
    /// <summary>
    /// Device Configuration Manager
    /// Handles saving/loading device configurations
    /// </summary>
    public class DeviceConfigService
    {
        private static readonly string ConfigDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "ZKTeco Manager"
        );
        
        private static readonly string ConfigFilePath = Path.Combine(ConfigDirectory, "devices.json");

        public DeviceConfigService()
        {
            EnsureConfigDirectoryExists();
        }

        private void EnsureConfigDirectoryExists()
        {
            if (!Directory.Exists(ConfigDirectory))
            {
                Directory.CreateDirectory(ConfigDirectory);
            }
        }

        public List<DeviceConfig> LoadDevices()
        {
            try
            {
                if (!File.Exists(ConfigFilePath))
                {
                    return new List<DeviceConfig>();
                }

                var json = File.ReadAllText(ConfigFilePath);
                return JsonConvert.DeserializeObject<List<DeviceConfig>>(json) ?? new List<DeviceConfig>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load device configurations: {ex.Message}", ex);
            }
        }

        public void SaveDevices(List<DeviceConfig> devices)
        {
            try
            {
                var json = JsonConvert.SerializeObject(devices, Formatting.Indented);
                File.WriteAllText(ConfigFilePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save device configurations: {ex.Message}", ex);
            }
        }

        public void AddDevice(DeviceConfig device)
        {
            var devices = LoadDevices();
            device.Id = devices.Count > 0 ? devices[devices.Count - 1].Id + 1 : 1;
            device.CreatedDate = DateTime.Now;
            devices.Add(device);
            SaveDevices(devices);
        }

        public void UpdateDevice(DeviceConfig device)
        {
            var devices = LoadDevices();
            var index = devices.FindIndex(d => d.Id == device.Id);
            if (index >= 0)
            {
                devices[index] = device;
                SaveDevices(devices);
            }
            else
            {
                throw new Exception($"Device with ID {device.Id} not found");
            }
        }

        public void DeleteDevice(int deviceId)
        {
            var devices = LoadDevices();
            var device = devices.Find(d => d.Id == deviceId);
            if (device != null)
            {
                devices.Remove(device);
                SaveDevices(devices);
            }
        }

        public DeviceConfig GetDevice(int deviceId)
        {
            var devices = LoadDevices();
            return devices.Find(d => d.Id == deviceId);
        }

        public string GetConfigFilePath()
        {
            return ConfigFilePath;
        }
    }
}
