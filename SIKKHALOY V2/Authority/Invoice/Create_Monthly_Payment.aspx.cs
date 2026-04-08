using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Authority.Invoice
{
    public partial class Create_Monthly_Payment : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Ins_LinkButton_Command(object sender, CommandEventArgs e)
        {
            DetailsSQL.SelectParameters["SchoolID"].DefaultValue = e.CommandName.ToString();
            Institution_Label.Text = e.CommandArgument.ToString();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "Pop", "openModal();", true);
        }

        protected void CategoryButton_Click(object sender, EventArgs e)
        {
            InvoiceCategorySQL.Insert();
            Category_TextBox.Text = "";
        }

        protected void Monthly_Button_Click(object sender, EventArgs e)
        {
            foreach (GridViewRow row in Payment_GridView.Rows)
            {
                var Invoice_CheckBox = row.FindControl("Invoice_CheckBox") as CheckBox;
                var Total_Student_Label = row.FindControl("Total_Student_Label") as Label;
                var Committee_Count_Label = row.FindControl("Committee_Count_Label") as Label;
                var PerStudent_Label = row.FindControl("PerStudent_Label") as Label;
                var Fixed_Label = row.FindControl("Fixed_Label") as Label;
                var Discount_TextBox = row.FindControl("Discount_TextBox") as TextBox;

                double Amount = 0;
                double TotalStudent = Convert.ToDouble(Total_Student_Label.Text);
                double CommitteeCount = Committee_Count_Label != null ? Convert.ToDouble(Committee_Count_Label.Text) : 0;
                double TotalBillableCount = TotalStudent + CommitteeCount; // Student + Committee
                double PerStudent = Convert.ToDouble(PerStudent_Label.Text);
                double Fixed = Convert.ToDouble(Fixed_Label.Text);
                double Discount = Convert.ToDouble(Discount_TextBox.Text);
                DateTime Issue = Convert.ToDateTime(sIssueDate_TextBox.Text);

                if (Invoice_CheckBox.Checked)
                {
                    if (Fixed == 0)
                    {
                        Amount = TotalBillableCount * PerStudent; // Changed: Use total billable count
                        PayOrderSQL.InsertParameters["UnitPrice"].DefaultValue = PerStudent.ToString();
                    }
                    else
                    {
                        Amount = Fixed;
                        PayOrderSQL.InsertParameters["UnitPrice"].DefaultValue = null;
                    }

                    PayOrderSQL.InsertParameters["EndDate"].DefaultValue = Issue.AddDays(15).ToString();
                    PayOrderSQL.InsertParameters["SchoolID"].DefaultValue = Payment_GridView.DataKeys[row.DataItemIndex]["SchoolID"].ToString();
                    PayOrderSQL.InsertParameters["TotalAmount"].DefaultValue = Amount.ToString();
                    PayOrderSQL.InsertParameters["Discount"].DefaultValue = Discount_TextBox.Text;
                    PayOrderSQL.InsertParameters["Unit"].DefaultValue = TotalBillableCount.ToString(); // Changed: Use total billable count
                    PayOrderSQL.Insert();
                }
            }

            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Record Inserted Successfully')", true);
        }


        protected void SMS_Paid_CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            // This functionality is no longer needed as invoices are auto-generated
            // Kept for backward compatibility
        }

        protected void SMS_Invoice_Button_Click(object sender, EventArgs e)
        {
            // This functionality is no longer needed as invoices are auto-generated
            // Kept for backward compatibility
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('SMS invoices are now generated automatically when recharging from Institution Details page.')", true);
        }

        protected void OtherInvoice_Button_Click(object sender, EventArgs e)
        {
            OthersInvoiceSQL.Insert();
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Record Inserted Successfully')", true);
        }

        protected void GenerateCountButton_Click(object sender, EventArgs e)
        {
            try
            {
                GenerateStatusLabel.Visible = false;

                if (string.IsNullOrWhiteSpace(GenerateMonth_TextBox.Text))
                {
                    ShowGenerateStatus("Please select a month", "alert-danger");
                    return;
                }

                // Parse the selected month - support multiple formats
                DateTime selectedMonth;
                string[] formats = { 
                    "MM yyyy", "MMM yyyy", "MMMM yyyy",  // March 2026, Mar 2026
                    "dd MMM yyyy", "d MMM yyyy",         // 01 Mar 2026
                    "yyyy-MM-dd", "MM/dd/yyyy"           // 2026-03-01
                };
                
                if (!DateTime.TryParseExact(GenerateMonth_TextBox.Text.Trim(), formats,
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out selectedMonth))
                {
                    // Try standard DateTime.Parse as fallback
                    if (!DateTime.TryParse(GenerateMonth_TextBox.Text.Trim(), out selectedMonth))
                    {
                        ShowGenerateStatus("Invalid month format. Please use format: March 2026 or Mar 2026", "alert-danger");
                        return;
                    }
                }

                // Get the last day of the month (EOMONTH)
                DateTime monthEnd = new DateTime(selectedMonth.Year, selectedMonth.Month, 
                    DateTime.DaysInMonth(selectedMonth.Year, selectedMonth.Month));

                string connectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // Call the stored procedure
                    using (SqlCommand cmd = new SqlCommand("sp_Generate_Monthly_Student_Count", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 120; // 2 minutes timeout for large data
                        
                        cmd.Parameters.AddWithValue("@TargetMonth", monthEnd);
                        
                        SqlParameter countParam = new SqlParameter("@GeneratedCount", SqlDbType.Int);
                        countParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(countParam);
                        
                        SqlParameter msgParam = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, 500);
                        msgParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(msgParam);
                        
                        cmd.ExecuteNonQuery();
                        
                        int generatedCount = countParam.Value != DBNull.Value ? Convert.ToInt32(countParam.Value) : 0;
                        string errorMessage = msgParam.Value != DBNull.Value ? msgParam.Value.ToString() : "Unknown error";
                        
                        // Refresh the month dropdown
                        MonthSQL.DataBind();
                        Month_DropDownList.DataBind();
                        
                        // Show result
                        if (errorMessage.StartsWith("Success") || errorMessage.Contains("already exists"))
                        {
                            ShowGenerateStatus(errorMessage, "alert-success");
                            
                            // Try to auto-select the month
                            try
                            {
                                string monthValue = monthEnd.ToString("yyyy-MM-dd");
                                if (Month_DropDownList.Items.FindByValue(monthValue) != null)
                                {
                                    Month_DropDownList.SelectedValue = monthValue;
                                }
                            }
                            catch { }
                        }
                        else
                        {
                            ShowGenerateStatus("Error: " + errorMessage, "alert-danger");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowGenerateStatus($"Error: {ex.Message}", "alert-danger");
            }
        }

        private void ShowGenerateStatus(string message, string cssClass)
        {
            GenerateStatusLabel.Text = message;
            GenerateStatusLabel.CssClass = $"alert {cssClass}";
            GenerateStatusLabel.Visible = true;
        }

    }
}