using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.EXAM.WeeklyExam
{
    public partial class Weekly_Merit_Status_Marks : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
        protected void Page_Load(object sender, EventArgs e)
        {

            Session["Group"] = GroupDropDownList.SelectedValue;
            Session["Shift"] = ShiftDropDownList.SelectedValue;
            Session["Section"] = SectionDropDownList.SelectedValue;


            try
            {
                if (!IsPostBack)
                {
                    MultiView1.ActiveViewIndex = 0;

                    GroupDropDownList.Visible = false;
                    GroupLabel.Visible = false;

                    GroupFormView.Visible = false;

                    SectionDropDownList.Visible = false;
                    SectionLabel.Visible = false;

                    SectionFormView.Visible = false;

                    ShiftDropDownList.Visible = false;
                    ShiftLabel.Visible = false;

                    ShiftFormView.Visible = false;

                }
            }

            catch { }
        }

        protected void view()
        {
            DataView GroupDV = new DataView();
            GroupDV = (DataView)GroupSQL.Select(DataSourceSelectArguments.Empty);
            if (GroupDV.Count < 1)
            {
                GroupDropDownList.Visible = false;
                GroupLabel.Visible = false;
                GRequiredFieldValidator.Visible = false;

                GroupFormView.Visible = false;

            }
            else
            {
                GroupDropDownList.Visible = true;
                GroupLabel.Visible = true;
                GRequiredFieldValidator.Visible = true;

                GroupFormView.Visible = true;
            }

            DataView SectionDV = new DataView();
            SectionDV = (DataView)SectionSQL.Select(DataSourceSelectArguments.Empty);
            if (SectionDV.Count < 1)
            {
                SectionDropDownList.Visible = false;
                SectionLabel.Visible = false;
                SeRequiredFieldValidator.Visible = false;

                SectionFormView.Visible = false;
            }
            else
            {
                SectionDropDownList.Visible = true;
                SectionLabel.Visible = true;
                SeRequiredFieldValidator.Visible = true;

                SectionFormView.Visible = true;
            }

            DataView ShiftDV = new DataView();
            ShiftDV = (DataView)ShiftSQL.Select(DataSourceSelectArguments.Empty);
            if (ShiftDV.Count < 1)
            {
                ShiftDropDownList.Visible = false;
                ShiftLabel.Visible = false;
                ShRequiredFieldValidator.Visible = false;

                ShiftFormView.Visible = false;

            }
            else
            {
                ShiftDropDownList.Visible = true;
                ShiftLabel.Visible = true;
                ShRequiredFieldValidator.Visible = true;

                ShiftFormView.Visible = true;

            }

        }

        //-----------------------check function-----------------------------------------//
        protected void GridViewRowDisable()
        {
            StudentsGridView.DataBind();
            foreach (GridViewRow row in StudentsGridView.Rows)
            {
                SqlCommand cmd = new SqlCommand("Select WeeklyExamID from WeeklyExam Where SchoolID = @SchoolID and StudentID = @StudentID and ExamDate = @ExamDate", con);
                cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                cmd.Parameters.AddWithValue("@StudentID", StudentsGridView.DataKeys[row.RowIndex][2].ToString());
                cmd.Parameters.AddWithValue("@ExamDate", ExamDateCalendar.SelectedDate);

                con.Open();
                Object MarksCheck = cmd.ExecuteScalar();
                con.Close();

                if (MarksCheck != null)
                {
                    row.BackColor = System.Drawing.Color.LightGray;
                    row.Enabled = false;
                }
            }
        }

        //---------------------------------------Class DDL-------------------------------------------
        protected void ClassDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session["Group"] = "%";
            Session["Shift"] = "%";
            Session["Section"] = "%";

            GroupDropDownList.DataBind();
            ShiftDropDownList.DataBind();
            SectionDropDownList.DataBind();
            view();
            GridViewRowDisable();
        }


        //---------------------------------------Group DDL-------------------------------------------
        protected void GroupDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
            GridViewRowDisable();
        }

        protected void GroupDropDownList_DataBound(object sender, EventArgs e)
        {
            GroupDropDownList.Items.Insert(0, new ListItem("- Select -", "%"));
            if (IsPostBack)
                GroupDropDownList.Items.FindByValue(Session["Group"].ToString()).Selected = true;
        }


        //---------------------------------------Section DDL-------------------------------------------
        protected void SectionDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
            GridViewRowDisable();
        }

        protected void SectionDropDownList_DataBound(object sender, EventArgs e)
        {
            SectionDropDownList.Items.Insert(0, new ListItem("- Select -", "%"));
            if (IsPostBack)
                SectionDropDownList.Items.FindByValue(Session["Section"].ToString()).Selected = true;
        }

        //---------------------------------------Shift DDL-------------------------------------------
        protected void ShiftDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
            GridViewRowDisable();
        }

        protected void ShiftDropDownList_DataBound(object sender, EventArgs e)
        {
            ShiftDropDownList.Items.Insert(0, new ListItem("- Select -", "%"));
            if (IsPostBack)
                ShiftDropDownList.Items.FindByValue(Session["Shift"].ToString()).Selected = true;
        }
        //---------------------------------------End DD-------------------------------------------



        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            bool TextBoxCheck = true;
            foreach (GridViewRow row in StudentsGridView.Rows)
            {
                TextBox MarksTextBox = (TextBox)row.FindControl("MarksTextBox");
                CheckBox AbsenceCheckBox = (CheckBox)row.FindControl("AbsenceCheckBox");

                if (MarksTextBox.Text == "" && !AbsenceCheckBox.Checked)
                {
                    TextBoxCheck = false;
                }
            }

            if (TextBoxCheck)
            {

                foreach (GridViewRow row in StudentsGridView.Rows)
                {
                    TextBox MarksTextBox = (TextBox)row.FindControl("MarksTextBox");
                    CheckBox AbsenceCheckBox = (CheckBox)row.FindControl("AbsenceCheckBox");

                    if (MarksTextBox.Text != "" || AbsenceCheckBox.Checked)
                    {
                        Session["MarksObtained"] = MarksTextBox.Text;
                        Session["StudentClassID"] = StudentsGridView.DataKeys[row.RowIndex][0].ToString();

                        Session["EducationYearID"] = StudentsGridView.DataKeys[row.RowIndex][1].ToString();
                        Session["StudentID"] = StudentsGridView.DataKeys[row.RowIndex][2].ToString();

                        WeeklyExamSQL.Insert();

                        MarksTextBox.Text = string.Empty;
                        AbsenceCheckBox.Checked = false;

                        row.BackColor = System.Drawing.Color.LightGray;
                        row.Enabled = false;

                    }
                }
            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Enter Exam Marks or select Abs.')", true);
            }

        }

        protected void ExamDateCalendar_SelectionChanged(object sender, EventArgs e)
        {
            DateTime date = ExamDateCalendar.SelectedDate;

            SqlCommand WeekCmd = new SqlCommand("SET DATEFIRST 6; SELECT datepart(WEEK,@Date)", con);
            WeekCmd.Parameters.AddWithValue("@Date", date);

            con.Open();
            DateLabel.Text = " for " + date.ToString("(dddd, MMMM d, yyyy)") + " and week " + WeekCmd.ExecuteScalar().ToString();
            con.Close();

            ClassDropDownList.SelectedIndex = 0;
            ClassDropDownList_SelectedIndexChanged(sender, e);
            MultiView1.ActiveViewIndex = 1;
        }

        protected void BackLinkButton_Click(object sender, EventArgs e)
        {
            MultiView1.ActiveViewIndex = 0;
            ExamDateCalendar.SelectedDates.Clear();
        }

        protected void StudentsGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                SqlCommand cmd = new SqlCommand("select Attendance from Attendance_Record where SchoolID = @SchoolID and EducationYearID = @EducationYearID and Attendance = 'Abs' and StudentID = @StudentID and AttendanceDate = @AttendanceDate", con);
                cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"].ToString());
                cmd.Parameters.AddWithValue("@StudentID", StudentsGridView.DataKeys[e.Row.DataItemIndex]["StudentID"].ToString());
                cmd.Parameters.AddWithValue("@AttendanceDate", ExamDateCalendar.SelectedDate);

                con.Open();
                Object Abs_Check = cmd.ExecuteScalar();
                con.Close();

                if (Abs_Check != null)
                {
                    CheckBox AttendanceCheckBox = (CheckBox)e.Row.FindControl("AbsenceCheckBox");
                    TextBox MarksTextBox = (TextBox)e.Row.FindControl("MarksTextBox");

                    AttendanceCheckBox.Checked = true;
                    MarksTextBox.Enabled = false;
                }
            }
        }
    }
}