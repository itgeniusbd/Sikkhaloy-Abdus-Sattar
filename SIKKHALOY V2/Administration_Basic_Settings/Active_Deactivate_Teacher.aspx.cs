using Education;
using System;
using System.Data;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.ADMINISTRATION_BASIC_SETTING
{
    public partial class Active_Deactivate_Teacher : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                DataView dv = (DataView)Teacher_DetailsSQL.Select(DataSourceSelectArguments.Empty);
                CountLabel.Text = "Total: " + dv.Count.ToString() + " Teacher(s)";
            }
        }

        protected void ApprovedCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            var approvedCheckBox = (CheckBox)sender;
            var row = (GridViewRow)approvedCheckBox.Parent.Parent;

            var usr = Membership.GetUser(TeacherGV.DataKeys[row.DataItemIndex]["UserName"].ToString());
            usr.IsApproved = approvedCheckBox.Checked;
            Membership.UpdateUser(usr);
        }

        protected void LockedOutCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox LockedOutCheckBox = (CheckBox)sender;
            GridViewRow Row = (GridViewRow)LockedOutCheckBox.Parent.Parent;

            MembershipUser usr = Membership.GetUser(TeacherGV.DataKeys[Row.DataItemIndex]["UserName"].ToString());
            usr.UnlockUser();

            LockedOutCheckBox.Checked = usr.IsLockedOut;
        }

        protected void SMS_Button_Click(object sender, EventArgs e)
        {
            SMS_Class SMS = new SMS_Class(Session["SchoolID"].ToString());

            string Msg = "";
            string PhoneNo = "";
            bool ValidSMS = true;
            int TotalSMS = 0;

            int SMSBalance = SMS.SMSBalance;

            foreach (GridViewRow Row in TeacherGV.Rows)
            {
                CheckBox SMSCheckBox = Row.FindControl("SingleCheckBox") as CheckBox;

                if (SMSCheckBox.Checked)
                {
                    PhoneNo = TeacherGV.DataKeys[Row.DataItemIndex]["Phone"].ToString();

                    Msg = "Dear, ";
                    Msg += TeacherGV.DataKeys[Row.DataItemIndex]["Name"].ToString() + " Your Login Username Is: ";
                    Msg += TeacherGV.DataKeys[Row.DataItemIndex]["UserName"].ToString() + " And Password Is: ";
                    Msg += TeacherGV.DataKeys[Row.DataItemIndex]["Password"].ToString();
                    Msg += " Visit: www.sikkhaloy.com and Login";
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
                        foreach (GridViewRow ROW in TeacherGV.Rows)
                        {
                            Msg = "Dear, ";
                            Msg += TeacherGV.DataKeys[ROW.DataItemIndex]["Name"].ToString() + " Your Login Username Is: ";
                            Msg += TeacherGV.DataKeys[ROW.DataItemIndex]["UserName"].ToString() + " And Password Is: ";
                            Msg += TeacherGV.DataKeys[ROW.DataItemIndex]["Password"].ToString();
                            Msg += " Visit: www.sikkhaloy.com and Login.";
                            Msg += " Regards: " + Session["School_Name"].ToString();

                            CheckBox SingleCheckBox = (CheckBox)ROW.FindControl("SingleCheckBox");

                            PhoneNo = TeacherGV.DataKeys[ROW.DataItemIndex]["Phone"].ToString();
                            if (SingleCheckBox.Checked)
                            {
                                Guid SMS_Send_ID = SMS.SMS_Send(PhoneNo, Msg, "Teacher User & Password");
                                if (SMS_Send_ID != Guid.Empty)
                                {
                                    SMS_OtherInfoSQL.InsertParameters["SMS_Send_ID"].DefaultValue = SMS_Send_ID.ToString();
                                    SMS_OtherInfoSQL.InsertParameters["SchoolID"].DefaultValue = Session["SchoolID"].ToString();
                                    SMS_OtherInfoSQL.InsertParameters["EducationYearID"].DefaultValue = Session["Edu_Year"].ToString();
                                    SMS_OtherInfoSQL.InsertParameters["StudentID"].DefaultValue = "";
                                    SMS_OtherInfoSQL.InsertParameters["TeacherID"].DefaultValue = TeacherGV.DataKeys[ROW.DataItemIndex]["TeacherID"].ToString(); ;

                                    SMS_OtherInfoSQL.Insert();

                                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Username And Password Sent Successfully!!')", true);
                                }
                            }
                        }
                    }
                }
            }
        }

        protected void FindButton_Click(object sender, EventArgs e)
        {
            DataView dv = (DataView)Teacher_DetailsSQL.Select(DataSourceSelectArguments.Empty);
            CountLabel.Text = "Total: " + dv.Count.ToString() + " Teacher(s)";
        }
    }
}
