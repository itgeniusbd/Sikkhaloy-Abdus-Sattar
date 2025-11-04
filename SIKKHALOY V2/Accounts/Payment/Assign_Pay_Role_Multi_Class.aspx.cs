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

namespace EDUCATION.COM.Accounts.Payment
{
    public partial class Assign_Pay_Role_Multi_Class : Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!User.Identity.IsAuthenticated)
            {
                Response.Redirect("~/Default.aspx");
            }
        }

        protected void FindButton_Click(object sender, EventArgs e)
        {
            string selectedValue = "";
            foreach (ListItem item in ClassCheckBoxList.Items)
            {
                if (item.Selected)
                {
                    selectedValue += item.Value + ",";
                }
            }

            Roles_1_SQL.SelectParameters["ClassIDs"].DefaultValue = selectedValue;
            OtherRolesSQL.SelectParameters["ClassIDs"].DefaultValue = selectedValue;

            One_Role_GridView.DataBind();
            Multi_Role_GridView.DataBind();
        }
        protected void RoleButton_Click(object sender, EventArgs e)
        {
            bool added = false;
            foreach (ListItem item in ClassCheckBoxList.Items)
            {
                if (item.Selected)
                {
                    #region Add One Role
                    foreach (GridViewRow One_row in One_Role_GridView.Rows)
                    {
                        CheckBox One_Role_CheckBox = (CheckBox)One_row.FindControl("One_Role_CheckBox");

                        if (One_Role_CheckBox.Checked)
                        {
                            SqlCommand OneRole_cmd = new SqlCommand("SELECT * FROM Income_Assign_Role WHERE (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (EducationYearID = @EducationYearID) AND (RoleID = @RoleID)", con);
                            OneRole_cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                            OneRole_cmd.Parameters.AddWithValue("@ClassID", item.Value);
                            OneRole_cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"].ToString());
                            OneRole_cmd.Parameters.AddWithValue("@RoleID", One_Role_GridView.DataKeys[One_row.DataItemIndex]["RoleID"].ToString());

                            con.Open();
                            object count = OneRole_cmd.ExecuteScalar();
                            con.Close();

                            if (count == null)
                            {
                                TextBox One_PayForTextBox = (TextBox)One_row.FindControl("One_PayForTextBox");
                                TextBox One_AmountTextBox = (TextBox)One_row.FindControl("One_AmountTextBox");
                                TextBox One_StartDateTextBox = (TextBox)One_row.FindControl("One_StartDateTextBox");
                                TextBox One_EndDateTextBox = (TextBox)One_row.FindControl("One_EndDateTextBox");
                                TextBox One_LateFeeTextBox = (TextBox)One_row.FindControl("One_LateFeeTextBox");

                                AssignRoleSQL.InsertParameters["PayFor"].DefaultValue = One_PayForTextBox.Text;
                                AssignRoleSQL.InsertParameters["Amount"].DefaultValue = One_AmountTextBox.Text;
                                AssignRoleSQL.InsertParameters["StartDate"].DefaultValue = One_StartDateTextBox.Text;
                                AssignRoleSQL.InsertParameters["EndDate"].DefaultValue = One_EndDateTextBox.Text;
                                AssignRoleSQL.InsertParameters["LateFee"].DefaultValue = One_LateFeeTextBox.Text;
                                AssignRoleSQL.InsertParameters["RoleID"].DefaultValue = One_Role_GridView.DataKeys[One_row.DataItemIndex]["RoleID"].ToString();
                                AssignRoleSQL.InsertParameters["ClassID"].DefaultValue = item.Value;
                                AssignRoleSQL.Insert();
                                added = true;
                            }

                        }
                    }

                    #endregion End One Role

                    #region Add Multi Role

                    foreach (GridViewRow Multi_Role_Row in Multi_Role_GridView.Rows)
                    {
                        CheckBox Multi_AddCheckBox = Multi_Role_Row.FindControl("Multi_AddCheckBox") as CheckBox;

                        if (Multi_AddCheckBox.Checked)
                        {
                            GridView Input_Multi_Role_GridView = Multi_Role_Row.FindControl("Input_Multi_Role_GridView") as GridView;

                            foreach (GridViewRow In_Multi_Row in Input_Multi_Role_GridView.Rows)
                            {
                                CheckBox Input_MultiCheckBox = In_Multi_Row.FindControl("Input_MultiCheckBox") as CheckBox;
                                if (Input_MultiCheckBox.Checked)
                                {
                                    SqlCommand In_Multi_cmd = new SqlCommand("SELECT COUNT(*) FROM Income_Assign_Role WHERE (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (EducationYearID = @EducationYearID) AND (RoleID = @RoleID)", con);
                                    In_Multi_cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                                    In_Multi_cmd.Parameters.AddWithValue("@ClassID", item.Value);
                                    In_Multi_cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"].ToString());
                                    In_Multi_cmd.Parameters.AddWithValue("@RoleID", Multi_Role_GridView.DataKeys[Multi_Role_Row.RowIndex]["RoleID"].ToString());

                                    con.Open();
                                    int count = Convert.ToInt32(In_Multi_cmd.ExecuteScalar());
                                    con.Close();

                                    Label Multi_No_PayLabel = (Label)Multi_Role_Row.FindControl("Multi_No_PayLabel");
                                    int NumberOfPay = Convert.ToInt32(Multi_No_PayLabel.Text.Trim());

                                    if (count < NumberOfPay)
                                    {
                                        TextBox Multi_PayForTextBox = In_Multi_Row.FindControl("Multi_PayForTextBox") as TextBox;
                                        TextBox Multi_AmountTextBox = In_Multi_Row.FindControl("Multi_AmountTextBox") as TextBox;
                                        TextBox Multi_StartDateTextBox = In_Multi_Row.FindControl("Multi_StartDateTextBox") as TextBox;
                                        TextBox Multi_EndDateTextBox = In_Multi_Row.FindControl("Multi_EndDateTextBox") as TextBox;
                                        TextBox Multi_LateFeeTextBox = In_Multi_Row.FindControl("Multi_LateFeeTextBox") as TextBox;
                                        TextBox Multi_DiscountTextBox = In_Multi_Row.FindControl("Multi_DiscountTextBox") as TextBox;

                                        AssignRoleSQL.InsertParameters["PayFor"].DefaultValue = Multi_PayForTextBox.Text;
                                        AssignRoleSQL.InsertParameters["Amount"].DefaultValue = Multi_AmountTextBox.Text;
                                        AssignRoleSQL.InsertParameters["StartDate"].DefaultValue = Multi_StartDateTextBox.Text;
                                        AssignRoleSQL.InsertParameters["EndDate"].DefaultValue = Multi_EndDateTextBox.Text;
                                        AssignRoleSQL.InsertParameters["LateFee"].DefaultValue = Multi_LateFeeTextBox.Text;
                                        AssignRoleSQL.InsertParameters["RoleID"].DefaultValue = Multi_Role_GridView.DataKeys[Multi_Role_Row.RowIndex]["RoleID"].ToString();
                                        AssignRoleSQL.InsertParameters["ClassID"].DefaultValue = item.Value;
                                        AssignRoleSQL.Insert();
                                        added = true;
                                    }
                                }
                            }
                        }
                    }
                    #endregion End Add Multi Role
                }
            }

            if (added)
            {
                foreach (ListItem item in ClassCheckBoxList.Items)
                {
                    item.Selected = false;
                }

                One_Role_GridView.DataBind();
                Multi_Role_GridView.DataBind();
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Successfully Done!')", true);
            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('You Don't Select Any Role')", true);
            }
        }
        protected void Multi_Role_GridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                GridView Input_Multi_Role_GridView = (GridView)e.Row.FindControl("Input_Multi_Role_GridView");
                Label Multi_No_PayLabel = (Label)e.Row.FindControl("Multi_No_PayLabel");

                int c = Convert.ToInt32(Multi_No_PayLabel.Text);
                ArrayList values = new ArrayList();
                for (int i = 0; i < c; i++)
                {
                    values.Add(1);
                }

                Input_Multi_Role_GridView.DataSource = values;
                Input_Multi_Role_GridView.DataBind();
            }
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

        // Get Session Year Information with all months
        [WebMethod]
        public static string GetSessionYearInfo()
        {
            try
       {
          var months = new List<MonthInfo>();
              string sessionStart = "";
           string sessionEnd = "";

     using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
   {
      // Get session year dates first
        string basicQuery = "SELECT StartDate, EndDate FROM Education_Year WHERE EducationYearID = @EducationYearID";
        
              using (SqlCommand basicCmd = new SqlCommand(basicQuery, con))
        {
     basicCmd.Parameters.AddWithValue("@EducationYearID", HttpContext.Current.Session["Edu_Year"].ToString());
   
    con.Open();
          SqlDataReader basicDr = basicCmd.ExecuteReader();
   
 if (basicDr.Read())
           {
           sessionStart = basicDr["StartDate"].ToString();
      sessionEnd = basicDr["EndDate"].ToString();
  }
      basicDr.Close();
           }

  // Now get all months in the session
           string monthsQuery = @"
           WITH months(MonthDate, RowNum) AS (
   SELECT StartDate, 0 FROM Education_Year WHERE EducationYearID = @EducationYearID
            UNION ALL
       SELECT DATEADD(month, 1, MonthDate), RowNum + 1
 FROM months
         WHERE DATEADD(month, 1, MonthDate) <= (SELECT EndDate FROM Education_Year WHERE EducationYearID = @EducationYearID)
     )
     SELECT 
  FORMAT(MonthDate, 'MMMM yyyy') AS MonthName,
      FORMAT(DATEADD(day, 1-DAY(MonthDate), MonthDate), 'dd MMM yyyy') AS StartDate,
    FORMAT(DATEADD(day, 9, DATEADD(day, 1-DAY(MonthDate), MonthDate)), 'dd MMM yyyy') AS EndDate,
RowNum AS SortOrder
       FROM months
    ORDER BY RowNum
        OPTION (MAXRECURSION 0)";
    using (SqlCommand monthCmd = new SqlCommand(monthsQuery, con))
              {
      monthCmd.Parameters.AddWithValue("@EducationYearID", HttpContext.Current.Session["Edu_Year"].ToString());
       
   SqlDataReader monthDr = monthCmd.ExecuteReader();
            
        while (monthDr.Read())
              {
     months.Add(new MonthInfo
        {
MonthName = monthDr["MonthName"].ToString(),
              StartDate = monthDr["StartDate"].ToString(),
              EndDate = monthDr["EndDate"].ToString(),
   SortOrder = Convert.ToInt32(monthDr["SortOrder"])
  });
            }
          monthDr.Close();
       }
   
   con.Close();
     }

        var result = new
       {
  StartDate = sessionStart,
   EndDate = sessionEnd,
         Months = months
        };

      var json = new JavaScriptSerializer().Serialize(result);
         return json;
            }
            catch (Exception ex)
 {
      // Return error info for debugging
            var error = new
  {
                StartDate = "",
       EndDate = "",
          Months = new List<MonthInfo>(),
         Error = ex.Message
                };
       return new JavaScriptSerializer().Serialize(error);
            }
        }
        class EduYear
        {
            public string Month { get; set; }
            public string MonthYear { get; set; }
        }

        class MonthInfo
        {
            public string MonthName { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public int SortOrder { get; set; }
        }
    }
}