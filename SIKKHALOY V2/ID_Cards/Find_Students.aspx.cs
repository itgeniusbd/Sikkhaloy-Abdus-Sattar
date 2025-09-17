using System;
using System.Data;
using System.Web.UI;

namespace EDUCATION.COM.ID_CARDS
{
    public partial class Find_Students : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                DataView dv = (DataView)AllStudentSQL.Select(DataSourceSelectArguments.Empty);
                StudentCountLabel.Text = "Total: " + dv.Count.ToString() + " Student(s)";
            }
        }

        protected void FindButton_Click(object sender, EventArgs e)
        {
            DataView dv = (DataView)AllStudentSQL.Select(DataSourceSelectArguments.Empty);
            StudentCountLabel.Text = "Total: " + dv.Count.ToString() + " Student(s)";
        }

    }
}