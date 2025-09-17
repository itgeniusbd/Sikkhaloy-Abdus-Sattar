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
                var PerStudent_Label = row.FindControl("PerStudent_Label") as Label;
                var Fixed_Label = row.FindControl("Fixed_Label") as Label;
                var Discount_TextBox = row.FindControl("Discount_TextBox") as TextBox;

                double Amount = 0;
                double TotalStudent = Convert.ToDouble(Total_Student_Label.Text);
                double PerStudent = Convert.ToDouble(PerStudent_Label.Text);
                double Fixed = Convert.ToDouble(Fixed_Label.Text);
                double Discount = Convert.ToDouble(Discount_TextBox.Text);
                DateTime Issue = Convert.ToDateTime(sIssueDate_TextBox.Text);

                if (Invoice_CheckBox.Checked)
                {
                    if (Fixed == 0)
                    {
                        Amount = TotalStudent * PerStudent;
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
                    PayOrderSQL.InsertParameters["Unit"].DefaultValue = TotalStudent.ToString();
                    PayOrderSQL.Insert();
                }
            }

            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Record Inserted Successfully')", true);
        }


        protected void SMS_Paid_CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = (CheckBox)sender;
            GridViewRow gvr = (GridViewRow)chk.NamingContainer;

            SMS_SQL.UpdateParameters["SMS_Recharge_RecordID"].DefaultValue = SMSGridView.DataKeys[gvr.DataItemIndex]["SMS_Recharge_RecordID"].ToString();
            SMS_SQL.Update();
        }

        protected void SMS_Invoice_Button_Click(object sender, EventArgs e)
        {
            foreach (GridViewRow row in SMSGridView.Rows)
            {
                var SMS_CheckBox = row.FindControl("SMS_CheckBox") as CheckBox;
                var SMS_Unit_Label = row.FindControl("SMS_Unit_Label") as Label;
                var PerSMS_Label = row.FindControl("PerSMS_Label") as Label;
                var TotalAmount_Label = row.FindControl("TotalAmount_Label") as Label;
                var RechargeDate_Label = row.FindControl("RechargeDate_Label") as Label;

                DateTime Issue = Convert.ToDateTime(SMS_Issue_TextBox.Text);

                if (SMS_CheckBox.Checked)
                {
                    SMS_SQL.InsertParameters["Invoice_For"].DefaultValue = "Recharged: " + RechargeDate_Label.Text;
                    SMS_SQL.InsertParameters["Unit"].DefaultValue = SMS_Unit_Label.Text;
                    SMS_SQL.InsertParameters["UnitPrice"].DefaultValue = PerSMS_Label.Text;
                    SMS_SQL.InsertParameters["TotalAmount"].DefaultValue = TotalAmount_Label.Text;
                    SMS_SQL.InsertParameters["EndDate"].DefaultValue = Issue.AddDays(15).ToString();
                    SMS_SQL.InsertParameters["SchoolID"].DefaultValue = SMSGridView.DataKeys[row.DataItemIndex]["SchoolID"].ToString();
                    SMS_SQL.Insert();
                }
            }

            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Record Inserted Successfully')", true);
        }

        protected void OtherInvoice_Button_Click(object sender, EventArgs e)
        {
            OthersInvoiceSQL.Insert();
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Record Inserted Successfully')", true);
        }

    }
}