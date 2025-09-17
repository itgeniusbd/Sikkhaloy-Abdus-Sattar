using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.EXAM.ExamSetting
{
    public partial class Marks_Distribution : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void MarksCheck()
        {
            foreach (GridViewRow row in SubjectGridView.Rows)
            {
                CheckBox AddSubExamCheckBox = (CheckBox)row.FindControl("AddSubExamCheckBox");
                GridView SubExamGridView = (GridView)row.FindControl("SubExamGridView");
                TextBox HMforExamTextBox = (TextBox)row.FindControl("HMforExamTextBox");
                RequiredFieldValidator ExamMarksRequired = row.FindControl("ExamMarksRequired") as RequiredFieldValidator;

                con.Open();
                SqlCommand SubExam_CheckCMD = new SqlCommand("Select FullMarks from Exam_Full_Marks Where SchoolID = @SchoolID and ClassID = @ClassID and ExamID = @ExamID and SubjectID = @SubjectID and SubExamID is null", con);
                SubExam_CheckCMD.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                SubExam_CheckCMD.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);
                SubExam_CheckCMD.Parameters.AddWithValue("@ExamID", ExamDropDownList.SelectedValue);
                SubExam_CheckCMD.Parameters.AddWithValue("@SubjectID", SubjectGridView.DataKeys[row.DataItemIndex][0].ToString());
                object checke = SubExam_CheckCMD.ExecuteScalar();

                if (checke != null)
                {
                    HMforExamTextBox.Text = SubExam_CheckCMD.ExecuteScalar().ToString();
                    ExamMarksRequired.Enabled = true;

                }
                else
                {
                    foreach (GridViewRow SXrow in SubExamGridView.Rows)
                    {
                        CheckBox SubExamCheckBox = (CheckBox)SXrow.FindControl("SubExamCheckBox");
                        TextBox SubExamFullMarkTextBox = (TextBox)SXrow.FindControl("SubExamFullMarkTextBox");
                        RequiredFieldValidator SubExamRequired = SXrow.FindControl("SubExamRequired") as RequiredFieldValidator;

                        SqlCommand ExamRoleCheckcmd = new SqlCommand("Select FullMarks from Exam_Full_Marks Where SchoolID = @SchoolID and ClassID = @ClassID and ExamID = @ExamID and SubjectID = @SubjectID and SubExamID = @SubExamID", con);
                        ExamRoleCheckcmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                        ExamRoleCheckcmd.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);
                        ExamRoleCheckcmd.Parameters.AddWithValue("@ExamID", ExamDropDownList.SelectedValue);
                        ExamRoleCheckcmd.Parameters.AddWithValue("@SubjectID", SubjectGridView.DataKeys[row.RowIndex]["SubjectID"].ToString());
                        ExamRoleCheckcmd.Parameters.AddWithValue("@SubExamID", SubExamGridView.DataKeys[SXrow.RowIndex]["SubExamID"].ToString());

                        object ExamRole = ExamRoleCheckcmd.ExecuteScalar();

                        if (ExamRole != null)
                        {
                            AddSubExamCheckBox.Checked = true;
                            HMforExamTextBox.Visible = false;
                            SubExamGridView.Visible = true;
                            ExamMarksRequired.Enabled = false;
                            SubExamRequired.Enabled = true;
                            SubExamCheckBox.Checked = true;
                            SubExamFullMarkTextBox.Enabled = true;
                            SubExamFullMarkTextBox.CssClass = "form-control";
                            SubExamFullMarkTextBox.Text = ExamRoleCheckcmd.ExecuteScalar().ToString();
                        }
                    }
                }
                con.Close();
            }
        }

        protected void SubjectGridView_DataBound(object sender, EventArgs e)
        {
            if (SubjectGridView.Rows.Count > 0)
            {
                MarksCheck();
                Label ExamNameLabel = SubjectGridView.HeaderRow.FindControl("ExamNameLabel") as Label;
                ExamNameLabel.Text = ExamDropDownList.SelectedItem.Text;
            }
        }

        protected void AddSubExamCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox CheckBox = sender as CheckBox;
            GridViewRow gvRow = (GridViewRow)CheckBox.NamingContainer;

            CheckBox AddSubExamCheckBox = (CheckBox)gvRow.FindControl("AddSubExamCheckBox");
            TextBox HMforExamTextBox = (TextBox)gvRow.FindControl("HMforExamTextBox");
            GridView SubExamGridView = (GridView)gvRow.FindControl("SubExamGridView");
            RequiredFieldValidator ExamMarksRequired = gvRow.FindControl("ExamMarksRequired") as RequiredFieldValidator;

            if (AddSubExamCheckBox.Checked)
            {
                HMforExamTextBox.Visible = false;
                SubExamGridView.Visible = true;
                ExamMarksRequired.Enabled = false;
                HMforExamTextBox.Text = "";
            }
            else
            {
                HMforExamTextBox.Visible = true;
                SubExamGridView.Visible = false;
                ExamMarksRequired.Enabled = true;
            }
        }


        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            if (Grading_System_DropDownList.Items.Count > 0)
            {
                bool IsChecked = true;
                string ErrorMgs = "";

                foreach (GridViewRow gvRow in SubjectGridView.Rows)
                {
                    CheckBox AddSubExamCheckBox = (CheckBox)gvRow.FindControl("AddSubExamCheckBox");

                    if (AddSubExamCheckBox.Checked)
                    {
                        IsChecked = false;
                        GridView SubExamGridView = (GridView)gvRow.FindControl("SubExamGridView");

                        if (SubExamGridView.Rows.Count > 0)
                        {
                            foreach (GridViewRow InnerRow in SubExamGridView.Rows)
                            {
                                CheckBox SubExamCheckBox = (CheckBox)InnerRow.FindControl("SubExamCheckBox");
                                if (SubExamCheckBox.Checked)
                                {
                                    IsChecked = true;
                                }
                                else
                                {
                                    ErrorMgs = "Select Sub-Exam";
                                }
                            }
                        }
                        else
                        {
                            IsChecked = false;
                            ErrorMgs = "Sub-Exam Not Created";
                        }
                    }
                }

                if (IsChecked)
                {
                    ExamFullMarksSQL.Delete();

                    foreach (GridViewRow gvRow in SubjectGridView.Rows)
                    {
                        CheckBox AddSubExamCheckBox = (CheckBox)gvRow.FindControl("AddSubExamCheckBox");

                        if (!AddSubExamCheckBox.Checked)
                        {
                            TextBox HMforExamTextBox = (TextBox)gvRow.FindControl("HMforExamTextBox");
                            ExamFullMarksSQL.InsertParameters["SubjectID"].DefaultValue = SubjectGridView.DataKeys[gvRow.RowIndex]["SubjectID"].ToString();
                            ExamFullMarksSQL.InsertParameters["FullMarks"].DefaultValue = HMforExamTextBox.Text;
                            ExamFullMarksSQL.InsertParameters["SubExamID"].DefaultValue = "";
                            ExamFullMarksSQL.Insert();
                            ErrorMgs = "Marks Distributed Successfully!!";
                        }
                        else
                        {
                            GridView SubExamGridView = (GridView)gvRow.FindControl("SubExamGridView");
                            foreach (GridViewRow InnerRow in SubExamGridView.Rows)
                            {
                                CheckBox SubExamCheckBox = (CheckBox)InnerRow.FindControl("SubExamCheckBox");

                                if (SubExamCheckBox.Checked)
                                {
                                    TextBox SubExamFullMarkTextBox = (TextBox)InnerRow.FindControl("SubExamFullMarkTextBox");
                                    ExamFullMarksSQL.InsertParameters["SubjectID"].DefaultValue = SubjectGridView.DataKeys[gvRow.RowIndex]["SubjectID"].ToString();
                                    ExamFullMarksSQL.InsertParameters["FullMarks"].DefaultValue = SubExamFullMarkTextBox.Text;
                                    ExamFullMarksSQL.InsertParameters["SubExamID"].DefaultValue = SubExamGridView.DataKeys[InnerRow.RowIndex]["SubExamID"].ToString();
                                    ExamFullMarksSQL.Insert();
                                    ErrorMgs = "Marks Distributed Successfully!!";
                                }
                            }
                        }
                    }

                    //Update Grade System or Insert
                    UpdateGradeSQL.Insert();

                    //PassMark update
                    ExamFullMarksSQL.Update();

                    //SP Update Exam_Mark_Re_Submit
                    Mark4ClassSQL.Update();
                }

                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('" + ErrorMgs + "')", true);
            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('ERROR!! Grading System Not Added!')", true);
            }
        }

        protected void ExamDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            SubjectGridView.DataBind();

            Grade_Select();

        }

        void Grade_Select()
        {
            SqlCommand Grade_SelectCMD = new SqlCommand(" SELECT GradeNameID FROM Exam_Grading_Assign WHERE(SchoolID = @SchoolID) AND(EducationYearID = @EducationYearID) AND(ClassID = @ClassID) AND(ExamID = @ExamID)", con);
            Grade_SelectCMD.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
            Grade_SelectCMD.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"].ToString());
            Grade_SelectCMD.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);
            Grade_SelectCMD.Parameters.AddWithValue("@ExamID", ExamDropDownList.SelectedValue);

            con.Open();
            object checke = Grade_SelectCMD.ExecuteScalar();
            con.Close();

            if (checke != null)
            {
                Grading_System_DropDownList.SelectedValue = checke.ToString();
            }
        }

        protected void ClassDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Grade_Select();
        }

        protected void CopyFromExam_DropDownList_DataBound(object sender, EventArgs e)
        {
            CopyFromExam_DropDownList.Items.Insert(0, new ListItem("[ SELECT ]", "0"));
        }

        protected void CopyToExam_DropDownList_DataBound(object sender, EventArgs e)
        {
            CopyToExam_DropDownList.Items.Insert(0, new ListItem("[ SELECT ]", "0"));
        }

        protected void CopyButton_Click(object sender, EventArgs e)
        {
            CopyMarksSQL.Insert();
            SuccessLabel.Text = "Marks Successfully Copied";
        }
    }
}