using Education;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Admission.New_Student_Admission
{
    public partial class Class_And_Subject : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
        protected void Page_Load(object sender, EventArgs e)
        {
            Session["Group"] = GroupDropDownList.SelectedValue;
            Session["Shift"] = ShiftDropDownList.SelectedValue;
            Session["Section"] = SectionDropDownList.SelectedValue;

            if (!IsPostBack)
            {
                if (string.IsNullOrEmpty(Request.QueryString["Student"]))
                {
                    Response.Redirect("Admission_New_Student.aspx");
                }

                GroupDropDownList.Visible = false;
                SectionDropDownList.Visible = false;
                ShiftDropDownList.Visible = false;
            }
        }

        protected void view()
        {
            DataView GroupDV = new DataView();
            GroupDV = (DataView)GroupSQL.Select(DataSourceSelectArguments.Empty);
            if (GroupDV.Count < 1)
            {
                GroupDropDownList.Visible = false;
                GRequiredFieldValidator.Visible = false;
            }
            else
            {
                GroupDropDownList.Visible = true;
                GRequiredFieldValidator.Visible = true;
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

        //Class DDL
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
        //Group DDL
        protected void GroupDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
        }
        protected void GroupDropDownList_DataBound(object sender, EventArgs e)
        {
            GroupDropDownList.Items.Insert(0, new ListItem("[ SELECT GROUP ]", "%"));
            if (IsPostBack)
                GroupDropDownList.Items.FindByValue(Session["Group"].ToString()).Selected = true;
        }
        //Section DDL
        protected void SectionDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
        }
        protected void SectionDropDownList_DataBound(object sender, EventArgs e)
        {
            SectionDropDownList.Items.Insert(0, new ListItem("[ SELECT SECTION ]", "%"));
            if (IsPostBack)
                SectionDropDownList.Items.FindByValue(Session["Section"].ToString()).Selected = true;
        }

        //Shift DDL
        protected void ShiftDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
        }
        protected void ShiftDropDownList_DataBound(object sender, EventArgs e)
        {
            ShiftDropDownList.Items.Insert(0, new ListItem("[  SELECT SHIFT ]", "%"));
            if (IsPostBack)
                ShiftDropDownList.Items.FindByValue(Session["Shift"].ToString()).Selected = true;
        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            SqlCommand StudentClass_cmd = new SqlCommand("SELECT * FROM StudentsClass WHERE (StudentID = @StudentID) AND (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID)", con);
            StudentClass_cmd.Parameters.AddWithValue("@StudentID", Request.QueryString["Student"]);
            StudentClass_cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
            StudentClass_cmd.Parameters.AddWithValue("@EducationYearID", Request.Cookies["Admission_Year"].Value);

            con.Open();
            object StudentClass_Obj = StudentClass_cmd.ExecuteScalar();
            con.Close();

            if (StudentClass_Obj == null)
            {
                #region Class-group-section-shift dropdownlist

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
                #endregion


                StudentClassSQL.Insert();
                SqlCommand cmd = new SqlCommand("Select IDENT_CURRENT('StudentsClass')", con);

                con.Open();
                Session["StudentClassID"] = cmd.ExecuteScalar().ToString();
                con.Close();


                foreach (GridViewRow row in GroupGridView.Rows)
                {
                    CheckBox SubjectCheckBox = (CheckBox)row.FindControl("SubjectCheckBox");
                    RadioButtonList SubjectType = (RadioButtonList)row.FindControl("SubjectTypeRadioButtonList");

                    if (SubjectCheckBox.Checked)
                    {
                        StudentRecordSQL.InsertParameters["SubjectID"].DefaultValue = GroupGridView.DataKeys[row.DataItemIndex]["SubjectID"].ToString();
                        StudentRecordSQL.InsertParameters["SubjectType"].DefaultValue = SubjectType.SelectedValue;
                        StudentRecordSQL.Insert();
                    }
                }

                if (SMSCheckBox.Checked) //Send admission confirmation sms
                {
                    SMS();
                }


                Response.Cookies["StudentName"].Expires = DateTime.Now;
                Response.Cookies["ID"].Expires = DateTime.Now;
                Response.Cookies["SMSPhone"].Expires = DateTime.Now;
                Response.Cookies["Class"].Expires = DateTime.Now;
                Response.Cookies["RolNo"].Expires = DateTime.Now;

                //For Update student info in device
                Device_DataUpdateSQL.Insert();

                if (PrintCheckBox.Checked)
                {
                    Response.Redirect("Admission_Form.aspx?Student=" + Request.QueryString["Student"] + "&StudentClass=" + Session["StudentClassID"].ToString());
                }
                else
                {
                    Response.Redirect("Admission_New_Student.aspx");
                }
            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('This Student has been already admitted in this session')", true);
            }
        }

        protected void GoPayorderButton_Click(object sender, EventArgs e)
        {
            SqlCommand StudentClass_cmd = new SqlCommand("SELECT * FROM  StudentsClass WHERE (StudentID = @StudentID) AND (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID)", con);
            StudentClass_cmd.Parameters.AddWithValue("@StudentID", Request.QueryString["Student"].ToString());
            StudentClass_cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
            StudentClass_cmd.Parameters.AddWithValue("@EducationYearID", Request.Cookies["Admission_Year"].Value);

            con.Open();
            object StudentClass_Obj = StudentClass_cmd.ExecuteScalar();
            con.Close();

            if (StudentClass_Obj == null)
            {
                #region Class-group-section-shift dropdownlist

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
                #endregion

                StudentClassSQL.Insert();
                SqlCommand cmd = new SqlCommand("Select IDENT_CURRENT('StudentsClass')", con);

                con.Open();
                Session["StudentClassID"] = cmd.ExecuteScalar().ToString();
                con.Close();


                foreach (GridViewRow row in GroupGridView.Rows)//insert Compulsory subject
                {
                    CheckBox SubjectCheckBox = (CheckBox)row.FindControl("SubjectCheckBox");
                    RadioButtonList SubjectType = (RadioButtonList)row.FindControl("SubjectTypeRadioButtonList");

                    if (SubjectCheckBox.Checked)
                    {
                        StudentRecordSQL.InsertParameters["SubjectID"].DefaultValue = GroupGridView.DataKeys[row.DataItemIndex]["SubjectID"].ToString();
                        StudentRecordSQL.InsertParameters["SubjectType"].DefaultValue = SubjectType.SelectedValue;
                        StudentRecordSQL.Insert();
                    }
                }

                if (SMSCheckBox.Checked) //Send admission confirmation sms
                {
                    SMS();
                }

                Response.Cookies["Class"].Value = ClassDropDownList.SelectedItem.Text;
                Response.Cookies["RollNo"].Value = RollNumberTextBox.Text;
                Response.Redirect("Payments.aspx?Student=" + Request.QueryString["Student"].ToString() + "&Class=" + ClassDropDownList.SelectedValue + "&StudentClass=" + Session["StudentClassID"]);
            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('This Student has been already admitted in this session')", true);
            }
        }

        protected void SMS()
        {
            ErrorLabel.Text = "";
            string Name = Request.Cookies["StudentName"].Value;
            string ID = Request.Cookies["ID"].Value;
            string Class = ClassDropDownList.SelectedItem.Text;
            string Roll = RollNumberTextBox.Text;
            string Phone = Request.Cookies["SMSPhone"].Value;

            string Text = "Congratulation!! " + Name + " You have been Online admitted into class: " + Class + ". ID: " + ID + ". Roll No: " + Roll + " Please save this information for future. Regards: " + Session["School_Name"].ToString();

            SMS_Class SMS = new SMS_Class(Session["SchoolID"].ToString());

            int TotalSMS = 0;
            int SMSBalance = SMS.SMSBalance;


            TotalSMS += SMS.SMS_Conut(Text);


            if (SMSBalance >= TotalSMS)
            {
                if (SMS.SMS_GetBalance() >= TotalSMS)
                {
                    Get_Validation IsValid = SMS.SMS_Validation(Phone, Text);

                    if (IsValid.Validation)
                    {
                        Guid SMS_Send_ID = SMS.SMS_Send(Phone, Text, "Admission");
                        if (SMS_Send_ID != Guid.Empty)
                        {
                            SMS_OtherInfoSQL.InsertParameters["SMS_Send_ID"].DefaultValue = SMS_Send_ID.ToString();
                            SMS_OtherInfoSQL.InsertParameters["SchoolID"].DefaultValue = Session["SchoolID"].ToString();
                            SMS_OtherInfoSQL.InsertParameters["EducationYearID"].DefaultValue = Session["Edu_Year"].ToString();
                            SMS_OtherInfoSQL.InsertParameters["StudentID"].DefaultValue = Request.QueryString["Student"].ToString();
                            SMS_OtherInfoSQL.InsertParameters["TeacherID"].DefaultValue = "";

                            SMS_OtherInfoSQL.Insert();
                        }
                    }
                    else
                    {
                        ErrorLabel.Text = IsValid.Message;
                    }
                }
                else
                {
                    ErrorLabel.Text = "SMS Service Updating. Try again later or contact to authority";
                }
            }
            else
            {
                ErrorLabel.Text = "You don't have sufficient SMS balance, Your Current Balance is " + SMSBalance;
            }
        }
    }
}