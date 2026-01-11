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
                }
            }

            School_DropDownList.DataBind();
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
    }
}