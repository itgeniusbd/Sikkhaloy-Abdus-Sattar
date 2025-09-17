using System;
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

            foreach (GridViewRow row in InvoiceGridView.Rows)
            {
                var Pay_CheckBox = row.FindControl("Pay_CheckBox") as CheckBox;
                var PaidAmount_TextBox = row.FindControl("PaidAmount_TextBox") as TextBox;
                var Discount_TextBox = row.FindControl("Discount_TextBox") as TextBox;

                if (Pay_CheckBox.Checked)
                {
                    InvoiceSQL.UpdateParameters["PaidAmount"].DefaultValue = PaidAmount_TextBox.Text.Trim();
                    InvoiceSQL.UpdateParameters["Discount"].DefaultValue = Discount_TextBox.Text.Trim();
                    InvoiceSQL.UpdateParameters["InvoiceID"].DefaultValue = InvoiceGridView.DataKeys[row.DataItemIndex]["InvoiceID"].ToString();
                    InvoiceSQL.Update();

                    if (IsInsert)
                    {
                        Invoice_ReceiptSQL.Insert();
                        IsInsert = false;
                    }

                    Invoice_Payment_RecordSQL.InsertParameters["Amount"].DefaultValue = PaidAmount_TextBox.Text.Trim();
                    Invoice_Payment_RecordSQL.InsertParameters["InvoiceID"].DefaultValue = InvoiceGridView.DataKeys[row.DataItemIndex]["InvoiceID"].ToString();
                    Invoice_Payment_RecordSQL.Insert();
                }
            }

            School_DropDownList.DataBind();
        }
    }
}