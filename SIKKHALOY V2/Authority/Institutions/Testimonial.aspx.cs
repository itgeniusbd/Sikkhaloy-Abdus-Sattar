using System;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Authority.Institutions
{
    public partial class Testimonial : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void ShowCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            GridViewRow row = (GridViewRow)cb.NamingContainer;
            if (row != null)
            {
                int rowindex = row.RowIndex;
                ShowUpdateSQL.UpdateParameters["Is_Show"].DefaultValue = cb.Checked.ToString();
                ShowUpdateSQL.UpdateParameters["TestimonialID"].DefaultValue = TestimonialGridView.DataKeys[row.RowIndex]["TestimonialID"].ToString();
                ShowUpdateSQL.Update();
            }
        }
    }
}