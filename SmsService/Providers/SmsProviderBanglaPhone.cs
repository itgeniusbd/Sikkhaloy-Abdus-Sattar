using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace SmsService
{
    public class SmsProviderBanglaPhone : ISmsProvider
    {
        private const string HostUrl = "http://loopsitbd.powersms.net.bd/httpapi/";
        private const string UserId = "Sikkhaloy";
        private const string Password = "Sikkhaloy@SMS_345";

        public int GetSmsBalance()
        {
            // Create Url
            const string actionUrl = "accountinfo";
            const string info = "package";

            const string dataFormat = "?userId={0}&password={1}&info={2}";
            var dataUrl = string.Format(dataFormat, UserId, Password, info);

            var address = new Uri(HostUrl + actionUrl + dataUrl);

            // Create the web request
            var request = WebRequest.Create(address) as HttpWebRequest;

            // Set type to GET
            request.Method = "GET";
            request.ContentType = "text/xml";

            try
            {
                using (var response = request.GetResponse())
                {
                    dynamic responseObject = ParseResponse(response);

                    if (responseObject.isError == "true")
                    {
                        throw new Exception(string.Format("Sms Sending was failed. Because: {0}",
                            responseObject.message));
                    }
                    else
                    {
                        // Get the response stream
                        return (int)responseObject.AvailableExternalSmsCount;
                    }
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

        public string SendSms(string massage, string number)
        {
            const string actionUrl = "sendsms"; // your powers ms site url; register the ip first
            var request = HttpWebRequest.Create(HostUrl + actionUrl);

            // Fix: Replace + with a safe alternative before encoding to preserve it in SMS
            // Option 1: Use full word "Plus" instead of symbol
            var safeMassage = massage.Replace("A+", "A Plus")
               .Replace("a+", "a Plus")
             .Replace("+", " Plus ");

            var smsText = Uri.EscapeDataString(safeMassage);
            var receiversParam = number;
            var dataFormat = "userId={0}&password={1}&smsText={2}&commaSeperatedReceiverNumbers={3}";


            var urlEncodedData = string.Format(dataFormat, UserId, Password, smsText, receiversParam);
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

                    if (responseObject.isError == "true")
                    {
                        throw new Exception(string.Format("Sms Sending was failed. Because: {0}",
                            responseObject.message));
                    }
                    else
                    {
                        return responseObject.insertedSmsIds;
                    }
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
            foreach (var smsModel in smsList)
            {
                SendSms(smsModel.Text, smsModel.Number);
            }
        }
    }
}