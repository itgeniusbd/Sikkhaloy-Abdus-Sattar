using System;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace AttendanceDevice.Config_Class
{
    public static class ApiUrl
    {
        public static readonly string EndPoint = "https://api.sikkhaloy.com/";
        //public static readonly string EndPoint = "http://192.168.0.108:45455/"; //developmnent

        public static readonly string WebUrl = "https://sikkhaloy.com";
        //public static readonly string WebUrl = "http://localhost:3326"; //developmnent

        public static async Task<bool> IsServerUnavailable()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
                return true;

            return !await ProbeUrlAsync(EndPoint);
        }

        public static async Task<bool> IsNoNetConnection()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
                return true;

            return !await ProbeUrlAsync("https://www.google.com/generate_204");
        }

        private static async Task<bool> ProbeUrlAsync(string url)
        {
            try
            {
                using (var http = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(PerformanceSettings.StartupNetworkProbeTimeoutSeconds)
                })
                using (var response = await http.GetAsync(url))
                {
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
