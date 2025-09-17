using System;

namespace EDUCATION.COM.Committee
{
    public partial class DonationCategory : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void AddCategoryButton_Click(object sender, EventArgs e)
        {
            CategorySQL.Insert();
            CategoryGridView.DataBind();
            DonationCategoryTextBox.Text = "";
        }
    }
}