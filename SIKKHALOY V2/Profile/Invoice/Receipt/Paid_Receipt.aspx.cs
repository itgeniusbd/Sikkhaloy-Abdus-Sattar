using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Profile.Invoice
{
    public partial class Paid_Invoice : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Request.QueryString["SID"]))
            {
                Response.Redirect("Invoice_List.aspx");
            }
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

        private bool hasPartialPayment = false;
        private bool hasDiscount = false;

        protected void DetailsRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var paid = DataBinder.Eval(e.Item.DataItem, "Paid");
                var total = DataBinder.Eval(e.Item.DataItem, "TotalAmount");
                var discount = DataBinder.Eval(e.Item.DataItem, "Discount");

                if (paid != null && total != null)
                {
                    decimal paidAmount = Convert.ToDecimal(paid);
                    decimal totalAmount = Convert.ToDecimal(total);
                    if (paidAmount > 0 && paidAmount < totalAmount)
                    {
                        hasPartialPayment = true;
                    }
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
                var headerRow = repeater.Controls[0].Controls[0];
                var table = headerRow.Parent as System.Web.UI.WebControls.Literal;

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