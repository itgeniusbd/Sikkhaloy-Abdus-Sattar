using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace EDUCATION.COM.Profile.Invoice
{
    /// <summary>
    /// ShurjoPay Payment Gateway Service
    /// Live API: https://engine.shurjopayment.com
    /// </summary>
    public class ShurjoPayService
    {
        private readonly string _baseUrl;
        private readonly string _username;
        private readonly string _password;
        private readonly string _orderPrefix;

        public ShurjoPayService()
        {
            // Sandbox mode: localhost অথবা web.config-এ ShurjoPaySandbox=true হলে sandbox ব্যবহার হবে
            bool isSandbox = IsLocalhostRequest()
                || string.Equals(
                    ConfigurationManager.AppSettings["ShurjoPaySandbox"],
                    "true",
                    StringComparison.OrdinalIgnoreCase);

            if (isSandbox)
            {
                _baseUrl     = "https://sandbox.shurjopayment.com";
                _username    = "sp_sandbox";
                _password    = "pyyk97hu&6u6";
                _orderPrefix = ConfigurationManager.AppSettings["ShurjoPayOrderPrefix"] ?? "SIK";
            }
            else
            {
                _baseUrl     = ConfigurationManager.AppSettings["ShurjoPayBaseUrl"]     ?? "https://engine.shurjopayment.com";
                _username    = ConfigurationManager.AppSettings["ShurjoPayUsername"]    ?? "sikkhaloy";
                _password    = ConfigurationManager.AppSettings["ShurjoPayPassword"]    ?? "sikkp22tmxq3499z";
                _orderPrefix = ConfigurationManager.AppSettings["ShurjoPayOrderPrefix"] ?? "SIK";
            }
        }

        private static bool IsLocalhostRequest()
        {
            try
            {
                HttpContext ctx = HttpContext.Current;
                if (ctx == null) return false;
                return ctx.Request.IsLocal;
            }
            catch { return false; }
        }

        // ─── Token ───────────────────────────────────────────────────────────────
        public ShurjoPayToken GetToken()
        {
            string url  = _baseUrl + "/api/get_token";
            string body = "username=" + Uri.EscapeDataString(_username)
                        + "&password=" + Uri.EscapeDataString(_password);

            string response   = PostForm(url, body);
            var    serializer = new JavaScriptSerializer();
            var    token      = serializer.Deserialize<ShurjoPayToken>(response);
            return token;
        }

        // ─── Create Order ─────────────────────────────────────────────────────────
        public string LastRawCreateOrderResponse { get; private set; }

        public ShurjoPayOrderResponse CreateOrder(ShurjoPayOrderRequest request)
        {
            ShurjoPayToken token = GetToken();
            if (token == null || string.IsNullOrEmpty(token.token))
                throw new Exception("ShurjoPay token নেওয়া সম্ভব হয়নি। Base URL: " + _baseUrl + " | User: " + _username);

            string url = !string.IsNullOrEmpty(token.execute_url)
                ? token.execute_url
                : _baseUrl + "/api/secret-pay";

            string orderId = !string.IsNullOrEmpty(request.InvoiceNote) && request.InvoiceNote.StartsWith("SMS_RECHARGE|")
                ? "SMSR_" + request.SchoolID + "_" + DateTime.Now.ToString("yyyyMMddHHmmss")
                : _orderPrefix + "_" + request.SchoolID + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");

            string body = "token="              + Uri.EscapeDataString(token.token)
                        + "&store_id="          + Uri.EscapeDataString(token.store_id ?? "")
                        + "&prefix="            + Uri.EscapeDataString(_orderPrefix)
                        + "&currency=BDT"
                        + "&return_url="        + Uri.EscapeDataString(request.ReturnUrl)
                        + "&cancel_url="        + Uri.EscapeDataString(request.CancelUrl)
                        + "&amount="            + request.Amount.ToString("F2")
                        + "&order_id="          + Uri.EscapeDataString(orderId)
                        + "&discount_amount=0"
                        + "&disc_percent=0"
                        + "&client_ip="         + Uri.EscapeDataString(GetClientIp())
                        + "&customer_name="     + Uri.EscapeDataString(request.CustomerName ?? "")
                        + "&customer_phone="    + Uri.EscapeDataString(request.CustomerPhone ?? "")
                        + "&customer_email="    + Uri.EscapeDataString(request.CustomerEmail ?? "")
                        + "&customer_address="  + Uri.EscapeDataString(request.CustomerAddress ?? "")
                        + "&customer_city="     + Uri.EscapeDataString(request.CustomerCity ?? "Dhaka")
                        + "&customer_state="    + Uri.EscapeDataString(request.CustomerState ?? "Dhaka")
                        + "&customer_postcode=" + Uri.EscapeDataString(request.CustomerPostcode ?? "1200")
                        + "&customer_country="  + Uri.EscapeDataString(request.CustomerCountry ?? "Bangladesh")
                        + "&value1="            + Uri.EscapeDataString(request.SchoolID.ToString())
                        + "&value2="            + Uri.EscapeDataString(request.InvoiceNote ?? "")
                        + "&value3="            + Uri.EscapeDataString(request.Value3 ?? "")
                        + "&value4=";

            string response   = PostFormWithBearer(url, body, token.token);
            LastRawCreateOrderResponse = response;
            System.Diagnostics.Debug.WriteLine("ShurjoPay CreateOrder raw response: " + response);
            var    serializer = new JavaScriptSerializer();
            var    result     = serializer.Deserialize<ShurjoPayOrderResponse>(response);

            if (result != null)
                result.order_id = orderId;

            return result;
        }

        // ─── Verify Payment ───────────────────────────────────────────────────────
        // spOrderId = ShurjoPay-এর নিজস্ব order_id যেটা callback URL-এ আসে
        public string LastRawVerifyResponse { get; private set; }

        public ShurjoPayVerifyResponse VerifyPayment(string spOrderId)
        {
            try
            {
                ShurjoPayToken token = GetToken();
                if (token == null || string.IsNullOrEmpty(token.token))
                    return null;

                string url  = _baseUrl + "/api/verification";
                string body = "order_id=" + Uri.EscapeDataString(spOrderId);

                string rawResponse = PostFormWithBearer(url, body, token.token);
                LastRawVerifyResponse = rawResponse;
                System.Diagnostics.Debug.WriteLine("ShurjoPay Verify Raw: " + rawResponse);

                var serializer = new JavaScriptSerializer();

                // ShurjoPay কখনো array, কখনো single object return করে
                if (rawResponse.TrimStart().StartsWith("["))
                {
                    var results = serializer.Deserialize<ShurjoPayVerifyResponse[]>(rawResponse);
                    return (results != null && results.Length > 0) ? results[0] : null;
                }
                else
                {
                    return serializer.Deserialize<ShurjoPayVerifyResponse>(rawResponse);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ShurjoPay VerifyPayment error: " + ex.Message);
                LastRawVerifyResponse = "Exception: " + ex.Message;
                return null;
            }
        }

        // ─── HTTP Helpers ─────────────────────────────────────────────────────────
        private static void EnsureTls12()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;
            ServicePointManager.ServerCertificateValidationCallback = (s, c, ch, e) => true;
        }

        private string PostForm(string url, string body)
        {
            EnsureTls12();
            WebRequest request    = WebRequest.Create(url);
            request.Method        = "POST";
            byte[] data           = Encoding.UTF8.GetBytes(body);
            request.ContentType   = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;
            using (Stream s = request.GetRequestStream()) s.Write(data, 0, data.Length);
            using (WebResponse resp   = request.GetResponse())
            using (StreamReader reader = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                return reader.ReadToEnd();
        }

        private string PostFormWithBearer(string url, string body, string bearerToken)
        {
            EnsureTls12();
            WebRequest request    = WebRequest.Create(url);
            request.Method        = "POST";
            request.Headers.Add("Authorization", "Bearer " + bearerToken);
            byte[] data           = Encoding.UTF8.GetBytes(body);
            request.ContentType   = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;
            using (Stream s = request.GetRequestStream()) s.Write(data, 0, data.Length);
            using (WebResponse resp   = request.GetResponse())
            using (StreamReader reader = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                return reader.ReadToEnd();
        }

        private string GetClientIp()
        {
            try
            {
                HttpContext ctx = HttpContext.Current;
                string ip = ctx.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (string.IsNullOrEmpty(ip))
                    ip = ctx.Request.ServerVariables["REMOTE_ADDR"];
                return ip ?? "127.0.0.1";
            }
            catch { return "127.0.0.1"; }
        }
    }

    // ─── Model classes ────────────────────────────────────────────────────────────

    public class ShurjoPayToken
    {
        public string token       { get; set; }
        public string store_id    { get; set; }
        public string execute_url { get; set; }
        public string token_type  { get; set; }
        public int    expires_in  { get; set; }
    }

    public class ShurjoPayOrderRequest
    {
        public int    SchoolID        { get; set; }
        public decimal Amount         { get; set; }
        public string CustomerName    { get; set; }
        public string CustomerPhone   { get; set; }
        public string CustomerEmail   { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerCity    { get; set; }
        public string CustomerState   { get; set; }
        public string CustomerPostcode { get; set; }
        public string CustomerCountry { get; set; }
        public string ReturnUrl       { get; set; }
        public string CancelUrl       { get; set; }
        public string InvoiceNote     { get; set; }
        /// <summary>Invoice due amount (excluding gateway charge) stored for callback reconciliation</summary>
        public string Value3          { get; set; }
    }

    public class ShurjoPayOrderResponse
    {
        public string checkout_url { get; set; }
        public string payment_url  { get; set; }
        public string order_id     { get; set; }
        public string amount       { get; set; }
        public string currency     { get; set; }
        public string message      { get; set; }
        public string sp_code      { get; set; }
        public string sp_massage   { get; set; }
    }

    public class ShurjoPayVerifyResponse
    {
        public string id                 { get; set; }
        public string order_id           { get; set; }
        public string currency           { get; set; }
        public string amount             { get; set; }
        public string payable_amount     { get; set; }
        public string discount_amount    { get; set; }
        public string disc_percent       { get; set; }
        public string usd_amt            { get; set; }
        public string usd_rate           { get; set; }
        public string method             { get; set; }
        public string sp_code            { get; set; }
        public string sp_message         { get; set; }
        public string name               { get; set; }
        public string email              { get; set; }
        public string address            { get; set; }
        public string city               { get; set; }
        public string value1             { get; set; }
        public string value2             { get; set; }
        public string value3             { get; set; }
        public string value4             { get; set; }
        public string transaction_status { get; set; }
        public string bank_trx_id        { get; set; }
        public string invoice_no         { get; set; }
        public string recv_amt           { get; set; }
        public string recv_dt            { get; set; }
        public string bank_status        { get; set; }
    }
}
