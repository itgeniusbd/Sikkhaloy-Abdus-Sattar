using AttendanceDevice.APIClass;
using Newtonsoft.Json;
using RestSharp;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace AttendanceDevice.Config_Class
{
    internal static class ApiLoginHelper
    {
        public static RestRequest CreateTokenRequest(string username, string password)
        {
            var request = new RestRequest("Token", Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("grant_type", "password", ParameterType.GetOrPost);
            request.AddParameter("username", username, ParameterType.GetOrPost);
            request.AddParameter("password", password, ParameterType.GetOrPost);
            return request;
        }

        public static string GetTokenErrorMessage(IRestResponse response)
        {
            if (response == null)
                return "No response from server";

            var content = ApiResponseHelper.ReadContent(response);
            var token = ApiResponseHelper.ParseToken(content);
            if (!string.IsNullOrWhiteSpace(token?.error_description))
                return token.error_description;

            if (!string.IsNullOrWhiteSpace(content))
            {
                content = content.Trim();
                if (content.StartsWith("<"))
                    return GetHtmlErrorMessage(response, content);

                try
                {
                    var tokenPayload = JsonConvert.DeserializeObject<Token>(content);
                    if (!string.IsNullOrWhiteSpace(tokenPayload?.error_description))
                        return tokenPayload.error_description;
                }
                catch
                {
                    if (content.Length > 300)
                        return content.Substring(0, 300) + "...";
                    return content;
                }
            }

            if (!string.IsNullOrWhiteSpace(response.ErrorMessage))
                return response.ErrorMessage;

            if (response.StatusCode != 0)
                return $"Login failed ({(int)response.StatusCode} {response.StatusDescription})";

            return "Response not found";
        }

        private static string GetHtmlErrorMessage(IRestResponse response, string content)
        {
            var titleMatch = Regex.Match(content, "<title>(.*?)</title>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (titleMatch.Success)
            {
                var title = WebUtility.HtmlDecode(titleMatch.Groups[1].Value.Trim());
                if (!string.IsNullOrWhiteSpace(title))
                    return $"API server error: {title} ({(int)response.StatusCode})";
            }

            return $"API server returned HTML instead of login JSON ({(int)response.StatusCode}). Check Attendance_API on api.sikkhaloy.com.";
        }
    }
}
