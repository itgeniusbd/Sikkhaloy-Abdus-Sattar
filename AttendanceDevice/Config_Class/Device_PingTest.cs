using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace AttendanceDevice.Config_Class
{
    public static class Device_PingTest
    {
        public static async Task<bool> PingHostAsync(string nameOrAddress)
        {
            var pingAble = false;

            try
            {
                using (var ping = new Ping())
                {
                    var reply = await ping.SendPingAsync(nameOrAddress, LocalData.Instance.institution.PingTimeOut);
                    pingAble = reply.Status == IPStatus.Success;
                }
            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }

            return pingAble;
        }

        public static bool PingHost(string nameOrAddress)
        {
            var pingAble = false;

            try
            {
                using (var ping = new Ping())
                {
                    var reply = ping.Send(nameOrAddress, LocalData.Instance.institution.PingTimeOut);
                    pingAble = reply.Status == IPStatus.Success;
                }
            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }

            return pingAble;
        }
    }
}
