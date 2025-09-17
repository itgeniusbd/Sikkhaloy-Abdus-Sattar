using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Security;
using System.Web.Services;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.ADMISSION_REGISTER
{
    public partial class Reject_Student_from_school : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                RejectButton.Visible = false;
                ActiveButton.Visible = false;
                PayorderRadioButtonList.Visible = false;
            }
        }

        protected void RejectButton_Click(object sender, EventArgs e)
        {
            Reject_StudentInfoSQL.UpdateParameters["StudentID"].DefaultValue = StudentInfoFormView.DataKey["StudentID"].ToString();
            Reject_StudentInfoSQL.Update();

            StatusGridView.DataBind();
            StudentInfoFormView.DataBind();

            //Log
            ActDeActLogSQL.InsertParameters["StudentClassID"].DefaultValue = StudentInfoFormView.DataKey["StudentClassID"].ToString();
            ActDeActLogSQL.InsertParameters["StudentID"].DefaultValue = StudentInfoFormView.DataKey["StudentID"].ToString();
            ActDeActLogSQL.InsertParameters["Status"].DefaultValue = "Active";
            ActDeActLogSQL.InsertParameters["time"].DefaultValue = StudentInfoFormView.DataKey["ActiveTime"].ToString();
            ActDeActLogSQL.Insert();

            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
            Membership.DeleteUser(Session["SchoolID"].ToString() + IDTextBox.Text.Trim(), true);

            SqlCommand UpdateCmd = new SqlCommand("DELETE FROM Registration WHERE (UserName = @UserName) AND (SchoolID = @SchoolID)", con);//Delete user from Registration table
            UpdateCmd.Parameters.AddWithValue("@UserName", Session["SchoolID"].ToString() + IDTextBox.Text);
            UpdateCmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());

            con.Open();
            UpdateCmd.ExecuteNonQuery();
            con.Close();

            if (PayorderRadioButtonList.SelectedIndex == 0)
            {
                PayOrderDeleteSQL.Delete();
            }
            else
            {
                PayOrderDeleteSQL.DeleteParameters["EndDate"].DefaultValue = DateTime.Today.ToShortDateString();
                PayOrderDeleteSQL.Delete();
                PayOrderDeleteSQL.Update();
            }

            RejectButton.Visible = false;
            ActiveButton.Visible = true;

            Device_DataUpdateSQL.InsertParameters["UpdateType"].DefaultValue = "TC Student";
            Device_DataUpdateSQL.InsertParameters["UpdateDescription"].DefaultValue = "Deactivate Student";
            Device_DataUpdateSQL.Insert();

            Response.Redirect("Print_TC.aspx?Student=" + StudentInfoFormView.DataKey["StudentID"].ToString() + "&S_Class=" + StudentInfoFormView.DataKey["StudentClassID"].ToString());
        }

        protected void ActiveButton_Click(object sender, EventArgs e)
        {
            ActiveSQL.Update();

            StatusGridView.DataBind();
            StudentInfoFormView.DataBind();

            //Log
            ActDeActLogSQL.InsertParameters["StudentClassID"].DefaultValue = StudentInfoFormView.DataKey["StudentClassID"].ToString();
            ActDeActLogSQL.InsertParameters["StudentID"].DefaultValue = StudentInfoFormView.DataKey["StudentID"].ToString();
            ActDeActLogSQL.InsertParameters["Status"].DefaultValue = "Rejected";
            ActDeActLogSQL.InsertParameters["time"].DefaultValue = StudentInfoFormView.DataKey["DeactivateTime"].ToString();
            ActDeActLogSQL.Insert();

            ActiveButton.Visible = false;
            RejectButton.Visible = true;

            Device_DataUpdateSQL.InsertParameters["UpdateType"].DefaultValue = "Active Student";
            Device_DataUpdateSQL.InsertParameters["UpdateDescription"].DefaultValue = "Activate Student";
            Device_DataUpdateSQL.Insert();
        }

        protected void StatusSQL_Selected(object sender, SqlDataSourceStatusEventArgs e)
        {
            CountStudentLabel.Text = "Total: " + e.AffectedRows.ToString() + " Student(s)";
        }

        protected void ClassDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            TiIDTextBox.Text = "";
        }

        protected void StudentInfoFormView_DataBound(object sender, EventArgs e)
        {
            if (StudentInfoFormView.DataItemCount > 0)
            {
                string Status = StudentInfoFormView.DataKey["Status"].ToString();
                if (Status == "Active")
                {
                    RejectButton.Visible = true;
                    ActiveButton.Visible = false;
                    PayorderRadioButtonList.Visible = true;
                }
                else
                {
                    ActiveButton.Visible = true;
                    RejectButton.Visible = false;
                    PayorderRadioButtonList.Visible = false;
                }
            }
        }


        [WebMethod]
        public static string GetAllID(string ids)
        {
            List<string> StudentId = new List<string>();
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "SELECT top(3) ID FROM Student WHERE SchoolID = @SchoolID AND ID like @ID + '%'";
                    cmd.Parameters.AddWithValue("@ID", ids);
                    cmd.Parameters.AddWithValue("@SchoolID", HttpContext.Current.Session["SchoolID"].ToString());
                    cmd.Connection = con;

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        StudentId.Add(dr["ID"].ToString());
                    }
                    con.Close();

                    var json = new JavaScriptSerializer().Serialize(StudentId);
                    return json;
                }
            }
        }
    }
}