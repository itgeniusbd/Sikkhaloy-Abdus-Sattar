using System;

namespace EDUCATION.COM.Employee
{
    public partial class MemberType : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void AddMemberTypeButton_Click(object sender, EventArgs e)
        {
            MemberTypeSQL.Insert();
            MemberTypeGridView.DataBind();
            MemberTypeTextBox.Text = "";
        }
    }
}