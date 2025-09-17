using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using EDUCATION.COM.Employee.Edit_Employee;
using EnvDTE;

namespace EDUCATION.COM.Exam
{
    public partial class TestSubExam : System.Web.UI.Page
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
            StudentsGridView.Visible = false;
            int totalsubExam = Convert.ToInt32(SubExamDownList.Items.Count.ToString());

            Session["subexamcount"] = totalsubExam;

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
            StudentsGridView.Visible = false;
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


            StudentsGridView.DataBind();
        }
        protected void SubExamDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            PassMarkFullMarkSQL.SelectParameters["SubExamID"].DefaultValue = SubExamDownList.SelectedValue;
            //SubExamDownList.Cou
            int totalrecord = Convert.ToInt32(SubExamDownList.Items.Count.ToString());


            SqlCommand cmd = new SqlCommand("SELECT Exam_SubExam_Name.SubExamID, Exam_SubExam_Name.SubExamName FROM Exam_SubExam_Name INNER JOIN Exam_Full_Marks ON Exam_SubExam_Name.SubExamID = Exam_Full_Marks.SubExamID WHERE (Exam_Full_Marks.SubjectID = @SubjectID) AND (Exam_Full_Marks.ClassID = @ClassID) AND (Exam_SubExam_Name.SchoolID = @SchoolID) AND (Exam_Full_Marks.ExamID = @ExamID) AND (Exam_Full_Marks.EducationYearID = @EducationYearID)", con);
            cmd.Parameters.AddWithValue("@SubjectID", SubjectDropDownList.SelectedValue);
            cmd.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);
            cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
            cmd.Parameters.AddWithValue("@ExamID", ExamDropDownList.SelectedValue);
            cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"].ToString());

            con.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();

            if (dt.Rows.Count > 0)
            {
                this.StudentsGridView.Columns[3].Visible = true;

                this.StudentsGridView.Columns[4].Visible = false;
                this.StudentsGridView.Columns[5].Visible = false;
            }
            else
            {
                this.StudentsGridView.Columns[3].Visible = false;

                this.StudentsGridView.Columns[4].Visible = true;
                this.StudentsGridView.Columns[5].Visible = true;
            }

            if (SubExamDownList.SelectedValue != null)
            {
                if (SubExamDownList.SelectedValue != "")
                {
                    this.StudentsGridView.Columns[3].Visible = false;

                    this.StudentsGridView.Columns[4].Visible = true;
                    this.StudentsGridView.Columns[5].Visible = true;
                }

            }       
     
        }

        protected void SubjectDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataView SubExamDV = new DataView();
            SubExamDownList.Items.Clear();
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
                // SubExamRequired.Enabled = true;
            }

            


            string schoolid = Session["SchoolID"].ToString();
            string eduId = Session["Edu_Year"].ToString();


            SqlCommand cmd = new SqlCommand("SELECT Exam_SubExam_Name.SubExamID, Exam_SubExam_Name.SubExamName FROM Exam_SubExam_Name INNER JOIN Exam_Full_Marks ON Exam_SubExam_Name.SubExamID = Exam_Full_Marks.SubExamID WHERE (Exam_Full_Marks.SubjectID = @SubjectID) AND (Exam_Full_Marks.ClassID = @ClassID) AND (Exam_SubExam_Name.SchoolID = @SchoolID) AND (Exam_Full_Marks.ExamID = @ExamID) AND (Exam_Full_Marks.EducationYearID = @EducationYearID)", con);
            cmd.Parameters.AddWithValue("@SubjectID", SubjectDropDownList.SelectedValue);
            cmd.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);
            cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
            cmd.Parameters.AddWithValue("@ExamID", ExamDropDownList.SelectedValue);
            cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"].ToString());

            con.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();

            if (dt.Rows.Count > 0)
            {
                this.StudentsGridView.Columns[3].Visible = true;

                this.StudentsGridView.Columns[4].Visible = false;
                this.StudentsGridView.Columns[5].Visible = false;
            }
            else
            {
                this.StudentsGridView.Columns[3].Visible = false;

                this.StudentsGridView.Columns[4].Visible = true;
                this.StudentsGridView.Columns[5].Visible = true;
            }


            //StudentsGridView.DataBind();

            



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

        protected void StudentsGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if(SubExamDownList.Items.Count ==0 )
                {
                    // Single SubExam
                    SqlCommand cmd = new SqlCommand("Select MarksObtained,AbsenceStatus from Exam_Obtain_Marks Where StudentClassID = @StudentClassID and SubjectID = @SubjectID and ExamID = @ExamID and (SubExamID = @SubExamID or SubExamID is null)", con);
                    cmd.Parameters.AddWithValue("@StudentClassID", StudentsGridView.DataKeys[e.Row.RowIndex]["StudentClassID"].ToString());
                    cmd.Parameters.AddWithValue("@SubjectID", SubjectDropDownList.SelectedValue);
                    cmd.Parameters.AddWithValue("@ExamID", ExamDropDownList.SelectedValue);
                    cmd.Parameters.AddWithValue("@SubExamID", SubExamDownList.SelectedValue);
                    con.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    con.Close();

                    if (dt.Rows.Count > 0)
                    {
                        (e.Row.FindControl("MarksTextBox") as TextBox).Text = dt.Rows[0]["MarksObtained"].ToString();
                        if (dt.Rows[0]["AbsenceStatus"].ToString() == "Absent")
                        {
                            (e.Row.FindControl("AbsenceCheckBox") as CheckBox).Checked = true;
                            (e.Row.FindControl("MarksTextBox") as TextBox).Enabled = false;
                        }
                    }
                    //-------------End
                }
                if (SubExamDownList.SelectedValue!="")
                {
                    // Single SubExam
                    SqlCommand cmd = new SqlCommand("Select MarksObtained,AbsenceStatus from Exam_Obtain_Marks Where StudentClassID = @StudentClassID and SubjectID = @SubjectID and ExamID = @ExamID and (SubExamID = @SubExamID or SubExamID is null)", con);
                    cmd.Parameters.AddWithValue("@StudentClassID", StudentsGridView.DataKeys[e.Row.RowIndex]["StudentClassID"].ToString());
                    cmd.Parameters.AddWithValue("@SubjectID", SubjectDropDownList.SelectedValue);
                    cmd.Parameters.AddWithValue("@ExamID", ExamDropDownList.SelectedValue);
                    cmd.Parameters.AddWithValue("@SubExamID", SubExamDownList.SelectedValue);
                    con.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    con.Close();

                    if (dt.Rows.Count > 0)
                    {
                        (e.Row.FindControl("MarksTextBox") as TextBox).Text = dt.Rows[0]["MarksObtained"].ToString();
                        if (dt.Rows[0]["AbsenceStatus"].ToString() == "Absent")
                        {
                            (e.Row.FindControl("AbsenceCheckBox") as CheckBox).Checked = true;
                            (e.Row.FindControl("MarksTextBox") as TextBox).Enabled = false;
                        }
                    }
                    //-------------End
                }
                else
                {
                    DataList subExamDataList = (DataList)e.Row.FindControl("SubExamlDataList");
                    foreach (DataListItem itm in subExamDataList.Items)
                    {                      
                        HiddenField subexamID = (HiddenField)itm.FindControl("subexamID");


                        SqlCommand cmd1 = new SqlCommand("Select MarksObtained,AbsenceStatus from Exam_Obtain_Marks Where StudentClassID = @StudentClassID and SubjectID = @SubjectID and ExamID = @ExamID And SubExamID=@SubExamID", con);
                        cmd1.Parameters.AddWithValue("@StudentClassID", StudentsGridView.DataKeys[e.Row.RowIndex]["StudentClassID"].ToString());
                        cmd1.Parameters.AddWithValue("@SubjectID", SubjectDropDownList.SelectedValue);
                        cmd1.Parameters.AddWithValue("@ExamID", ExamDropDownList.SelectedValue);
                        cmd1.Parameters.AddWithValue("@SubExamID", subexamID.Value);
                        con.Open();
                        SqlDataAdapter da1 = new SqlDataAdapter(cmd1);
                        DataTable dt1 = new DataTable();
                        da1.Fill(dt1);
                        con.Close();

                        if(dt1.Rows.Count > 0)
                        {
                            (itm.FindControl("MarksTextBox") as TextBox).Text = dt1.Rows[0]["MarksObtained"].ToString();
                            if (dt1.Rows[0]["AbsenceStatus"].ToString() == "Absent")
                            {
                                (itm.FindControl("AbsenceCheckBox") as CheckBox).Checked = true;
                                (itm.FindControl("MarksTextBox") as TextBox).Enabled = false;
                            }
                        }
                    }
                }
   
            }
            string count = StudentsGridView.Rows.Count.ToString();
            totalStudent.Text = count;
        }


        protected void ShowStudentButton_Click(object sender, EventArgs e)
        {
            StudentsGridView.Visible = true;
            SubmitButton.Visible = true;
            FmPmFormView.Visible = true;
            StudentsGridView.DataBind();
        }
        private void AddGridviewColumn(string name)
        {
            TemplateField col = new TemplateField();
            col.HeaderText = name;
            StudentsGridView.Columns.Add(col);
        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {

            if (SubExamDownList.Visible == true)
            {
                if (SubExamDownList.Items.Count > 0)
                {
                    if (SubExamDownList.SelectedValue == "")
                    {
                        InsertWithDynamicSubExam();
                    }
                }
            }
            else
            {
                SubExamDownList.Items.Clear();
            }
            if (SubExamDownList.Items.Count == 0 || SubExamDownList.SelectedValue != "")
            {
                InsertWithout_DynamicSubExam();
            }
        }
        private void InsertWithout_DynamicSubExam()
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

                ScriptManager.RegisterStartupScript(this, GetType(), "Msg", "Success();", true);
            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Obtained Mark greater than Full Mark')", true);
            }
        }

        private void InsertWithDynamicSubExam()
        {
            bool IS_FullMark = true;
            double FullMark = 0;
            double PassMark = 0;
            double PassPercentage = 0;


            if (SubExamDownList.SelectedValue != "" && SubExamDownList.SelectedValue != null)
            {
                FullMark = Convert.ToDouble(FmPmFormView.DataKey["FullMark"].ToString());
                PassMark = Convert.ToDouble(FmPmFormView.DataKey["PassMark"].ToString());
                PassPercentage = Convert.ToDouble(FmPmFormView.DataKey["PassPercentage"].ToString());
            }            
            foreach (GridViewRow row in StudentsGridView.Rows)
            {
                DataList subExamDataList = (DataList)row.FindControl("SubExamlDataList");

                foreach (DataListItem itm in subExamDataList.Items)
                {
                    CheckBox AbsenceCheckBox = (CheckBox)itm.FindControl("AbsenceCheckBox");
                    TextBox ObtainedMarks = (TextBox)itm.FindControl("MarksTextBox");

                    Label fullmarksLabel = (Label)itm.FindControl("lblFullMarks");
                    Label passmarksLabel = (Label)itm.FindControl("labelPassMark");
                    Label markParcentageLabel = (Label)itm.FindControl("labelParcentage");

                    FullMark = Convert.ToDouble(fullmarksLabel.Text.ToString());
                    PassMark = Convert.ToDouble(passmarksLabel.Text.ToString());
                    PassPercentage = Convert.ToDouble(markParcentageLabel.Text.ToString());

                    if (ObtainedMarks.Text.Trim() != "")
                    {
                        if (FullMark < Convert.ToDouble(ObtainedMarks.Text.Trim()))
                        {
                            ObtainedMarks.ForeColor = System.Drawing.Color.Red;
                            IS_FullMark = false;
                        }
                    }

                }
            }


            if (IS_FullMark)
            {
                bool IsEmpty = true;
                foreach (GridViewRow row in StudentsGridView.Rows)
                {                   
                    DataList subExamDataList = (DataList)row.FindControl("SubExamlDataList");
                    foreach (DataListItem itm in subExamDataList.Items)
                    {
                        TextBox ObtainedMarks = (TextBox)itm.FindControl("MarksTextBox");
                        CheckBox AbsenceCheckBox = (CheckBox)itm.FindControl("AbsenceCheckBox");

                        HiddenField subexamId = (HiddenField)itm.FindControl("subexamID");
                                              
                        Label fullmarksLabel = (Label)itm.FindControl("lblFullMarks");
                        Label passmarksLabel = (Label)itm.FindControl("labelPassMark");
                        Label markParcentageLabel = (Label)itm.FindControl("labelParcentage");

                        FullMark = Convert.ToDouble(fullmarksLabel.Text.ToString());
                        PassMark = Convert.ToDouble(passmarksLabel.Text.ToString());
                        PassPercentage = Convert.ToDouble(markParcentageLabel.Text.ToString());

                        if (ObtainedMarks.Text.Trim() == "" && !AbsenceCheckBox.Checked)
                        {
                            IsEmpty = false;
                            itm.CssClass = "EmptyRows";
                        }
                        else
                        {
                            itm.CssClass = "";
                        }


                        if (ObtainedMarks.Text.Trim() != "" || AbsenceCheckBox.Checked)
                        {
                            if (AbsenceCheckBox.Checked)
                            {
                                Exam_Result_of_StudentSQL.InsertParameters["StudentID"].DefaultValue = StudentsGridView.DataKeys[row.RowIndex]["StudentID"].ToString();
                                Exam_Result_of_StudentSQL.InsertParameters["MarksObtained"].DefaultValue = "";

                                if (SubExamDownList.Visible)
                                {
                                    Exam_Result_of_StudentSQL.InsertParameters["SubExamID"].DefaultValue = subexamId.Value.ToString();
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
                                        Exam_Result_of_StudentSQL.InsertParameters["SubExamID"].DefaultValue = subexamId.Value.ToString(); //SubExamDownList.SelectedValue;
                                    }

                                    try
                                    {
                                        Exam_Result_of_StudentSQL.InsertParameters["AbsenceStatus"].DefaultValue = "Present";
                                        Exam_Result_of_StudentSQL.InsertParameters["FullMark"].DefaultValue = FullMark.ToString();
                                        Exam_Result_of_StudentSQL.InsertParameters["PassMark"].DefaultValue = PassMark.ToString();
                                        Exam_Result_of_StudentSQL.InsertParameters["PassPercentage"].DefaultValue = PassPercentage.ToString();
                                        Exam_Result_of_StudentSQL.Insert();
                                    }
                                    catch(Exception ex)
                                    {
                                        string msg = ex.Message;
                                    }

                                    
                                }
                            }
                        }
                    }                    
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