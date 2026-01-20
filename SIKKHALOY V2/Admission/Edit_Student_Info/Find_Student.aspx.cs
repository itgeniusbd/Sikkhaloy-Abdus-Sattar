using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Admission.Edit_Student_Info
{
    public partial class Find_Student : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Session["Group"] = GroupDropDownList.SelectedValue;
            Session["Shift"] = ShiftDropDownList.SelectedValue;
            Session["Section"] = SectionDropDownList.SelectedValue;

            if (!IsPostBack)
            {
                GroupDropDownList.Visible = false;
                SectionDropDownList.Visible = false;
                ShiftDropDownList.Visible = false;
            }

            ExamMultiView.ActiveViewIndex = CuOrExamRadioButtonList.SelectedIndex;
        }
        protected void view()
        {
            DataView GroupDV = new DataView();
            GroupDV = (DataView)GroupSQL.Select(DataSourceSelectArguments.Empty);
            if (GroupDV.Count < 1)
            {
                GroupDropDownList.Visible = false;
            }
            else
            {
                GroupDropDownList.Visible = true;
            }

            DataView SectionDV = new DataView();
            SectionDV = (DataView)SectionSQL.Select(DataSourceSelectArguments.Empty);
            if (SectionDV.Count < 1)
            {
                SectionDropDownList.Visible = false;
            }
            else
            {
                SectionDropDownList.Visible = true;
            }

            DataView ShiftDV = new DataView();
            ShiftDV = (DataView)ShiftSQL.Select(DataSourceSelectArguments.Empty);
            if (ShiftDV.Count < 1)
            {
                ShiftDropDownList.Visible = false;
            }
            else
            {
                ShiftDropDownList.Visible = true;
            }

            IDTextBox.Text = "";

            string name = "";

            if (ClassDropDownList.SelectedIndex != 0)
            {
                name += " Class: " + ClassDropDownList.SelectedItem.Text;
            }
            if (SectionDropDownList.SelectedIndex != 0)
            {
                name += ", Section: " + SectionDropDownList.SelectedItem.Text;
            }
            if (GroupDropDownList.SelectedIndex != 0)
            {
                name += ", Group: " + GroupDropDownList.SelectedItem.Text;
            }
            if (ShiftDropDownList.SelectedIndex != 0)
            {
                name += ", Shift: " + ShiftDropDownList.SelectedItem.Text;
            }
            CGSSLabel.Text = name;
        }
        protected void ClassDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session["Group"] = "%";
            Session["Shift"] = "%";
            Session["Section"] = "%";

            GroupDropDownList.DataBind();
            ShiftDropDownList.DataBind();
            SectionDropDownList.DataBind();
            view();
        }

        protected void GroupDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
        }

        protected void GroupDropDownList_DataBound(object sender, EventArgs e)
        {
            GroupDropDownList.Items.Insert(0, new ListItem("[ ALL GROUP ]", "%"));
            if (IsPostBack)
                GroupDropDownList.Items.FindByValue(Session["Group"].ToString()).Selected = true;
        }
        protected void SectionDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
        }

        protected void SectionDropDownList_DataBound(object sender, EventArgs e)
        {
            SectionDropDownList.Items.Insert(0, new ListItem("[ ALL SECTION ]", "%"));
            if (IsPostBack)
                SectionDropDownList.Items.FindByValue(Session["Section"].ToString()).Selected = true;
        }

        protected void ShiftDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
        }

        protected void ShiftDropDownList_DataBound(object sender, EventArgs e)
        {
            ShiftDropDownList.Items.Insert(0, new ListItem("[ ALL SHIFT ]", "%"));
            if (IsPostBack)
                ShiftDropDownList.Items.FindByValue(Session["Shift"].ToString()).Selected = true;
        }//End DDL


        protected void IDFindButton_Click(object sender, EventArgs e)
        {
            DataView ShowIDDV = new DataView();
            ShowIDDV = (DataView)ShowIDSQL.Select(DataSourceSelectArguments.Empty);
            if (ShowIDDV.Count < 1)
            {
                ClassDropDownList.SelectedIndex = 0;

                GroupDropDownList.Visible = false;
                SectionDropDownList.Visible = false;
                ShiftDropDownList.Visible = false;
            }
            else
            {
                Response.Redirect("Edit_Student_information.aspx?Student=" + ShowIDDV[0]["StudentID"].ToString() + "&Student_Class=" + ShowIDDV[0]["StudentClassID"].ToString());
            }
        }

        //Set Roll No
        protected void ExamDropDownList_DataBound(object sender, EventArgs e)
        {
            ExamDropDownList.Items.Insert(0, new ListItem("[ SELECT EXAM ]", "0"));
        }
        protected void Cu_ExamDropDownList_DataBound(object sender, EventArgs e)
        {
            Cu_ExamDropDownList.Items.Insert(0, new ListItem("[ SELECT EXAM ]", "0"));
        }
        protected void ShowPositionButton_Click(object sender, EventArgs e)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
            string StudentID = "";

            TextBox Merit_StatusTextBox = new TextBox();
            foreach (GridViewRow Row in RollNoGridView.Rows)
            {

                StudentID = RollNoGridView.DataKeys[Row.DataItemIndex]["StudentID"].ToString();
                Merit_StatusTextBox = Row.FindControl("RollTextBox") as TextBox;

                if (CuOrExamRadioButtonList.SelectedIndex == 0)
                {
                    if (Class_Sec_RadioButtonList.SelectedIndex == 0)
                    {
                        SqlCommand Cu_Position_Class_com = new SqlCommand("SELECT Position_InExam_Class FROM  Exam_Cumulative_Student WHERE (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (EducationYearID = @EducationYearID) AND (CumulativeNameID = @CumulativeNameID) AND (StudentID = @StudentID)", con);
                        Cu_Position_Class_com.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                        Cu_Position_Class_com.Parameters.AddWithValue("@EducationYearID", EduYearDropDownList.SelectedValue);
                        Cu_Position_Class_com.Parameters.AddWithValue("@ClassID", Roll_Class_DropDownList.SelectedValue);
                        Cu_Position_Class_com.Parameters.AddWithValue("@CumulativeNameID", Cu_ExamDropDownList.SelectedValue);
                        Cu_Position_Class_com.Parameters.AddWithValue("@StudentID", StudentID);

                        con.Open();
                        object Cu_Position_Class_Check = Cu_Position_Class_com.ExecuteScalar();
                        con.Close();

                        if (Cu_Position_Class_Check != null)
                        {
                            Merit_StatusTextBox.Text = Cu_Position_Class_Check.ToString();
                        }
                        else
                        {
                            Merit_StatusTextBox.Text = "";
                        }
                    }
                    else
                    {
                        SqlCommand Cu_Position_Subsection_com = new SqlCommand("SELECT Position_InExam_Subsection FROM  Exam_Cumulative_Student WHERE (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (EducationYearID = @EducationYearID) AND (CumulativeNameID = @CumulativeNameID) AND (StudentID = @StudentID)", con);
                        Cu_Position_Subsection_com.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                        Cu_Position_Subsection_com.Parameters.AddWithValue("@EducationYearID", EduYearDropDownList.SelectedValue);
                        Cu_Position_Subsection_com.Parameters.AddWithValue("@ClassID", Roll_Class_DropDownList.SelectedValue);
                        Cu_Position_Subsection_com.Parameters.AddWithValue("@CumulativeNameID", Cu_ExamDropDownList.SelectedValue);
                        Cu_Position_Subsection_com.Parameters.AddWithValue("@StudentID", StudentID);

                        con.Open();
                        object Cu_Position_Subsection_Check = Cu_Position_Subsection_com.ExecuteScalar();
                        con.Close();

                        if (Cu_Position_Subsection_Check != null)
                        {
                            Merit_StatusTextBox.Text = Cu_Position_Subsection_Check.ToString();
                        }
                        else
                        {
                            Merit_StatusTextBox.Text = "";
                        }
                    }
                }
                else
                {
                    if (Class_Sec_RadioButtonList.SelectedIndex == 0)
                    {
                        SqlCommand Exam_Position_Class_com = new SqlCommand("SELECT Position_InExam_Class FROM Exam_Result_of_Student WHERE (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (EducationYearID = @EducationYearID) AND (StudentID = @StudentID) AND (ExamID = @ExamID)", con);
                        Exam_Position_Class_com.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                        Exam_Position_Class_com.Parameters.AddWithValue("@EducationYearID", EduYearDropDownList.SelectedValue);
                        Exam_Position_Class_com.Parameters.AddWithValue("@ClassID", Roll_Class_DropDownList.SelectedValue);
                        Exam_Position_Class_com.Parameters.AddWithValue("@ExamID", ExamDropDownList.SelectedValue);
                        Exam_Position_Class_com.Parameters.AddWithValue("@StudentID", StudentID);

                        con.Open();
                        object Exam_Position_Class_Check = Exam_Position_Class_com.ExecuteScalar();
                        con.Close();

                        if (Exam_Position_Class_Check != null)
                        {
                            Merit_StatusTextBox.Text = Exam_Position_Class_Check.ToString();
                        }
                        else
                        {
                            Merit_StatusTextBox.Text = "";
                        }
                    }
                    else
                    {
                        SqlCommand Exam_Position_Subsection_com = new SqlCommand("SELECT Position_InExam_Subsection FROM Exam_Result_of_Student WHERE (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (EducationYearID = @EducationYearID) AND (StudentID = @StudentID) AND (ExamID = @ExamID)", con);
                        Exam_Position_Subsection_com.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                        Exam_Position_Subsection_com.Parameters.AddWithValue("@EducationYearID", EduYearDropDownList.SelectedValue);
                        Exam_Position_Subsection_com.Parameters.AddWithValue("@ClassID", Roll_Class_DropDownList.SelectedValue);
                        Exam_Position_Subsection_com.Parameters.AddWithValue("@ExamID", ExamDropDownList.SelectedValue);
                        Exam_Position_Subsection_com.Parameters.AddWithValue("@StudentID", StudentID);

                        con.Open();
                        object Exam_Position_Subsection_Check = Exam_Position_Subsection_com.ExecuteScalar();
                        con.Close();

                        if (Exam_Position_Subsection_Check != null)
                        {
                            Merit_StatusTextBox.Text = Exam_Position_Subsection_Check.ToString();
                        }
                        else
                        {
                            Merit_StatusTextBox.Text = "";
                        }

                    }
                }
            }
        }

        //Update Roll
        protected void UpdateRollButton_Click(object sender, EventArgs e)
        {
            foreach (GridViewRow rows in RollNoGridView.Rows)
            {
                TextBox RollTextBox = (TextBox)rows.FindControl("RollTextBox");
                TextBox SeatTextBox = (TextBox)rows.FindControl("SeatTextBox");
                if (RollTextBox.Text.Trim() != "" || SeatTextBox.Text.Trim() != "")
                {
                    ShowStudentClassSQL.UpdateParameters["StudentClassID"].DefaultValue = RollNoGridView.DataKeys[rows.DataItemIndex]["StudentClassID"].ToString();
                    ShowStudentClassSQL.UpdateParameters["RollNo"].DefaultValue = RollTextBox.Text;
                    ShowStudentClassSQL.UpdateParameters["SeatNo"].DefaultValue = SeatTextBox.Text;
                    ShowStudentClassSQL.Update();
                }
            }

            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Updated Successfully!!')", true);
        }

        //Update Image
        [WebMethod]
        public static void UpdateImage(string StudentImageID, string Image)
        {
            string constr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand("UPDATE Student_Image SET Image = CAST(N'' AS xml).value('xs:base64Binary(sql:variable(\"@Image\"))', 'varbinary(max)') Where StudentImageID = @StudentImageID"))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@StudentImageID", StudentImageID);
                    cmd.Parameters.AddWithValue("@Image", Image);
                    cmd.Connection = con;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
        }
    }
}