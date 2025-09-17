using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Admission.Re_Admission
{
    public partial class Multiple_Re_Admission : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Session["Group"] = GroupDropDownList.SelectedValue;
            Session["Shift"] = ShiftDropDownList.SelectedValue;
            Session["Section"] = SectionDropDownList.SelectedValue;

            Session["Re_Group"] = Re_GroupDropDownList.SelectedValue;
            Session["Re_Shift"] = Re_ShiftDropDownList.SelectedValue;
            Session["Re_Section"] = Re_SectionDropDownList.SelectedValue;

            if (!IsPostBack)
            {
                GroupDropDownList.Visible = false;
                SectionDropDownList.Visible = false;
                ShiftDropDownList.Visible = false;

                Re_GroupDropDownList.Visible = false;
                Re_SectionDropDownList.Visible = false;
                Re_ShiftDropDownList.Visible = false;
            }

            ExamMultiView.ActiveViewIndex = CuOrExamRadioButtonList.SelectedIndex;
        }

        //Old Session
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
        }

        //--Class DDL--
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

        //--Group DDL--
        protected void GroupDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
        }

        protected void GroupDropDownList_DataBound(object sender, EventArgs e)
        {
            GroupDropDownList.Items.Insert(0, new ListItem("[ SELECT GROUP ]", "%"));
            if (IsPostBack)
                GroupDropDownList.Items.FindByValue(Session["Group"].ToString()).Selected = true;
        }

        //--Section DDL--
        protected void SectionDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
        }

        protected void SectionDropDownList_DataBound(object sender, EventArgs e)
        {
            SectionDropDownList.Items.Insert(0, new ListItem("[ SELECT SECTION ]", "%"));
            if (IsPostBack)
                SectionDropDownList.Items.FindByValue(Session["Section"].ToString()).Selected = true;
        }

        //--Shift DDL--
        protected void ShiftDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
        }

        protected void ShiftDropDownList_DataBound(object sender, EventArgs e)
        {
            ShiftDropDownList.Items.Insert(0, new ListItem("[ SELECT SHIFT ]", "%"));
            if (IsPostBack)
                ShiftDropDownList.Items.FindByValue(Session["Shift"].ToString()).Selected = true;
        }


        //NewSession
        protected void Re_view()
        {
            DataView GroupDV = new DataView();
            GroupDV = (DataView)Re_GroupSQL.Select(DataSourceSelectArguments.Empty);
            if (GroupDV.Count < 1)
            {
                Re_GroupDropDownList.Visible = false;
            }
            else
            {
                Re_GroupDropDownList.Visible = true;
            }

            DataView SectionDV = new DataView();
            SectionDV = (DataView)Re_SectionSQL.Select(DataSourceSelectArguments.Empty);
            if (SectionDV.Count < 1)
            {
                Re_SectionDropDownList.Visible = false;
            }
            else
            {
                Re_SectionDropDownList.Visible = true;
            }

            DataView ShiftDV = new DataView();
            ShiftDV = (DataView)Re_ShiftSQL.Select(DataSourceSelectArguments.Empty);
            if (ShiftDV.Count < 1)
            {
                Re_ShiftDropDownList.Visible = false;
            }
            else
            {
                Re_ShiftDropDownList.Visible = true;
            }
        }

        //--Class DDL--
        protected void Re_ClassDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session["Re_Group"] = "%";
            Session["Re_Shift"] = "%";
            Session["Re_Section"] = "%";

            Re_GroupDropDownList.DataBind();
            Re_ShiftDropDownList.DataBind();
            Re_SectionDropDownList.DataBind();
            Re_view();
        }

        //--Group DDL--
        protected void Re_GroupDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Re_view();
        }

        protected void Re_GroupDropDownList_DataBound(object sender, EventArgs e)
        {
            Re_GroupDropDownList.Items.Insert(0, new ListItem("[ SELECT GROUP ]", "%"));
            if (IsPostBack)
                Re_GroupDropDownList.Items.FindByValue(Session["Re_Group"].ToString()).Selected = true;
        }

        //--Section DDL--
        protected void Re_SectionDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Re_view();
        }

        protected void Re_SectionDropDownList_DataBound(object sender, EventArgs e)
        {
            Re_SectionDropDownList.Items.Insert(0, new ListItem("[ SELECT SECTION ]", "%"));
            if (IsPostBack)
                Re_SectionDropDownList.Items.FindByValue(Session["Re_Section"].ToString()).Selected = true;
        }

        //--Shift DDL--
        protected void Re_ShiftDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Re_view();
        }

        protected void Re_ShiftDropDownList_DataBound(object sender, EventArgs e)
        {
            Re_ShiftDropDownList.Items.Insert(0, new ListItem("[ SELECT SHIFT ]", "%"));
            if (IsPostBack)
                Re_ShiftDropDownList.Items.FindByValue(Session["Re_Shift"].ToString()).Selected = true;
        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());

            string StudentID = "";
            string StudentClassID = "";

            TextBox Merit_StatusTextBox = new TextBox();
            CheckBox SingleCheckBox = new CheckBox();
            CheckBox SubjectCheckBox = new CheckBox();
            RadioButtonList SubjectType = new RadioButtonList();

            foreach (GridViewRow Row in StudentsGridView.Rows)
            {
                SingleCheckBox = Row.FindControl("SingleCheckBox") as CheckBox;

                if (SingleCheckBox.Checked)
                {
                    StudentID = StudentsGridView.DataKeys[Row.DataItemIndex]["StudentID"].ToString();

                    SqlCommand StudnetIDcommand = new SqlCommand("SELECT StudentClassID FROM StudentsClass WHERE (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID) AND (StudentID = @StudentID)", con);
                    StudnetIDcommand.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                    StudnetIDcommand.Parameters.AddWithValue("@EducationYearID", New_Session_DropDownList.SelectedValue);
                    StudnetIDcommand.Parameters.AddWithValue("@StudentID", StudentID);

                    con.Open();
                    object StudentID_Check = StudnetIDcommand.ExecuteScalar();
                    con.Close();

                    if (StudentID_Check == null)
                    {
                        ErrorLabel.Text = "";
                        Merit_StatusTextBox = Row.FindControl("Merit_StatusTextBox") as TextBox;

                        #region Class-group-section-shift dropdownlist

                        if (Re_GroupDropDownList.SelectedValue == "%")
                        {
                            StudentClassSQL.InsertParameters["SubjectGroupID"].DefaultValue = "0";
                        }
                        else
                        {
                            StudentClassSQL.InsertParameters["SubjectGroupID"].DefaultValue = Re_GroupDropDownList.SelectedValue;
                        }

                        if (Re_SectionDropDownList.SelectedValue == "%")
                        {
                            StudentClassSQL.InsertParameters["SectionID"].DefaultValue = "0";
                        }
                        else
                        {
                            StudentClassSQL.InsertParameters["SectionID"].DefaultValue = Re_SectionDropDownList.SelectedValue;
                        }

                        if (Re_ShiftDropDownList.SelectedValue == "%")
                        {
                            StudentClassSQL.InsertParameters["ShiftID"].DefaultValue = "0";
                        }
                        else
                        {
                            StudentClassSQL.InsertParameters["ShiftID"].DefaultValue = Re_ShiftDropDownList.SelectedValue;
                        }
                        #endregion

                        StudentClassSQL.InsertParameters["StudentID"].DefaultValue = StudentID;
                        StudentClassSQL.InsertParameters["RollNo"].DefaultValue = Merit_StatusTextBox.Text.Trim();
                        StudentClassSQL.Insert();

                        SqlCommand cmd = new SqlCommand("Select IDENT_CURRENT('StudentsClass')", con);

                        con.Open();
                        StudentClassID = cmd.ExecuteScalar().ToString();
                        con.Close();

                        foreach (GridViewRow row in GroupGridView.Rows) //insert subject
                        {
                            SubjectCheckBox = (CheckBox)row.FindControl("SubjectCheckBox");
                            SubjectType = (RadioButtonList)row.FindControl("SubjectTypeRadioButtonList");

                            if (SubjectCheckBox.Checked)
                            {
                                StudentRecordSQL.InsertParameters["StudentID"].DefaultValue = StudentID;
                                StudentRecordSQL.InsertParameters["SubjectID"].DefaultValue = GroupGridView.DataKeys[row.DataItemIndex]["SubjectID"].ToString();
                                StudentRecordSQL.InsertParameters["StudentClassID"].DefaultValue = StudentClassID;
                                StudentRecordSQL.InsertParameters["SubjectType"].DefaultValue = SubjectType.SelectedValue;
                                StudentRecordSQL.Insert();
                            }
                        }

                        StudentClassSQL.UpdateParameters["StudentClassID"].DefaultValue = StudentsGridView.DataKeys[Row.DataItemIndex]["StudentClassID"].ToString();
                        StudentClassSQL.Update();
                    }
                    else
                    {
                        ErrorLabel.Text = "Same session re-admission not allow!";
                    }
                }
            }
            Response.Redirect(Request.Url.AbsoluteUri);
        }

        protected void ShowPositionButton_Click(object sender, EventArgs e)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
            string StudentID = "";
            string StudentClassID = "";

            TextBox Merit_StatusTextBox = new TextBox();
            foreach (GridViewRow Row in StudentsGridView.Rows)
            {
                StudentID = StudentsGridView.DataKeys[Row.DataItemIndex]["StudentID"].ToString();
                StudentClassID = StudentsGridView.DataKeys[Row.DataItemIndex]["StudentClassID"].ToString();
                Merit_StatusTextBox = Row.FindControl("Merit_StatusTextBox") as TextBox;

                if (CuOrExamRadioButtonList.SelectedIndex == 0)
                {
                    if (Class_Sec_RadioButtonList.SelectedIndex == 0)
                    {
                        SqlCommand Cu_Position_Class_com = new SqlCommand("SELECT Position_InExam_Class FROM  Exam_Cumulative_Student WHERE (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (EducationYearID = @EducationYearID) AND (CumulativeNameID = @CumulativeNameID) AND (StudentID = @StudentID) AND (StudentClassID = @StudentClassID)", con);
                        Cu_Position_Class_com.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                        Cu_Position_Class_com.Parameters.AddWithValue("@EducationYearID", SessionYearDropDownList.SelectedValue);
                        Cu_Position_Class_com.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);
                        Cu_Position_Class_com.Parameters.AddWithValue("@CumulativeNameID", Cu_ExamDropDownList.SelectedValue);
                        Cu_Position_Class_com.Parameters.AddWithValue("@StudentID", StudentID);
                        Cu_Position_Class_com.Parameters.AddWithValue("@StudentClassID", StudentClassID);

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
                        SqlCommand Cu_Position_Subsection_com = new SqlCommand("SELECT Position_InExam_Subsection FROM  Exam_Cumulative_Student WHERE (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (EducationYearID = @EducationYearID) AND (CumulativeNameID = @CumulativeNameID) AND (StudentID = @StudentID) AND (StudentClassID = @StudentClassID)", con);
                        Cu_Position_Subsection_com.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                        Cu_Position_Subsection_com.Parameters.AddWithValue("@EducationYearID", SessionYearDropDownList.SelectedValue);
                        Cu_Position_Subsection_com.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);
                        Cu_Position_Subsection_com.Parameters.AddWithValue("@CumulativeNameID", Cu_ExamDropDownList.SelectedValue);
                        Cu_Position_Subsection_com.Parameters.AddWithValue("@StudentID", StudentID);
                        Cu_Position_Subsection_com.Parameters.AddWithValue("@StudentClassID", StudentClassID);

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
                        SqlCommand Exam_Position_Class_com = new SqlCommand("SELECT Position_InExam_Class FROM Exam_Result_of_Student WHERE (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (EducationYearID = @EducationYearID) AND (StudentID = @StudentID) AND (StudentClassID = @StudentClassID) AND (ExamID = @ExamID)", con);
                        Exam_Position_Class_com.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                        Exam_Position_Class_com.Parameters.AddWithValue("@EducationYearID", SessionYearDropDownList.SelectedValue);
                        Exam_Position_Class_com.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);
                        Exam_Position_Class_com.Parameters.AddWithValue("@ExamID", ExamDropDownList.SelectedValue);
                        Exam_Position_Class_com.Parameters.AddWithValue("@StudentID", StudentID);
                        Exam_Position_Class_com.Parameters.AddWithValue("@StudentClassID", StudentClassID);

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
                        SqlCommand Exam_Position_Subsection_com = new SqlCommand("SELECT Position_InExam_Subsection FROM Exam_Result_of_Student WHERE (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (EducationYearID = @EducationYearID) AND (StudentID = @StudentID) AND (StudentClassID = @StudentClassID) AND (ExamID = @ExamID)", con);
                        Exam_Position_Subsection_com.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                        Exam_Position_Subsection_com.Parameters.AddWithValue("@EducationYearID", SessionYearDropDownList.SelectedValue);
                        Exam_Position_Subsection_com.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);
                        Exam_Position_Subsection_com.Parameters.AddWithValue("@ExamID", ExamDropDownList.SelectedValue);
                        Exam_Position_Subsection_com.Parameters.AddWithValue("@StudentID", StudentID);
                        Exam_Position_Subsection_com.Parameters.AddWithValue("@StudentClassID", StudentClassID);

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

        protected void ExamDropDownList_DataBound(object sender, EventArgs e)
        {
            ExamDropDownList.Items.Insert(0, new ListItem("[ SELECT EXAM ]", "0"));
        }

        protected void Cu_ExamDropDownList_DataBound(object sender, EventArgs e)
        {
            Cu_ExamDropDownList.Items.Insert(0, new ListItem("[ SELECT EXAM ]", "0"));
        }

        protected void New_Session_DropDownList_DataBound(object sender, EventArgs e)
        {
            New_Session_DropDownList.Items.Insert(0, new ListItem("[NEW SESSION]", "0"));
        }
    }
}