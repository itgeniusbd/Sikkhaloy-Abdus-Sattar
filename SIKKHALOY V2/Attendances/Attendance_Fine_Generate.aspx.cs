using System;

namespace EDUCATION.COM.Attendances
{
    public partial class Attendance_Fine_Generate : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void GenarateButton_Click(object sender, EventArgs e)
        {
            FineSQL.Update();
            FineGridView.DataBind();
        }
    }
}