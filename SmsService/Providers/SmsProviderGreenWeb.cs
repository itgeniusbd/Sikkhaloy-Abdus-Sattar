using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace SmsService
{
    public class SmsProviderGreenWeb : ISmsProvider
    {
        private const string HostUrl = "https://api.greenweb.com.bd/";
        private const string ApiKey = "90282210471675095047ee665e3d0ba098844814cab35e133dc4";
        public int GetSmsBalance()
        {
            // Create Url
            var actionUrl = $"g_api.php?token={ApiKey}&balance&rate&json";

            var address = new Uri(HostUrl + actionUrl);

            // Create the web request
            var request = WebRequest.Create(address) as HttpWebRequest;

            // Set type to POST
            request.Method = "GET";
            //request.ContentType = "text/xml";

            try
            {
                using (var response = request.GetResponse())
                {
                    dynamic responseObject = ParseResponse(response);
                    var amount = (double)responseObject[0].response;
                    var rate = (double)responseObject[1].response;
                    return (int)(amount / rate);
                }
            }
            catch (WebException e)
            {
                dynamic responseObject = ParseResponse(e.Response);

                if (responseObject.isError == "true")
                {
                    throw new Exception("Sms Sending was failed. Because: " + responseObject.message);
                }
            }

            return 0;

        }

        public string SendSms(string massage, string number)
        {
            const string actionUrl = "api.php?json"; // your powers ms site url; register the ip first
            var request = HttpWebRequest.Create(HostUrl + actionUrl);
            var smsText = Uri.EscapeDataString(massage);
            var dataFormat = "token={0}&to={1}&message={2}";


            var urlEncodedData = string.Format(dataFormat, ApiKey, number, smsText);
            var data = Encoding.ASCII.GetBytes(urlEncodedData);

            request.Method = "post";
            request.Proxy = null;
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;


            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(data, 0, data.Length);
            }

            try
            {
                using (var response = request.GetResponse())
                {
                    dynamic responseObject = ParseResponse(response);

                    if (responseObject[0].status != "SENT")
                    {
                        throw new Exception(string.Format("Sms Sending was failed. Because: {0}",
                            responseObject[0].statusmsg));
                    }

                    return responseObject[0].statusmsg;
                }
            }
            catch (WebException e)
            {
                dynamic responseObject = ParseResponse(e.Response);

                if (responseObject.isError == "true")
                {
                    throw new Exception("Sms Sending was failed. Because: " + responseObject.message);
                }
            }

            return string.Empty;
        }

        public void SendSmsMultiple(IEnumerable<SendSmsModel> smsList)
        {
            const string actionUrl = "api.php?json"; // your powers ms site url; register the ip first
            var request = HttpWebRequest.Create(HostUrl + actionUrl);

            var smsData = smsList.Select(s =>
                new
                {
                    to = s.Number,
                    message = Uri.EscapeDataString(s.Text)
                }).ToList();

            var jsonSmsData = JsonConvert.SerializeObject(smsData);
            var dataFormat = "token={0}&smsdata={1}";


            var urlEncodedData = string.Format(dataFormat, ApiKey, jsonSmsData);
            var data = Encoding.ASCII.GetBytes(urlEncodedData);

            request.Method = "post";
            request.Proxy = null;
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;


            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(data, 0, data.Length);
            }

            try
            {
                using (var response = request.GetResponse())
                {
                    dynamic responseObject = ParseResponse(response);

                    //if (responseObject[0].status != "SENT")
                    //{
                    //    throw new Exception(string.Format("Sms Sending was failed. Because: {0}",
                    //        responseObject[0].statusmsg));
                    //}
                }
            }
            catch (WebException e)
            {
                dynamic responseObject = ParseResponse(e.Response);

                if (responseObject.isError == "true")
                {
                    throw new Exception("Sms Sending was failed. Because: " + responseObject.message);
                }
            }
        }
        private static object ParseResponse(WebResponse r)
        {
            var response = (HttpWebResponse)r;

            var responseStream = response.GetResponseStream();

            if (responseStream == null) throw new Exception("Response stream found null.");

            using (var responseReader = new StreamReader(responseStream))
            {
                var responseString = responseReader.ReadToEnd();

                try
                {
                    return JsonConvert.DeserializeObject(responseString);
                }
                catch
                {
                    throw new Exception(
                        $"The sms service calling was unsuccessful with code:{(int)response.StatusCode}[{response.StatusCode}]");
                }
            }
        }
    }
}