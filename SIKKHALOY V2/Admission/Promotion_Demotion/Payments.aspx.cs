using EDUCATION.COM.PaymentDataSetTableAdapters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Admission.Promotion_Demotion
{
    public partial class Payments : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string StudentID = Request.QueryString["Student"];

                if (string.IsNullOrEmpty(StudentID))
                    Response.Redirect("Change_Class_And_Subjects.aspx");
            }
        }

        protected void PayOrderButton_Click(object sender, EventArgs e)
        {
            OrdersTableAdapter Payment_DataSet = new OrdersTableAdapter();
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());



            #region Role One_A_RoleGridView

            foreach (GridViewRow Role_Row in One_A_RoleGridView.Rows)
            {
                CheckBox A_OR_CheckBox = Role_Row.FindControl("A_OR_CheckBox") as CheckBox;

                if (A_OR_CheckBox.Checked)
                {
                    int RoleID = Convert.ToInt32(One_A_RoleGridView.DataKeys[Role_Row.RowIndex]["RoleID"]);
                    int count = Convert.ToInt32(Payment_DataSet.Count_Inserted_PayOrder(Session["Edu_Year"].ToString(), RoleID, Convert.ToInt32(Request.QueryString["StudentClass"]), Convert.ToInt32(Session["SchoolID"].ToString())));
                    int NumberOfPay = Convert.ToInt32(Payment_DataSet.NumberOfPay_Count(RoleID));

                    if (count < NumberOfPay)
                    {
                        TextBox AmountTextBox = Role_Row.FindControl("AmountTextBox") as TextBox;
                        TextBox LateFeeTextBox = Role_Row.FindControl("LateFeeTextBox") as TextBox;
                        TextBox StartDateTextBox = Role_Row.FindControl("StartDateTextBox") as TextBox;
                        TextBox EndDateTextBox = Role_Row.FindControl("EndDateTextBox") as TextBox;
                        TextBox DiscountTextBox = Role_Row.FindControl("DiscountTextBox") as TextBox;

                        if (AmountTextBox.Text.Trim() != "" && StartDateTextBox.Text.Trim() != "" && EndDateTextBox.Text.Trim() != "")
                        {
                            PayOrderSQL.InsertParameters["Amount"].DefaultValue = AmountTextBox.Text;
                            PayOrderSQL.InsertParameters["LateFee"].DefaultValue = LateFeeTextBox.Text;
                            PayOrderSQL.InsertParameters["StartDate"].DefaultValue = StartDateTextBox.Text;
                            PayOrderSQL.InsertParameters["EndDate"].DefaultValue = EndDateTextBox.Text;
                            PayOrderSQL.InsertParameters["Discount"].DefaultValue = DiscountTextBox.Text;
                            PayOrderSQL.InsertParameters["RoleID"].DefaultValue = One_A_RoleGridView.DataKeys[Role_Row.RowIndex]["RoleID"].ToString();
                            PayOrderSQL.InsertParameters["AssignRoleID"].DefaultValue = One_A_RoleGridView.DataKeys[Role_Row.RowIndex]["AssignRoleID"].ToString();
                            PayOrderSQL.InsertParameters["PayFor"].DefaultValue = One_A_RoleGridView.DataKeys[Role_Row.RowIndex]["PayFor"].ToString();

                            PayOrderSQL.Insert();
                        }
                    }
                }
            }
            #endregion Role One_A_RoleGridView

            #region Multi_A_Role_GridView

            foreach (GridViewRow Multi_Role_Row in Multi_A_Role_GridView.Rows)
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

                            int RoleID = Convert.ToInt32(Multi_A_Role_GridView.DataKeys[Multi_Role_Row.RowIndex]["RoleID"]);
                            int count = Convert.ToInt32(Payment_DataSet.Count_Inserted_PayOrder(Session["Edu_Year"].ToString(), RoleID, Convert.ToInt32(Request.QueryString["StudentClass"]), Convert.ToInt32(Session["SchoolID"].ToString())));
                            int NumberOfPay = Convert.ToInt32(Payment_DataSet.NumberOfPay_Count(RoleID));

                            if (count < NumberOfPay)
                            {
                                TextBox AmountTextBox = In_Multi_Row.FindControl("Multi_AmountTextBox") as TextBox;
                                TextBox LateFeeTextBox = In_Multi_Row.FindControl("Multi_LateFeeTextBox") as TextBox;
                                TextBox StartDateTextBox = In_Multi_Row.FindControl("Multi_StartDateTextBox") as TextBox;
                                TextBox EndDateTextBox = In_Multi_Row.FindControl("Multi_EndDateTextBox") as TextBox;
                                TextBox DiscountTextBox = In_Multi_Row.FindControl("Multi_DiscountTextBox") as TextBox;
                                TextBox PayForTextBox = In_Multi_Row.FindControl("Multi_PayForTextBox") as TextBox;

                                if (PayForTextBox.Text.Trim() != "" && AmountTextBox.Text.Trim() != "" && StartDateTextBox.Text.Trim() != "" && EndDateTextBox.Text.Trim() != "")
                                {
                                    PayOrderSQL.InsertParameters["Amount"].DefaultValue = AmountTextBox.Text;
                                    PayOrderSQL.InsertParameters["LateFee"].DefaultValue = LateFeeTextBox.Text;
                                    PayOrderSQL.InsertParameters["StartDate"].DefaultValue = StartDateTextBox.Text;
                                    PayOrderSQL.InsertParameters["EndDate"].DefaultValue = EndDateTextBox.Text;
                                    PayOrderSQL.InsertParameters["Discount"].DefaultValue = DiscountTextBox.Text;
                                    PayOrderSQL.InsertParameters["RoleID"].DefaultValue = RoleID.ToString();
                                    PayOrderSQL.InsertParameters["AssignRoleID"].DefaultValue = Input_Multi_Role_GridView.DataKeys[In_Multi_Row.RowIndex]["AssignRoleID"].ToString();
                                    PayOrderSQL.InsertParameters["PayFor"].DefaultValue = PayForTextBox.Text;

                                    PayOrderSQL.Insert();
                                }
                            }
                        }
                    }
                }
            }
            #endregion Multi_A_Role_GridView

            #region One_Role_GridView

            foreach (GridViewRow One_Role_Row in One_Role_GridView.Rows)
            {
                CheckBox One_Role_CheckBox = One_Role_Row.FindControl("One_Role_CheckBox") as CheckBox;

                if (One_Role_CheckBox.Checked)
                {
                    int RoleID = Convert.ToInt32(One_Role_GridView.DataKeys[One_Role_Row.RowIndex]["RoleID"]);
                    int count = Convert.ToInt32(Payment_DataSet.Count_Inserted_PayOrder(Session["Edu_Year"].ToString(), RoleID, Convert.ToInt32(Request.QueryString["StudentClass"]), Convert.ToInt32(Session["SchoolID"].ToString())));
                    int NumberOfPay = Convert.ToInt32(Payment_DataSet.NumberOfPay_Count(RoleID));

                    if (count < NumberOfPay)
                    {
                        TextBox One_AmountTextBox = One_Role_Row.FindControl("One_AmountTextBox") as TextBox;
                        TextBox One_LateFeeTextBox = One_Role_Row.FindControl("One_LateFeeTextBox") as TextBox;
                        TextBox One_StartDateTextBox = One_Role_Row.FindControl("One_StartDateTextBox") as TextBox;
                        TextBox One_EndDateTextBox = One_Role_Row.FindControl("One_EndDateTextBox") as TextBox;
                        TextBox One_DiscountTextBox = One_Role_Row.FindControl("One_DiscountTextBox") as TextBox;
                        TextBox One_PayForTextBox = One_Role_Row.FindControl("One_PayForTextBox") as TextBox;

                        if (One_PayForTextBox.Text.Trim() != "" && One_AmountTextBox.Text.Trim() != "" && One_StartDateTextBox.Text.Trim() != "" && One_EndDateTextBox.Text.Trim() != "")
                        {
                            PayOrderSQL.InsertParameters["Amount"].DefaultValue = One_AmountTextBox.Text;
                            PayOrderSQL.InsertParameters["LateFee"].DefaultValue = One_LateFeeTextBox.Text;
                            PayOrderSQL.InsertParameters["StartDate"].DefaultValue = One_StartDateTextBox.Text;
                            PayOrderSQL.InsertParameters["EndDate"].DefaultValue = One_EndDateTextBox.Text;
                            PayOrderSQL.InsertParameters["Discount"].DefaultValue = One_DiscountTextBox.Text;
                            PayOrderSQL.InsertParameters["RoleID"].DefaultValue = One_Role_GridView.DataKeys[One_Role_Row.RowIndex]["RoleID"].ToString();
                            PayOrderSQL.InsertParameters["PayFor"].DefaultValue = One_PayForTextBox.Text;

                            PayOrderSQL.Insert();

                        }
                    }

                }
            }

            #endregion One_Role_GridView

            #region Multi_R_GridView
            foreach (GridViewRow Multi_R_Row in Multi_R_GridView.Rows)
            {
                CheckBox Multi_AddCheckBox = Multi_R_Row.FindControl("Multi_AddCheckBox") as CheckBox;

                if (Multi_AddCheckBox.Checked)
                {
                    GridView Input_Multi_Role_GridView = Multi_R_Row.FindControl("Input_Multi_Role_GridView") as GridView;
                    foreach (GridViewRow Row in Input_Multi_Role_GridView.Rows)
                    {
                        CheckBox Input_MultiCheckBox = Row.FindControl("Input_MultiCheckBox") as CheckBox;
                        if (Input_MultiCheckBox.Checked)
                        {
                            int RoleID = Convert.ToInt32(Multi_R_GridView.DataKeys[Multi_R_Row.RowIndex]["RoleID"]);
                            int count = Convert.ToInt32(Payment_DataSet.Count_Inserted_PayOrder(Session["Edu_Year"].ToString(), RoleID, Convert.ToInt32(Request.QueryString["StudentClass"]), Convert.ToInt32(Session["SchoolID"].ToString())));
                            int NumberOfPay = Convert.ToInt32(Payment_DataSet.NumberOfPay_Count(RoleID));

                            if (count < NumberOfPay)
                            {
                                TextBox PayForTextBox = Row.FindControl("Multi_PayForTextBox") as TextBox;
                                TextBox AmountTextBox = Row.FindControl("Multi_AmountTextBox") as TextBox;
                                TextBox StartDateTextBox = Row.FindControl("Multi_StartDateTextBox") as TextBox;
                                TextBox EndDateTextBox = Row.FindControl("Multi_EndDateTextBox") as TextBox;
                                TextBox LateFeeTextBox = Row.FindControl("Multi_LateFeeTextBox") as TextBox;
                                TextBox DiscountTextBox = Row.FindControl("Multi_DiscountTextBox") as TextBox;

                                if (PayForTextBox.Text.Trim() != "" && AmountTextBox.Text.Trim() != "" && StartDateTextBox.Text.Trim() != "" && EndDateTextBox.Text.Trim() != "")
                                {
                                    PayOrderSQL.InsertParameters["Amount"].DefaultValue = AmountTextBox.Text;
                                    PayOrderSQL.InsertParameters["LateFee"].DefaultValue = LateFeeTextBox.Text;
                                    PayOrderSQL.InsertParameters["StartDate"].DefaultValue = StartDateTextBox.Text;
                                    PayOrderSQL.InsertParameters["EndDate"].DefaultValue = EndDateTextBox.Text;
                                    PayOrderSQL.InsertParameters["Discount"].DefaultValue = DiscountTextBox.Text;
                                    PayOrderSQL.InsertParameters["RoleID"].DefaultValue = RoleID.ToString();
                                    PayOrderSQL.InsertParameters["PayFor"].DefaultValue = PayForTextBox.Text;

                                    PayOrderSQL.Insert();

                                }
                            }

                        }
                    }
                }
            }
            #endregion Multi_R_GridView


            Response.Redirect("List_Of_Students.aspx");
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
        class EduYear
        {
            public string Month { get; set; }
            public string MonthYear { get; set; }
        }
    }
}