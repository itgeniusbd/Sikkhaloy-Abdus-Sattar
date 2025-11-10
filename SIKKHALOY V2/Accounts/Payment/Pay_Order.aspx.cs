using EDUCATION.COM.PaymentDataSetTableAdapters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.ACCOUNTS.Payment
{
    public partial class Pay_Order : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                //PayorderMsgLabel.Text = Request.Cookies["massage"].Value;
                Response.Cookies["massage"].Expires = DateTime.Now;
            }
            catch
            { }

            DataView ClassNameDV = new DataView();
            ClassNameDV = (DataView)ClassNameSQL.Select(DataSourceSelectArguments.Empty);
            if (ClassNameDV.Count > 0)
            {
                EmptyStudentLabel.Text = "";
                ClassDropDownList.Visible = true;
            }

            else
            {
                ClassDropDownList.Visible = false;
                EmptyStudentLabel.Text = "You have to Add Student before you can order your payment";
            }
        }

        protected void PayOrderButton_Click(object sender, EventArgs e)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
            OrdersTableAdapter Payment_DataSet = new OrdersTableAdapter();
            double success_count = 0;
            double fail_count = 0;

            if (ClassDropDownList.SelectedIndex == 1)
            {
                #region All Students Pay Order

                SqlCommand Student_Class_Cmd = new SqlCommand("SELECT StudentsClass.* FROM  StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID WHERE (StudentsClass.SchoolID = @SchoolID) AND  (StudentsClass.EducationYearID = @EducationYearID) AND (Student.Status = 'Active')", con);
                Student_Class_Cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                Student_Class_Cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"].ToString());

                con.Open();
                SqlDataReader Student_Class_DR;
                Student_Class_DR = Student_Class_Cmd.ExecuteReader();

                while (Student_Class_DR.Read())
                {
                    #region One_Role_GridView

                    foreach (GridViewRow One_Role_Row in One_Role_GridView.Rows)
                    {
                        CheckBox One_Role_CheckBox = One_Role_Row.FindControl("One_Role_CheckBox") as CheckBox;

                        if (One_Role_CheckBox.Checked)
                        {
                            int RoleID = Convert.ToInt32(One_Role_GridView.DataKeys[One_Role_Row.RowIndex]["RoleID"]);
                            int count = Convert.ToInt32(Payment_DataSet.Count_Inserted_PayOrder(Session["Edu_Year"].ToString(), RoleID, Convert.ToInt32(Student_Class_DR["StudentClassID"]), Convert.ToInt32(Session["SchoolID"].ToString())));
                            int NumberOfPay = Convert.ToInt32(Payment_DataSet.NumberOfPay_Count(RoleID));

                            if (count < NumberOfPay)
                            {
                                TextBox One_AmountTextBox = One_Role_Row.FindControl("One_AmountTextBox") as TextBox;
                                TextBox One_LateFeeTextBox = One_Role_Row.FindControl("One_LateFeeTextBox") as TextBox;
                                TextBox One_StartDateTextBox = One_Role_Row.FindControl("One_StartDateTextBox") as TextBox;
                                TextBox One_EndDateTextBox = One_Role_Row.FindControl("One_EndDateTextBox") as TextBox;
                                TextBox One_DiscountTextBox = One_Role_Row.FindControl("One_DiscountTextBox") as TextBox;
                                TextBox One_PayForTextBox = One_Role_Row.FindControl("One_PayForTextBox") as TextBox;



                                double Amount;
                                double DiscountAmount = 0;

                                double.TryParse(One_DiscountTextBox.Text.Trim(), out DiscountAmount);

                                if (One_PayForTextBox.Text.Trim() != "" && One_AmountTextBox.Text.Trim() != "0" && double.TryParse(One_AmountTextBox.Text.Trim(), out Amount) && One_StartDateTextBox.Text.Trim() != "" && One_EndDateTextBox.Text.Trim() != "")
                                {
                                    if (Amount >= DiscountAmount)
                                    {
                                        PayOrderSQL.InsertParameters["StudentID"].DefaultValue = Student_Class_DR["StudentID"].ToString();
                                        PayOrderSQL.InsertParameters["ClassID"].DefaultValue = Student_Class_DR["ClassID"].ToString();
                                        PayOrderSQL.InsertParameters["StudentClassID"].DefaultValue = Student_Class_DR["StudentClassID"].ToString();

                                        PayOrderSQL.InsertParameters["Amount"].DefaultValue = One_AmountTextBox.Text;
                                        PayOrderSQL.InsertParameters["LateFee"].DefaultValue = One_LateFeeTextBox.Text;
                                        PayOrderSQL.InsertParameters["StartDate"].DefaultValue = One_StartDateTextBox.Text;
                                        PayOrderSQL.InsertParameters["EndDate"].DefaultValue = One_EndDateTextBox.Text;
                                        PayOrderSQL.InsertParameters["Discount"].DefaultValue = One_DiscountTextBox.Text;
                                        PayOrderSQL.InsertParameters["RoleID"].DefaultValue = RoleID.ToString();
                                        PayOrderSQL.InsertParameters["PayFor"].DefaultValue = One_PayForTextBox.Text;

                                        PayOrderSQL.Insert();
                                        success_count++;
                                    }
                                }
                            }
                            else
                            {
                                fail_count++;
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
                                    int count = Convert.ToInt32(Payment_DataSet.Count_Inserted_PayOrder(Session["Edu_Year"].ToString(), RoleID, Convert.ToInt32(Student_Class_DR["StudentClassID"]), Convert.ToInt32(Session["SchoolID"].ToString())));
                                    int NumberOfPay = Convert.ToInt32(Payment_DataSet.NumberOfPay_Count(RoleID));

                                    if (count < NumberOfPay)
                                    {
                                        TextBox PayForTextBox = Row.FindControl("Multi_PayForTextBox") as TextBox;
                                        TextBox AmountTextBox = Row.FindControl("Multi_AmountTextBox") as TextBox;
                                        TextBox StartDateTextBox = Row.FindControl("Multi_StartDateTextBox") as TextBox;
                                        TextBox EndDateTextBox = Row.FindControl("Multi_EndDateTextBox") as TextBox;
                                        TextBox LateFeeTextBox = Row.FindControl("Multi_LateFeeTextBox") as TextBox;
                                        TextBox DiscountTextBox = Row.FindControl("Multi_DiscountTextBox") as TextBox;


                                        double Amount;
                                        double DiscountAmount = 0;

                                        double.TryParse(DiscountTextBox.Text.Trim(), out DiscountAmount);

                                        if (PayForTextBox.Text.Trim() != "" && AmountTextBox.Text.Trim() != "0" && double.TryParse(AmountTextBox.Text.Trim(), out Amount) && StartDateTextBox.Text.Trim() != "" && EndDateTextBox.Text.Trim() != "")
                                        {
                                            if (Amount >= DiscountAmount)
                                            {
                                                PayOrderSQL.InsertParameters["StudentID"].DefaultValue = Student_Class_DR["StudentID"].ToString();
                                                PayOrderSQL.InsertParameters["ClassID"].DefaultValue = Student_Class_DR["ClassID"].ToString();
                                                PayOrderSQL.InsertParameters["StudentClassID"].DefaultValue = Student_Class_DR["StudentClassID"].ToString();

                                                PayOrderSQL.InsertParameters["Amount"].DefaultValue = AmountTextBox.Text;
                                                PayOrderSQL.InsertParameters["LateFee"].DefaultValue = LateFeeTextBox.Text;
                                                PayOrderSQL.InsertParameters["StartDate"].DefaultValue = StartDateTextBox.Text;
                                                PayOrderSQL.InsertParameters["EndDate"].DefaultValue = EndDateTextBox.Text;
                                                PayOrderSQL.InsertParameters["Discount"].DefaultValue = DiscountTextBox.Text;
                                                PayOrderSQL.InsertParameters["RoleID"].DefaultValue = RoleID.ToString();
                                                PayOrderSQL.InsertParameters["PayFor"].DefaultValue = PayForTextBox.Text;

                                                PayOrderSQL.Insert();
                                                success_count++;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        fail_count++;
                                    }
                                }
                            }
                        }
                    }
                    #endregion  Multi_R_GridView
                }

                con.Close();

                #endregion All Students Pay Order
            }
            else
            {
                #region Class wise Students Pay Order

                foreach (GridViewRow Student_Row in StudentsGridView.Rows)
                {
                    CheckBox SingleCheckBox = Student_Row.FindControl("SingleCheckBox") as CheckBox;
                    if (SingleCheckBox.Checked)
                    {
                        PayOrderSQL.InsertParameters["StudentID"].DefaultValue = StudentsGridView.DataKeys[Student_Row.RowIndex]["StudentID"].ToString();
                        PayOrderSQL.InsertParameters["StudentClassID"].DefaultValue = StudentsGridView.DataKeys[Student_Row.RowIndex]["StudentClassID"].ToString();

                        #region Role One_A_RoleGridView

                        foreach (GridViewRow Role_Row in One_A_RoleGridView.Rows)
                        {
                            CheckBox A_OR_CheckBox = Role_Row.FindControl("A_OR_CheckBox") as CheckBox;

                            if (A_OR_CheckBox.Checked)
                            {
                                int RoleID = Convert.ToInt32(One_A_RoleGridView.DataKeys[Role_Row.RowIndex]["RoleID"]);
                                int count = Convert.ToInt32(Payment_DataSet.Count_Inserted_PayOrder(Session["Edu_Year"].ToString(), RoleID, Convert.ToInt32(StudentsGridView.DataKeys[Student_Row.RowIndex]["StudentClassID"]), Convert.ToInt32(Session["SchoolID"].ToString())));
                                int NumberOfPay = Convert.ToInt32(Payment_DataSet.NumberOfPay_Count(RoleID));

                                if (count < NumberOfPay)
                                {
                                    TextBox AmountTextBox = Role_Row.FindControl("AmountTextBox") as TextBox;
                                    TextBox LateFeeTextBox = Role_Row.FindControl("LateFeeTextBox") as TextBox;
                                    TextBox StartDateTextBox = Role_Row.FindControl("StartDateTextBox") as TextBox;
                                    TextBox EndDateTextBox = Role_Row.FindControl("EndDateTextBox") as TextBox;
                                    TextBox DiscountTextBox = Role_Row.FindControl("DiscountTextBox") as TextBox;

                                    double Amount;
                                    double DiscountAmount = 0;

                                    double.TryParse(DiscountTextBox.Text.Trim(), out DiscountAmount);


                                    if (AmountTextBox.Text.Trim() != "0" && double.TryParse(AmountTextBox.Text.Trim(), out Amount) && StartDateTextBox.Text.Trim() != "" && EndDateTextBox.Text.Trim() != "")
                                    {
                                        if (Amount >= DiscountAmount)
                                        {
                                            PayOrderSQL.InsertParameters["ClassID"].DefaultValue = StudentsGridView.DataKeys[Student_Row.RowIndex]["ClassID"].ToString();
                                            PayOrderSQL.InsertParameters["Amount"].DefaultValue = AmountTextBox.Text;
                                            PayOrderSQL.InsertParameters["LateFee"].DefaultValue = LateFeeTextBox.Text;
                                            PayOrderSQL.InsertParameters["StartDate"].DefaultValue = StartDateTextBox.Text;
                                            PayOrderSQL.InsertParameters["EndDate"].DefaultValue = EndDateTextBox.Text;
                                            PayOrderSQL.InsertParameters["Discount"].DefaultValue = DiscountTextBox.Text;
                                            PayOrderSQL.InsertParameters["RoleID"].DefaultValue = One_A_RoleGridView.DataKeys[Role_Row.RowIndex]["RoleID"].ToString();
                                            PayOrderSQL.InsertParameters["AssignRoleID"].DefaultValue = One_A_RoleGridView.DataKeys[Role_Row.RowIndex]["AssignRoleID"].ToString();
                                            PayOrderSQL.InsertParameters["PayFor"].DefaultValue = One_A_RoleGridView.DataKeys[Role_Row.RowIndex]["PayFor"].ToString();

                                            PayOrderSQL.Insert();
                                            success_count++;
                                        }
                                    }
                                }
                                else
                                {
                                    fail_count++;
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
                                        int count = Convert.ToInt32(Payment_DataSet.Count_Inserted_PayOrder(Session["Edu_Year"].ToString(), RoleID, Convert.ToInt32(StudentsGridView.DataKeys[Student_Row.RowIndex]["StudentClassID"]), Convert.ToInt32(Session["SchoolID"].ToString())));
                                        int NumberOfPay = Convert.ToInt32(Payment_DataSet.NumberOfPay_Count(RoleID));

                                        if (count < NumberOfPay)
                                        {
                                            TextBox AmountTextBox = In_Multi_Row.FindControl("Multi_AmountTextBox") as TextBox;
                                            TextBox LateFeeTextBox = In_Multi_Row.FindControl("Multi_LateFeeTextBox") as TextBox;
                                            TextBox StartDateTextBox = In_Multi_Row.FindControl("Multi_StartDateTextBox") as TextBox;
                                            TextBox EndDateTextBox = In_Multi_Row.FindControl("Multi_EndDateTextBox") as TextBox;
                                            TextBox DiscountTextBox = In_Multi_Row.FindControl("Multi_DiscountTextBox") as TextBox;
                                            TextBox PayForTextBox = In_Multi_Row.FindControl("Multi_PayForTextBox") as TextBox;

                                            double Amount;
                                            double DiscountAmount = 0;

                                            double.TryParse(DiscountTextBox.Text.Trim(), out DiscountAmount);

                                            if (PayForTextBox.Text.Trim() != "" && AmountTextBox.Text.Trim() != "0" && double.TryParse(AmountTextBox.Text.Trim(), out Amount) && StartDateTextBox.Text.Trim() != "" && EndDateTextBox.Text.Trim() != "")
                                            {
                                                if (Amount >= DiscountAmount)
                                                {
                                                    PayOrderSQL.InsertParameters["ClassID"].DefaultValue = StudentsGridView.DataKeys[Student_Row.RowIndex]["ClassID"].ToString();
                                                    PayOrderSQL.InsertParameters["Amount"].DefaultValue = AmountTextBox.Text;
                                                    PayOrderSQL.InsertParameters["LateFee"].DefaultValue = LateFeeTextBox.Text;
                                                    PayOrderSQL.InsertParameters["StartDate"].DefaultValue = StartDateTextBox.Text;
                                                    PayOrderSQL.InsertParameters["EndDate"].DefaultValue = EndDateTextBox.Text;
                                                    PayOrderSQL.InsertParameters["Discount"].DefaultValue = DiscountTextBox.Text;
                                                    PayOrderSQL.InsertParameters["RoleID"].DefaultValue = RoleID.ToString();
                                                    PayOrderSQL.InsertParameters["AssignRoleID"].DefaultValue = Input_Multi_Role_GridView.DataKeys[In_Multi_Row.RowIndex]["AssignRoleID"].ToString();
                                                    PayOrderSQL.InsertParameters["PayFor"].DefaultValue = PayForTextBox.Text;

                                                    PayOrderSQL.Insert();
                                                    success_count++;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            fail_count++;
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
                                int count = Convert.ToInt32(Payment_DataSet.Count_Inserted_PayOrder(Session["Edu_Year"].ToString(), RoleID, Convert.ToInt32(StudentsGridView.DataKeys[Student_Row.RowIndex]["StudentClassID"]), Convert.ToInt32(Session["SchoolID"].ToString())));
                                int NumberOfPay = Convert.ToInt32(Payment_DataSet.NumberOfPay_Count(RoleID));

                                if (count < NumberOfPay)
                                {
                                    TextBox One_AmountTextBox = One_Role_Row.FindControl("One_AmountTextBox") as TextBox;
                                    TextBox One_LateFeeTextBox = One_Role_Row.FindControl("One_LateFeeTextBox") as TextBox;
                                    TextBox One_StartDateTextBox = One_Role_Row.FindControl("One_StartDateTextBox") as TextBox;
                                    TextBox One_EndDateTextBox = One_Role_Row.FindControl("One_EndDateTextBox") as TextBox;
                                    TextBox One_DiscountTextBox = One_Role_Row.FindControl("One_DiscountTextBox") as TextBox;
                                    TextBox One_PayForTextBox = One_Role_Row.FindControl("One_PayForTextBox") as TextBox;

                                    double Amount;
                                    double DiscountAmount = 0;

                                    double.TryParse(One_DiscountTextBox.Text.Trim(), out DiscountAmount);

                                    if (One_PayForTextBox.Text.Trim() != "" && One_AmountTextBox.Text.Trim() != "0" && double.TryParse(One_AmountTextBox.Text.Trim(), out Amount) && One_StartDateTextBox.Text.Trim() != "" && One_EndDateTextBox.Text.Trim() != "")
                                    {
                                        if (Amount >= DiscountAmount)
                                        {
                                            PayOrderSQL.InsertParameters["ClassID"].DefaultValue = StudentsGridView.DataKeys[Student_Row.RowIndex]["ClassID"].ToString();
                                            PayOrderSQL.InsertParameters["Amount"].DefaultValue = One_AmountTextBox.Text;
                                            PayOrderSQL.InsertParameters["LateFee"].DefaultValue = One_LateFeeTextBox.Text;
                                            PayOrderSQL.InsertParameters["StartDate"].DefaultValue = One_StartDateTextBox.Text;
                                            PayOrderSQL.InsertParameters["EndDate"].DefaultValue = One_EndDateTextBox.Text;
                                            PayOrderSQL.InsertParameters["Discount"].DefaultValue = One_DiscountTextBox.Text;
                                            PayOrderSQL.InsertParameters["RoleID"].DefaultValue = One_Role_GridView.DataKeys[One_Role_Row.RowIndex]["RoleID"].ToString();
                                            PayOrderSQL.InsertParameters["PayFor"].DefaultValue = One_PayForTextBox.Text;

                                            PayOrderSQL.Insert();
                                            success_count++;
                                        }
                                    }
                                }
                                else
                                {
                                    fail_count++;
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
                                        int count = Convert.ToInt32(Payment_DataSet.Count_Inserted_PayOrder(Session["Edu_Year"].ToString(), RoleID, Convert.ToInt32(StudentsGridView.DataKeys[Student_Row.RowIndex]["StudentClassID"]), Convert.ToInt32(Session["SchoolID"].ToString())));
                                        int NumberOfPay = Convert.ToInt32(Payment_DataSet.NumberOfPay_Count(RoleID));

                                        if (count < NumberOfPay)
                                        {
                                            TextBox PayForTextBox = Row.FindControl("Multi_PayForTextBox") as TextBox;
                                            TextBox AmountTextBox = Row.FindControl("Multi_AmountTextBox") as TextBox;
                                            TextBox StartDateTextBox = Row.FindControl("Multi_StartDateTextBox") as TextBox;
                                            TextBox EndDateTextBox = Row.FindControl("Multi_EndDateTextBox") as TextBox;
                                            TextBox LateFeeTextBox = Row.FindControl("Multi_LateFeeTextBox") as TextBox;
                                            TextBox DiscountTextBox = Row.FindControl("Multi_DiscountTextBox") as TextBox;


                                            double Amount;
                                            double DiscountAmount = 0;

                                            double.TryParse(DiscountTextBox.Text.Trim(), out DiscountAmount);

                                            if (PayForTextBox.Text.Trim() != "" && AmountTextBox.Text.Trim() != "0" && double.TryParse(AmountTextBox.Text.Trim(), out Amount) && StartDateTextBox.Text.Trim() != "" && EndDateTextBox.Text.Trim() != "")
                                            {
                                                if (Amount >= DiscountAmount)
                                                {
                                                    PayOrderSQL.InsertParameters["ClassID"].DefaultValue = StudentsGridView.DataKeys[Student_Row.RowIndex]["ClassID"].ToString();
                                                    PayOrderSQL.InsertParameters["Amount"].DefaultValue = AmountTextBox.Text;
                                                    PayOrderSQL.InsertParameters["LateFee"].DefaultValue = LateFeeTextBox.Text;
                                                    PayOrderSQL.InsertParameters["StartDate"].DefaultValue = StartDateTextBox.Text;
                                                    PayOrderSQL.InsertParameters["EndDate"].DefaultValue = EndDateTextBox.Text;
                                                    PayOrderSQL.InsertParameters["Discount"].DefaultValue = DiscountTextBox.Text;
                                                    PayOrderSQL.InsertParameters["RoleID"].DefaultValue = RoleID.ToString();
                                                    PayOrderSQL.InsertParameters["PayFor"].DefaultValue = PayForTextBox.Text;

                                                    PayOrderSQL.Insert();
                                                    success_count++;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            fail_count++;
                                        }
                                    }
                                }
                            }
                        }
                        #endregion Multi_R_GridView
                    }
                }

                #endregion Class wise Students Pay Order
            }

            // Show success message using JavaScript
            string message = success_count.ToString() + " Pay Order has been created successfully";
            if (fail_count > 0)
            {
                message += " and " + fail_count.ToString() + " Pay Order already existed";
            }

            ScriptManager.RegisterStartupScript(this, this.GetType(), "PayOrderSuccess",
            $"alert('{message}'); window.location.href = window.location.href;", true);
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

        // For Assigned Multiple Instalments GridView
        protected void Multi_A_Role_GridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                GridView Input_Multi_Role_GridView = (GridView)e.Row.FindControl("Input_Multi_Role_GridView");
                HiddenField RoleHiddenField = (HiddenField)e.Row.FindControl("RoleHiddenField");

                if (Input_Multi_Role_GridView != null && RoleHiddenField != null)
                {
                    // Get ClassID from the SqlDataSource parameter
                    string classID = Multi_A_RoleSQL.SelectParameters["ClassID"].DefaultValue;

                    if (string.IsNullOrEmpty(classID) || classID == "0")
                    {
                        classID = ClassDropDownList.SelectedValue;
                    }

                    // Get the assigned roles for this RoleID
                    SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
                    SqlCommand cmd = new SqlCommand(@"SELECT Income_Roles.Role, Income_Assign_Role.AssignRoleID, Income_Assign_Role.RegistrationID, 
                Income_Assign_Role.SchoolID, Income_Assign_Role.ClassID, Income_Assign_Role.PayFor, 
     Income_Assign_Role.Amount, Income_Assign_Role.LateFee, Income_Assign_Role.StartDate, Income_Assign_Role.EndDate, 
         Income_Assign_Role.EducationYearID, Income_Assign_Role.RoleID
     FROM Income_Assign_Role INNER JOIN Income_Roles ON Income_Assign_Role.RoleID = Income_Roles.RoleID 
          WHERE (Income_Assign_Role.ClassID = @ClassID) AND (Income_Assign_Role.EducationYearID = @EducationYearID) 
      AND (Income_Assign_Role.SchoolID = @SchoolID) AND (Income_Assign_Role.RoleID = @RoleID) 
    ORDER BY Income_Assign_Role.StartDate", con);

                    cmd.Parameters.AddWithValue("@ClassID", classID);
                    cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"].ToString());
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                    cmd.Parameters.AddWithValue("@RoleID", RoleHiddenField.Value);

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    DataTable dt = new DataTable();
                    dt.Load(dr);
                    con.Close();

                    Input_Multi_Role_GridView.DataSource = dt;
                    Input_Multi_Role_GridView.DataBind();
                }
            }
        }

        protected void Find_ID_Button_Click(object sender, EventArgs e)
        {
            ClassDropDownList.SelectedValue = "0";
            StudentsGridView.DataBind();

         // Get ClassID from selected students to load assigned roles
    if (StudentsGridView.Rows.Count > 0)
        {
 int classID = Convert.ToInt32(StudentsGridView.DataKeys[0]["ClassID"]);

 // Set parameters for assigned roles SQL - MUST do this BEFORE DataBind()
   One_A_RoleSQL.SelectParameters["ClassID"].DefaultValue = classID.ToString();
                One_A_RoleSQL.SelectParameters["SchoolID"].DefaultValue = Session["SchoolID"].ToString();
     One_A_RoleSQL.SelectParameters["EducationYearID"].DefaultValue = Session["Edu_Year"].ToString();

      Multi_A_RoleSQL.SelectParameters["ClassID"].DefaultValue = classID.ToString();
                Multi_A_RoleSQL.SelectParameters["SchoolID"].DefaultValue = Session["SchoolID"].ToString();
      Multi_A_RoleSQL.SelectParameters["EducationYearID"].DefaultValue = Session["Edu_Year"].ToString();

   // Also bind unassigned roles for the same class
          Roles_1_SQL.SelectParameters["ClassID"].DefaultValue = classID.ToString();
      Multi_R_SQL.SelectParameters["ClassID"].DefaultValue = classID.ToString();

                // Now bind all GridViews
     One_A_RoleGridView.DataBind();
    Multi_A_Role_GridView.DataBind();
     One_Role_GridView.DataBind();
      Multi_R_GridView.DataBind();

   // Debug: Check database and GridView results
      SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());

           // Check single instalment roles
         SqlCommand oneCmd = new SqlCommand(@"
       SELECT COUNT(*) FROM Income_Assign_Role as I_Role 
       INNER JOIN Income_Roles ON I_Role.RoleID = Income_Roles.RoleID 
      WHERE I_Role.ClassID = @ClassID 
 AND I_Role.EducationYearID = @EducationYearID 
      AND I_Role.SchoolID = @SchoolID
    AND Income_Roles.NumberOfPay = 1", con);
         oneCmd.Parameters.AddWithValue("@ClassID", classID);
     oneCmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"].ToString());
      oneCmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());

       // Check multiple instalment roles
            SqlCommand multiCmd = new SqlCommand(@"
   SELECT COUNT(DISTINCT I_Role.RoleID) FROM Income_Assign_Role as I_Role 
       INNER JOIN Income_Roles ON I_Role.RoleID = Income_Roles.RoleID 
WHERE I_Role.ClassID = @ClassID 
      AND I_Role.EducationYearID = @EducationYearID 
  AND I_Role.SchoolID = @SchoolID
             AND Income_Roles.NumberOfPay > 1", con);
         multiCmd.Parameters.AddWithValue("@ClassID", classID);
        multiCmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"].ToString());
          multiCmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());

    con.Open();
   int oneSingleCount = Convert.ToInt32(oneCmd.ExecuteScalar());
         int multiCount = Convert.ToInt32(multiCmd.ExecuteScalar());
     con.Close();

                // Check and display sections based on data availability
     System.Text.StringBuilder sb = new System.Text.StringBuilder();
     sb.AppendLine("<script type='text/javascript'>");
     sb.AppendLine("$(document).ready(function() {");

      // Student Grid
           if (StudentsGridView.Rows.Count > 0)
        {
         sb.AppendLine("    $('.Hide_S_Gv').show();");
               sb.AppendLine($"    console.log('Students: {StudentsGridView.Rows.Count} rows');");
          }

       // Debug info
  sb.AppendLine($"    console.log('=== Database Query Results ===');");
      sb.AppendLine($"    console.log('ClassID: {classID}');");
           sb.AppendLine($"    console.log('SchoolID: {Session["SchoolID"]}');");
    sb.AppendLine($"    console.log('EducationYearID: {Session["Edu_Year"]}');");
          sb.AppendLine($"    console.log('Single Instalment Roles in DB: {oneSingleCount}');");
                sb.AppendLine($"    console.log('Multiple Instalment Roles in DB: {multiCount}');");
   sb.AppendLine($"    console.log('One_A_Role GridView Rows: {One_A_RoleGridView.Rows.Count}');");
sb.AppendLine($"    console.log('Multi_A_Role GridView Rows: {Multi_A_Role_GridView.Rows.Count}');");

   // One Assigned Role
       if (One_A_RoleGridView.Rows.Count > 0)
         {
 sb.AppendLine("    $('.A_R').show();");
    sb.AppendLine($"    console.log('✅ Showing One_A_Role section');");
      }
                else
     {
       sb.AppendLine($"    console.warn('❌ One_A_Role: GridView has no rows. DB has {oneSingleCount} single instalment roles.');");
    }

           // Multi Assigned Role
         if (Multi_A_Role_GridView.Rows.Count > 0)
{
           sb.AppendLine("    $('.A_MR').show();");
              sb.AppendLine($"    console.log('✅ Showing Multi_A_Role section');");
      }
      else
 {
  sb.AppendLine($"    console.warn('❌ Multi_A_Role: GridView has no rows. DB has {multiCount} multiple instalment roles.');");
        }

           // One Unassigned Role
     if (One_Role_GridView.Rows.Count > 0)
  {
   sb.AppendLine("  $('.UAR1').show();");
         sb.AppendLine($"    console.log('One_Role: {One_Role_GridView.Rows.Count} rows');");
          }

      // Multi Unassigned Role
   if (Multi_R_GridView.Rows.Count > 0)
     {
         sb.AppendLine("    $('.UAR2').show();");
           sb.AppendLine($"    console.log('Multi_R: {Multi_R_GridView.Rows.Count} rows');");
       }

      sb.AppendLine("});");
   sb.AppendLine("</script>");

                ClientScript.RegisterStartupScript(this.GetType(), "ShowGrids", sb.ToString(), false);
        }

      foreach (GridViewRow row in StudentsGridView.Rows)
       {
      CheckBox SingleCheckBox = row.FindControl("SingleCheckBox") as CheckBox;
          SingleCheckBox.Checked = true;
           row.CssClass = "selected";
            }
        }
        protected void ClassDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
          IDTextBox.Text = "";
       StudentsGridView.DataBind();

  // Load assigned roles when a specific class is selected
    if (ClassDropDownList.SelectedIndex > 1) // Not "Select" or "All Students"
    {
                int classID = Convert.ToInt32(ClassDropDownList.SelectedValue);

            // Set parameters for assigned roles SQL
       One_A_RoleSQL.SelectParameters["ClassID"].DefaultValue = classID.ToString();
    One_A_RoleSQL.SelectParameters["SchoolID"].DefaultValue = Session["SchoolID"].ToString();
             One_A_RoleSQL.SelectParameters["EducationYearID"].DefaultValue = Session["Edu_Year"].ToString();

    Multi_A_RoleSQL.SelectParameters["ClassID"].DefaultValue = classID.ToString();
   Multi_A_RoleSQL.SelectParameters["SchoolID"].DefaultValue = Session["SchoolID"].ToString();
     Multi_A_RoleSQL.SelectParameters["EducationYearID"].DefaultValue = Session["Edu_Year"].ToString();

        // Also bind unassigned roles for the same class
       Roles_1_SQL.SelectParameters["ClassID"].DefaultValue = classID.ToString();
    Multi_R_SQL.SelectParameters["ClassID"].DefaultValue = classID.ToString();

         // Now bind all GridViews
     One_A_RoleGridView.DataBind();
          Multi_A_Role_GridView.DataBind();
                One_Role_GridView.DataBind();
        Multi_R_GridView.DataBind();

// Show sections if they have data
      System.Text.StringBuilder sb = new System.Text.StringBuilder();
  sb.AppendLine("<script type='text/javascript'>");
         sb.AppendLine("$(document).ready(function() {");
  
    if (One_A_RoleGridView.Rows.Count > 0)
 {
           sb.AppendLine("    $('.A_R').show();");
           }
                
                if (Multi_A_Role_GridView.Rows.Count > 0)
         {
    sb.AppendLine("    $('.A_MR').show();");
      }
      
          if (One_Role_GridView.Rows.Count > 0)
       {
           sb.AppendLine("    $('.UAR1').show();");
    }
         
if (Multi_R_GridView.Rows.Count > 0)
           {
    sb.AppendLine("    $('.UAR2').show();");
             }

            sb.AppendLine("});");
      sb.AppendLine("</script>");

    ClientScript.RegisterStartupScript(this.GetType(), "ShowGridsOnClassChange", sb.ToString(), false);
   }
        }
        protected void GroupDropDownList_DataBound(object sender, EventArgs e)
        {
            GroupDropDownList.Items.Insert(0, new ListItem("[ ALL GROUP ]", "%"));
        }
        protected void SectionDropDownList_DataBound(object sender, EventArgs e)
        {
            SectionDropDownList.Items.Insert(0, new ListItem("[ ALL SECTION ]", "%"));
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

        protected void ShowStudentClassSQL_Selected(object sender, SqlDataSourceStatusEventArgs e)
        {
            TotalStudentLabel.Text = e.AffectedRows.ToString();
        }
    }
}