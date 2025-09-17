using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Exam.CumulativeResult
{
    public partial class Cumulative_Setting : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
        protected void Page_Load(object sender, EventArgs e)
        {


        }
        protected void AddCumulativeButton_Click(object sender, EventArgs e)
        {
            CumulativeNameSQL.Insert();
            CumulativeNameTextBox.Text = "";
            Response.Redirect(Request.Url.AbsoluteUri);
        }
        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            #region Exam_Publish_SettingSQL

            Exam_Publish_SettingSQL.Delete();
            if (OptionalSubjectRadioButtonList.SelectedIndex == 0)
            {
                Exam_Publish_SettingSQL.InsertParameters["Optional_Percentage_Deduction"].DefaultValue = "0";
            }

            if (OptionalSubjectRadioButtonList.SelectedIndex == 1)
            {
                Exam_Publish_SettingSQL.InsertParameters["Optional_Percentage_Deduction"].DefaultValue = MinPercentageTextBox.Text;
            }
            if (OptionalSubjectRadioButtonList.SelectedIndex == 2)
            {
                Exam_Publish_SettingSQL.InsertParameters["Optional_Percentage_Deduction"].DefaultValue = "100";
            }

            Exam_Publish_SettingSQL.Insert();

            #endregion


            #region ExamListSQL
            ExamListSQL.Delete();
            foreach (DataListItem item in ExamListDataList.Items)
            {
                CheckBox ExamCheckBox = (CheckBox)item.FindControl("ExamCheckBox");
                CheckBox EnableFailCheckBox = (CheckBox)item.FindControl("EnableFailCheckBox");
                TextBox PercentageTextBox = (TextBox)item.FindControl("PercentageTextBox");
                HiddenField ExamID_HF = (HiddenField)item.FindControl("ExamID_HF");
                if (ExamCheckBox.Checked)
                {
                    ExamListSQL.InsertParameters["ExamID"].DefaultValue = ExamID_HF.Value;
                    ExamListSQL.InsertParameters["ExamAdd_Percentage"].DefaultValue = PercentageTextBox.Text;
                    ExamListSQL.InsertParameters["Exam_EnableFail"].DefaultValue = EnableFailCheckBox.Checked.ToString();
                    ExamListSQL.InsertParameters["Cumulative_SettingID"].DefaultValue = ViewState["Cumulative_SettingID"].ToString();
                    ExamListSQL.Insert();
                }
            }
            #endregion


            #region DifferentMarksSQL

            DifferentMarksSQL.Delete();

            if (ExamRadioButtonList.SelectedIndex == 0)
            {
                foreach (GridViewRow row in DifferntMarksGridView.Rows)
                {
                    HiddenField SubID_HF = (HiddenField)row.FindControl("SubID_HF");
                    DifferentMarksSQL.InsertParameters["SubjectID"].DefaultValue = SubID_HF.Value;
                    DifferentMarksSQL.InsertParameters["FullMarks"].DefaultValue = AlTotalMarksTextBox.Text;
                    DifferentMarksSQL.InsertParameters["Cumulative_SettingID"].DefaultValue = ViewState["Cumulative_SettingID"].ToString();
                    DifferentMarksSQL.Insert();

                }
            }


            if (ExamRadioButtonList.SelectedIndex == 1)
            {
                foreach (GridViewRow row in DifferntMarksGridView.Rows)
                {
                    TextBox HMforExamTextBox = (TextBox)row.FindControl("HMforExamTextBox");
                    HiddenField SubID_HF = (HiddenField)row.FindControl("SubID_HF");
                    DifferentMarksSQL.InsertParameters["SubjectID"].DefaultValue = SubID_HF.Value;
                    DifferentMarksSQL.InsertParameters["FullMarks"].DefaultValue = HMforExamTextBox.Text;
                    DifferentMarksSQL.InsertParameters["Cumulative_SettingID"].DefaultValue = ViewState["Cumulative_SettingID"].ToString();
                    DifferentMarksSQL.Insert();
                }
            }

            #endregion


            //---------SP SP_Cumulative_Exam_Subject---------------------
            Sub_Add_ExamSQL.InsertParameters["Cumulative_SettingID"].DefaultValue = ViewState["Cumulative_SettingID"].ToString();
            Sub_Add_ExamSQL.Insert();


            if (ExamRadioButtonList.SelectedIndex == 0)
            {
                foreach (GridViewRow row in DifferntMarksGridView.Rows)
                {
                    HiddenField SubID_HF = (HiddenField)row.FindControl("SubID_HF");

                    Sub_Add_ExamSQL.UpdateParameters["IS_Add_InExam"].DefaultValue = "1";
                    Sub_Add_ExamSQL.UpdateParameters["SubjectID"].DefaultValue = SubID_HF.Value;
                    Sub_Add_ExamSQL.Update();
                }
            }


            if (ExamRadioButtonList.SelectedIndex == 1)
            {
                foreach (GridViewRow row in DifferntMarksGridView.Rows)
                {
                    HiddenField SubID_HF = (HiddenField)row.FindControl("SubID_HF");


                    CheckBox IS_Add_ExamCheckBox = (CheckBox)row.FindControl("IS_Add_ExamCheckBox");

                    Sub_Add_ExamSQL.UpdateParameters["IS_Add_InExam"].DefaultValue = IS_Add_ExamCheckBox.Checked ? "1" : "0";
                    Sub_Add_ExamSQL.UpdateParameters["SubjectID"].DefaultValue = SubID_HF.Value;
                    Sub_Add_ExamSQL.Update();
                }
            }


            //---------SP SP_Cumulative_Exam_Student---------------------
            Exam_PS_SQL.InsertParameters["Cumulative_SettingID"].DefaultValue = ViewState["Cumulative_SettingID"].ToString();
            Exam_PS_SQL.Insert();

            DifferntMarksGridView.DataBind();
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Result has been published successfully!!')", true);
        }
        protected void ExamDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            DifferntMarksGridView.DataBind();
            ShowPanel();
            ExamList();
            SubjectList();
        }
        protected void ClassDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ExamDropDownList.SelectedIndex = 0;
            ShowPanel();
        }
        protected void ShowPanel()
        {
            DataView ResetExamDV = new DataView();
            ResetExamDV = (DataView)ResetExamMarkSQL.Select(DataSourceSelectArguments.Empty);
            if (ResetExamDV.Count != 0)
            {
                if (ResetExamDV.Count > 1)
                {
                    ExamRadioButtonList.SelectedIndex = 1;
                }
                else
                {
                    ExamRadioButtonList.SelectedIndex = 0;
                    AlTotalMarksTextBox.Text = ResetExamDV[0]["FullMarks"].ToString();
                }
            }
            EMMultiView.ActiveViewIndex = ExamRadioButtonList.SelectedIndex;



            DataView ResetDv = new DataView();
            ResetDv = (DataView)Exam_Publish_SettingSQL.Select(DataSourceSelectArguments.Empty);

            if (ResetDv.Count == 1)
            {
                if (ResetDv[0]["Optional_Percentage_Deduction"].ToString() == "0")
                {
                    OptionalSubjectRadioButtonList.SelectedIndex = 0;

                }
                else if (ResetDv[0]["Optional_Percentage_Deduction"].ToString() == "100")
                {
                    OptionalSubjectRadioButtonList.SelectedIndex = 2;

                }
                else if (ResetDv[0]["Optional_Percentage_Deduction"].ToString() == "")
                {
                    OptionalSubjectRadioButtonList.SelectedIndex = 2;

                }
                else
                {
                    OptionalSubjectRadioButtonList.SelectedIndex = 1;
                    MinPercentageTextBox.Text = ResetDv[0]["Optional_Percentage_Deduction"].ToString();

                }


                if (OptionalSubjectRadioButtonList.SelectedIndex == 1)
                {
                    SpecificMarkPanel.Visible = true;
                    OptionalPercentageRequired.Enabled = true;
                    OptionalRegularExpressionValidator.Enabled = true;
                }
                else
                {
                    SpecificMarkPanel.Visible = false;
                    OptionalPercentageRequired.Enabled = false;
                    OptionalRegularExpressionValidator.Enabled = false;
                }

                //-----------Fail Enable In Optional CheckBox       
                FailEnableInOptionalCheckBox.Checked = Convert.ToBoolean(ResetDv[0]["IS_Fail_Enable_Optional_Subject"]);


                //-----------Add Optional Mark in Total Mark CheckBox
                AddOptionalinTotalMarkCheckBox.Checked = Convert.ToBoolean(ResetDv[0]["IS_Add_Optional_Mark_In_FullMarks"]);

                //----------------------Grade Setting
                GradeSetting_RBList.SelectedValue = ResetDv[0]["IS_Grade_BasePoint"].ToString();

                //-----------Grade AS It is CheckBox
                Grade_AS_ItisCheckBox.Checked = Convert.ToBoolean(ResetDv[0]["IS_Enable_Grade_as_it_is_if_Fail"]);

                //------------------- SectionPositionCheckBox
                SectionPositionCheckBox.Checked = Convert.ToBoolean(ResetDv[0]["IS_Hide_Sec_Position"]);
                //------------------- ClassPositionCheckBox
                ClassPositionCheckBox.Checked = Convert.ToBoolean(ResetDv[0]["IS_Hide_Class_Position"]);
                //-------------------SubExam_ShowCheckBox
                SubExam_ShowCheckBox.Checked = Convert.ToBoolean(ResetDv[0]["IS_Hide_SubExam"]);

                //-----------Position_RadioButtonList CheckBox
                DataView ResetPositionDv = new DataView();
                ResetPositionDv = (DataView)Exam_Publish_SettingSQL.Select(DataSourceSelectArguments.Empty);
                if (ResetPositionDv.Count == 1)
                {
                    Position_RadioButtonList.SelectedValue = ResetPositionDv[0]["Exam_Position_Format"].ToString();
                }


                if (ResetDv[0]["Attendance_FromDate"].ToString() != "")
                {
                    FromDateTextBox.Text = Convert.ToDateTime(ResetDv[0]["Attendance_FromDate"]).ToString("d MMM yyyy");
                }
                else
                {
                    FromDateTextBox.Text = string.Empty;
                }

                if (ResetDv[0]["Attendance_ToDate"].ToString() != "")
                {
                    ToDateTextBox.Text = Convert.ToDateTime(ResetDv[0]["Attendance_ToDate"]).ToString("d MMM yyyy");
                }
                else
                {
                    ToDateTextBox.Text = string.Empty;
                }
            }
            else
            {
                MinPercentageTextBox.Text = string.Empty;
                FromDateTextBox.Text = string.Empty;
                ToDateTextBox.Text = string.Empty;
            }
        }
        protected void OptionalSubjectRadioButtonList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (OptionalSubjectRadioButtonList.SelectedIndex == 1)
            {
                SpecificMarkPanel.Visible = true;
                OptionalPercentageRequired.Enabled = true;
                OptionalRegularExpressionValidator.Enabled = true;
            }
            else
            {
                SpecificMarkPanel.Visible = false;
                OptionalPercentageRequired.Enabled = false;
                OptionalRegularExpressionValidator.Enabled = false;
            }
        }
        protected void ExamRadioButtonList_SelectedIndexChanged(object sender, EventArgs e)
        {
            EMMultiView.ActiveViewIndex = ExamRadioButtonList.SelectedIndex;
        }
        protected void Exam_Publish_SettingSQL_Inserted(object sender, SqlDataSourceStatusEventArgs e)
        {
            ViewState["Cumulative_SettingID"] = e.Command.Parameters["@Cumulative_SettingID"].Value;
        }

        protected void ExamList()
        {
            con.Open();
            SqlCommand Examcommand = new SqlCommand("SELECT ExamID, ExamAdd_Percentage, Exam_EnableFail FROM Exam_Cumulative_ExamList WHERE(CumulativeNameID = @CumultiveNameID) AND(SchoolID = @SchoolID) AND(EducationYearID = @EducationYearID) AND(ClassID = @ClassID)", con);
            Examcommand.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
            Examcommand.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"].ToString());
            Examcommand.Parameters.AddWithValue("@CumultiveNameID", ExamDropDownList.SelectedValue);
            Examcommand.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);


            SqlDataReader ExamDR;
            ExamDR = Examcommand.ExecuteReader();

            Dictionary<string, Exam_Class> ExamList = new Dictionary<string, Exam_Class>();

            while (ExamDR.Read())
            {
                ExamList.Add(ExamDR["ExamID"].ToString(), new Exam_Class() { Exam_EnableFail = ExamDR["Exam_EnableFail"].ToString(), Percentage = ExamDR["ExamAdd_Percentage"].ToString() });
            }
            con.Close();

            if (ExamList.Count > 0)
            {
                foreach (DataListItem item in ExamListDataList.Items)
                {

                    CheckBox ExamCheckBox = (CheckBox)item.FindControl("ExamCheckBox");
                    TextBox PercentageTextBox = (TextBox)item.FindControl("PercentageTextBox");
                    CheckBox EnableFailCheckBox = (CheckBox)item.FindControl("EnableFailCheckBox");
                    HiddenField ExamID_HF = (HiddenField)item.FindControl("ExamID_HF");

                    Exam_Class value = new Exam_Class();
                    if (ExamList.TryGetValue(ExamID_HF.Value, out value))
                    {
                        ExamCheckBox.Checked = true;
                        PercentageTextBox.Text = value.Percentage;
                        if (value.Exam_EnableFail == "1")
                            EnableFailCheckBox.Checked = true;
                    }

                }
            }

        }
        protected void SubjectList()
        {
            con.Open();
            SqlCommand SubjectCommand = new SqlCommand("SELECT DISTINCT Exam_Cumulative_FullMarks.SubjectID, Exam_Cumulative_FullMarks.FullMarks, Exam_Cumulative_Subject.IS_Add_InExam FROM Exam_Cumulative_FullMarks INNER JOIN Exam_Cumulative_Subject ON Exam_Cumulative_FullMarks.SubjectID = Exam_Cumulative_Subject.SubjectID AND Exam_Cumulative_FullMarks.Cumulative_SettingID = Exam_Cumulative_Subject.Cumulative_SettingID WHERE(Exam_Cumulative_FullMarks.CumulativeNameID = @CumulativeNameID) AND(Exam_Cumulative_FullMarks.SchoolID = @SchoolID) AND(Exam_Cumulative_FullMarks.EducationYearID = @EducationYearID) AND (Exam_Cumulative_FullMarks.ClassID = @ClassID)", con);

            SubjectCommand.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
            SubjectCommand.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"].ToString());
            SubjectCommand.Parameters.AddWithValue("@CumulativeNameID", ExamDropDownList.SelectedValue);
            SubjectCommand.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);


            SqlDataReader SubjectDR;
            SubjectDR = SubjectCommand.ExecuteReader();

            Dictionary<string, Sub_Add_Exam> SubjectList = new Dictionary<string, Sub_Add_Exam>();

            while (SubjectDR.Read())
            {
                SubjectList.Add(SubjectDR["SubjectID"].ToString(), new Sub_Add_Exam() { FullMarks = SubjectDR["FullMarks"].ToString(), IS_Add_InExam = SubjectDR["IS_Add_InExam"].ToString() });
            }
            con.Close();

            if (SubjectList.Count > 0)
            {
                foreach (GridViewRow Row in DifferntMarksGridView.Rows)
                {
                    TextBox HMforExamTextBox = (TextBox)Row.FindControl("HMforExamTextBox");
                    HiddenField SubID_HF = (HiddenField)Row.FindControl("SubID_HF");
                    CheckBox IS_Add_ExamCheckBox = (CheckBox)Row.FindControl("IS_Add_ExamCheckBox");


                    Sub_Add_Exam value = new Sub_Add_Exam();

                    if (SubjectList.TryGetValue(SubID_HF.Value, out value))
                    {
                        HMforExamTextBox.Text = value.FullMarks;

                        IS_Add_ExamCheckBox.Checked = Convert.ToBoolean(value.IS_Add_InExam);
                    }

                }
            }
        }

        class Exam_Class
        {
            public string Percentage { get; set; }
            public string Exam_EnableFail { get; set; }
        }
        class Sub_Add_Exam
        {
            public string FullMarks { get; set; }
            public string IS_Add_InExam { get; set; }
        }
    }
}