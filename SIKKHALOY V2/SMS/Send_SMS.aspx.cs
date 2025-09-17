using Education;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.SMS
{
    public partial class Send_SMS : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Session["Group"] = GroupDropDownList.SelectedValue;
            Session["Shift"] = ShiftDropDownList.SelectedValue;
            Session["Section"] = SectionDropDownList.SelectedValue;
            try
            {
                if (!IsPostBack)
                {
                    GroupDropDownList.Visible = false;
                    SectionDropDownList.Visible = false;
                    ShiftDropDownList.Visible = false;
                }

                SMSMultiView.ActiveViewIndex = SelectRadioButtonList.SelectedIndex;
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

            AllStudentsGridView.DataSource = AllStudentsSQL;
            AllStudentsGridView.DataBind();
            IDTextBox.Text = "";
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
            GroupDropDownList.Items.Insert(0, new ListItem("[ ALL GROUP ]", "%"));
            if (IsPostBack)
                GroupDropDownList.Items.FindByValue(Session["Group"].ToString()).Selected = true;
        }

        //-Section DDL
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

        //Shift DDL
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

        //End DDL

        protected void SMSButton_Click(object sender, EventArgs e)
        {
            ErrorLabel.Text = "";

            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());

            string Msg = SMSTextBox.Text;

            SMS_Class SMS = new SMS_Class(Session["SchoolID"].ToString());

            int TotalSMS = 0;
            string PhoneNo = "";
            bool SentMgsConfirm = false;
            int SentMsgCont = 0;
            int FailedMsgCont = 0;

            int SMSBalance = SMS.SMSBalance;


            #region Send SMS To Selected Students
            if (SMSMultiView.ActiveViewIndex == 0)
            {
                foreach (GridViewRow Row in AllStudentsGridView.Rows)
                {
                    CheckBox SMSCheckBox = Row.FindControl("SelectCheckBox") as CheckBox;

                    if (SMSCheckBox.Checked)
                    {
                        PhoneNo = AllStudentsGridView.DataKeys[Row.DataItemIndex]["SMSPhoneNo"].ToString();

                        Get_Validation IsValid = SMS.SMS_Validation(PhoneNo, Msg);

                        if (IsValid.Validation)
                        {
                            TotalSMS += SMS.SMS_Conut(Msg);
                        }

                    }
                }


                if (SMSBalance >= TotalSMS)
                {
                    if (SMS.SMS_GetBalance() >= TotalSMS)
                    {
                        foreach (GridViewRow ROW in AllStudentsGridView.Rows)
                        {
                            CheckBox SelectCheckbox = (CheckBox)ROW.FindControl("SelectCheckBox");
                            if (SelectCheckbox.Checked)
                            {
                                CheckBox SMSCheckBox = ROW.FindControl("SelectCheckBox") as CheckBox;

                                if (SMSCheckBox.Checked)
                                {
                                    PhoneNo = AllStudentsGridView.DataKeys[ROW.DataItemIndex]["SMSPhoneNo"].ToString();

                                    Get_Validation IsValid = SMS.SMS_Validation(PhoneNo, Msg);

                                    if (IsValid.Validation)
                                    {
                                        Guid SMS_Send_ID = SMS.SMS_Send(PhoneNo, Msg, "SMS Service");
                                        if (SMS_Send_ID != Guid.Empty)
                                        {
                                            SMS_OtherInfoSQL.InsertParameters["SMS_Send_ID"].DefaultValue = SMS_Send_ID.ToString();
                                            SMS_OtherInfoSQL.InsertParameters["SchoolID"].DefaultValue = Session["SchoolID"].ToString();
                                            SMS_OtherInfoSQL.InsertParameters["EducationYearID"].DefaultValue = Session["Edu_Year"].ToString();
                                            SMS_OtherInfoSQL.InsertParameters["StudentID"].DefaultValue = AllStudentsGridView.DataKeys[ROW.DataItemIndex]["StudentID"].ToString();
                                            SMS_OtherInfoSQL.InsertParameters["TeacherID"].DefaultValue = "";

                                            SMS_OtherInfoSQL.Insert();
                                            SentMsgCont++;
                                            SentMgsConfirm = true;
                                        }
                                        else
                                        {
                                            ROW.BackColor = System.Drawing.Color.Red;
                                            FailedMsgCont++;
                                        }
                                    }
                                    else
                                    {
                                        ROW.BackColor = System.Drawing.Color.Red;
                                        FailedMsgCont++;
                                    }
                                }

                            }
                        }


                        if (SentMgsConfirm)
                        {
                            SMSTextBox.Text = string.Empty;
                            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('You Have Successfully Sent " + SentMsgCont.ToString() + " SMS and Failed " + FailedMsgCont.ToString() + ".')", true);
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


            #endregion

            #region Send SMS to All students
            if (SMSMultiView.ActiveViewIndex == 1)
            {
                con.Open();
                SqlCommand SMScommand = new SqlCommand("SELECT Student.StudentID, Student.SMSPhoneNo FROM Student INNER JOIN StudentsClass ON Student.StudentID = StudentsClass.StudentID WHERE(StudentsClass.EducationYearID = @EducationYearID) AND (Student.Status = N'Active') AND (StudentsClass.SchoolID = @SchoolID)", con);
                SMScommand.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                SMScommand.Parameters.AddWithValue("@EducationYearID", Session["Edu_Year"].ToString());

                SqlDataReader SMSDR;
                SMSDR = SMScommand.ExecuteReader();

                while (SMSDR.Read())
                {
                    PhoneNo = SMSDR["SMSPhoneNo"].ToString();

                    Get_Validation IsValid = SMS.SMS_Validation(PhoneNo, Msg);
                    if (IsValid.Validation)
                    {
                        TotalSMS += SMS.SMS_Conut(Msg);
                    }
                }
                con.Close();

                if (SMSBalance >= TotalSMS)
                {
                    if (SMS.SMS_GetBalance() >= TotalSMS)
                    {
                        con.Open();
                        SMSDR = SMScommand.ExecuteReader();

                        while (SMSDR.Read())
                        {
                            PhoneNo = SMSDR["SMSPhoneNo"].ToString();
                            Get_Validation IsValid = SMS.SMS_Validation(PhoneNo, Msg);

                            if (IsValid.Validation)
                            {
                                Guid SMS_Send_ID = SMS.SMS_Send(PhoneNo, Msg, "SMS Service");
                                if (SMS_Send_ID != Guid.Empty)
                                {
                                    SMS_OtherInfoSQL.InsertParameters["SMS_Send_ID"].DefaultValue = SMS_Send_ID.ToString();
                                    SMS_OtherInfoSQL.InsertParameters["SchoolID"].DefaultValue = Session["SchoolID"].ToString();
                                    SMS_OtherInfoSQL.InsertParameters["EducationYearID"].DefaultValue = Session["Edu_Year"].ToString();
                                    SMS_OtherInfoSQL.InsertParameters["StudentID"].DefaultValue = SMSDR["StudentID"].ToString();
                                    SMS_OtherInfoSQL.InsertParameters["TeacherID"].DefaultValue = "";

                                    SMS_OtherInfoSQL.Insert();
                                    SentMsgCont++;
                                    SentMgsConfirm = true;
                                }
                            }
                        }
                        con.Close();

                        if (SentMgsConfirm)
                        {
                            SMSTextBox.Text = string.Empty;
                            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('You Have Successfully Sent " + SentMsgCont.ToString() + " SMS.')", true);
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

            #endregion

            #region Send single SMS

            if (SMSMultiView.ActiveViewIndex == 2)
            {
                PhoneNo = SingleMobileNoTextBox.Text;
                string[] nums =(PhoneNo.Split(','));



                TotalSMS = SMS.SMS_Conut(Msg);

                if (SMSBalance >= TotalSMS)
                {
                    if (SMS.SMS_GetBalance() >= TotalSMS)
                    {

                        foreach(var num in nums)
                        {
                            PhoneNo = num;
                            Get_Validation isValid = SMS.SMS_Validation(PhoneNo, Msg);
                            if (isValid.Validation)
                            {
                                Guid smsSendId = SMS.SMS_Send(PhoneNo, Msg, "SMS Service");
                                if (smsSendId != Guid.Empty)
                                {
                                    SMS_OtherInfoSQL.InsertParameters["SMS_Send_ID"].DefaultValue = smsSendId.ToString();
                                    SMS_OtherInfoSQL.InsertParameters["SchoolID"].DefaultValue = Session["SchoolID"].ToString();
                                    SMS_OtherInfoSQL.InsertParameters["EducationYearID"].DefaultValue = Session["Edu_Year"].ToString();
                                    SMS_OtherInfoSQL.InsertParameters["StudentID"].DefaultValue = "";
                                    SMS_OtherInfoSQL.InsertParameters["TeacherID"].DefaultValue = "";

                                    SMS_OtherInfoSQL.Insert();

                                    SMSTextBox.Text = string.Empty;
                                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('SMS Sent Successfully.')", true);
                                }
                            }
                            else
                            {
                                ErrorLabel.Text = isValid.Message;
                            }
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
            #endregion

            #region Send SMS to teacher

            if (SMSMultiView.ActiveViewIndex == 3)
            {
                foreach (GridViewRow ROW in AllTeachersGridView.Rows)
                {
                    CheckBox SelectCheckbox = (CheckBox)ROW.FindControl("SelectCheckBox");
                    if (SelectCheckbox.Checked)
                    {
                        PhoneNo = AllTeachersGridView.DataKeys[ROW.DataItemIndex]["Phone"].ToString();

                        Get_Validation IsValid = SMS.SMS_Validation(PhoneNo, Msg);

                        if (IsValid.Validation)
                        {
                            TotalSMS += SMS.SMS_Conut(Msg);
                        }
                    }
                }

                if (SMSBalance >= TotalSMS)
                {
                    if (SMS.SMS_GetBalance() >= TotalSMS)
                    {
                        foreach (GridViewRow ROW in AllTeachersGridView.Rows)
                        {
                            CheckBox SelectCheckbox = (CheckBox)ROW.FindControl("SelectCheckBox");
                            if (SelectCheckbox.Checked)
                            {
                                PhoneNo = AllTeachersGridView.DataKeys[ROW.DataItemIndex]["Phone"].ToString();

                                Get_Validation IsValid = SMS.SMS_Validation(PhoneNo, Msg);

                                if (IsValid.Validation)
                                {
                                    Guid SMS_Send_ID = SMS.SMS_Send(PhoneNo, Msg, "SMS Service");
                                    if (SMS_Send_ID != Guid.Empty)
                                    {
                                        SMS_OtherInfoSQL.InsertParameters["SMS_Send_ID"].DefaultValue = SMS_Send_ID.ToString();
                                        SMS_OtherInfoSQL.InsertParameters["SchoolID"].DefaultValue = Session["SchoolID"].ToString();
                                        SMS_OtherInfoSQL.InsertParameters["EducationYearID"].DefaultValue = Session["Edu_Year"].ToString();
                                        SMS_OtherInfoSQL.InsertParameters["StudentID"].DefaultValue = "";
                                        SMS_OtherInfoSQL.InsertParameters["TeacherID"].DefaultValue = AllTeachersGridView.DataKeys[ROW.DataItemIndex]["EmployeeID"].ToString();

                                        SMS_OtherInfoSQL.Insert();
                                        SentMsgCont++;
                                        SentMgsConfirm = true;
                                    }
                                    else
                                    {
                                        ROW.BackColor = System.Drawing.Color.Red;
                                        FailedMsgCont++;
                                    }
                                }
                                else
                                {
                                    ROW.BackColor = System.Drawing.Color.Red;
                                    FailedMsgCont++;
                                }
                            }
                        }

                        if (SentMgsConfirm)
                        {
                            SMSTextBox.Text = string.Empty;
                            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('You Have Successfully Sent " + SentMsgCont.ToString() + " SMS and Failed " + FailedMsgCont.ToString() + ".')", true);
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
            #endregion

            SMSFormView.DataBind();
        }

        protected void IDFindButton_Click(object sender, EventArgs e)
        {
            ClassDropDownList.SelectedIndex = 0;

            GroupDropDownList.Visible = false;
            SectionDropDownList.Visible = false;
            ShiftDropDownList.Visible = false;

            AllStudentsGridView.DataSource = FindIDSQL;
            AllStudentsGridView.DataBind();
        }
    }
}