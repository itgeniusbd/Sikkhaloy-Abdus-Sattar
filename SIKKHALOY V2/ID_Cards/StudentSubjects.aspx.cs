using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.ID_Cards
{
    public partial class StudentSubjects : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
        protected void Page_Load(object sender, EventArgs e)
        {
            try
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
            catch { FormsAuthentication.SignOut(); }
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

        }
        protected void ClassDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session["Group"] = "%";
            Session["Shift"] = "%";
            Session["Section"] = "%";
            Session["Subject"] = "0";

            GroupDropDownList.DataBind();
            ShiftDropDownList.DataBind();
            SectionDropDownList.DataBind();

            view();

            con.Open();
            foreach (GridViewRow row in AllStu_SubChangeGridView.Rows)
            {
                CheckBox SubjectCheckBox = (CheckBox)row.FindControl("SubjectCheckBox");
                RadioButtonList SubjectType = (RadioButtonList)row.FindControl("SubjectTypeRadioButtonList");

                SqlCommand SubjectTypecmd = new SqlCommand("select SubjectType from SubjectForGroup where ClassID = @ClassID and SubjectID = @SubjectID and SubjectGroupID Like @SubjectGroupID", con);
                SubjectTypecmd.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);
                SubjectTypecmd.Parameters.AddWithValue("@SubjectID", AllStu_SubChangeGridView.DataKeys[row.DataItemIndex].Value.ToString());
                SubjectTypecmd.Parameters.AddWithValue("@SubjectGroupID", GroupDropDownList.SelectedValue);

                object CheckSub = SubjectTypecmd.ExecuteScalar();

                if (CheckSub != null)
                {
                    SubjectCheckBox.Checked = true;
                    SubjectType.SelectedValue = SubjectTypecmd.ExecuteScalar().ToString();
                    row.CssClass = "selected";
                }
                else
                {
                    SubjectCheckBox.Checked = false;
                    SubjectType.SelectedIndex = -1;
                    row.CssClass = "Deselected";
                }
            }
            con.Close();
        }

        protected void GroupDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session["Subject"] = "0";
            view();

            con.Open();
            foreach (GridViewRow row in AllStu_SubChangeGridView.Rows)
            {
                CheckBox SubjectCheckBox = (CheckBox)row.FindControl("SubjectCheckBox");
                RadioButtonList SubjectType = (RadioButtonList)row.FindControl("SubjectTypeRadioButtonList");

                SqlCommand SubjectTypecmd = new SqlCommand("select SubjectType from SubjectForGroup where ClassID = @ClassID and SubjectID = @SubjectID and SubjectGroupID Like @SubjectGroupID", con);
                SubjectTypecmd.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);
                SubjectTypecmd.Parameters.AddWithValue("@SubjectID", AllStu_SubChangeGridView.DataKeys[row.DataItemIndex].Value.ToString());
                SubjectTypecmd.Parameters.AddWithValue("@SubjectGroupID", GroupDropDownList.SelectedValue);

                object CheckSub = SubjectTypecmd.ExecuteScalar();

                if (CheckSub != null)
                {
                    SubjectCheckBox.Checked = true;
                    SubjectType.SelectedValue = SubjectTypecmd.ExecuteScalar().ToString();
                    row.CssClass = "selected";
                }
                else
                {
                    SubjectCheckBox.Checked = false;
                    SubjectType.SelectedIndex = -1;
                    row.CssClass = "Deselected";
                }
            }
            con.Close();
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
        protected void SubjectDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            con.Open();
            foreach (GridViewRow row in StudentsGridView.Rows)
            {
                CheckBox SubjectCheckBox = (CheckBox)row.FindControl("SubjectCheckBox");
                RadioButtonList SubjectType = (RadioButtonList)row.FindControl("SubjectTypeRadioButtonList");

                SqlCommand SubjectTypecmd = new SqlCommand("SELECT  SubjectType FROM  StudentRecord WHERE (SubjectID = @SubjectID) AND (StudentClassID = @StudentClassID) AND (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID)", con);
                SubjectTypecmd.Parameters.AddWithValue("@SubjectID", SubjectDropDownList.SelectedValue);
                SubjectTypecmd.Parameters.AddWithValue("@StudentClassID", StudentsGridView.DataKeys[row.DataItemIndex]["StudentClassID"].ToString());
                SubjectTypecmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"].ToString());
                SubjectTypecmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());

                Object CheckSub = SubjectTypecmd.ExecuteScalar();

                if (CheckSub != null)
                {
                    SubjectCheckBox.Checked = true;
                    SubjectType.SelectedValue = SubjectTypecmd.ExecuteScalar().ToString();
                    row.CssClass = "selected";
                }
                else
                {
                    SubjectCheckBox.Checked = false;
                    SubjectType.SelectedIndex = -1;
                    row.CssClass = "Deselected";
                }
            }
            con.Close();
        }
        //End DDL

        protected void AllSubSubmitButton_Click(object sender, EventArgs e)
        {
            try
            {
                bool Is_Subject_Checked = true;
                bool Is_SubjectType_Checked = true;
                foreach (GridViewRow row in AllStu_SubChangeGridView.Rows)
                {
                    CheckBox SubjectCheckBox = (CheckBox)row.FindControl("SubjectCheckBox");
                    RadioButtonList SubjectType = (RadioButtonList)row.FindControl("SubjectTypeRadioButtonList");

                    if (SubjectCheckBox.Checked)
                    {
                        Is_Subject_Checked = false;
                        //If SubjectType not Selected
                        if (SubjectType.SelectedIndex == -1)
                        {
                            Is_SubjectType_Checked = false;
                        }
                    }
                }

                if (Is_Subject_Checked)
                {
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Please Select Subject.')", true);
                }
                else
                {
                    if (Is_SubjectType_Checked)
                    {
                        SqlCommand AllStudentCmd = new SqlCommand(
                            "SELECT Student.ID, StudentsClass.StudentClassID, StudentsClass.StudentID FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID WHERE (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID) AND (StudentsClass.SubjectGroupID LIKE @SubjectGroupID) AND (StudentsClass.ShiftID LIKE @ShiftID) AND (Student.Status = 'Active') AND (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.SchoolID = @SchoolID)",
                            con);

                        AllStudentCmd.Parameters.AddWithValue("@ClassID", ClassDropDownList.SelectedValue);
                        AllStudentCmd.Parameters.AddWithValue("@SubjectGroupID", GroupDropDownList.SelectedValue);
                        AllStudentCmd.Parameters.AddWithValue("@SectionID", SectionDropDownList.SelectedValue);
                        AllStudentCmd.Parameters.AddWithValue("@ShiftID", ShiftDropDownList.SelectedValue);
                        AllStudentCmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"].ToString());
                        AllStudentCmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());


                        con.Open();
                        SqlDataReader AllSubjectDR;

                        AllSubjectDR = AllStudentCmd.ExecuteReader();

                        while (AllSubjectDR.Read())
                        {
                            StudentSubjectRecordsSQL.DeleteParameters["StudentClassID"].DefaultValue =
                                AllSubjectDR["StudentClassID"].ToString();
                            StudentSubjectRecordsSQL.Delete();

                            foreach (GridViewRow row in AllStu_SubChangeGridView.Rows)
                            {
                                CheckBox SubjectCheckBox = (CheckBox)row.FindControl("SubjectCheckBox");
                                RadioButtonList SubjectType =
                                    (RadioButtonList)row.FindControl("SubjectTypeRadioButtonList");
                                if (SubjectCheckBox.Checked)
                                {
                                    Is_Subject_Checked = false;
                                    string SubjectID = AllStu_SubChangeGridView.DataKeys[row.DataItemIndex].Value
                                        .ToString();
                                    string Subject_Type = SubjectType.SelectedValue;

                                    StudentSubjectRecordsSQL.InsertParameters["SubjectID"].DefaultValue = SubjectID;
                                    StudentSubjectRecordsSQL.InsertParameters["SubjectType"].DefaultValue =
                                        Subject_Type;
                                    StudentSubjectRecordsSQL.InsertParameters["StudentID"].DefaultValue =
                                        AllSubjectDR["StudentID"].ToString();
                                    StudentSubjectRecordsSQL.InsertParameters["StudentClassID"].DefaultValue =
                                        AllSubjectDR["StudentClassID"].ToString();
                                    StudentSubjectRecordsSQL.Insert();
                                }
                            }
                        }

                        con.Close();

                        #region Subject Assign in class

                        CreateGroupSQL.Delete();
                        foreach (GridViewRow row in AllStu_SubChangeGridView.Rows)
                        {
                            CheckBox SubjectCheckBox = (CheckBox)row.FindControl("SubjectCheckBox");
                            RadioButtonList SubjectType = (RadioButtonList)row.FindControl("SubjectTypeRadioButtonList");
                            if (SubjectCheckBox.Checked)
                            {
                                Is_Subject_Checked = false;
                                string SubjectID = AllStu_SubChangeGridView.DataKeys[row.DataItemIndex].Value.ToString();
                                string Subject_Type = SubjectType.SelectedValue;

                                CreateGroupSQL.InsertParameters["SubjectID"].DefaultValue = SubjectID;
                                CreateGroupSQL.InsertParameters["SubjectType"].DefaultValue = Subject_Type;
                                CreateGroupSQL.InsertParameters["SubjectGroupID"].DefaultValue = GroupDropDownList.SelectedValue == "%" ? "0" : GroupDropDownList.SelectedValue.ToString();

                                CreateGroupSQL.Insert();
                            }
                            else
                            {
                                SubjectType.SelectedIndex = -1;
                            }
                        }

                        #endregion Subject Assign in class

                        //AllStu_SubChangeGridView.DataBind();

                        UpdateStudentRecordIDSQL.UpdateParameters["SchoolID"].DefaultValue =
                            Session["SchoolID"].ToString();
                        UpdateStudentRecordIDSQL.UpdateParameters["EducationYearID"].DefaultValue =
                            Session["Edu_Year"].ToString();
                        UpdateStudentRecordIDSQL.UpdateParameters["ClassID"].DefaultValue =
                            ClassDropDownList.SelectedValue;
                        UpdateStudentRecordIDSQL.Update();




                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage",
                            "alert('Subjects Assigned Successfully.')", true);
                    }
                    else
                    {
                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage",
                            "alert('Please Select Subject Type.')", true);
                    }
                }
            }
            catch
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('System error.')", true);
            }
        }

        protected void All_Student_SubmitButton_Click(object sender, EventArgs e)
        {
            foreach (GridViewRow row in StudentsGridView.Rows)
            {
                CheckBox SubjectCheckBox = (CheckBox)row.FindControl("SubjectCheckBox");
                RadioButtonList SubjectType = (RadioButtonList)row.FindControl("SubjectTypeRadioButtonList");

                if (SubjectCheckBox.Checked)
                {
                    SubjectSQL.InsertParameters["SubjectType"].DefaultValue = SubjectType.SelectedValue;
                    SubjectSQL.InsertParameters["StudentID"].DefaultValue = StudentsGridView.DataKeys[row.DataItemIndex]["StudentID"].ToString();
                    SubjectSQL.InsertParameters["StudentClassID"].DefaultValue = StudentsGridView.DataKeys[row.DataItemIndex]["StudentClassID"].ToString();
                    SubjectSQL.Insert();
                }
                else
                {
                    SubjectSQL.DeleteParameters["StudentClassID"].DefaultValue = StudentsGridView.DataKeys[row.DataItemIndex]["StudentClassID"].ToString();
                    SubjectSQL.Delete();
                }
            }

            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Subjects Assigned Successfully.')", true);
        }

    }
}