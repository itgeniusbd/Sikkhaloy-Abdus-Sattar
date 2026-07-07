using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Education;

namespace EDUCATION.COM.Authority.Reference
{
    public partial class Referral_Commission_Report : System.Web.UI.Page
    {
        private string ConnStr => ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadReferrersDropDown();
                DateTime today = DateTime.Today;
                FromDateTextBox.Text = new DateTime(today.Year, 1, 1).ToString("dd MMM yyyy");
                ToDateTextBox.Text = today.ToString("dd MMM yyyy");
                LoadReport(0, new DateTime(today.Year, 1, 1), today, "");
            }

            if (PaymentPanel.Visible)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "ShowPaymentModal", "$('#PaymentModal').modal('show');", true);
            }
        }

        private void LoadReferrersDropDown()
        {
            string sql = "SELECT ReferenceID, Reference_Name FROM AAP_Reference ORDER BY Reference_Name";
            using (SqlConnection con = new SqlConnection(ConnStr))
            using (SqlDataAdapter da = new SqlDataAdapter(sql, con))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                ReferrerDropDown.DataSource = dt;
                ReferrerDropDown.DataTextField = "Reference_Name";
                ReferrerDropDown.DataValueField = "ReferenceID";
                ReferrerDropDown.DataBind();
            }
        }

        protected void ReferrerDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        protected void SearchButton_Click(object sender, EventArgs e)
        {
            ErrorLabel.Text = "";
            DateTime fromDate, toDate;
            if (!DateTime.TryParseExact(FromDateTextBox.Text.Trim(), "dd MMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out fromDate))
            {
                ErrorLabel.Text = "Invalid from date."; return;
            }
            if (!DateTime.TryParseExact(ToDateTextBox.Text.Trim(), "dd MMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out toDate))
            {
                ErrorLabel.Text = "Invalid to date."; return;
            }

            int refID = 0;
            int.TryParse(ReferrerDropDown.SelectedValue, out refID);
            LoadReport(refID, fromDate, toDate, StatusDropDown.SelectedValue);
        }

        private void LoadReport(int refID, DateTime fromDate, DateTime toDate, string status)
        {
            string sql = @"
                SELECT 
                    r.ReferenceID,
                    r.Reference_Name,
                    r.Reference_Phone,
                    COUNT(DISTINCT rs.Reference_School_ID) AS TotalSchools,
                    ISNULL((SELECT SUM(rc.Commission_Amount) FROM AAP_Reference_Commission rc WHERE rc.ReferenceID = r.ReferenceID AND rc.Commission_Date BETWEEN @From AND @To), 0) AS TotalCommission,
                    ISNULL((SELECT SUM(Amount) FROM AAP_Reference_PaymentRecord pr WHERE pr.ReferenceID = r.ReferenceID AND pr.PaidDate BETWEEN @From AND @To), 0) AS PaidAmount,
                    ISNULL((SELECT SUM(rc.Commission_Amount) FROM AAP_Reference_Commission rc WHERE rc.ReferenceID = r.ReferenceID AND rc.Commission_Date BETWEEN @From AND @To), 0)
                        - ISNULL((SELECT SUM(Amount) FROM AAP_Reference_PaymentRecord pr WHERE pr.ReferenceID = r.ReferenceID AND pr.PaidDate BETWEEN @From AND @To), 0) AS DueAmount
                FROM AAP_Reference r
                LEFT JOIN AAP_Reference_School rs ON r.ReferenceID = rs.ReferenceID
                WHERE (@RefID = 0 OR r.ReferenceID = @RefID)
                GROUP BY r.ReferenceID, r.Reference_Name, r.Reference_Phone
                ORDER BY r.Reference_Name";

            using (SqlConnection con = new SqlConnection(ConnStr))
            using (SqlDataAdapter da = new SqlDataAdapter(sql, con))
            {
                da.SelectCommand.Parameters.AddWithValue("@From", fromDate);
                da.SelectCommand.Parameters.AddWithValue("@TO", toDate);
                da.SelectCommand.Parameters.AddWithValue("@RefID", refID);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (status == "due")
                    dt = FilterTable(dt, "DueAmount > 0");
                else if (status == "paid")
                    dt = FilterTable(dt, "PaidAmount > 0 AND DueAmount <= 0");

                RefSummaryGridView.DataSource = dt;
                RefSummaryGridView.DataBind();

                double totalComm = 0, totalPaid = 0, totalDue = 0;
                foreach (DataRow row in dt.Rows)
                {
                    totalComm += Convert.ToDouble(row["TotalCommission"]);
                    totalPaid += Convert.ToDouble(row["PaidAmount"]);
                    totalDue  += Convert.ToDouble(row["DueAmount"]);
                }
                TotalCommLabel.Text = string.Format("{0:N0}", totalComm);
                TotalPaidLabel.Text = string.Format("{0:N0}", totalPaid);
                TotalDueLabel.Text  = string.Format("{0:N0}", totalDue);
                TotalRefLabel.Text  = dt.Rows.Count.ToString();
                SummaryPanel.Visible = true;
            }
        }

        private DataTable FilterTable(DataTable dt, string filter)
        {
            DataView dv = new DataView(dt) { RowFilter = filter };
            return dv.ToTable();
        }

        protected void RefSummaryGridView_SelectedIndexChanged(object sender, EventArgs e)
        {
            int refID = (int)RefSummaryGridView.SelectedDataKey["ReferenceID"];
            string refName = RefSummaryGridView.SelectedRow.Cells[1].Text;
            LoadSchoolDetail(refID, refName);
        }

        private void LoadSchoolDetail(int refID, string refName)
        {
            DateTime fromDate = DateTime.Today.AddYears(-5);
            DateTime toDate = DateTime.Today;
            DateTime.TryParseExact(FromDateTextBox.Text.Trim(), "dd MMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out fromDate);
            DateTime.TryParseExact(ToDateTextBox.Text.Trim(), "dd MMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out toDate);

            string sql = @"
                SELECT 
                    s.SchoolName,
                    rs.Percentage,
                    rs.School_SignUp_Date,
                    rs.End_Reference_Date,
                    ISNULL((SELECT SUM(rc.ServiceCharge_Amount) FROM AAP_Reference_Commission rc WHERE rc.Reference_School_ID = rs.Reference_School_ID AND rc.Commission_Date BETWEEN @From AND @To), 0) AS TotalServiceCharge,
                    ISNULL((SELECT SUM(rc.Commission_Amount) FROM AAP_Reference_Commission rc WHERE rc.Reference_School_ID = rs.Reference_School_ID AND rc.Commission_Date BETWEEN @From AND @To), 0) AS CommissionAmount,
                    ISNULL((SELECT SUM(pr.Amount) FROM AAP_Reference_PaymentRecord pr WHERE pr.Reference_School_ID = rs.Reference_School_ID AND pr.PaidDate BETWEEN @From AND @To), 0) AS PaidAmount,
                    ISNULL((SELECT SUM(rc.Commission_Amount) FROM AAP_Reference_Commission rc WHERE rc.Reference_School_ID = rs.Reference_School_ID AND rc.Commission_Date BETWEEN @From AND @To), 0)
                        - ISNULL((SELECT SUM(pr.Amount) FROM AAP_Reference_PaymentRecord pr WHERE pr.Reference_School_ID = rs.Reference_School_ID AND pr.PaidDate BETWEEN @From AND @To), 0) AS DueAmount
                FROM AAP_Reference_School rs
                INNER JOIN SchoolInfo s ON rs.SchoolID = s.SchoolID
                WHERE rs.ReferenceID = @RefID
                ORDER BY s.SchoolName";

            using (SqlConnection con = new SqlConnection(ConnStr))
            using (SqlDataAdapter da = new SqlDataAdapter(sql, con))
            {
                da.SelectCommand.Parameters.AddWithValue("@RefID", refID);
                da.SelectCommand.Parameters.AddWithValue("@From", fromDate);
                da.SelectCommand.Parameters.AddWithValue("@To", toDate);
                DataTable dt = new DataTable();
                da.Fill(dt);
                DetailGridView.DataSource = dt;
                DetailGridView.DataBind();
            }
            DetailRefNameLabel.Text = refName;
            DetailPanel.Visible = true;

            PaymentPanel.Visible = false;
        }

        protected void GridView_RowCommand(object source, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Pay")
            {
                int refID = int.Parse(e.CommandArgument.ToString());
                LoadPayPanel(refID);
            }
        }

        private void LoadPayPanel(int refID)
        {
            string refName = "";
            using (SqlConnection con = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand("SELECT Reference_Name FROM AAP_Reference WHERE ReferenceID=@ID", con))
            {
                cmd.Parameters.AddWithValue("@ID", refID);
                con.Open();
                object result = cmd.ExecuteScalar();
                if (result != null) refName = result.ToString();
            }

            PayRefNameLabel.Text = refName;
            PayReferenceIDHidden.Value = refID.ToString();
            PayDateTextBox.Text = DateTime.Today.ToString("dd MMM yyyy");
            PaymentPanel.Visible = true;
            LoadPayHistory(refID);
            ViewState["PayRefID"] = refID;

            ClearPayOtpSession();
            PayOTPTextBox.Text = "";
            PayMsgLabel.Text = "";

            ScriptManager.RegisterStartupScript(this, GetType(), "ShowPaymentModal", "$('#PaymentModal').modal('show');", true);
        }

        private void LoadPayHistory(int refID)
        {
            string sql = @"SELECT pr.ReferencePaymentRecordID, pr.PaidDate, pr.Amount, pr.Paid_By, pr.Payment_Method, pr.Note, r.Reference_Phone
                           FROM AAP_Reference_PaymentRecord pr
                           INNER JOIN AAP_Reference r ON pr.ReferenceID = r.ReferenceID
                           WHERE pr.ReferenceID = @RefID
                           ORDER BY pr.PaidDate DESC";
            using (SqlConnection con = new SqlConnection(ConnStr))
            using (SqlDataAdapter da = new SqlDataAdapter(sql, con))
            {
                da.SelectCommand.Parameters.AddWithValue("@RefID", refID);
                DataTable dt = new DataTable();
                da.Fill(dt);
                PayHistoryGridView.DataSource = dt;
                PayHistoryGridView.DataBind();
            }
        }

        protected void SendPayOTPButton_Click(object sender, EventArgs e)
        {
            PayMsgLabel.Text = "";

            int refID = 0;
            int.TryParse(PayReferenceIDHidden.Value, out refID);
            if (refID == 0)
            {
                PayMsgLabel.CssClass = "text-danger font-weight-bold";
                PayMsgLabel.Text = "Referrer not selected.";
                return;
            }

            string phoneNumber = GetReferrerPhone(refID);
            if (string.IsNullOrEmpty(phoneNumber))
            {
                PayMsgLabel.CssClass = "text-danger font-weight-bold";
                PayMsgLabel.Text = "Referrer phone number not found.";
                return;
            }

            if (!phoneNumber.StartsWith("01") || phoneNumber.Length != 11)
            {
                PayMsgLabel.CssClass = "text-danger font-weight-bold";
                PayMsgLabel.Text = "Referrer phone number is invalid.";
                return;
            }

            DateTime? lastOTPTime = Session["RefPayOTPTime"] as DateTime?;
            if (lastOTPTime.HasValue && DateTime.Now.Subtract(lastOTPTime.Value).TotalSeconds < 60)
            {
                double remainingSeconds = 60 - DateTime.Now.Subtract(lastOTPTime.Value).TotalSeconds;
                PayMsgLabel.CssClass = "text-warning font-weight-bold";
                PayMsgLabel.Text = $"Please wait {Math.Ceiling(remainingSeconds)} seconds before requesting new OTP.";
                return;
            }

            string otp = new Random().Next(100000, 999999).ToString();
            Session["RefPayOTP"] = otp;
            Session["RefPayOTPTime"] = DateTime.Now;
            Session["RefPayOTPPhone"] = phoneNumber;

            try
            {
                string schoolId = GetSmsSchoolId();
                string schoolName = Session["SchoolName"] != null ? Session["SchoolName"].ToString() : "SIKKHALOY";

                SMS_Class SMS = new SMS_Class(schoolId);
                string smsText = $"Your OTP for referral commission payment is: {otp}. Valid for 5 minutes. - {schoolName}";
                Get_Validation validation = SMS.SMS_Validation(phoneNumber, smsText);

                if (validation.Validation)
                {
                    Guid smsSendId = SMS.SMS_Send(phoneNumber, smsText, "Referral Commission Payment OTP");
                    if (smsSendId != Guid.Empty)
                    {
                        PayMsgLabel.CssClass = "text-info font-weight-bold";
                        PayMsgLabel.Text = $"OTP sent successfully to {phoneNumber}.";
                        SendOTPButton.Text = "OTP Sent";
                        ScriptManager.RegisterStartupScript(this, GetType(), "PayOTPSent", "payOTPSent();", true);
                    }
                    else
                    {
                        PayMsgLabel.CssClass = "text-danger font-weight-bold";
                        PayMsgLabel.Text = "Failed to send OTP. Please try again.";
                    }
                }
                else
                {
                    PayMsgLabel.CssClass = "text-danger font-weight-bold";
                    PayMsgLabel.Text = validation.Message;
                }
            }
            catch (Exception ex)
            {
                PayMsgLabel.CssClass = "text-danger font-weight-bold";
                PayMsgLabel.Text = "Error sending OTP: " + ex.Message;
            }
        }

        private string GetSmsSchoolId()
        {
            // Normal school user
            if (Session["SchoolID"] != null)
            {
                string sid = Session["SchoolID"].ToString();
                int id;
                if (int.TryParse(sid, out id) && id > 0)
                    return sid;
            }

            // Authority/Sub-Authority fallback: use first school with SMS balance
            using (SqlConnection con = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 SchoolID FROM SMS WHERE SMS_Balance > 0 ORDER BY SchoolID", con))
            {
                con.Open();
                object result = cmd.ExecuteScalar();
                if (result != null) return result.ToString();
            }

            // Last fallback
            return "0";
        }

        private string GetReferrerPhone(int refID)
        {
            using (SqlConnection con = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand("SELECT Reference_Phone FROM AAP_Reference WHERE ReferenceID=@ID", con))
            {
                cmd.Parameters.AddWithValue("@ID", refID);
                con.Open();
                object result = cmd.ExecuteScalar();
                return result != null ? result.ToString() : "";
            }
        }

        private bool ValidatePayOtp()
        {
            PayMsgLabel.Text = "";
            string enteredOTP = PayOTPTextBox.Text.Trim();
            string sessionOTP = Session["RefPayOTP"] as string;

            if (string.IsNullOrEmpty(sessionOTP))
            {
                PayMsgLabel.CssClass = "text-danger font-weight-bold";
                PayMsgLabel.Text = "OTP not generated. Please click 'Send OTP'.";
                return false;
            }

            DateTime? otpTime = Session["RefPayOTPTime"] as DateTime?;
            if (otpTime.HasValue && DateTime.Now.Subtract(otpTime.Value).TotalMinutes > 5)
            {
                PayMsgLabel.CssClass = "text-danger font-weight-bold";
                PayMsgLabel.Text = "OTP expired. Please send a new OTP.";
                ClearPayOtpSession();
                return false;
            }

            if (enteredOTP != sessionOTP)
            {
                PayMsgLabel.CssClass = "text-danger font-weight-bold";
                PayMsgLabel.Text = "Invalid OTP. Please enter correct OTP.";
                return false;
            }

            return true;
        }

        private void ClearPayOtpSession()
        {
            Session.Remove("RefPayOTP");
            Session.Remove("RefPayOTPTime");
            Session.Remove("RefPayOTPPhone");
        }

        protected void SavePayButton_Click(object sender, EventArgs e)
        {
            PayMsgLabel.Text = "";
            int refID = 0;
            int.TryParse(PayReferenceIDHidden.Value, out refID);
            if (refID == 0) { PayMsgLabel.CssClass = "text-danger"; PayMsgLabel.Text = "Referrer not selected."; return; }

            if (!ValidatePayOtp()) return;

            double amount = 0;
            if (!double.TryParse(PayAmountTextBox.Text.Trim(), out amount) || amount <= 0)
            {
                PayMsgLabel.CssClass = "text-danger font-weight-bold";
                PayMsgLabel.Text = "Please enter valid amount."; return;
            }

            DateTime payDate = DateTime.Today;
            DateTime.TryParseExact(PayDateTextBox.Text.Trim(), "dd MMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out payDate);
            payDate = payDate.Date + DateTime.Now.TimeOfDay;

            string sql = @"INSERT INTO AAP_Reference_PaymentRecord(Reference_PayOrderID, ReferenceID, Reference_School_ID, SchoolID, InvoiceID, Amount, PaidDate, Paid_By, Payment_Method, Note)
                           VALUES(@PayOrderID, @RefID, @RefSchoolID, @SchoolID, @InvoiceID, @Amount, @Date, @PaidBy, @Method, @Note)";
            using (SqlConnection con = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@PayOrderID", 0);
                cmd.Parameters.AddWithValue("@RefID", refID);
                cmd.Parameters.AddWithValue("@RefSchoolID", 0);
                cmd.Parameters.AddWithValue("@SchoolID", 0);
                cmd.Parameters.AddWithValue("@InvoiceID", 0);
                cmd.Parameters.AddWithValue("@Amount", amount);
                cmd.Parameters.AddWithValue("@Date", payDate);
                cmd.Parameters.AddWithValue("@PaidBy", PaidByTextBox.Text.Trim());
                cmd.Parameters.AddWithValue("@Method", PayMethodDropDown.SelectedValue);
                cmd.Parameters.AddWithValue("@Note", PayNoteTextBox.Text.Trim());
                con.Open();
                cmd.ExecuteNonQuery();
            }

            ClearPayOtpSession();
            PayAmountTextBox.Text = "";
            PayOTPTextBox.Text = "";
            PayNoteTextBox.Text = "";
            SendOTPButton.Text = "Send OTP";
            PayMsgLabel.CssClass = "text-success font-weight-bold";
            PayMsgLabel.Text = "Payment recorded successfully!";
            LoadPayHistory(refID);

            DateTime fromDate = DateTime.Today.AddYears(-5);
            DateTime toDate = DateTime.Today;
            DateTime.TryParseExact(FromDateTextBox.Text.Trim(), "dd MMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out fromDate);
            DateTime.TryParseExact(ToDateTextBox.Text.Trim(), "dd MMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out toDate);
            int filterRefID = 0;
            int.TryParse(ReferrerDropDown.SelectedValue, out filterRefID);
            LoadReport(filterRefID, fromDate, toDate, StatusDropDown.SelectedValue);
        }
    }
}
