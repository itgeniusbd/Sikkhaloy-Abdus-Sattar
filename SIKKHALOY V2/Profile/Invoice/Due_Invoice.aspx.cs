using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Profile.Invoice
{
    public partial class Due_Invoice : System.Web.UI.Page
    {
        private bool hasPartialPayment = false;
        private bool hasDiscount = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Modal এ due amount এবং subscription status দেখানোর জন্য
                if (Session["SchoolID"] != null)
                {
                    int schoolId = 0;
                    if (int.TryParse(Session["SchoolID"].ToString(), out schoolId) && schoolId > 0)
                    {
                        decimal due = GetTotalDueAmount(schoolId);
                                decimal gwCharge = Math.Round(due / 1000m * 10m, 2);
                                hfDueAmount.Value = due.ToString("F0");
                                hfGatewayCharge.Value = gwCharge.ToString("F0");
                                hfTotalPayable.Value = (due + gwCharge).ToString("F0");

                        var status = GetSubscriptionStatus(schoolId);
                        hfIsBlocked.Value = status.IsBlocked ? "1" : "0";
                        hfDaysLeft.Value = status.DaysUntilExpiry.ToString();
                    }
                }

                if (Request.QueryString["pay"] == "1")
                {
                    btnShurjoPay_Click(this, EventArgs.Empty);
                }
            }
        }

        // ─── ShurjoPay Payment ───
        protected void btnShurjoPay_Click(object sender, EventArgs e)
        {
            try
            {
                int schoolId = 0;
                if (Session["SchoolID"] != null)
                    int.TryParse(Session["SchoolID"].ToString(), out schoolId);

                if (schoolId == 0)
                {
                    hfPaymentMsg.Value = "Session expired. Please login again.";
                    return;
                }

                // Due amount ক্যালকুলেট
                decimal dueAmount = GetTotalDueAmount(schoolId);
                if (dueAmount <= 0)
                {
                    hfPaymentMsg.Value = "কোনো বকেয়া নেই।";
                    return;
                }

                // গেটওয়ে চার্জ: প্রতি হাজারে ১০ টাকা (১%)
                decimal gatewayCharge = Math.Round(dueAmount / 1000m * 10m, 2);
                decimal totalPayable  = dueAmount + gatewayCharge;

                // School info নেওয়া
                SchoolContactInfo info = GetSchoolInfo(schoolId);

                string baseUrl    = Request.Url.GetLeftPart(UriPartial.Authority);
                string returnUrl  = baseUrl + "/Profile/Invoice/ShurjoPayCallback.aspx";
                string cancelUrl  = baseUrl + "/Profile/Invoice/Due_Invoice.aspx";

                string customerName = !string.IsNullOrWhiteSpace(info.SchoolName) ? info.SchoolName : "School";
                if (customerName.Length > 50) customerName = customerName.Substring(0, 50);

                // একাধিক ফোন নাম্বার থাকলে শুধু প্রথমটি নেওয়া হবে
                string customerPhone = "01700000000";
                if (!string.IsNullOrWhiteSpace(info.Phone))
                {
                    string rawPhone = info.Phone.Split(new char[] { ',', '/', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
                    customerPhone = rawPhone.Length > 0 ? rawPhone : "01700000000";
                }

                var request = new ShurjoPayOrderRequest
                {
                    SchoolID         = schoolId,
                    Amount           = totalPayable,
                    CustomerName     = customerName,
                    CustomerPhone    = customerPhone,
                    CustomerEmail    = !string.IsNullOrWhiteSpace(info.Email)      ? info.Email      : "info@school.com",
                    CustomerAddress  = !string.IsNullOrWhiteSpace(info.Address)    ? info.Address    : "Dhaka",
                    CustomerCity     = "Dhaka",
                    CustomerState    = "Dhaka",
                    CustomerPostcode = "1200",
                    CustomerCountry  = "Bangladesh",
                    ReturnUrl        = returnUrl,
                    CancelUrl        = cancelUrl,
                    InvoiceNote      = "Sikkhaloy Invoice - SchoolID:" + schoolId,
                    // Callback-এ gateway charge calculate করার জন্য invoice due amount store
                    Value3           = dueAmount.ToString("F2")  // original due (gateway charge ছাড়া)
                };

                var service  = new ShurjoPayService();
                var response = service.CreateOrder(request);

                if (response != null)
                {
                    string redirectUrl = response.checkout_url ?? response.payment_url;
                    if (!string.IsNullOrEmpty(redirectUrl))
                    {
                        Response.Redirect(redirectUrl, false);
                        Context.ApplicationInstance.CompleteRequest();
                        return;
                    }

                    // API responded but no checkout URL — show ShurjoPay's actual error message
                    string apiMsg = !string.IsNullOrWhiteSpace(response.message)
                        ? response.message
                        : "checkout_url পাওয়া যায়নি। (sp_code: " + (response.sp_code ?? "?") + ")";

                    string rawResp = service.LastRawCreateOrderResponse ?? "";
                    System.Diagnostics.Debug.WriteLine("ShurjoPay no redirect URL. API message: " + apiMsg + " | Raw: " + rawResp);
                    hfPaymentMsg.Value = "পেমেন্ট গেটওয়ে এরর: " + apiMsg
                        + (rawResp.Length > 0 ? " | Raw: " + rawResp : "");
                    return;
                }

                hfPaymentMsg.Value = "পেমেন্ট গেটওয়েতে সংযোগ করতে সমস্যা হয়েছে। (response null)";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ShurjoPay button click error: " + ex.Message);
                hfPaymentMsg.Value = "পেমেন্ট গেটওয়ে ত্রুটি: " + ex.Message;
            }
        }

        private class SubscriptionStatus
        {
            public bool IsBlocked { get; set; }
            public int DaysUntilExpiry { get; set; }
        }

        private SubscriptionStatus GetSubscriptionStatus(int schoolId)
        {
            var result = new SubscriptionStatus { IsBlocked = false, DaysUntilExpiry = int.MaxValue };
            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    // Unpaid service invoice past EndDate blocks access unless a grace period is active.
                    using (SqlCommand expCmd = new SqlCommand(
                        @"SELECT COUNT(*) FROM AAP_Invoice i
                          INNER JOIN AAP_Invoice_Category c ON i.InvoiceCategoryID = c.InvoiceCategoryID
                          WHERE i.SchoolID = @SID AND i.IsPaid = 0
                            AND c.InvoiceCategory <> N'SMS'
                            AND i.IssuDate IS NOT NULL
                            AND CASE
                                  WHEN DAY(i.IssuDate) <= 15 THEN DATEFROMPARTS(YEAR(i.IssuDate), MONTH(i.IssuDate), 15)
                                  ELSE DATEADD(MONTH, 1, DATEFROMPARTS(YEAR(i.IssuDate), MONTH(i.IssuDate), 15))
                                END < CAST(GETDATE() AS DATE)", conn))
                    {
                        expCmd.Parameters.AddWithValue("@SID", schoolId);
                        int expiredCount = (int)expCmd.ExecuteScalar();
                        if (expiredCount > 0)
                        {
                            using (SqlCommand graceCmd = new SqlCommand(
                                "SELECT AccessGraceUntil FROM SchoolInfo WHERE SchoolID = @SID", conn))
                            {
                                graceCmd.Parameters.AddWithValue("@SID", schoolId);
                                object graceVal = graceCmd.ExecuteScalar();
                                if (graceVal != null && graceVal != DBNull.Value
                                    && Convert.ToDateTime(graceVal).Date >= DateTime.Today)
                                {
                                    result.IsBlocked = false;
                                    result.DaysUntilExpiry = (int)(Convert.ToDateTime(graceVal).Date - DateTime.Today).TotalDays;
                                    return result;
                                }
                            }
                            result.IsBlocked = true;
                            result.DaysUntilExpiry = 0;
                            return result;
                        }
                    }

                    using (SqlCommand futureCmd = new SqlCommand(
                        @"SELECT MIN(CASE
                                  WHEN DAY(i.IssuDate) <= 15 THEN DATEFROMPARTS(YEAR(i.IssuDate), MONTH(i.IssuDate), 15)
                                  ELSE DATEADD(MONTH, 1, DATEFROMPARTS(YEAR(i.IssuDate), MONTH(i.IssuDate), 15))
                                END) FROM AAP_Invoice i
                          INNER JOIN AAP_Invoice_Category c ON i.InvoiceCategoryID = c.InvoiceCategoryID
                          WHERE i.SchoolID = @SID AND i.IsPaid = 0
                            AND c.InvoiceCategory <> N'SMS'
                            AND i.IssuDate IS NOT NULL
                            AND CASE
                                  WHEN DAY(i.IssuDate) <= 15 THEN DATEFROMPARTS(YEAR(i.IssuDate), MONTH(i.IssuDate), 15)
                                  ELSE DATEADD(MONTH, 1, DATEFROMPARTS(YEAR(i.IssuDate), MONTH(i.IssuDate), 15))
                                END >= CAST(GETDATE() AS DATE)", conn))
                    {
                        futureCmd.Parameters.AddWithValue("@SID", schoolId);
                        object futureEnd = futureCmd.ExecuteScalar();
                        if (futureEnd != null && futureEnd != DBNull.Value)
                        {
                            DateTime nearestEnd = Convert.ToDateTime(futureEnd);
                            result.IsBlocked = false;
                            result.DaysUntilExpiry = (int)(nearestEnd.Date - DateTime.Today).TotalDays;
                            return result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetSubscriptionStatus error: " + ex.Message);
            }
            return result;
        }

        private decimal GetTotalDueAmount(int schoolId)
        {
            string connStr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"SELECT ISNULL(SUM(TotalAmount - PaidAmount - Discount), 0) 
                               FROM AAP_Invoice 
                               WHERE SchoolID = @SchoolID AND IsPaid = 0";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@SchoolID", schoolId);
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return result == DBNull.Value ? 0 : Convert.ToDecimal(result);
                }
            }
        }

        private SchoolContactInfo GetSchoolInfo(int schoolId)
        {
            var info = new SchoolContactInfo();
            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string sql = "SELECT SchoolName, Phone, Email, Address FROM SchoolInfo WHERE SchoolID = @SchoolID";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetSchoolInfo error: " + ex.Message);
            }
            return info;
        }

        private class SchoolContactInfo
        {
            public string SchoolName { get; set; }
            public string Phone      { get; set; }
            public string Email      { get; set; }
            public string Address    { get; set; }
        }

        protected void PrintFormView_DataBound(object sender, EventArgs e)
        {
            if (PrintFormView.Row != null)
            {
                Repeater detailsRepeater = (Repeater)PrintFormView.Row.FindControl("DetailsRepeater");
                if (detailsRepeater != null)
                {
                    detailsRepeater.ItemDataBound += DetailsRepeater_ItemDataBound;
                }
            }
        }

        protected void DetailsRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var paid = DataBinder.Eval(e.Item.DataItem, "PaidAmount");
                var discount = DataBinder.Eval(e.Item.DataItem, "Discount");

                if (paid != null && Convert.ToDecimal(paid) > 0)
                {
                    hasPartialPayment = true;
                }

                if (discount != null && Convert.ToDecimal(discount) > 0)
                {
                    hasDiscount = true;
                }
            }
            else if (e.Item.ItemType == ListItemType.Header)
            {
                hasPartialPayment = false;
                hasDiscount = false;
            }
        }

        protected void DetailsRepeater_PreRender(object sender, EventArgs e)
        {
            Repeater repeater = (Repeater)sender;
            if (repeater.Items.Count > 0)
            {
                System.Text.StringBuilder script = new System.Text.StringBuilder();
                script.Append("<script type='text/javascript'>");
                
                if (!hasPartialPayment)
                {
                    script.Append("$(document).ready(function() {");
                    script.Append("  $('.invoice-table th:nth-child(7), .invoice-table td:nth-child(7)').hide();");
                    script.Append("});");
                }
                
                if (!hasDiscount)
                {
                    script.Append("$(document).ready(function() {");
                    script.Append("  $('.invoice-table th:nth-child(6), .invoice-table td:nth-child(6)').hide();");
                    script.Append("});");
                }
                
                script.Append("</script>");
                
                Page.ClientScript.RegisterStartupScript(this.GetType(), "HideColumns", script.ToString());
            }
        }
    }
}