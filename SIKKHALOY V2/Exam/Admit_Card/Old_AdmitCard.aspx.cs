using System;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Exam.Admit_Card
{
    public partial class Old_AdmitCard : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void SectionDropDownList_DataBound(object sender, EventArgs e)
        {
            SectionDropDownList.Items.Insert(0, new ListItem("[ ALL SECTION ]", "%"));
        }
    }
}