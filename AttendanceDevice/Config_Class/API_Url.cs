using System.Net;
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
            {
                return true;
            }

            try
            {
                using (var client = new WebClient())

                using (await client.OpenReadTaskAsync(EndPoint))
                {
                    return false;
                }
            }
            catch
            {
                return true;
            }
        }


        public static async Task<bool> IsNoNetConnection()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return true;
            }

            try
            {
                using (var client = new WebClient())

                using (await client.OpenReadTaskAsync("https://google.com"))
                {
                    return false;
                }
            }
            catch
            {
                return true;
            }
        }
    }
}
