using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Authority.Reference
{
    public partial class School_Reference : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            foreach (GridViewRow row in SchoolGridView.Rows)
            {
                CheckBox SelectCheckBox = row.FindControl("SelectCheckBox") as CheckBox;
               
                if (SelectCheckBox.Checked)
                {
                    AAP_Reference_SchoolSQL.InsertParameters["SchoolID"].DefaultValue = SchoolGridView.DataKeys[row.DataItemIndex]["SchoolID"].ToString();
                    AAP_Reference_SchoolSQL.Insert();
                }
            }
            SchoolGridView.DataBind();
        }
    }
}