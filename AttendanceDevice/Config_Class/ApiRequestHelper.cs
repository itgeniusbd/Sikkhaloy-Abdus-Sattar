using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;

namespace AttendanceDevice.Config_Class
{
    internal static class ApiRequestHelper
    {
        private static readonly JsonSerializerSettings CamelCaseJsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        public static void AddAuthorizedJsonHeaders(RestRequest request, string token)
        {
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddHeader("Accept", "application/json");
        }

        public static void AddCamelCaseJsonBody(RestRequest request, object body)
        {
            request.AddParameter(
                "application/json",
                JsonConvert.SerializeObject(body, CamelCaseJsonSettings),
                ParameterType.RequestBody);
        }
    }
}
