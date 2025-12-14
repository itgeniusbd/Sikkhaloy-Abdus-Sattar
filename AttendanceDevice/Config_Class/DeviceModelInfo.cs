namespace AttendanceDevice.Config_Class
{
    /// <summary>
    /// Device Model and Firmware Information
    /// </summary>
    public class DeviceModelInfo
    {
        public string FirmwareVersion { get; set; } = "";
        public string DeviceModel { get; set; } = "";
        public string SerialNumber { get; set; } = "";
        public string Platform { get; set; } = "";
    }
}
