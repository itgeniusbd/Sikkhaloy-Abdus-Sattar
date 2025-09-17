using Education;
using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Exam
{
    public partial class ExamPosition_Subject : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Session["Group"] = GroupDropDownList.SelectedValue;
            Session["Shift"] = ShiftDropDownList.SelectedValue;
            Session["Section"] = SectionDropDownList.SelectedValue;
            Session["Subject"] = SubjectDropDownList.SelectedValue;
            try
            {
                if (!IsPostBack)
                {
                    GroupDropDownList.Visible = false;
                    SectionDropDownList.Visible = false;
                    ShiftDropDownList.Visible = false;
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

            if (SubjectDropDownList.SelectedIndex > 0)
            {
                string name = "Subject Position Of " + SubjectDropDownList.SelectedItem.Text;

                name += " In: " + ExamDropDownList.SelectedItem.Text;

                name += " For Class: " + ClassDropDownList.SelectedItem.Text;

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
        }

        protected void GroupDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
            Session["Subject"] = "0";
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
        //End DDL
        protected void ExamDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
            Session["Subject"] = "0";
        }
        protected void ExamDropDownList_DataBound(object sender, EventArgs e)
        {
            ExamDropDownList.Items.Insert(0, new ListItem("[ SELECT EXAM ]", "0"));
        }

        protected void SubjectDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            view();
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

        protected void StudentsGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //Class
                if (e.Row.Cells[9].Text == "1")
                {
                    e.Row.Cells[9].CssClass = "First";

                    e.Row.Cells[9].Text += " st";
                }

                else if (e.Row.Cells[9].Text == "2")
                {
                    e.Row.Cells[9].CssClass = "Second";
                    e.Row.Cells[9].Text += " nd";
                }

                else if (e.Row.Cells[9].Text == "3")
                {
                    e.Row.Cells[9].CssClass = "Third";
                    e.Row.Cells[9].Text += " rd";
                }
                else
                {
                    e.Row.Cells[9].Text += " th";
                }

                //Section
                if (e.Row.Cells[10].Text == "1")
                {
                    e.Row.Cells[10].CssClass = "First";
                    e.Row.Cells[10].Text += " st";
                }

                else if (e.Row.Cells[10].Text == "2")
                {
                    e.Row.Cells[10].CssClass = "Second";
                    e.Row.Cells[10].Text += " nd";

                }

                else if (e.Row.Cells[10].Text == "3")
                {
                    e.Row.Cells[10].CssClass = "Third";
                    e.Row.Cells[10].Text += " rd";
                }
                else
                {
                    e.Row.Cells[10].Text += " th";
                }


                if (StudentsGridView.DataKeys[e.Row.DataItemIndex]["PassStatus_Subject"].ToString() == "F")
                {
                    e.Row.CssClass = "RowColor";
                }
            }
        }
        protected void SMSButton_Click(object sender, EventArgs e)
        {
            ErrorLabel.Text = "";
            SMS_Class SMS = new SMS_Class(Session["SchoolID"].ToString());

            int TotalSMS = 0;
            string PhoneNo = "";
            string Text = "";
            int SMSBalance = SMS.SMSBalance;
            bool SentMgsConfirm = false;
            int SentMsgCont = 0;
            int FailedMsgCont = 0;

            foreach (GridViewRow row in StudentsGridView.Rows)
            {
                CheckBox SMSCheckbox = (CheckBox)row.FindControl("SingleCheckBox");

                if (SMSCheckbox.Checked)
                {
                    string details = "";
                    PhoneNo = StudentsGridView.DataKeys[row.DataItemIndex]["SMSPhoneNo"].ToString();

                    if (StudentsGridView.DataKeys[row.RowIndex]["PassStatus_Subject"].ToString() == "P")
                    {
                        if (Send_DetailsCheckBox.Checked)
                        {
                            DataList SMarksDataList = (DataList)row.FindControl("SMarksDataList");

                            foreach (DataListItem item in SMarksDataList.Items)
                            {
                                Label SubjectNameLabel = (Label)item.FindControl("SubjectNameLabel");
                                Label MarkLabel = (Label)item.FindControl("MarkLabel");

                                if (SubjectNameLabel.Text != "")
                                {
                                    if (MarkLabel.Text != "A")
                                    {
                                        details += SubjectNameLabel.Text + ": " + MarkLabel.Text + ", ";
                                    }
                                    else
                                    {
                                        details += SubjectNameLabel.Text + " Absent ,";
                                    }
                                }
                            }
                        }

                        Text = "Congratulation! " + StudentsGridView.DataKeys[row.RowIndex]["StudentsName"].ToString();
                        Text += " You have successfully Passed In " + SubjectDropDownList.SelectedItem.Text + " Of " + ExamDropDownList.SelectedItem.Text + ". Your ";
                        Text += details;
                        Text += "Total Marks: " + Convert.ToDecimal(StudentsGridView.DataKeys[row.RowIndex]["ObtainedMark_ofSubject"].ToString()).ToString("0.00");
                        Text += ", Grade: " + StudentsGridView.DataKeys[row.RowIndex]["SubjectGrades"].ToString();
                        Text += ", Point: " + Convert.ToDecimal(StudentsGridView.DataKeys[row.RowIndex]["SubjectPoint"].ToString()).ToString("0.00");
                        Text += ", Position In Class: " + row.Cells[9].Text;
                        //Text += ", Position In Section: " + row.Cells[10].Text;
                        Text += ". Regards: " + Session["School_Name"].ToString();
                    }
                    else
                    {
                        Text = "Alas!! " + StudentsGridView.DataKeys[row.RowIndex]["StudentsName"].ToString();
                        Text += " You have failed In " + SubjectDropDownList.SelectedItem.Text + " Of " + ExamDropDownList.SelectedItem.Text;
                        Text += details;
                        Text += ". Your Total Marks Is : " + Convert.ToDecimal(StudentsGridView.DataKeys[row.RowIndex]["ObtainedMark_ofSubject"].ToString()).ToString("0.00");
                        Text += ". Regards: " + Session["School_Name"].ToString();
                    }

                    Get_Validation IsValid = SMS.SMS_Validation(PhoneNo, Text);

                    if (IsValid.Validation)
                    {
                        TotalSMS += SMS.SMS_Conut(Text);
                    }
                }
            }

            if (SMSBalance >= TotalSMS)
            {
                if (SMS.SMS_GetBalance() >= TotalSMS)
                {
                    foreach (GridViewRow row in StudentsGridView.Rows)
                    {
                        CheckBox SMSCheckbox = (CheckBox)row.FindControl("SingleCheckBox");

                        if (SMSCheckbox.Checked)
                        {
                            string details = "";
                            if (Send_DetailsCheckBox.Checked)
                            {
                                DataList SMarksDataList = (DataList)row.FindControl("SMarksDataList");
                                foreach (DataListItem item in SMarksDataList.Items)
                                {
                                    Label SubjectNameLabel = (Label)item.FindControl("SubjectNameLabel");
                                    Label MarkLabel = (Label)item.FindControl("MarkLabel");

                                    if (SubjectNameLabel.Text != "")
                                    {
                                        if (MarkLabel.Text != "A")
                                        {
                                            details += SubjectNameLabel.Text + ": " + MarkLabel.Text + ", ";
                                        }
                                        else
                                        {
                                            details += SubjectNameLabel.Text + " : Absent ,";
                                        }
                                    }
                                }
                            }


                            PhoneNo = StudentsGridView.DataKeys[row.RowIndex]["SMSPhoneNo"].ToString();

                            if (StudentsGridView.DataKeys[row.RowIndex]["PassStatus_Subject"].ToString() == "P")
                            {
                                Text = "Congratulation! " + StudentsGridView.DataKeys[row.RowIndex]["StudentsName"].ToString();
                                Text += " You have successfully Passed In " + SubjectDropDownList.SelectedItem.Text + " Of " + ExamDropDownList.SelectedItem.Text + ". Your ";
                                Text += details;
                                Text += "Total Mark: " + Convert.ToDecimal(StudentsGridView.DataKeys[row.RowIndex]["ObtainedMark_ofSubject"].ToString()).ToString("0.00");
                                Text += ", Grade: " + StudentsGridView.DataKeys[row.RowIndex]["SubjectGrades"].ToString();
                                Text += ", Point: " + Convert.ToDecimal(StudentsGridView.DataKeys[row.RowIndex]["SubjectPoint"].ToString()).ToString("0.00");
                                Text += ", Position In Class: " + row.Cells[9].Text;
                                //Text += ", Position In Section: " + row.Cells[10].Text;
                                Text += ". Regards: " + Session["School_Name"].ToString();
                            }
                            else
                            {
                                Text = "Alas!! " + StudentsGridView.DataKeys[row.RowIndex]["StudentsName"].ToString();
                                Text += " You have failed In " + SubjectDropDownList.SelectedItem.Text + " Of " + ExamDropDownList.SelectedItem.Text;
                                Text += details;
                                Text += ". Your Total Marks Is : " + Convert.ToDecimal(StudentsGridView.DataKeys[row.RowIndex]["ObtainedMark_ofSubject"].ToString()).ToString("0.00");
                                Text += ". Regards: " + Session["School_Name"].ToString();
                            }


                            Get_Validation IsValid = SMS.SMS_Validation(PhoneNo, Text);

                            if (IsValid.Validation)
                            {
                                Guid SMS_Send_ID = SMS.SMS_Send(PhoneNo, Text, "Position Subject Result");
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
                                    row.BackColor = System.Drawing.Color.Silver;
                                    FailedMsgCont++;
                                }
                            }
                            else
                            {
                                row.BackColor = System.Drawing.Color.Silver;
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
    }
}