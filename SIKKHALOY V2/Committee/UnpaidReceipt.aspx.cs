using System;

namespace EDUCATION.COM.Committee
{
    public partial class UnpaidReceipt : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void DeleteReceiptButton_Click(object sender, EventArgs e)
        {
            MoneyRSQL.Delete();
            StudentInfoFormView.DataBind();
            ReceiptFormView.DataBind();
            PaymentGridView.DataBind();
            RByFormView.DataBind();
        }
    }
}