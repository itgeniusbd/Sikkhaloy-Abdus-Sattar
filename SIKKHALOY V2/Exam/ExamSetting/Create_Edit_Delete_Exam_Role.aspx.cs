using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.EXAM.ExamSetting
{
    public partial class Create_Edit_Delete_Exam_Role : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void ExamRoleListView_ItemInserting(object sender, ListViewInsertEventArgs e)
        {
            TextBox ExamNameTextBox = (TextBox)e.Item.FindControl("ExamNameTextBox");

            if (ExamNameTextBox.Text == string.Empty)
            {
                e.Cancel = true;
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Enter Exam role name')", true);
            }
        }

        protected void ExamRoleListView_ItemDeleted(object sender, ListViewDeletedEventArgs e)
        {
            if (e.Exception == null)
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Sub-Exam Name has been Successfully deleted!')", true);
            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('This sub-Exam Name has been already Used!')", true);
                e.ExceptionHandled = true;
            }
        }
    }
}