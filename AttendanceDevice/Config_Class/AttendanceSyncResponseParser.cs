using AttendanceDevice.ViewModel;
using Newtonsoft.Json;
using RestSharp;
using System.Net;
using System.Text.RegularExpressions;

namespace AttendanceDevice.Config_Class
{
    internal static class AttendanceSyncResponseParser
    {
        public static AttendanceSyncResultDto Parse(IRestResponse response)
        {
            if (response == null || string.IsNullOrWhiteSpace(response.Content))
                return null;

            var content = response.Content.Trim();
            if (content == "null")
                return null;

            try
            {
                return JsonConvert.DeserializeObject<AttendanceSyncResultDto>(content);
            }
            catch
            {
                return TryParseXmlResult(content);
            }
        }

        private static AttendanceSyncResultDto TryParseXmlResult(string content)
        {
            if (!content.StartsWith("<"))
                return null;

            var matched = Regex.Match(content, @"<Matched[^>]*>(\d+)</Matched>", RegexOptions.IgnoreCase);
            var inserted = Regex.Match(content, @"<Inserted[^>]*>(\d+)</Inserted>", RegexOptions.IgnoreCase);
            var message = Regex.Match(content, @"<Message[^>]*>(.*?)</Message>", RegexOptions.IgnoreCase);

            if (!matched.Success && !inserted.Success)
                return null;

            return new AttendanceSyncResultDto
            {
                Matched = matched.Success ? int.Parse(matched.Groups[1].Value) : 0,
                Inserted = inserted.Success ? int.Parse(inserted.Groups[1].Value) : 0,
                Message = message.Success ? message.Groups[1].Value : null
            };
        }

        public static bool IsLegacyEmptyOk(IRestResponse response)
        {
            return response != null
                   && response.StatusCode == HttpStatusCode.OK
                   && string.IsNullOrWhiteSpace(response.Content);
        }

        public static bool TryGetStudentPostFailureMessage(
            IRestResponse response,
            AttendanceSyncResultDto syncResult,
            out string message)
        {
            return TryGetPostFailureMessage("Student", response, syncResult,
                "DeviceID not found on server (VW_Attendance_Stus / Education Year).", out message);
        }

        public static bool TryGetEmployeePostFailureMessage(
            IRestResponse response,
            AttendanceSyncResultDto syncResult,
            out string message)
        {
            return TryGetPostFailureMessage("Employee", response, syncResult,
                "DeviceID not found on server (VW_Emp_Info).", out message);
        }

        private static bool TryGetPostFailureMessage(
            string entityLabel,
            IRestResponse response,
            AttendanceSyncResultDto syncResult,
            string noMatchMessage,
            out string message)
        {
            message = null;

            if (response.StatusCode != HttpStatusCode.OK)
            {
                message = $"{entityLabel} sync failed ({(int)response.StatusCode})";
                if (!string.IsNullOrWhiteSpace(response.Content))
                    message += ": " + Truncate(response.Content, 180);
                return true;
            }

            if (syncResult == null)
            {
                if (!string.IsNullOrWhiteSpace(response.Content))
                    message = $"{entityLabel} sync failed (200): " + Truncate(response.Content, 180);
                else
                    message = $"{entityLabel} sync failed (200): empty server response.";
                return true;
            }

            if (syncResult.Matched > 0 || syncResult.Inserted > 0)
                return false;

            if (!string.IsNullOrWhiteSpace(syncResult.Message))
            {
                message = syncResult.Message;
                return true;
            }

            message = $"{entityLabel} sync failed (200): {noMatchMessage}";
            return true;
        }

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
                return value;

            return value.Substring(0, maxLength) + "...";
        }
    }
}
