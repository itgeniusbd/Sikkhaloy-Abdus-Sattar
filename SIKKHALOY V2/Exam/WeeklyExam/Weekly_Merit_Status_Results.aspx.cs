using Education;
using EDUCATION.COM.PaymentDataSetTableAdapters;
using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.EXAM.WeeklyExam
{
    public partial class Weekly_Merit_Status_Results : System.Web.UI.Page
    {
        int SchoolID;
        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack)
            {
                SchoolID = Convert.ToInt32(Session["SchoolID"].ToString());
            }

            Session["Group"] = GroupDropDownList.SelectedValue;
            Session["Shift"] = ShiftDropDownList.SelectedValue;
            Session["Section"] = SectionDropDownList.SelectedValue;

            try
            {
                if (!IsPostBack)
                {
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

            StudentsGridView.DataBind();
            string name = ExamDropDownList.SelectedItem.Text + " , " + WeekDropDownList.SelectedItem.Text;

            name += " for Class: " + ClassDropDownList.SelectedItem.Text;

            if (SectionDropDownList.SelectedIndex != 0)
            {
                name += ", Section: " + SectionDropDownList.SelectedItem.Text;
            }
            if (GroupDropDownList.SelectedIndex != 0)
            {
                name += ", Group: " + GroupDropDownList.SelectedItem.Text;
            }
            if (ShiftDropDownList.SelectedIndex != 0)
            {
                name += ", Shift: " + ShiftDropDownList.SelectedItem.Text;
            }
            CGSSLabel.Text = name;
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

        protected void ClassDropDownList_DataBound(object sender, EventArgs e)
        {
            try
            {
                DataView ClassNameDV = new DataView();
                ClassNameDV = (DataView)ClassNameSQL.Select(DataSourceSelectArguments.Empty);
                if (ClassNameDV.Count < 1)
                {
                    ClassDropDownList.Items.Clear();
                }
                else
                {
                    ClassDropDownList.Items.Insert(0, new ListItem("[ Select ]", "0"));
                }
            }
            catch
            { }
        }

        //---------------------------------------Group DDL-------------------------------------------
        protected void GroupDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
        }

        protected void GroupDropDownList_DataBound(object sender, EventArgs e)
        {
            GroupDropDownList.Items.Insert(0, new ListItem("[ ALL ]", "%"));
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
            SectionDropDownList.Items.Insert(0, new ListItem("[ ALL ]", "%"));
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
            ShiftDropDownList.Items.Insert(0, new ListItem("[ ALL ]", "%"));
            if (IsPostBack)
                ShiftDropDownList.Items.FindByValue(Session["Shift"].ToString()).Selected = true;
        }


        //---------------------------------------End DD-------------------------------------------


        protected void SmsButton_Click(object sender, EventArgs e)
        {
            ErrorLabel.Text = "";
            SMS_Class SMS = new SMS_Class(Session["SchoolID"].ToString());

            int TotalSMS = 0;
            string PhoneNo = "";
            string Msg = "";
            int SMSBalance = SMS.SMSBalance;
            bool SentMgsConfirm = false;
            int SentMsgCont = 0;
            int FailedMsgCont = 0;

            foreach (GridViewRow row in StudentsGridView.Rows)
            {
                CheckBox SMSCheckBox = row.FindControl("SMSCheckBox") as CheckBox;

                if (SMSCheckBox.Checked)
                {
                    PhoneNo = StudentsGridView.DataKeys[row.RowIndex]["SMSPhoneNo"].ToString();
                    Msg = WeekDropDownList.SelectedItem.Text + " Merit Status For: " + row.Cells[3].Text + " Class: " + ClassDropDownList.SelectedItem.Text + " Roll: " + row.Cells[2].Text + " ID: " + row.Cells[1].Text + " Total Marks: " + row.Cells[11].Text + " Position: " + row.Cells[12].Text + ". Regards: " + Session["School_Name"].ToString();

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

                    foreach (GridViewRow row in StudentsGridView.Rows)
                    {
                        CheckBox SMSCheckBox = row.FindControl("SMSCheckBox") as CheckBox;

                        if (SMSCheckBox.Checked)
                        {
                            PhoneNo = StudentsGridView.DataKeys[row.RowIndex]["SMSPhoneNo"].ToString();
                            Msg = WeekDropDownList.SelectedItem.Text + " Merit Status For: " + row.Cells[3].Text + " Class: " + ClassDropDownList.SelectedItem.Text + " Roll: " + row.Cells[2].Text + " ID: " + row.Cells[1].Text + " Total Marks: " + row.Cells[11].Text + " Position: " + row.Cells[12].Text + ". Regards: " + Session["School_Name"].ToString();


                            Get_Validation IsValid = SMS.SMS_Validation(PhoneNo, Msg);

                            if (IsValid.Validation)
                            {
                                Guid SMS_Send_ID = SMS.SMS_Send(PhoneNo, Msg, "Weekly Result");
                                if (SMS_Send_ID != Guid.Empty)
                                {
                                    SMS_OtherInfoSQL.InsertParameters["SMS_Send_ID"].DefaultValue = SMS_Send_ID.ToString();
                                    SMS_OtherInfoSQL.InsertParameters["SchoolID"].DefaultValue = Session["SchoolID"].ToString();
                                    SMS_OtherInfoSQL.InsertParameters["EducationYearID"].DefaultValue = Session["Edu_Year"].ToString();
                                    SMS_OtherInfoSQL.InsertParameters["StudentID"].DefaultValue = StudentsGridView.DataKeys[row.RowIndex]["StudentID"].ToString();
                                    SMS_OtherInfoSQL.InsertParameters["TeacherID"].DefaultValue = "";

                                    SMS_OtherInfoSQL.Insert();
                                    SentMgsConfirm = true;
                                    SentMsgCont++;
                                }
                                else
                                {
                                    row.BackColor = System.Drawing.Color.Red;
                                    FailedMsgCont++;
                                }
                            }
                            else
                            {
                                row.BackColor = System.Drawing.Color.Red;
                                FailedMsgCont++;
                            }
                        }
                    }

                    if (SentMgsConfirm)
                    {
                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Successfully Sent " + SentMsgCont.ToString() + " SMS. & Failed " + FailedMsgCont.ToString() + ".')", true);
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

        protected void StudentsGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
                ((CheckBox)e.Row.FindControl("SelectAllCheckBox")).Attributes.Add("onclick", "javascript:SelectAll('" + ((CheckBox)e.Row.FindControl("SelectAllCheckBox")).ClientID + "')");
            }

            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (e.Row.Cells[12].Text == "1st")
                {
                    e.Row.Cells[12].CssClass = "First";
                }

                if (e.Row.Cells[12].Text == "2nd")
                {
                    e.Row.Cells[12].CssClass = "Second";
                }

                if (e.Row.Cells[12].Text == "3rd")
                {
                    e.Row.Cells[12].CssClass = "Third";
                }
            }

        }

        protected void ExamDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            WeekDropDownList.Items.Clear();
            ClassDropDownList.Items.Clear();

            WeeklyExamTableAdapter exam = new WeeklyExamTableAdapter();

            DataTable Weeks = exam.WeekNo(Session["Edu_Year"].ToString(), Convert.ToInt32(ExamDropDownList.SelectedValue), Convert.ToInt32(Session["SchoolID"].ToString()));
            for (int i = 0; i < Weeks.Rows.Count; i++)
            {
                string WeekDetails = "Week " + Weeks.Rows[i]["WEEK NUMBER"].ToString() + " (" + Convert.ToDateTime(Weeks.Rows[i]["FROM"].ToString()).ToString("d MMM yy") + " to " + Convert.ToDateTime(Weeks.Rows[i]["TO"].ToString()).ToString("d MMM yy") + ")";
                WeekDropDownList.Items.Insert(0, new ListItem(WeekDetails, Weeks.Rows[i]["WEEK NUMBER"].ToString()));
            }

            if (Weeks.Rows.Count > 0)
            { WeekDropDownList.Items.Insert(0, new ListItem("[Select]", "0")); }

            GroupDropDownList.Visible = false;
            GroupLabel.Visible = false;
            GRequiredFieldValidator.Visible = false;

            GroupFormView.Visible = false;


            SectionDropDownList.Visible = false;
            SectionLabel.Visible = false;
            SeRequiredFieldValidator.Visible = false;

            SectionFormView.Visible = false;



            ShiftDropDownList.Visible = false;
            ShiftLabel.Visible = false;
            ShRequiredFieldValidator.Visible = false;

            ShiftFormView.Visible = false;

            if (ExamDropDownList.SelectedIndex != 0)
            {
                CGSSLabel.Text = ExamDropDownList.SelectedItem.Text;
            }
            else
            {
                CGSSLabel.Text = string.Empty;
            }
        }

        protected void WeekDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {

            GroupDropDownList.Visible = false;
            GroupLabel.Visible = false;
            GRequiredFieldValidator.Visible = false;

            GroupFormView.Visible = false;


            SectionDropDownList.Visible = false;
            SectionLabel.Visible = false;
            SeRequiredFieldValidator.Visible = false;

            SectionFormView.Visible = false;



            ShiftDropDownList.Visible = false;
            ShiftLabel.Visible = false;
            ShRequiredFieldValidator.Visible = false;

            ShiftFormView.Visible = false;

            if (WeekDropDownList.SelectedIndex != 0)
            {
                CGSSLabel.Text = ExamDropDownList.SelectedItem.Text + " , " + WeekDropDownList.SelectedItem.Text;
            }
            else
            {
                CGSSLabel.Text = string.Empty;
            }
        }
    }
}