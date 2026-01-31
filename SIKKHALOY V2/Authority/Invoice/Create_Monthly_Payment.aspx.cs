using System;
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

    }
}