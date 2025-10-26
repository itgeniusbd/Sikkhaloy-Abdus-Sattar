﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using Education;

namespace EDUCATION.COM.Admission.New_Student_Admission
{
    public partial class Admission_New_Student : System.Web.UI.Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            Session["Group"] = GroupDropDownList.SelectedValue;
            Session["Shift"] = ShiftDropDownList.SelectedValue;
            Session["Section"] = SectionDropDownList.SelectedValue;

            if (!IsPostBack)
            {
                try
                {
                    if (Session["Edu_Year"] == null && EducationYearDropDownList.Items.Count > 0)
                    {
                        Session["Edu_Year"] = EducationYearDropDownList.SelectedValue;
                    }

                    GroupDropDownList.Visible = false;
                    SectionDropDownList.Visible = false;
                    ShiftDropDownList.Visible = false;

                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        con.Open();
                        SqlCommand cmd = new SqlCommand("SELECT TOP 1 ID FROM Student WHERE SchoolID = @SchoolID ORDER BY StudentID DESC", con);
                        cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                        var lastID = cmd.ExecuteScalar();
                        if (lastID != null)
                        {
                            LastIDLabel.Text = "Last Entry ID: " + lastID.ToString();
                        }
                    }
                }
                catch { }
            }
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

        private int GetLastIdentity(string tableName)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand($"SELECT IDENT_CURRENT('{tableName}')", con);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "ValidationError",
                    "alert('Please fill in all required fields correctly.');", true);
                return;
            }

            try
            {
                if (Session["SchoolID"] == null)
                {
                    ClientScript.RegisterStartupScript(this.GetType(), "SessionError",
                        "alert('Session expired. Please login again.');", true);
                    return;
                }

                if (Session["Edu_Year"] == null)
                {
                    Session["Edu_Year"] = EducationYearDropDownList.SelectedValue;
                }

                if (ClassDropDownList.SelectedValue == "0")
                {
                    ClientScript.RegisterStartupScript(this.GetType(), "ClassError",
                        "alert('Please select a class.');", true);
                    return;
                }

                StudentImageSQL.Insert();
                int ImageID = GetLastIdentity("Student_Image");

                StudentInfoSQL.InsertParameters["StudentImageID"].DefaultValue = ImageID.ToString();
                StudentInfoSQL.Insert();
                int StudentID = GetLastIdentity("Student");

                if (GroupDropDownList.SelectedValue == "%")
                {
                    Session["GroupID"] = "0";
                }
                else
                {
                    Session["GroupID"] = GroupDropDownList.SelectedValue;
                }

                if (SectionDropDownList.SelectedValue == "%")
                {
                    Session["SectionID"] = "0";
                }
                else
                {
                    Session["SectionID"] = SectionDropDownList.SelectedValue;
                }

                if (ShiftDropDownList.SelectedValue == "%")
                {
                    Session["ShiftID"] = "0";
                }
                else
                {
                    Session["ShiftID"] = ShiftDropDownList.SelectedValue;
                }

                StudentClassSQL.InsertParameters["StudentID"].DefaultValue = StudentID.ToString();
                StudentClassSQL.Insert();
                int StudentClassID = GetLastIdentity("StudentsClass");
                Session["StudentClassID"] = StudentClassID;

                foreach (GridViewRow row in GroupGridView.Rows)
                {
                    CheckBox SubjectCheckBox = (CheckBox)row.FindControl("SubjectCheckBox");
                    if (SubjectCheckBox != null && SubjectCheckBox.Checked)
                    {
                        RadioButtonList SubjectTypeRadioButtonList = (RadioButtonList)row.FindControl("SubjectTypeRadioButtonList");
                        if (SubjectTypeRadioButtonList != null && !string.IsNullOrEmpty(SubjectTypeRadioButtonList.SelectedValue))
                        {
                            StudentRecordSQL.InsertParameters["StudentID"].DefaultValue = StudentID.ToString();
                            StudentRecordSQL.InsertParameters["SubjectID"].DefaultValue = GroupGridView.DataKeys[row.RowIndex].Values["SubjectID"].ToString();
                            StudentRecordSQL.InsertParameters["SubjectType"].DefaultValue = SubjectTypeRadioButtonList.SelectedValue;
                            StudentRecordSQL.Insert();
                        }
                    }
                }

                if (SMSCheckBox.Checked && !string.IsNullOrEmpty(SMSPhoneNoTextBox.Text))
                {
                    SendAdmissionSMS(StudentNameTextBox.Text, IDTextBox.Text, ClassDropDownList.SelectedItem.Text,
                        RollNumberTextBox.Text, SMSPhoneNoTextBox.Text, StudentID.ToString(), false);
                }

                if (BanglaSMSCheckBox.Checked && !string.IsNullOrEmpty(SMSPhoneNoTextBox.Text))
                {
                    SendAdmissionSMS(StudentNameTextBox.Text, IDTextBox.Text, ClassDropDownList.SelectedItem.Text,
                        RollNumberTextBox.Text, SMSPhoneNoTextBox.Text, StudentID.ToString(), true);
                }

                if (PrintCheckBox.Checked)
                {
                    Response.Redirect($"Admission_Form.aspx?Student={StudentID}&StudentClass={StudentClassID}", false);
                    Context.ApplicationInstance.CompleteRequest();
                }
                else if (BanglaPrintCheckBox.Checked)
                {
                    Response.Redirect($"Form_Bangla.aspx?Student={StudentID}&StudentClass={StudentClassID}", false);
                    Context.ApplicationInstance.CompleteRequest();
                }
                else
                {
                    ClientScript.RegisterStartupScript(this.GetType(), "Success",
                        "alert('Admission completed successfully! Student ID: " + StudentID + "'); window.location='Admission_New_Student.aspx';", true);
                }
            }
            catch (SqlException sqlEx)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "SQLError",
                    $"alert('Database Error: {sqlEx.Message.Replace("'", "\\'")}');", true);
            }
            catch (Exception ex)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "Error",
                    $"alert('Error: {ex.Message.Replace("'", "\\'")}');", true);
            }
        }

        protected void GoPayorderButton_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "ValidationError",
                    "alert('Please fill in all required fields correctly.');", true);
                return;
            }

            try
            {
                if (Session["SchoolID"] == null)
                {
                    ClientScript.RegisterStartupScript(this.GetType(), "SessionError",
                        "alert('Session expired. Please login again.');", true);
                    return;
                }

                if (Session["Edu_Year"] == null)
                {
                    Session["Edu_Year"] = EducationYearDropDownList.SelectedValue;
                }

                if (ClassDropDownList.SelectedValue == "0")
                {
                    ClientScript.RegisterStartupScript(this.GetType(), "ClassError",
                        "alert('Please select a class.');", true);
                    return;
                }

                StudentImageSQL.Insert();
                int ImageID = GetLastIdentity("Student_Image");

                StudentInfoSQL.InsertParameters["StudentImageID"].DefaultValue = ImageID.ToString();
                StudentInfoSQL.Insert();
                int StudentID = GetLastIdentity("Student");

                if (GroupDropDownList.SelectedValue == "%")
                {
                    Session["GroupID"] = "0";
                }
                else
                {
                    Session["GroupID"] = GroupDropDownList.SelectedValue;
                }

                if (SectionDropDownList.SelectedValue == "%")
                {
                    Session["SectionID"] = "0";
                }
                else
                {
                    Session["SectionID"] = SectionDropDownList.SelectedValue;
                }

                if (ShiftDropDownList.SelectedValue == "%")
                {
                    Session["ShiftID"] = "0";
                }
                else
                {
                    Session["ShiftID"] = ShiftDropDownList.SelectedValue;
                }

                StudentClassSQL.InsertParameters["StudentID"].DefaultValue = StudentID.ToString();
                StudentClassSQL.Insert();
                int StudentClassID = GetLastIdentity("StudentsClass");
                Session["StudentClassID"] = StudentClassID;

                foreach (GridViewRow row in GroupGridView.Rows)
                {
                    CheckBox SubjectCheckBox = (CheckBox)row.FindControl("SubjectCheckBox");
                    if (SubjectCheckBox != null && SubjectCheckBox.Checked)
                    {
                        RadioButtonList SubjectTypeRadioButtonList = (RadioButtonList)row.FindControl("SubjectTypeRadioButtonList");
                        if (SubjectTypeRadioButtonList != null && !string.IsNullOrEmpty(SubjectTypeRadioButtonList.SelectedValue))
                        {
                            StudentRecordSQL.InsertParameters["StudentID"].DefaultValue = StudentID.ToString();
                            StudentRecordSQL.InsertParameters["SubjectID"].DefaultValue = GroupGridView.DataKeys[row.RowIndex].Values["SubjectID"].ToString();
                            StudentRecordSQL.InsertParameters["SubjectType"].DefaultValue = SubjectTypeRadioButtonList.SelectedValue;
                            StudentRecordSQL.Insert();
                        }
                    }
                }

                if (SMSCheckBox.Checked && !string.IsNullOrEmpty(SMSPhoneNoTextBox.Text))
                {
                    SendAdmissionSMS(StudentNameTextBox.Text, IDTextBox.Text, ClassDropDownList.SelectedItem.Text,
                        RollNumberTextBox.Text, SMSPhoneNoTextBox.Text, StudentID.ToString(), false);
                }

                if (BanglaSMSCheckBox.Checked && !string.IsNullOrEmpty(SMSPhoneNoTextBox.Text))
                {
                    SendAdmissionSMS(StudentNameTextBox.Text, IDTextBox.Text, ClassDropDownList.SelectedItem.Text,
                        RollNumberTextBox.Text, SMSPhoneNoTextBox.Text, StudentID.ToString(), true);
                }

                Response.Cookies["Class"].Value = ClassDropDownList.SelectedItem.Text;
                Response.Cookies["RollNo"].Value = RollNumberTextBox.Text;
                Response.Cookies["Admission_Year"].Value = Session["Edu_Year"].ToString();
                Response.Cookies["Admission_Year"].Expires = DateTime.Now.AddDays(1);

                Response.Redirect($"Payments.aspx?Student={StudentID}&Class={ClassDropDownList.SelectedValue}&StudentClass={StudentClassID}", false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (SqlException sqlEx)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "SQLError",
                    $"alert('Database Error: {sqlEx.Message.Replace("'", "\\'")}');", true);
            }
            catch (Exception ex)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "Error",
                    $"alert('Error: {ex.Message.Replace("'", "\\'")}');", true);
            }
        }

        private void SendAdmissionSMS(string studentName, string studentID, string className, string rollNo, string phone, string studentIDforDB, bool isBangla)
        {
            try
            {
                string Text;
                
                if (isBangla)
                {
                    // Bangla SMS format
                    Text = $"অভিনন্দন! {studentName} আপনি। {className},শ্রেণীতে অনলাইনে ভর্তি হয়েছেন। আইডি: {studentID}, রোল: {rollNo} ভবিষ্যতের জন্য এই তথ্য সংরক্ষণ করুন। ধন্যবাদ - {Session["School_Name"]}";
                }
                else
                {
                    // English SMS format
                    Text = $"Congratulation!! {studentName} You have been Online admitted into class: {className}. ID: {studentID}. Roll No: {rollNo} Please save this information for future. Regards: {Session["School_Name"]}";
                }

                SMS_Class SMS = new SMS_Class(Session["SchoolID"].ToString());
                int TotalSMS = SMS.SMS_Conut(Text);
                int SMSBalance = SMS.SMSBalance;

                if (SMSBalance >= TotalSMS)
                {
                    if (SMS.SMS_GetBalance() >= TotalSMS)
                    {
                        Get_Validation IsValid = SMS.SMS_Validation(phone, Text);

                        if (IsValid.Validation)
                        {
                            Guid SMS_Send_ID = SMS.SMS_Send(phone, Text, "Admission");

                            if (SMS_Send_ID != Guid.Empty)
                            {
                                using (SqlConnection con = new SqlConnection(connectionString))
                                {
                                    con.Open();
                                    SqlCommand cmd = new SqlCommand(@"
                                        INSERT INTO SMS_OtherInfo(SMS_Send_ID, SchoolID, StudentID, TeacherID, EducationYearID) 
                                        VALUES (@SMS_Send_ID, @SchoolID, @StudentID, @TeacherID, @EducationYearID)", con);

                                    cmd.Parameters.AddWithValue("@SMS_Send_ID", SMS_Send_ID);
                                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                                    cmd.Parameters.AddWithValue("@StudentID", studentIDforDB);
                                    cmd.Parameters.AddWithValue("@TeacherID", "");
                                    cmd.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"]);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SMS Error: " + ex.Message);
            }
        }

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

        protected void GroupDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            SectionDropDownList.DataBind();
            ShiftDropDownList.DataBind();
            view();
        }

        protected void SectionDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShiftDropDownList.DataBind();
            view();
        }

        protected void ShiftDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            GroupGridView.DataBind();
        }

        protected void GroupDropDownList_DataBound(object sender, EventArgs e)
        {
            GroupDropDownList.Items.Insert(0, new ListItem("[ ALL ]", "%"));
            if (IsPostBack)
                GroupDropDownList.Items.FindByValue(Session["Group"].ToString()).Selected = true;
        }

        protected void SectionDropDownList_DataBound(object sender, EventArgs e)
        {
            SectionDropDownList.Items.Insert(0, new ListItem("[ ALL ]", "%"));
            if (IsPostBack)
                SectionDropDownList.Items.FindByValue(Session["Section"].ToString()).Selected = true;
        }

        protected void ShiftDropDownList_DataBound(object sender, EventArgs e)
        {
            ShiftDropDownList.Items.Insert(0, new ListItem("[ ALL ]", "%"));
            if (IsPostBack)
                ShiftDropDownList.Items.FindByValue(Session["Shift"].ToString()).Selected = true;
        }

        [WebMethod]
        public static string GetAllID(string ids)
        {
            try
            {
                List<string> IDList = new List<string>();
                string connString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("SELECT TOP 10 ID FROM Student WHERE ID LIKE @ID + '%' ORDER BY ID", con);
                    cmd.Parameters.AddWithValue("@ID", ids);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        IDList.Add(reader["ID"].ToString());
                    }
                }
                return new JavaScriptSerializer().Serialize(IDList);
            }
            catch
            {
                return "[]";
            }
        }
    }
}
