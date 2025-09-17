using System;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Authority.Invoice
{
    public partial class Print_Invoice : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void InvoiceButton_Click(object sender, EventArgs e)
        {
            string ids = "";
            foreach (GridViewRow row in InvoiceGridView.Rows)
            {
                var Pay_CheckBox = row.FindControl("Pay_CheckBox") as CheckBox;

                if (Pay_CheckBox.Checked)
                {
                    ids += InvoiceGridView.DataKeys[row.DataItemIndex]["InvoiceID"].ToString() + ",";
                }
            }

            if (ids != "")
            {
                Response.Redirect("Print/Pay_Invoice.aspx?SID=" + School_DropDownList.SelectedValue + "&InID=" + ids);
            }
        }

    }
}