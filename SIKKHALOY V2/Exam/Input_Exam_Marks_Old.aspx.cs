using EnvDTE;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace EDUCATION.COM.Exam
{
    public partial class Input_Exam_Marks : System.Web.UI.Page
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
            SubExamRequired.Enabled = false;


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
            //SubExamDownList.Cou
            int totalrecord = Convert.ToInt32(SubExamDownList.Items.Count.ToString());





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
                if (Session["Subject"] != null)
                    SubjectDropDownList.Items.FindByValue(Session["Subject"].ToString()).Selected = true;
            }
        }
        //End DDL

        protected void StudentsGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
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




            }
        }
        private void AddGridviewColumn(string name)
        {
            TemplateField col = new TemplateField();
            col.HeaderText = name;
            StudentsGridView.Columns.Add(col);



            //BoundField testField = new BoundField();
            //testField.DataField = "New_testField_Name";
            //testField.HeaderText = "testField_Header";
            //StudentsGridView.Columns.Add(testField);
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