using System;

namespace EDUCATION.COM.Accounts.Payment
{
    public partial class Unpaid_Money_Receipt : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void DeleteReceiptButton_Click(object sender, EventArgs e)
        {
            MoneyRSQL.Delete();
            StudentInfoFormView.DataBind();
            ReceiptFormView.DataBind();
            PaidDetailsGridView.DataBind();
            RByFormView.DataBind();
        }
    }
}
