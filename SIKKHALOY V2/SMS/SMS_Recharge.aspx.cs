using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;
using EDUCATION.COM.Profile.Invoice;

namespace EDUCATION.COM.SMS
{
    public partial class SMS_Recharge : Page
    {
        private const double PerSMSRate = 0.36;

        private readonly string _connectionString =
            ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
        }
                
        protected void RechargeButton_Click(object sender, EventArgs e)
        {
            int smsQty;
            if (!int.TryParse(SMSQtyTextBox.Text.Trim(), out smsQty) || smsQty <= 0)
            {
                MessageLabel.ForeColor = System.Drawing.Color.Red;
                MessageLabel.Text = "সঠিক SMS পরিমাণ দিন।";
                return;
            }

            // ── Server-side duplicate submit guard ──────────────────────────────
            // একই session-এ বারবার button click করলে duplicate order তৈরি হবে না
            string lockKey = "SMSRechargeInProgress_" + Session.SessionID;
            if (System.Web.HttpContext.Current.Application[lockKey] != null)
            {
                // ৫ মিনিট পুরনো lock হলে সরিয়ে দাও (stuck হলে)
                DateTime lockTime = (DateTime)System.Web.HttpContext.Current.Application[lockKey];
                if ((DateTime.Now - lockTime).TotalMinutes < 5)
                {
                    MessageLabel.ForeColor = System.Drawing.Color.Orange;
                    MessageLabel.Text = "পেমেন্ট প্রক্রিয়াকরণ চলছে। অনুগ্রহ করে অপেক্ষা করুন।";
                    return;
                }
            }
            System.Web.HttpContext.Current.Application[lockKey] = DateTime.Now;
            // ────────────────────────────────────────────────────────────────────

            int schoolID       = Convert.ToInt32(Session["SchoolID"]);
            int registrationID = Convert.ToInt32(Session["RegistrationID"]);
            double totalAmount = smsQty * PerSMSRate;

            // পেমেন্ট সফল হওয়ার পর callback-এ DB save করার জন্য session-এ রাখি
            Session["PendingSMSRecharge_SchoolID"]       = schoolID;
            Session["PendingSMSRecharge_RegistrationID"] = registrationID;
            Session["PendingSMSRecharge_SMSQty"]         = smsQty;
            Session["PendingSMSRecharge_Amount"]         = totalAmount;

            try
            {
                SchoolContactInfo info = GetSchoolInfo(schoolID);

                string baseUrl   = Request.Url.GetLeftPart(UriPartial.Authority);
                string returnUrl = baseUrl + "/Profile/Invoice/ShurjoPayCallback.aspx";
                string cancelUrl = baseUrl + "/SMS/SMS_Recharge.aspx?cancelled=1";

                // value2 = "SMS_RECHARGE" — callback-এ এটা দেখে SMS save করবে
                // value3 = smsQty, value4 = registrationID
                var spRequest = new ShurjoPayOrderRequest
                {
                    SchoolID         = schoolID,
                    Amount           = (decimal)totalAmount,
                    CustomerName     = info.SchoolName ?? Session["School_Name"]?.ToString() ?? "School",
                    CustomerPhone    = info.Phone ?? "01700000000",
                    CustomerEmail    = info.Email ?? "info@school.com",
                    CustomerAddress  = info.Address ?? "Dhaka",
                    CustomerCity     = "Dhaka",
                    CustomerState    = "Dhaka",
                    CustomerPostcode = "1200",
                    CustomerCountry  = "Bangladesh",
                    ReturnUrl        = returnUrl,
                    CancelUrl        = cancelUrl,
                    InvoiceNote      = "SMS_RECHARGE|" + smsQty + "|" + registrationID + "|" + schoolID
                };

                var service  = new ShurjoPayService();
                var response = service.CreateOrder(spRequest);

                if (response != null)
                {
                    string redirectUrl = response.checkout_url ?? response.payment_url;
                    if (!string.IsNullOrEmpty(redirectUrl))
                    {
                        // mer_order_id = আমাদের নিজস্ব order_id, verify-এ এটাই লাগবে
                        if (!string.IsNullOrEmpty(response.order_id))
                        {
                            spRequest.ReturnUrl = returnUrl + "?mer_order_id=" + Uri.EscapeDataString(response.order_id);
                            // ShurjoPay ইতিমধ্যে order create করেছে, তাই শুধু session update করি
                            Session["PendingSMSRecharge_MerOrderID"] = response.order_id;
                        }
                        // redirect-এর আগে lock সরাই — payment page-এ গেলে আর lock দরকার নেই
                        System.Web.HttpContext.Current.Application.Remove(lockKey);
                        Response.Redirect(redirectUrl, false);
                        Context.ApplicationInstance.CompleteRequest();
                        return;
                    }
                }

                // Gateway সংযোগ না হলে session ও lock clear করে error দেখাও
                System.Web.HttpContext.Current.Application.Remove(lockKey);
                ClearPendingSession();
                MessageLabel.ForeColor = System.Drawing.Color.Red;
                MessageLabel.Text      = "ShurjoPay গেটওয়ে সংযোগ হয়নি। পুনরায় চেষ্টা করুন।";
            }
            catch (Exception ex)
            {
                System.Web.HttpContext.Current.Application.Remove(lockKey);
                ClearPendingSession();
                MessageLabel.ForeColor = System.Drawing.Color.Red;
                MessageLabel.Text      = "পেমেন্ট গেটওয়ে ত্রুটি: " + ex.Message;
            }
        }

        private void ClearPendingSession()
        {
            Session.Remove("PendingSMSRecharge_SchoolID");
            Session.Remove("PendingSMSRecharge_RegistrationID");
            Session.Remove("PendingSMSRecharge_SMSQty");
            Session.Remove("PendingSMSRecharge_Amount");
        }

        private SchoolContactInfo GetSchoolInfo(int schoolId)
        {
            var info = new SchoolContactInfo();
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT SchoolName, Phone, Email, Address FROM SchoolInfo WHERE SchoolID = @SchoolID", conn))
                    {
                        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
                        conn.Open();
                        using (System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                info.SchoolName = reader["SchoolName"]?.ToString();
                                info.Phone      = reader["Phone"]?.ToString();
                                info.Email      = reader["Email"]?.ToString();
                                info.Address    = reader["Address"]?.ToString();
                            }
                        }
                    }
                }
            }
            catch { }
            return info;
        }

        private class SchoolContactInfo
        {
            public string SchoolName { get; set; }
            public string Phone      { get; set; }
            public string Email      { get; set; }
            public string Address    { get; set; }
        }
    }
}
