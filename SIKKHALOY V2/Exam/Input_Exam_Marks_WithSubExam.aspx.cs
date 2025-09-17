using EnvDTE;
using Microsoft.VisualStudio.OLE.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Exam
{
    public partial class Input_Exam_Marks_WithSubExam : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
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
        protected void Page_Load(object sender, EventArgs e)
        {
            Session["Group"] = GroupDropDownList.SelectedValue;
            Session["Shift"] = ShiftDropDownList.SelectedValue;
            Session["Section"] = SectionDropDownList.SelectedValue;
            Session["Subject"] = SubjectDropDownList.SelectedValue;

            if (!IsPostBack)
            {
                GroupDropDownList.Visible = false;
                SectionDropDownList.Visible = false;
                ShiftDropDownList.Visible = false;

            }
            
        }
        protected void StylDataList_ItemDataBound(object sender, DataListItemEventArgs e)
        {
            
        }
        protected void StudentsGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
              
                CheckBox StyleCheckBox = e.Row.FindControl("AbsenceCheckBox") as CheckBox;
                TextBox ObtainedMarks = e.Row.FindControl("MarksTextBox") as TextBox;
              
                foreach (GridViewRow row in StudentsGridView.Rows)
                {
                    DataList StylDataList = (DataList)row.FindControl("StylDataList");

                    string studentId = StudentsGridView.DataKeys[row.RowIndex]["StudentID"].ToString();

                    string schoolId = Session["SchoolID"].ToString();

                    SqlCommand cmd = new SqlCommand("Select MarksObtained,AbsenceStatus from Exam_Obtain_Marks Where SchoolID='"+ schoolId + "' and StudentID='" + studentId + "' and ExamID =@ExamID", con);
                   
                    cmd.Parameters.AddWithValue("@SubjectID", SubjectDropDownList.SelectedValue);
                    cmd.Parameters.AddWithValue("@ExamID", ExamDropDownList.SelectedValue);
                    con.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    con.Close();

                    if(StylDataList!=null)
                    {
                        foreach (DataListItem itm in StylDataList.Items)  ////  Starting point here 05/09/2024
                        {
                            string studentclassId = StudentsGridView.DataKeys[row.RowIndex]["StudentClassID"].ToString();
                            string subject = SubjectDropDownList.SelectedItem.Text.ToString();
                            string exam = ExamDropDownList.SelectedItem.Text.ToString();
                            if (SubExamDownList.SelectedValue != "")
                            {
                                string subexam = SubExamDownList.SelectedItem.Text.ToString();
                            }


                            if (dt.Rows.Count > 0)
                            {

                                string marks = dt.Rows[0]["MarksObtained"].ToString();

                                Session["obtainMarks"] = marks;

                                TextBox obtaintextbox = (TextBox)itm.FindControl("MarksTextBox");

                                obtaintextbox.Text = marks;

                                if (dt.Rows[0]["AbsenceStatus"].ToString() == "Absent")
                                {
                                    (e.Row.FindControl("AbsenceCheckBox") as CheckBox).Checked = true;
                                    (e.Row.FindControl("MarksTextBox") as TextBox).Enabled = false;
                                }
                            }
                        }
                    }
                    
                }
           
            }
        }
    
        protected void ExamDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session["Group"] = "%";
            Session["Shift"] = "%";
            Session["Section"] = "%";
            Session["Subject"] = "0";

            GroupDropDownList.Visible = false;
            SectionDropDownList.Visible = false;
            ShiftDropDownList.Visible = false;
            SubExamDownList.Visible = false;
            //SubExamRequired.Enabled = false;
        }
        protected void ClassDropDownList_SelectedIndexChanged(object sender, EventArgs e)//Class DDL
        {
            Session["Group"] = "%";
            Session["Shift"] = "%";
            Session["Section"] = "%";
            Session["Subject"] = "0";

            GroupDropDownList.DataBind();
            ShiftDropDownList.DataBind();
            SectionDropDownList.DataBind();
            StudentsGridView.DataBind();
            view();

            
        }
        protected void ClassDropDownList_DataBound(object sender, EventArgs e)
        {
            ClassDropDownList.Items.Insert(0, new ListItem("[ SELECT CLASS ]", "0"));
        }
        protected void GroupDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
            Session["Subject"] = "0";
        }
        protected void GroupDropDownList_DataBound(object sender, EventArgs e)
        {
            GroupDropDownList.Items.Insert(0, new ListItem("[ SELECT GROUP ]", "%"));
            if (IsPostBack)
                GroupDropDownList.Items.FindByValue(Session["Group"].ToString()).Selected = true;
        }

        protected void SectionDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();

            
        }
        protected void SectionDropDownList_DataBound(object sender, EventArgs e)
        {
            SectionDropDownList.Items.Insert(0, new ListItem("[ All SECTION ]", "%"));
            if (IsPostBack)
                SectionDropDownList.Items.FindByValue(Session["Section"].ToString()).Selected = true;
        }

        protected void ShiftDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
        }
        protected void ShiftDropDownList_DataBound(object sender, EventArgs e)
        {
            ShiftDropDownList.Items.Insert(0, new ListItem("[ All SHIFT]", "%"));
            if (IsPostBack)
                ShiftDropDownList.Items.FindByValue(Session["Shift"].ToString()).Selected = true;
        }

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
            int id = Convert.ToInt32(SubjectDropDownList.SelectedValue);
            string eduyear = Session["Edu_Year"].ToString();
            DataView SubExamDV = new DataView();
            SubExamDV = (DataView)SubExamSQL.Select(DataSourceSelectArguments.Empty);
            if (SubExamDV.Count < 1)
            {
                PassMarkFullMarkSQL.SelectParameters["SubExamID"].DefaultValue = "0";
                SubExamDownList.Visible = false;
                //SubExamRequired.Enabled = false;
            }
            else
            {
                PassMarkFullMarkSQL.SelectParameters["SubExamID"].DefaultValue = "";
                SubExamDownList.Visible = true;
                //SubExamRequired.Enabled = true;
            }

            StudentsGridView.DataBind();
        }
        protected void SubjectDropDownList_DataBound(object sender, EventArgs e)
        {
            if (SubjectDropDownList.Items.FindByValue("0") == null)
                SubjectDropDownList.Items.Insert(0, new ListItem("[ SELECT SUBJECT ]", "0"));
            if (IsPostBack)
            {
                if (Session["Subject"] != null)
                    SubjectDropDownList.Items.FindByValue(Session["Subject"].ToString()).Selected = true;
            }
        }
        //End DDL

        protected void AbsenceCheckBox_OnCheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = (CheckBox)sender;
            RepeaterItem item = (RepeaterItem)chk.NamingContainer;
            Label lblCampCode = (Label)item.FindControl("lblCampCode");
            string CampCode = lblCampCode.Text;//  here i want to get the value of CampCode in that row
        }

        protected void ShowStudentButton_Click(object sender, EventArgs e)
        {
            StudentsGridView.Visible = true;
            SubmitButton.Visible = true;
            FmPmFormView.Visible = true;
            StudentsGridView.DataBind();
        }
        protected void ShowStudentButton_Click_bkp(object sender, EventArgs e)
        {
            StudentsGridView.Visible = true;
            SubmitButton.Visible = true;
            FmPmFormView.Visible = true;
            int totalrecord = Convert.ToInt32(SubExamDownList.Items.Count.ToString());

            string eduyear = Session["Edu_Year"].ToString();
            string schoolId = Session["SchoolID"].ToString();

            SqlCommand cmd = new SqlCommand("SELECT Student.StudentsName, Student.FathersName, StudentsClass.StudentID, StudentsClass.StudentClassID, Student.ID, StudentsClass.RollNo FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN StudentRecord ON StudentsClass.StudentClassID = StudentRecord.StudentClassID  WHERE (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID) AND (StudentsClass.SubjectGroupID LIKE @SubjectGroupID) AND (StudentsClass.EducationYearID = @EducationYearID) AND (StudentRecord.SubjectID = @SubjectID) AND (StudentsClass.ShiftID LIKE @ShiftID) AND (StudentsClass.SchoolID = @SchoolID) AND (Student.Status = N'Active') ORDER BY CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(REPLACE(REPLACE(StudentsClass.RollNo, '$', ''), ',', '') AS INT) ELSE 0 END", con);
            cmd.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);
            cmd.Parameters.AddWithValue("@SectionID", SectionDropDownList.SelectedValue);
            cmd.Parameters.AddWithValue("@SubjectGroupID", GroupDropDownList.SelectedValue);
            cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"].ToString());
            cmd.Parameters.AddWithValue("@SubjectID", SubjectDropDownList.SelectedValue);
            cmd.Parameters.AddWithValue("@ShiftID", ShiftDropDownList.SelectedValue);
            cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());

            con.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            StudentsGridView.DataSource = dt;
            con.Close();
        
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
                DataList StylDataList = (DataList)row.FindControl("StylDataList");

                foreach (DataListItem itm in StylDataList.Items)  ////  Starting point here 05/09/2024
                {
                    string value = itm.FindControl("MarksTextBox").ToString();
                
                }
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
    }
}