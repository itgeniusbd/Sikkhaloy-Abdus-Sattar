using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Teacher
{
    public partial class Input_Marks : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
        protected void Page_Load(object sender, EventArgs e)
        {
            Session["IGroup"] = Input_Group_DropDownList.SelectedValue;
            Session["IShift"] = Input_Shift_DropDownList.SelectedValue;
            Session["ISection"] = Input_Section_DropDownList.SelectedValue;
            Session["ISubject"] = SubjectDropDownList.SelectedValue;

            if (!IsPostBack)
            {
                Input_Group_DropDownList.Visible = false;
                Input_Section_DropDownList.Visible = false;
                Input_Shift_DropDownList.Visible = false;
            }
        }

        //Input marks
        protected void Input_view()
        {
            DataView GroupDV = new DataView();
            GroupDV = (DataView)Input_GroupSQL.Select(DataSourceSelectArguments.Empty);
            if (GroupDV.Count < 1)
            {
                Input_Group_DropDownList.Visible = false;
            }
            else
            {
                Input_Group_DropDownList.Visible = true;
            }

            DataView SectionDV = new DataView();
            SectionDV = (DataView)Input_SectionSQL.Select(DataSourceSelectArguments.Empty);
            if (SectionDV.Count < 1)
            {
                Input_Section_DropDownList.Visible = false;
            }
            else
            {
                Input_Section_DropDownList.Visible = true;
            }

            DataView ShiftDV = new DataView();
            ShiftDV = (DataView)Input_ShiftSQL.Select(DataSourceSelectArguments.Empty);
            if (ShiftDV.Count < 1)
            {
                Input_Shift_DropDownList.Visible = false;
            }
            else
            {
                Input_Shift_DropDownList.Visible = true;
            }
        }
        protected void Input_ExamDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session["IGroup"] = "%";
            Session["IShift"] = "%";
            Session["ISection"] = "%";
            Session["ISubject"] = "0";

            Input_Group_DropDownList.Visible = false;
            Input_Section_DropDownList.Visible = false;
            Input_Shift_DropDownList.Visible = false;
            SubExamDownList.Visible = false;
            SubExamRequired.Enabled = false;
        }

        protected void Input_Class_DropDownList_DataBound(object sender, EventArgs e)
        {
            Input_Class_DropDownList.Items.Insert(0, new ListItem("[ SELECT CLASS ]", "0"));
        }

        protected void Input_Class_DropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session["IGroup"] = "%";
            Session["IShift"] = "%";
            Session["ISection"] = "%";
            Session["ISubject"] = "0";

            Input_Group_DropDownList.DataBind();
            Input_Shift_DropDownList.DataBind();
            Input_Section_DropDownList.DataBind();
            SubjectDropDownList.DataBind();

            StudentsGridView.DataBind();
            Input_view();
        }

        protected void Input_Group_DropDownList_DataBound(object sender, EventArgs e)
        {
            Input_Group_DropDownList.Items.Insert(0, new ListItem("[ ALL GROUP ]", "%"));
            if (IsPostBack)
                Input_Group_DropDownList.Items.FindByValue(Session["IGroup"].ToString()).Selected = true;
        }

        protected void Input_Group_DropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            SubjectDropDownList.DataBind();
            Input_view();
            Session["ISubject"] = "0";
        }

        protected void Input_Section_DropDownList_DataBound(object sender, EventArgs e)
        {
            Input_Section_DropDownList.Items.Insert(0, new ListItem("[ ALL SECTION ]", "%"));
            if (IsPostBack)
                Input_Section_DropDownList.Items.FindByValue(Session["ISection"].ToString()).Selected = true;
        }

        protected void Input_Section_DropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Input_view();
        }

        protected void Input_Shift_DropDownList_DataBound(object sender, EventArgs e)
        {
            Input_Shift_DropDownList.Items.Insert(0, new ListItem("[ ALL SHIFT ]", "%"));
            if (IsPostBack)
                Input_Shift_DropDownList.Items.FindByValue(Session["IShift"].ToString()).Selected = true;
        }

        protected void Input_Shift_DropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Input_view();
        }

        //...Exam and subject
        protected void SubExamDownList_DataBound(object sender, EventArgs e)
        {
            SubExamDownList.Items.Insert(0, new ListItem("[ SELECT SUB-EXAM ]", ""));
        }
        protected void SubExamDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            PassMarkFullMarkSQL.SelectParameters["SubExamID"].DefaultValue = SubExamDownList.SelectedValue;
            StudentsGridView.DataBind();
        }

        protected void SubjectDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataView SubExamDV = new DataView();
            SubExamDV = (DataView)SubExamSQL.Select(DataSourceSelectArguments.Empty);
            if (SubExamDV.Count < 1)
            {
                PassMarkFullMarkSQL.SelectParameters["SubExamID"].DefaultValue = "0";
                SubExamDownList.Visible = false;
                SubExamRequired.Enabled = false;
            }
            else
            {
                PassMarkFullMarkSQL.SelectParameters["SubExamID"].DefaultValue = "";
                SubExamDownList.Visible = true;
                SubExamRequired.Enabled = true;
            }
            StudentsGridView.DataBind();
        }
        protected void SubjectDropDownList_DataBound(object sender, EventArgs e)
        {
            if (SubjectDropDownList.Items.FindByValue("0") == null)
                SubjectDropDownList.Items.Insert(0, new ListItem("[ SELECT SUBJECT ]", "0"));


            if (IsPostBack)
            {
                if (Session["ISubject"] != null)
                    SubjectDropDownList.Items.FindByValue(Session["ISubject"].ToString()).Selected = true;
            }
        }

        protected void StudentsGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                SqlCommand cmd = new SqlCommand("Select MarksObtained,AbsenceStatus from Exam_Obtain_Marks Where StudentClassID = @StudentClassID and SubjectID = @SubjectID and ExamID = @ExamID and (SubExamID = @SubExamID or SubExamID is null)", con);
                cmd.Parameters.AddWithValue("@StudentClassID", StudentsGridView.DataKeys[e.Row.RowIndex]["StudentClassID"].ToString());
                cmd.Parameters.AddWithValue("@SubjectID", SubjectDropDownList.SelectedValue);
                cmd.Parameters.AddWithValue("@ExamID", Input_ExamDropDownList.SelectedValue);
                cmd.Parameters.AddWithValue("@SubExamID", SubExamDownList.SelectedValue);

                con.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable t = new DataTable();
                da.Fill(t);
                con.Close();

                if (t.Rows.Count > 0)
                {
                    (e.Row.FindControl("MarksTextBox") as TextBox).Text = t.Rows[0]["MarksObtained"].ToString();

                    if (t.Rows[0]["AbsenceStatus"].ToString() == "Absent")
                    {
                        (e.Row.FindControl("AbsenceCheckBox") as CheckBox).Checked = true;
                        (e.Row.FindControl("MarksTextBox") as TextBox).Enabled = false;
                    }
                }
            }
        }

        protected void ShowStudentButton_Click(object sender, EventArgs e)
        {
            StudentsGridView.Visible = true;
            SubmitButton.Visible = true;
            FmPmFormView.Visible = true;
            StudentsGridView.DataBind();
        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            bool IS_FullMark = true;
            double FullMark = Convert.ToDouble(FmPmFormView.DataKey["FullMark"].ToString());
            double PassMark = Convert.ToDouble(FmPmFormView.DataKey["PassMark"].ToString());
            double PassPercentage = Convert.ToDouble(FmPmFormView.DataKey["PassPercentage"].ToString());

            foreach (GridViewRow row in StudentsGridView.Rows)
            {
                TextBox ObtainedMarks = (TextBox)row.FindControl("MarksTextBox");

                if (ObtainedMarks.Text.Trim() != "")
                {
                    if (FullMark < Convert.ToDouble(ObtainedMarks.Text.Trim()))
                    {
                        ObtainedMarks.ForeColor = System.Drawing.Color.Red;
                        IS_FullMark = false;
                    }
                }
            }

            if (IS_FullMark)
            {
                bool IsEmpty = true;
                foreach (GridViewRow row in StudentsGridView.Rows)
                {
                    TextBox ObtainedMarks = (TextBox)row.FindControl("MarksTextBox");
                    CheckBox AbsenceCheckBox = (CheckBox)row.FindControl("AbsenceCheckBox");

                    if (ObtainedMarks.Text.Trim() == "" && !AbsenceCheckBox.Checked)
                    {
                        IsEmpty = false;
                        row.CssClass = "EmptyRows";
                    }
                    else
                    {
                        row.CssClass = "";
                    }
                }

                if (IsEmpty)
                {
                    foreach (GridViewRow row in StudentsGridView.Rows)
                    {
                        TextBox ObtainedMarks = (TextBox)row.FindControl("MarksTextBox");
                        CheckBox AbsenceCheckBox = (CheckBox)row.FindControl("AbsenceCheckBox");

                        if (ObtainedMarks.Text.Trim() != "" || AbsenceCheckBox.Checked)
                        {
                            Session["ObtainedMarks"] = ObtainedMarks.Text.Trim();

                            if (AbsenceCheckBox.Checked)
                            {
                                Exam_Result_of_StudentSQL.InsertParameters["StudentID"].DefaultValue = StudentsGridView.DataKeys[row.RowIndex]["StudentID"].ToString();
                                Exam_Result_of_StudentSQL.InsertParameters["MarksObtained"].DefaultValue = "";

                                if (SubExamDownList.Visible)
                                {
                                    Exam_Result_of_StudentSQL.InsertParameters["SubExamID"].DefaultValue = SubExamDownList.SelectedValue;
                                }

                                Exam_Result_of_StudentSQL.InsertParameters["AbsenceStatus"].DefaultValue = "Absent";
                                Exam_Result_of_StudentSQL.InsertParameters["FullMark"].DefaultValue = FullMark.ToString();
                                Exam_Result_of_StudentSQL.InsertParameters["PassMark"].DefaultValue = PassMark.ToString();
                                Exam_Result_of_StudentSQL.InsertParameters["PassPercentage"].DefaultValue = PassPercentage.ToString();
                                Exam_Result_of_StudentSQL.Insert();
                            }
                            else
                            {
                                if (ObtainedMarks.Text.Trim() != "")
                                {
                                    Exam_Result_of_StudentSQL.InsertParameters["StudentID"].DefaultValue = StudentsGridView.DataKeys[row.RowIndex]["StudentID"].ToString();
                                    Exam_Result_of_StudentSQL.InsertParameters["MarksObtained"].DefaultValue = ObtainedMarks.Text.Trim();

                                    if (SubExamDownList.Visible)
                                    {
                                        Exam_Result_of_StudentSQL.InsertParameters["SubExamID"].DefaultValue = SubExamDownList.SelectedValue;
                                    }

                                    Exam_Result_of_StudentSQL.InsertParameters["AbsenceStatus"].DefaultValue = "Present";
                                    Exam_Result_of_StudentSQL.InsertParameters["FullMark"].DefaultValue = FullMark.ToString();
                                    Exam_Result_of_StudentSQL.InsertParameters["PassMark"].DefaultValue = PassMark.ToString();
                                    Exam_Result_of_StudentSQL.InsertParameters["PassPercentage"].DefaultValue = PassPercentage.ToString();
                                    Exam_Result_of_StudentSQL.Insert();
                                }
                            }
                        }
                    }
                }
                else
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "Msg", "ErrorM();", true);
                }

                ScriptManager.RegisterStartupScript(this, GetType(), "Msg", "Success();", true);
            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Obtained Mark greater than Full Mark')", true);
            }
        }

        protected void Input_Locked()
        {
            if (Session["SchoolID"] == null) return;

            SqlCommand cmd = new SqlCommand("SELECT Marks_Input_Locked FROM Exam_Publish_Setting WHERE(SchoolID = @SchoolID) AND(EducationYearID = @EducationYearID) AND(ClassID = @ClassID) AND(ExamID = @ExamID)", con);
            cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
            cmd.Parameters.AddWithValue("EducationYearID", Session["Edu_Year"].ToString());
            cmd.Parameters.AddWithValue("@ExamID", Input_ExamDropDownList.SelectedValue);
            cmd.Parameters.AddWithValue("@ClassID", Input_Class_DropDownList.SelectedValue);

            bool Input_Lock = false;
            con.Open();
            var exc = cmd.ExecuteScalar();
            con.Close();

            if (exc != null)
                Input_Lock = (bool)exc;

            foreach (GridViewRow row in StudentsGridView.Rows)
            {
                TextBox ObtainedMarks = (TextBox)row.FindControl("MarksTextBox");
                CheckBox AbsenceCheckBox = (CheckBox)row.FindControl("AbsenceCheckBox");

                row.Enabled = !Input_Lock;
            }

            SubmitButton.Enabled = !Input_Lock;
        }

        protected void StudentsGridView_DataBound(object sender, EventArgs e)
        {
            Input_Locked();
        }
    }
}