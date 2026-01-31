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
            const string actionUrl = "api.php?json";
            var request = HttpWebRequest.Create(HostUrl + actionUrl);
            
            // Fix: Replace + with a safe alternative before encoding to preserve it in SMS
            var safeMassage = massage.Replace("A+", "A Plus")
                .Replace("a+", "a Plus")
                .Replace("+", " Plus ");
            
            // GreenWeb API expects form-urlencoded data, but we need proper UTF-8 encoding
            // Use direct URL encoding with proper UTF-8 handling
            var smsText = Uri.EscapeDataString(safeMassage);  // This properly handles Unicode
            
            var dataFormat = "token={0}&to={1}&message={2}";
            var urlEncodedData = string.Format(dataFormat, ApiKey, number, smsText);
            var data = Encoding.UTF8.GetBytes(urlEncodedData);

            request.Method = "POST";
            request.Proxy = null;
            request.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
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
            const string actionUrl = "api.php?json";
            var request = HttpWebRequest.Create(HostUrl + actionUrl);

            // Create array of SMS data
            var smsData = smsList.Select(s =>
                new
                {
                    to = s.Number,
                    message = s.Text.Replace("A+", "A Plus").Replace("a+", "a Plus").Replace("+", " Plus ")
                }).ToList();

            // Send as form data with smsdata parameter
            var jsonSmsData = JsonConvert.SerializeObject(smsData);
            var dataFormat = "token={0}&smsdata={1}";
            var urlEncodedData = string.Format(dataFormat, ApiKey, Uri.EscapeDataString(jsonSmsData));
            var data = Encoding.UTF8.GetBytes(urlEncodedData);

            request.Method = "POST";
            request.Proxy = null;
            request.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
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
                    // Multiple SMS response handling
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