using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.ACCOUNTS.Payment
{
    public partial class Assign_Payment_Roles : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void ClassDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (RoleDropDownList.SelectedIndex > 0)
            {
                addRole();
                IncludRoleGridView.DataBind();
                AssignedGridView.DataBind();
            }
        }
        protected void RoleDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (RoleDropDownList.SelectedIndex > 0)
            {
                addRole();
                AssignedRoleSQL.SelectParameters["RoleID"].DefaultValue = RoleDropDownList.SelectedValue.Split(',')[0];
            }
        }


        protected void RoleButton_Click(object sender, EventArgs e)
        {
            foreach (GridViewRow row in IncludRoleGridView.Rows)
            {
                CheckBox ActiveCheckBox = (CheckBox)row.FindControl("ActiveCheckBox");
                TextBox PayForTextBox = (TextBox)row.FindControl("PayForTextBox");
                TextBox AmountTextBox = (TextBox)row.FindControl("AmountTextBox");
                TextBox StartDateTextBox = (TextBox)row.FindControl("StartDateTextBox");
                TextBox EndDateTextBox = (TextBox)row.FindControl("EndDateTextBox");
                TextBox LateFeeTextBox = (TextBox)row.FindControl("LateFeeTextBox");

                AssignRoleSQL.InsertParameters["RoleID"].DefaultValue = RoleDropDownList.SelectedValue.Split(',')[0];
                AssignRoleSQL.InsertParameters["PayFor"].DefaultValue = PayForTextBox.Text;
                AssignRoleSQL.InsertParameters["Amount"].DefaultValue = AmountTextBox.Text;
                AssignRoleSQL.InsertParameters["StartDate"].DefaultValue = StartDateTextBox.Text;
                AssignRoleSQL.InsertParameters["EndDate"].DefaultValue = EndDateTextBox.Text;
                AssignRoleSQL.InsertParameters["LateFee"].DefaultValue = LateFeeTextBox.Text;

                if (ActiveCheckBox.Checked)
                {
                    SqlCommand cmd3 = new SqlCommand("SELECT COUNT(*) FROM Income_Assign_Role WHERE (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (EducationYearID = @EducationYearID) AND (RoleID = @RoleID)", con);
                    cmd3.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                    cmd3.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);
                    cmd3.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"].ToString());
                    cmd3.Parameters.AddWithValue("@RoleID", RoleDropDownList.SelectedValue.Split(',')[0]);

                    con.Open();
                    int count = Convert.ToInt32(cmd3.ExecuteScalar());
                    con.Close();

                    int NumberOfPay = Convert.ToInt32(RoleDropDownList.SelectedValue.Split(',')[1]);

                    if (count < NumberOfPay)
                    {
                        AssignRoleSQL.Insert();
                    }
                    else
                    {
                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('You can't add more than limit')", true);
                    }
                }
            }

            addRole();
            AssignedGridView.DataBind();
        }

        protected void ActiveCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = ((CheckBox)sender);
            GridViewRow item = ((GridViewRow)checkBox.NamingContainer);
            CheckBox ActiveCheckBox = (CheckBox)IncludRoleGridView.Rows[item.RowIndex].FindControl("ActiveCheckBox");

            RequiredFieldValidator payForRF = IncludRoleGridView.Rows[item.RowIndex].FindControl("payForRF") as RequiredFieldValidator;
            RequiredFieldValidator StartdateRF = IncludRoleGridView.Rows[item.RowIndex].FindControl("StartdateRF") as RequiredFieldValidator;
            RequiredFieldValidator EndDateRF = IncludRoleGridView.Rows[item.RowIndex].FindControl("EndDateRF") as RequiredFieldValidator;

            payForRF.Enabled = ActiveCheckBox.Checked;
            StartdateRF.Enabled = ActiveCheckBox.Checked;
            EndDateRF.Enabled = ActiveCheckBox.Checked;
        }

        protected void AssignedGridView_RowDeleted(object sender, GridViewDeletedEventArgs e)
        {
            addRole();
        }

        protected void addRole()
        {
       
            SqlCommand cmd3 = new SqlCommand("SELECT COUNT(*) FROM Income_Assign_Role WHERE (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (EducationYearID = @EducationYearID) AND (RoleID = @RoleID)", con);
            cmd3.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
            cmd3.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);
            cmd3.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"].ToString());
            cmd3.Parameters.AddWithValue("@RoleID", RoleDropDownList.SelectedValue.Split(',')[0]);

            con.Open();
            int count = Convert.ToInt32(cmd3.ExecuteScalar());
            con.Close();

            int NumberOfPay = Convert.ToInt32(RoleDropDownList.SelectedValue.Split(',')[1]);

            ArrayList values = new ArrayList();
            for (int i = 0; i < NumberOfPay - count; i++)
            {
                values.Add(1);
            }

            IncludRoleGridView.DataSource = values;
            IncludRoleGridView.DataBind();
        }

        //Get Month Name
        [WebMethod]
        public static string GetMonth(string prefix)
        {
            List<EduYear> User = new List<EduYear>();
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "WITH months(date) AS (SELECT StartDate FROM Education_Year WHERE (EducationYearID = @EducationYearID) UNION ALL SELECT DATEADD(month,1,date) from months where DATEADD(month,1,date)<= (SELECT EndDate FROM Education_Year WHERE (EducationYearID = @EducationYearID))) SELECT FORMAT(Date,'MMM yyyy') as Month_Year,FORMAT(date,'MMMM') AS [Month] from months where FORMAT(date,'MMMM') LIKE @Search + '%'";
                    cmd.Parameters.AddWithValue("@Search", prefix);
                    cmd.Parameters.AddWithValue("@EducationYearID", HttpContext.Current.Session["Edu_Year"].ToString());
                    cmd.Connection = con;

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        User.Add(new EduYear
                        {
                            Month = dr["Month"].ToString(),
                            MonthYear = dr["Month_Year"].ToString()
                        });
                    }
                    con.Close();

                    var json = new JavaScriptSerializer().Serialize(User);
                    return json;
                }
            }
        }
        class EduYear
        {
            public string Month { get; set; }
            public string MonthYear { get; set; }
        }
    }
}