using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDUCATION.COM.Student.OnlinePayment
{
    public class PaymentRequest
    {
        public string store_id { get; set; }
        public string signature_key { get; set; }
        public string tran_id { get; set; }
        public double amount { get; set; }
        public string cus_name { get; set; }
        public string cus_email { get; set; }
        public string cus_phone { get; set; }
        public string currency { get; set; }
        public string desc { get; set; }
        public string success_url { get; set; }
        public string fail_url { get; set; }
        public string cancel_url { get; set; }
        public string cus_add1 { get; set; }
        public string cus_add2 { get; set; }
        public string cus_city { get; set; }
        public string cus_state { get; set; }
        public string cus_postcode { get; set; }
        public string cus_country { get; set; }
        public string type { get; set; }
        public string opt_a { get; set; }
        public string opt_b { get; set; }
        public string opt_c { get; set; }
        public string opt_d { get; set; }

    }
}
