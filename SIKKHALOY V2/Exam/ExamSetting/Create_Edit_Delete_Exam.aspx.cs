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
    public partial class Create_Edit_Delete_Exam : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void ExamGridView_RowDeleted(object sender, GridViewDeletedEventArgs e)
        {
            if (e.Exception != null)
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('This Exam Name has been already Used!')", true);
                e.ExceptionHandled = true;
            }
        }

        protected void ExamButton_Click(object sender, EventArgs e)
        {
            ExamSQL.Insert();

            ExamNameTextBox.Text = "";
            Period_StartDateTextBox.Text = "";
            Period_EndDateTextBox.Text = "";
        }

    }
}