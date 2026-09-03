using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Authority.Invoice
{
    public partial class Paid_Invoice : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Pay_Button_Click(object sender, EventArgs e)
        {
            bool IsInsert = true;
            string connectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

            foreach (GridViewRow row in InvoiceGridView.Rows)
            {
                var Pay_CheckBox = row.FindControl("Pay_CheckBox") as CheckBox;
                var PaidAmount_TextBox = row.FindControl("PaidAmount_TextBox") as TextBox;
                var Discount_TextBox = row.FindControl("Discount_TextBox") as TextBox;

                if (Pay_CheckBox.Checked)
                {
                    string invoiceID = InvoiceGridView.DataKeys[row.DataItemIndex]["InvoiceID"].ToString();
                    
                    InvoiceSQL.UpdateParameters["PaidAmount"].DefaultValue = PaidAmount_TextBox.Text.Trim();
                    InvoiceSQL.UpdateParameters["Discount"].DefaultValue = Discount_TextBox.Text.Trim();
                    InvoiceSQL.UpdateParameters["InvoiceID"].DefaultValue = invoiceID;
                    InvoiceSQL.Update();

                    if (IsInsert)
                    {
                        Invoice_ReceiptSQL.Insert();
                        IsInsert = false;
                    }

                    Invoice_Payment_RecordSQL.InsertParameters["Amount"].DefaultValue = PaidAmount_TextBox.Text.Trim();
                    Invoice_Payment_RecordSQL.InsertParameters["InvoiceID"].DefaultValue = invoiceID;
                    Invoice_Payment_RecordSQL.Insert();

                    // Update SMS_Recharge_Record if this is an SMS invoice
                    UpdateSMSRechargeStatus(invoiceID, connectionString);

                    // রেফারেল কমিশন স্বয়ংক্রিয়ভাবে রেকর্ড করুন
                    RecordReferralCommission(invoiceID, connectionString);
                }
            }

            // Grace remains until the given date while any invoice is still unpaid.
            // Only clear it when nothing is due anymore.
            int schoolId = 0;
            int.TryParse(School_DropDownList?.SelectedValue, out schoolId);
            ClearGracePeriodIfNoDue(schoolId, connectionString);

            School_DropDownList.DataBind();
        }

        private void ClearGracePeriodIfNoDue(int schoolId, string connectionString)
        {
            if (schoolId <= 0) return;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(@"
IF NOT EXISTS (SELECT 1 FROM AAP_Invoice WHERE SchoolID = @SID AND IsPaid = 0)
    UPDATE SchoolInfo SET AccessGraceUntil = NULL
    WHERE SchoolID = @SID AND AccessGraceUntil IS NOT NULL", conn))
                    {
                        cmd.Parameters.AddWithValue("@SID", schoolId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ClearGracePeriod error: " + ex.Message);
            }
        }

        private void UpdateSMSRechargeStatus(string invoiceID, string connectionString)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Check if this invoice is for SMS category and get related info
                    string checkQuery = @"
                        SELECT AAP_Invoice.SchoolID, AAP_Invoice.IssuDate, AAP_Invoice.Unit, AAP_Invoice.UnitPrice
                        FROM AAP_Invoice
                        INNER JOIN AAP_Invoice_Category ON AAP_Invoice.InvoiceCategoryID = AAP_Invoice_Category.InvoiceCategoryID
                        WHERE AAP_Invoice.InvoiceID = @InvoiceID 
                        AND AAP_Invoice_Category.InvoiceCategory = N'SMS'";

                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@InvoiceID", invoiceID);
                        conn.Open();
                        
                        using (SqlDataReader reader = checkCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int schoolID = reader.GetInt32(0);
                                DateTime issueDate = reader.GetDateTime(1);
                                object unitObj = reader.IsDBNull(2) ? null : (object)reader.GetValue(2);
                                object unitPriceObj = reader.IsDBNull(3) ? null : (object)reader.GetValue(3);
                                
                                conn.Close();

                                // Update SMS_Recharge_Record to mark as paid
                                string updateQuery = @"
                                    UPDATE SMS_Recharge_Record 
                                    SET Is_Paid = 1 
                                    WHERE SchoolID = @SchoolID 
                                    AND CONVERT(DATE, Date) = CONVERT(DATE, @IssueDate)
                                    AND Is_Paid = 0";

                                // If Unit and UnitPrice are available, use them for more specific matching
                                if (unitObj != null && unitPriceObj != null)
                                {
                                    updateQuery += " AND RechargeSMS = @Unit AND PerSMS_Price = @UnitPrice";
                                }

                                using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                                {
                                    updateCmd.Parameters.AddWithValue("@SchoolID", schoolID);
                                    updateCmd.Parameters.AddWithValue("@IssueDate", issueDate);
                                    
                                    if (unitObj != null && unitPriceObj != null)
                                    {
                                        updateCmd.Parameters.AddWithValue("@Unit", unitObj);
                                        updateCmd.Parameters.AddWithValue("@UnitPrice", unitPriceObj);
                                    }

                                    conn.Open();
                                    updateCmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't break the payment flow
                System.Diagnostics.Debug.WriteLine("Error updating SMS Recharge Status: " + ex.Message);
            }
        }

        // সার্ভিস চার্জ পেমেন্ট হলে স্বয়ংক্রিয়ভাবে রেফারেল কমিশন রেকর্ড করা
        private void RecordReferralCommission(string invoiceID, string connectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string sql = @"
                        -- শুধুমাত্র Service Charge ইনভয়েসের জন্য এবং ডুপ্লিকেট এড়িয়ে
                        IF NOT EXISTS (SELECT 1 FROM AAP_Reference_Commission WHERE InvoiceID = @InvoiceID)
                        BEGIN
                            INSERT INTO AAP_Reference_Commission
                                (ReferenceID, Reference_School_ID, InvoiceID, SchoolID, 
                                 Commission_Amount, Commission_Percentage, ServiceCharge_Amount, Commission_Date)
                            SELECT 
                                rs.ReferenceID,
                                rs.Reference_School_ID,
                                i.InvoiceID,
                                i.SchoolID,
                                CAST(i.TotalAmount * rs.Percentage / 100.0 AS DECIMAL(18,2)),
                                rs.Percentage,
                                i.TotalAmount,
                                GETDATE()
                            FROM AAP_Invoice i
                            INNER JOIN AAP_Reference_School rs ON rs.SchoolID = i.SchoolID
                                AND (rs.End_Reference_Date IS NULL OR GETDATE() <= rs.End_Reference_Date)
                            INNER JOIN AAP_Invoice_Category cat ON i.InvoiceCategoryID = cat.InvoiceCategoryID
                                AND cat.InvoiceCategory = N'Service Charge'
                            WHERE i.InvoiceID = @InvoiceID
                        END";

                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@InvoiceID", invoiceID);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("RecordReferralCommission Error: " + ex.Message);
            }
        }
    }
}