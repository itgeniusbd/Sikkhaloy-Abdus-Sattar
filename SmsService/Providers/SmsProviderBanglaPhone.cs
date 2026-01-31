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

            // Fix for Bangla/Unicode characters: Use UTF-8 byte encoding and manually convert to percent-encoding
            var smsText = UrlEncodeBangla(safeMassage);
            var receiversParam = number;
            var dataFormat = "userId={0}&password={1}&smsText={2}&commaSeperatedReceiverNumbers={3}";


            var urlEncodedData = string.Format(dataFormat, UserId, Password, smsText, receiversParam);
            var data = Encoding.UTF8.GetBytes(urlEncodedData);

            request.Method = "post";
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

        /// <summary>
        /// URL encode text with UTF-8 encoding (including Bangla/Unicode characters)
        /// </summary>
        private static string UrlEncodeBangla(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var bytes = Encoding.UTF8.GetBytes(text);
            var sb = new StringBuilder();

            foreach (var b in bytes)
            {
                // Only encode non-ASCII characters and special characters
                if ((b >= 'a' && b <= 'z') || (b >= 'A' && b <= 'Z') || (b >= '0' && b <= '9') ||
                    b == '-' || b == '_' || b == '.' || b == '~')
                {
                    sb.Append((char)b);
                }
                else if (b == ' ')
                {
                    sb.Append('+');
                }
                else
                {
                    sb.Append('%');
                    sb.Append(b.ToString("X2"));
                }
            }

            return sb.ToString();
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