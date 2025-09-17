using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Exam.Result
{
    public partial class Delete_Exam_And_Result : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());

        protected void Page_Load(object sender, EventArgs e)
        {
         
        }

        protected void ExamDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClassDropDownList.DataBind();
        }

        protected void ClassDropDownList_SelectedIndexChanged(object sender, EventArgs e)//Class DDL
        {
            SubjectDropDownList.DataBind();
        }

        protected void ClassDropDownList_DataBound(object sender, EventArgs e)
        {
            ClassDropDownList.Items.Insert(0, new ListItem("[ Delete All ]", "0"));


            //-----------------------For Class Drop Down------------------------------------------------------
            DataView ClassDV = new DataView();
            ClassDV = (DataView)ClassNameSQL.Select(DataSourceSelectArguments.Empty);
            if (ClassDV.Count < 1)
            {
                ClassDropDownList.Visible = false;
                ClassLabel.Visible = false;
            }
            else
            {
                ClassDropDownList.Visible = true;
                ClassLabel.Visible = true;
            }
        }

        protected void SubjectDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            SubExamDownList.DataBind();
        }

        protected void SubjectDropDownList_DataBound(object sender, EventArgs e)
        {
           SubjectDropDownList.Items.Insert(0, new ListItem("[ Delete All ]", "0"));
          

            //-----------------------For Subject Drop Down------------------------------------------------------
            DataView SubjectDV = new DataView();
            SubjectDV = (DataView)SubjectSQL.Select(DataSourceSelectArguments.Empty);
            if (SubjectDV.Count < 1)
            {
                SubjectDropDownList.Visible = false;
                SubjectLabel.Visible = false;
            }
            else
            {
                SubjectDropDownList.Visible = true;
                SubjectLabel.Visible = true;
            }

           
        }

        protected void SubExamDownList_DataBound(object sender, EventArgs e)  //...Exam and subject
        {
            SubExamDownList.Items.Insert(0, new ListItem("[ Delete All ]", "0"));

            //-----------------------For SubExam Drop Down------------------------------------------------------
            DataView SubExamDV = new DataView();
            SubExamDV = (DataView)SubExamSQL.Select(DataSourceSelectArguments.Empty);
            if (SubExamDV.Count < 1)
            {
                SubExamDownList.Visible = false;
                SubExamLabel.Visible = false;
            }
            else
            {
                SubExamDownList.Visible = true;
                SubExamLabel.Visible = true;
            }
        }

        protected void DeleteButton_Click(object sender, EventArgs e)
        {
            string EducationYearID = Session["Edu_Year"].ToString();
            string SchoolID = Session["SchoolID"].ToString();

            if (ExamDropDownList.SelectedValue != "0" && ClassDropDownList.SelectedValue == "0")
            {
                SqlCommand Delete_Exam_cmd = new SqlCommand("DELETE FROM Exam_Result_of_Student WHERE (ExamID = @ExamID) AND (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID)", con);
                Delete_Exam_cmd.Parameters.AddWithValue("@ExamID", ExamDropDownList.SelectedValue);
                Delete_Exam_cmd.Parameters.AddWithValue("@EducationYearID", EducationYearID);
                Delete_Exam_cmd.Parameters.AddWithValue("@SchoolID", SchoolID);
                con.Open();
                Delete_Exam_cmd.ExecuteNonQuery();
                con.Close();
            }

            if (ExamDropDownList.SelectedValue != "0" && ClassDropDownList.SelectedValue != "0" && SubjectDropDownList.SelectedValue == "0")
            {
                SqlCommand Delete_Exam_Class_cmd = new SqlCommand("DELETE FROM Exam_Result_of_Student WHERE (ExamID = @ExamID) AND (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID) AND (ClassID = @ClassID)", con);
                Delete_Exam_Class_cmd.Parameters.AddWithValue("@ExamID", ExamDropDownList.SelectedValue);
                Delete_Exam_Class_cmd.Parameters.AddWithValue("@EducationYearID", EducationYearID);
                Delete_Exam_Class_cmd.Parameters.AddWithValue("@SchoolID", SchoolID);
                Delete_Exam_Class_cmd.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);

                con.Open();
                Delete_Exam_Class_cmd.ExecuteNonQuery();
                con.Close();
            }

            if (ExamDropDownList.SelectedValue != "0" && ClassDropDownList.SelectedValue != "0" && SubjectDropDownList.SelectedValue != "0" && SubExamDownList.SelectedValue == "0")
            {
                SqlCommand Delete_Exam_Class_Subject_cmd = new SqlCommand("DELETE FROM Exam_Result_of_Subject WHERE  (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ExamID = @ExamID) AND (ClassID = @ClassID) AND (SubjectID = @SubjectID)", con);
                Delete_Exam_Class_Subject_cmd.Parameters.AddWithValue("@SchoolID", SchoolID);
                Delete_Exam_Class_Subject_cmd.Parameters.AddWithValue("@EducationYearID", EducationYearID);
                Delete_Exam_Class_Subject_cmd.Parameters.AddWithValue("@ExamID", ExamDropDownList.SelectedValue);
                Delete_Exam_Class_Subject_cmd.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);
                Delete_Exam_Class_Subject_cmd.Parameters.AddWithValue("@SubjectID", SubjectDropDownList.SelectedValue);

                SqlCommand Delete_Exam_Class_Subject_Obtain_Marks_cmd = new SqlCommand("DELETE FROM Exam_Obtain_Marks WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID)  AND (ExamID = @ExamID) AND (ClassID = @ClassID) AND (SubjectID = @SubjectID)", con);
                Delete_Exam_Class_Subject_Obtain_Marks_cmd.Parameters.AddWithValue("@SchoolID", SchoolID);
                Delete_Exam_Class_Subject_Obtain_Marks_cmd.Parameters.AddWithValue("@EducationYearID", EducationYearID);
                Delete_Exam_Class_Subject_Obtain_Marks_cmd.Parameters.AddWithValue("@ExamID", ExamDropDownList.SelectedValue);
                Delete_Exam_Class_Subject_Obtain_Marks_cmd.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);
                Delete_Exam_Class_Subject_Obtain_Marks_cmd.Parameters.AddWithValue("@SubjectID", SubjectDropDownList.SelectedValue);

                con.Open();
                Delete_Exam_Class_Subject_cmd.ExecuteNonQuery();
                Delete_Exam_Class_Subject_Obtain_Marks_cmd.ExecuteNonQuery();
                con.Close();
            }

            if (ExamDropDownList.SelectedValue != "0" && ClassDropDownList.SelectedValue != "0" && SubjectDropDownList.SelectedValue != "0" && SubExamDownList.SelectedValue != "0")
            {
                SqlCommand Delete_Exam_Class_Subject_SubExam_cmd = new SqlCommand("DELETE FROM Exam_Obtain_Marks WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID)  AND (ExamID = @ExamID) AND (ClassID = @ClassID) AND (SubjectID = @SubjectID) AND (SubExamID = @SubExamID)", con);
                Delete_Exam_Class_Subject_SubExam_cmd.Parameters.AddWithValue("@SchoolID", SchoolID);
                Delete_Exam_Class_Subject_SubExam_cmd.Parameters.AddWithValue("@EducationYearID", EducationYearID);
                Delete_Exam_Class_Subject_SubExam_cmd.Parameters.AddWithValue("@ExamID", ExamDropDownList.SelectedValue);
                Delete_Exam_Class_Subject_SubExam_cmd.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);
                Delete_Exam_Class_Subject_SubExam_cmd.Parameters.AddWithValue("@SubjectID", SubjectDropDownList.SelectedValue);
                Delete_Exam_Class_Subject_SubExam_cmd.Parameters.AddWithValue("@SubExamID", SubExamDownList.SelectedValue);

                con.Open();
                Delete_Exam_Class_Subject_SubExam_cmd.ExecuteNonQuery();
                con.Close();


                DataView SubExamDV = new DataView();
                SubExamDV = (DataView)SubExamSQL.Select(DataSourceSelectArguments.Empty);
                if (SubExamDV.Count < 1)
                {
                    SqlCommand Delete_Exam_Class_Subject_cmd = new SqlCommand("DELETE FROM Exam_Result_of_Subject WHERE  (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ExamID = @ExamID) AND (ClassID = @ClassID) AND (SubjectID = @SubjectID)", con);
                    Delete_Exam_Class_Subject_cmd.Parameters.AddWithValue("@SchoolID", SchoolID);
                    Delete_Exam_Class_Subject_cmd.Parameters.AddWithValue("@EducationYearID", EducationYearID);
                    Delete_Exam_Class_Subject_cmd.Parameters.AddWithValue("@ExamID", ExamDropDownList.SelectedValue);
                    Delete_Exam_Class_Subject_cmd.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);
                    Delete_Exam_Class_Subject_cmd.Parameters.AddWithValue("@SubjectID", SubjectDropDownList.SelectedValue);

                    con.Open();
                    Delete_Exam_Class_Subject_cmd.ExecuteNonQuery();
                    con.Close();
                }

            }

            ClassDropDownList.DataBind();
            SubjectDropDownList.DataBind();
            SubExamDownList.DataBind();

            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Result has been Successfully deleted!')", true);
        }

    }
}