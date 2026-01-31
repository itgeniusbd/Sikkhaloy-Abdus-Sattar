using Microsoft.VisualStudio.DebuggerVisualizers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace EDUCATION.COM.Student.OnlinePayment
{
    /// <summary>
    /// Summary description for PaymentFactory
    /// </summary>
    public class PaymentFactory<T> where T : class, new()
    {
        public T GetPaymentInfoFromQueryString(HttpRequest Request)
        {
            var obj = new T();
            var properties = typeof(T).GetProperties();
            Dictionary<string, string> bodyValues = null;
            var paramValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (string key in Request.Params.Keys)
            {
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                if (!paramValues.ContainsKey(key))
                {
                    paramValues[key] = Request.Params[key];
                }
            }

            if (!string.IsNullOrEmpty(Request.ContentType) && Request.ContentType.IndexOf("application/json", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                try
                {
                    Request.InputStream.Position = 0;
                    using (var reader = new StreamReader(Request.InputStream))
                    {
                        var body = reader.ReadToEnd();
                        if (!string.IsNullOrWhiteSpace(body))
                        {
                            var bodyDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(body);
                            if (bodyDictionary != null)
                            {
                                bodyValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                                foreach (var item in bodyDictionary)
                                {
                                    bodyValues[item.Key] = item.Value?.ToString();
                                    if (!paramValues.ContainsKey(item.Key))
                                    {
                                        paramValues[item.Key] = item.Value?.ToString();
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    bodyValues = null;
                }
            }

            foreach (var property in properties)
            {
                var valueAsString = Request.Form[property.Name];
                if (string.IsNullOrEmpty(valueAsString))
                {
                    valueAsString = Request.QueryString[property.Name];
                }
                if (string.IsNullOrEmpty(valueAsString))
                {
                    paramValues.TryGetValue(property.Name, out valueAsString);
                }
                if (string.IsNullOrEmpty(valueAsString))
                {
                    var normalizedKey = NormalizeKey(property.Name);
                    foreach (var entry in paramValues)
                    {
                        if (NormalizeKey(entry.Key) == normalizedKey)
                        {
                            valueAsString = entry.Value;
                            break;
                        }
                    }
                }
                if (string.IsNullOrEmpty(valueAsString) && bodyValues != null)
                {
                    bodyValues.TryGetValue(property.Name, out valueAsString);
                }

                var value = Parse(valueAsString, property.PropertyType);

                if (value == null)
                    continue;

                property.SetValue(obj, value, null);
            }
            return obj;
        }

        private static string NormalizeKey(string key)
        {
            return key == null ? string.Empty : key.Replace("_", string.Empty).Replace("-", string.Empty).ToLowerInvariant();
        }

        public object Parse(string valueToConvert, Type dataType)
        {
            if (string.IsNullOrEmpty(valueToConvert))
            {
                return null;
            }

            TypeConverter obj = TypeDescriptor.GetConverter(dataType);
            object value = obj.ConvertFromString(null, CultureInfo.InvariantCulture, valueToConvert);
            return value;
        }
    }
}
