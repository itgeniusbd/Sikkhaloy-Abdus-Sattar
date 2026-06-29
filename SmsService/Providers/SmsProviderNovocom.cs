using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace SmsService
{
    public class SmsProviderNovocom : ISmsProvider
    {
        private const string HostUrl = "https://sms.novocom-bd.com/api/v2/";
        private const string ApiKey = "NmOEbTPV33xPQ3iTZczN6Uc99jMs/p/oljruf6NzJyI=";
        private const string ClientId = "621e744e-91cd-4ddd-8312-99c7a4cd8736";
        private const string SenderId = "8809658016341";

        static SmsProviderNovocom()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;
        }

        public int GetSmsBalance()
        {
            var address = BuildUri("Balance", new Dictionary<string, string>
            {
                { "ApiKey", ApiKey },
                { "ClientId", ClientId }
            });

            return ExecuteRequest(address, parseBalance: true, out _);
        }

        public string SendSms(string massage, string number, string senderId = null)
        {
            var normalizedNumber = NormalizePhoneNumber(number);
            var address = BuildUri("SendSMS", new Dictionary<string, string>
            {
                { "ApiKey", ApiKey },
                { "ClientId", ClientId },
                { "SenderId", SenderId },
                { "MobileNumbers", normalizedNumber },
                { "Message", massage ?? string.Empty },
                { "Is_Unicode", RequiresUnicode(massage) ? "true" : "false" }
            });

            ExecuteRequest(address, parseBalance: false, out var responseText);

            if (string.IsNullOrWhiteSpace(responseText))
                throw new Exception("Novocom accepted the request but returned no MessageId.");

            return responseText;
        }

        public void SendSmsMultiple(IEnumerable<SendSmsModel> smsList)
        {
            foreach (var smsModel in smsList)
            {
                SendSms(smsModel.Text, smsModel.Number);
            }
        }

        internal static string NormalizePhoneNumber(string number)
        {
            if (string.IsNullOrWhiteSpace(number))
                throw new Exception("Invalid mobile number.");

            var digits = Regex.Replace(number.Trim(), @"[^\d]", string.Empty);
            if (digits.StartsWith("880") && digits.Length == 13)
                return digits;

            if (digits.StartsWith("01") && digits.Length == 11)
                return "88" + digits;

            if (digits.Length == 10 && digits.StartsWith("1"))
                return "880" + digits;

            throw new Exception("Invalid mobile number format for Novocom: " + number);
        }

        private static bool RequiresUnicode(string message)
        {
            return !string.IsNullOrEmpty(message) && message.Any(c => c > 127);
        }

        private static Uri BuildUri(string action, Dictionary<string, string> queryParams)
        {
            var query = string.Join("&", queryParams.Select(p =>
                string.Format("{0}={1}", p.Key, Uri.EscapeDataString(p.Value ?? string.Empty))));

            return new Uri(HostUrl + action + "?" + query);
        }

        private static int ExecuteRequest(Uri address, bool parseBalance, out string responseText)
        {
            responseText = string.Empty;

            var request = WebRequest.Create(address) as HttpWebRequest;
            request.Method = "GET";
            request.Timeout = 30000;

            try
            {
                using (var response = request.GetResponse())
                {
                    var responseObject = ParseResponse(response) as JObject;
                    EnsureSuccess(responseObject);
                    responseText = responseObject.ToString(Formatting.None);

                    if (!parseBalance)
                    {
                        var messageId = responseObject["Data"]?.First?["MessageId"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(messageId))
                            responseText = messageId;

                        return 0;
                    }

                    var creditsToken = responseObject["Data"]?.First?["Credits"];
                    if (creditsToken == null)
                        return 0;

                    var creditsText = creditsToken.ToString();
                    var numericPart = new string(creditsText.Where(c => char.IsDigit(c) || c == '.').ToArray());
                    return double.TryParse(numericPart, out var credits) ? (int)Math.Floor(credits) : 0;
                }
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    var responseObject = ParseResponse(e.Response) as JObject;
                    throw new Exception(GetErrorMessage(responseObject));
                }

                throw;
            }
        }

        private static object ParseResponse(WebResponse r)
        {
            var response = (HttpWebResponse)r;
            var responseStream = response.GetResponseStream();

            if (responseStream == null)
                throw new Exception("Response stream found null.");

            using (var responseReader = new StreamReader(responseStream, Encoding.UTF8))
            {
                var responseString = responseReader.ReadToEnd()?.Trim();

                if (string.IsNullOrWhiteSpace(responseString))
                    throw new Exception("Empty response from Novocom SMS service.");

                if (!responseString.StartsWith("{") && !responseString.StartsWith("["))
                    throw new Exception("Novocom SMS error: " + responseString);

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

        private static void EnsureSuccess(JObject responseObject)
        {
            if (responseObject == null)
                throw new Exception("Invalid response from Novocom SMS service.");

            var errorCode = responseObject["ErrorCode"]?.Value<int?>();
            if (errorCode.HasValue && errorCode.Value != 0)
                throw new Exception(GetErrorMessage(responseObject));

            var firstItem = responseObject["Data"]?.First as JObject;
            if (firstItem == null)
                throw new Exception("Novocom response has no message data.");

            var messageErrorCode = firstItem["MessageErrorCode"]?.Value<int?>();
            if (messageErrorCode.HasValue && messageErrorCode.Value != 0)
            {
                var messageError = firstItem["MessageErrorDescription"]?.ToString();
                throw new Exception(string.IsNullOrWhiteSpace(messageError)
                    ? "Sms sending was failed."
                    : "Sms sending was failed. Because: " + messageError);
            }

            var messageId = firstItem["MessageId"]?.ToString();
            if (string.IsNullOrWhiteSpace(messageId))
                throw new Exception("Novocom did not return a MessageId.");
        }

        private static string GetErrorMessage(JObject responseObject)
        {
            if (responseObject == null)
                return "Sms sending was failed.";

            var description = responseObject["ErrorDescription"]?.ToString();
            if (!string.IsNullOrWhiteSpace(description))
                return "Sms sending was failed. Because: " + description;

            var firstItem = responseObject["Data"]?.First as JObject;
            var messageError = firstItem?["MessageErrorDescription"]?.ToString();
            return string.IsNullOrWhiteSpace(messageError)
                ? "Sms sending was failed."
                : "Sms sending was failed. Because: " + messageError;
        }
    }
}
