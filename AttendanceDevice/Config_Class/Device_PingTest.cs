using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace AttendanceDevice.Config_Class
{
    public static class Device_PingTest
    {
        // Default timeout if not configured
        private const int DefaultTimeout = 5000; // 5 seconds

        public static async Task<bool> PingHostAsync(string nameOrAddress)
        {
            var pingAble = false;

            try
            {
                using (var ping = new Ping())
                {
                    // Use configured timeout or default
                    var timeout = LocalData.Instance.institution?.PingTimeOut ?? DefaultTimeout;

                    // Ensure minimum timeout of 1000ms
                    if (timeout < 1000)
                        timeout = 1000;

                    var reply = await ping.SendPingAsync(nameOrAddress, timeout);
                    pingAble = reply.Status == IPStatus.Success;

                    // Log ping result for debugging
                    if (!pingAble && reply.Status != IPStatus.Success)
                    {
                        Console.WriteLine($"Ping failed for {nameOrAddress}: {reply.Status}");
                    }
                }
            }
            catch (PingException ex)
            {
                Console.WriteLine($"PingException for {nameOrAddress}: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error pinging {nameOrAddress}: {ex.Message}");
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
                    // Use configured timeout or default
                    var timeout = LocalData.Instance.institution?.PingTimeOut ?? DefaultTimeout;

                    // Ensure minimum timeout of 1000ms
                    if (timeout < 1000)
                        timeout = 1000;

                    var reply = ping.Send(nameOrAddress, timeout);
                    pingAble = reply.Status == IPStatus.Success;

                    // Log ping result for debugging
                    if (!pingAble && reply.Status != IPStatus.Success)
                    {
                        Console.WriteLine($"Ping failed for {nameOrAddress}: {reply.Status}");
                    }
                }
            }
            catch (PingException ex)
            {
                Console.WriteLine($"PingException for {nameOrAddress}: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error pinging {nameOrAddress}: {ex.Message}");
            }

            return pingAble;
        }
    }
}
