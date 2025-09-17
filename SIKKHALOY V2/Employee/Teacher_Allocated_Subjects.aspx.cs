using System;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Employee
{
    public partial class Teacher_Allocated_Subjects : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void SubjectCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox Checkbox = (CheckBox)sender;
            GridViewRow row = (GridViewRow)Checkbox.NamingContainer;

            var SubjectID = SubjectGridView.DataKeys[row.DataItemIndex].Value.ToString();
            if (Checkbox.Checked)
            {
                TeacherSubjectSQL.InsertParameters["SubjectID"].DefaultValue = SubjectID;
                TeacherSubjectSQL.Insert();
            }
            else
            {
                TeacherSubjectSQL.DeleteParameters["SubjectID"].DefaultValue = SubjectID;
                TeacherSubjectSQL.Delete();
            }
        }
    }
}