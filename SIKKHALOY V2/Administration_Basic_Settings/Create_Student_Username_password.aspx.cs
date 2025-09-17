using Education;
using System;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Administration_Basic_Settings
{
    public partial class Create_Student_Username_password : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Session["Group"] = GroupDropDownList.SelectedValue;
            Session["Shift"] = ShiftDropDownList.SelectedValue;
            Session["Section"] = SectionDropDownList.SelectedValue;

            if (!IsPostBack)
            {
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

            StudentsGridView.DataSource = ShowStudentClassSQL;
            StudentsGridView.DataBind();
            StudentUserGV.DataSource = StudentSQL;
            StudentUserGV.DataBind();
            StuIdTextBox.Text = string.Empty;
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
        }


        //---------------------------------------Group DDL-------------------------------------------
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


        //---------------------------------------Section DDL-------------------------------------------
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

        //---------------------------------------Shift DDL-------------------------------------------
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

        //---------------------------------------End DD-------------------------------------------
        protected void IDFindButton_Click(object sender, EventArgs e)
        {
            ClassDropDownList.SelectedIndex = 0;

            GroupDropDownList.Visible = false;
            SectionDropDownList.Visible = false;
            ShiftDropDownList.Visible = false;

            StudentsGridView.DataSource = ShowIDSQL;
            StudentsGridView.DataBind();
            StudentUserGV.DataSource = Student_ID_SQL;
            StudentUserGV.DataBind();
        }

        protected void UserPasswordButton_Click(object sender, EventArgs e)
        {
            string Username = "";
            string Password = "";

            bool IsCreated = false;
            foreach (GridViewRow Allrow in StudentsGridView.Rows)
            {
                CheckBox SingleCheckBox = (CheckBox)Allrow.FindControl("SingleCheckBox");

                if (SingleCheckBox.Checked)
                {
                    try
                    {
                        Username = Session["SchoolID"].ToString() + StudentsGridView.DataKeys[Allrow.RowIndex]["ID"].ToString();
                        Random rnd = new Random();
                        Password = rnd.Next(100000, 999999).ToString();

                        // Create new user.
                        MembershipUser newUser = Membership.CreateUser(Username, Password);
                        Roles.AddUserToRole(Username, "Student");

                        ReSQL.InsertParameters["UserName"].DefaultValue = Username;
                        ReSQL.InsertParameters["StudentID"].DefaultValue = StudentsGridView.DataKeys[Allrow.RowIndex]["StudentID"].ToString();
                        ReSQL.InsertParameters["Password"].DefaultValue = Password;
                        ReSQL.Insert();

                        Allrow.Visible = false;
                        IsCreated = true;
                    }
                    catch (MembershipCreateUserException ex)
                    {
                        ErrorLabel.Text = GetErrorMessage(ex.StatusCode) + "<br />";
                    }
                    catch (HttpException ex)
                    {
                        ErrorLabel.Text = ex.Message + "<br />";
                    }
                }
            }
            if (IsCreated)
            {
                ClassDropDownList.SelectedIndex = 0;
                ErrorLabel.Text = "";
                StuIdTextBox.Text = "";
                view();
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('User Created Successfully')", true);
            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('User Not Created')", true);
            }
        }

        public string GetErrorMessage(MembershipCreateStatus status)
        {
            switch (status)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "Username already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A username for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }

        protected void SMSButton_Click(object sender, EventArgs e)
        {
            SMS_Class SMS = new SMS_Class(Session["SchoolID"].ToString());

            /*Send Web Address*/
            DataView SchoolDv = new DataView();
            string School_website = "";

            SchoolDv = (DataView)School_InfoSQL.Select(DataSourceSelectArguments.Empty);
            if (SchoolDv[0]["Website"].ToString() != "")
            {
                School_website = SchoolDv[0]["Website"].ToString();
            }
            else
            {
                School_website = "www.sikkhaloy.com";
            }


            string Msg = "";
            string PhoneNo = "";
            bool ValidSMS = true;
            int TotalSMS = 0;

            int SMSBalance = SMS.SMSBalance;

            foreach (GridViewRow Row in StudentUserGV.Rows)
            {
                CheckBox SMSCheckBox = Row.FindControl("SingleCheckBox") as CheckBox;

                if (SMSCheckBox.Checked)
                {
                    PhoneNo = StudentUserGV.DataKeys[Row.DataItemIndex]["SMSPhoneNo"].ToString();

                    Msg = "Dear, ";
                    Msg += StudentUserGV.DataKeys[Row.DataItemIndex]["StudentsName"].ToString() + " Your Login Username Is: ";
                    Msg += StudentUserGV.DataKeys[Row.DataItemIndex]["UserName"].ToString() + " And Password Is: ";
                    Msg += StudentUserGV.DataKeys[Row.DataItemIndex]["Password"].ToString();
                    Msg += " Visit: " + School_website + " and Login.";
                    Msg += " Regards: " + Session["School_Name"].ToString();

                    Get_Validation IsValid = SMS.SMS_Validation(PhoneNo, Msg);

                    if (IsValid.Validation)
                    {
                        TotalSMS += SMS.SMS_Conut(Msg);
                    }
                    else
                    {
                        ErrorLabel.Text = IsValid.Message;
                        Row.BackColor = System.Drawing.Color.Red;
                        ValidSMS = false;
                    }
                }
            }


            if (ValidSMS)
            {
                if (SMSBalance >= TotalSMS)
                {
                    if (SMS.SMS_GetBalance() >= TotalSMS)
                    {
                        foreach (GridViewRow ROW in StudentUserGV.Rows)
                        {
                            Msg = "Dear, ";
                            Msg += StudentUserGV.DataKeys[ROW.DataItemIndex]["StudentsName"].ToString() + " Your Login Username Is: ";
                            Msg += StudentUserGV.DataKeys[ROW.DataItemIndex]["UserName"].ToString() + " And Password Is: ";
                            Msg += StudentUserGV.DataKeys[ROW.DataItemIndex]["Password"].ToString();
                            Msg += " Visit: " + School_website + " and Login.";
                            Msg += " Regards: " + Session["School_Name"].ToString();

                            CheckBox SingleCheckBox = (CheckBox)ROW.FindControl("SingleCheckBox");
                            PhoneNo = StudentUserGV.DataKeys[ROW.DataItemIndex]["SMSPhoneNo"].ToString();

                            if (SingleCheckBox.Checked)
                            {
                                Guid SMS_Send_ID = SMS.SMS_Send(PhoneNo, Msg, "Student User & Password");
                                if (SMS_Send_ID != Guid.Empty)
                                {
                                    SMS_OtherInfoSQL.InsertParameters["SMS_Send_ID"].DefaultValue = SMS_Send_ID.ToString();
                                    SMS_OtherInfoSQL.InsertParameters["SchoolID"].DefaultValue = Session["SchoolID"].ToString();
                                    SMS_OtherInfoSQL.InsertParameters["EducationYearID"].DefaultValue = Session["Edu_Year"].ToString();
                                    SMS_OtherInfoSQL.InsertParameters["StudentID"].DefaultValue = StudentUserGV.DataKeys[ROW.DataItemIndex]["StudentID"].ToString();
                                    SMS_OtherInfoSQL.InsertParameters["TeacherID"].DefaultValue = "";

                                    SMS_OtherInfoSQL.Insert();

                                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('SMS Sent Successfully!!')", true);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}