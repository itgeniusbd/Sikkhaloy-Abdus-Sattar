using Microsoft.VisualStudio.DebuggerVisualizers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

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
            foreach (var property in properties)
            {
                var valueAsString = Request.Form[property.Name];
                var value = Parse(valueAsString, property.PropertyType);

                if (value == null)
                    continue;

                property.SetValue(obj, value, null);
            }
            return obj;
        }
        public object Parse(string valueToConvert, Type dataType)
        {
            TypeConverter obj = TypeDescriptor.GetConverter(dataType);
            object value = obj.ConvertFromString(null, CultureInfo.InvariantCulture, valueToConvert);
            return value;
        }
    }
}
