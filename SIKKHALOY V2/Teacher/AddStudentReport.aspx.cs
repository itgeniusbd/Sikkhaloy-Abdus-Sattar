using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Web.Services;
using Microsoft.Reporting.WebForms;

namespace EDUCATION.COM.Teacher
{
    public partial class AddStudentReport : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        //find by ID
        protected void IDFindButton_Click(object sender, EventArgs e)
        {
            DataView ShowIDDV = new DataView();
            ShowIDDV = (DataView)ShowIDSQL.Select(DataSourceSelectArguments.Empty);
            if (ShowIDDV.Count > 0)
            {
                Response.Redirect("AddStudentReport.aspx?Student=" + ShowIDDV[0]["StudentID"].ToString() + "&Student_Class=" + ShowIDDV[0]["StudentClassID"].ToString());
            }
        }
   

        protected void Fault_Add_Button_Click(object sender, EventArgs e)
        {
            FaultSQL.Insert();
            Fault_Title_TextBox.Text = "";
            Fault_TextBox.Text = "";
            Fault_Date_TextBox.Text = "";
            Fault_Gridview.DataBind();

            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Record Inserted Successfully')", true);
        }

    }
}