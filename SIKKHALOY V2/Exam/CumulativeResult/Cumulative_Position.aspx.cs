using Education;
using System;
using System.Data;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Exam.CumulativeResult
{
    public partial class Cumulative_Position : System.Web.UI.Page
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

            if (ExamDropDownList.SelectedIndex > 0)
            {
                string name = "Position Of " + ExamDropDownList.SelectedItem.Text;

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

            GroupDropDownList.DataBind();
            ShiftDropDownList.DataBind();
            SectionDropDownList.DataBind();
            view();

            string eduyear = Session["Edu_Year"].ToString();
        }

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
        }
        protected void ExamDropDownList_DataBound(object sender, EventArgs e)
        {
            ExamDropDownList.Items.Insert(0, new ListItem("[ SELECT EXAM ]", "0"));
        }

        protected void StudentsGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //Class
                if (e.Row.Cells[8].Text == "1")
                {
                    e.Row.Cells[8].CssClass = "First";

                    e.Row.Cells[8].Text += " st";
                }

                else if (e.Row.Cells[8].Text == "2")
                {
                    e.Row.Cells[8].CssClass = "Second";
                    e.Row.Cells[8].Text += " nd";
                }

                else if (e.Row.Cells[8].Text == "3")
                {
                    e.Row.Cells[8].CssClass = "Third";
                    e.Row.Cells[8].Text += " rd";
                }
                else
                {
                    e.Row.Cells[8].Text += " th";
                }

                //Section
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


                if (StudentsGridView.DataKeys[e.Row.DataItemIndex]["PassStatus_InSubject"].ToString() == "F")
                {
                    e.Row.CssClass = "RowColor";
                }

                if (StudentsGridView.Rows.Count > 0)
                {
                    StudentsGridView.UseAccessibleHeader = true;
                    StudentsGridView.HeaderRow.TableSection = TableRowSection.TableHeader;
                }
            }
        }

        //Cumulative Exam SMS
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
                    PhoneNo = StudentsGridView.DataKeys[row.DataItemIndex]["SMSPhoneNo"].ToString();
                    if (StudentsGridView.DataKeys[row.RowIndex]["PassStatus_InSubject"].ToString() == "P")
                    {
                        Text = "Congratulation! " + StudentsGridView.DataKeys[row.RowIndex]["StudentsName"];
                        Text += "(ID " + StudentsGridView.DataKeys[row.RowIndex]["ID"] + ")";
                        Text += " You have successfully Passed " + ExamDropDownList.SelectedItem.Text + ". Your ";
                        Text += "Total Marks: " + Convert.ToDecimal(StudentsGridView.DataKeys[row.RowIndex]["ObtainedMark_ofStudent"].ToString()).ToString("0.00");
                        Text += ", Grade: " + StudentsGridView.DataKeys[row.RowIndex]["Student_Grade"];
                        Text += ", Point: " + Convert.ToDecimal(StudentsGridView.DataKeys[row.RowIndex]["Student_Point"].ToString()).ToString("0.00");
                        if (ClassPositionCheckBox.Checked)
                        {
                            Text += ", Position In Class: " + row.Cells[8].Text;
                        }
                        if (SecPositionCheckBox.Checked)
                        {
                            Text += ", Position In Section: " + row.Cells[9].Text;
                        }

                        Text += ". Regards: " + Session["School_Name"];
                    }
                    else
                    {
                        Text = "Alas!! " + StudentsGridView.DataKeys[row.RowIndex]["StudentsName"];
                        Text += "(ID " + StudentsGridView.DataKeys[row.RowIndex]["ID"] + ")";
                        Text += " You have failed " + ExamDropDownList.SelectedItem.Text;
                        Text += ". Your Total Marks Is : " + Convert.ToDecimal(StudentsGridView.DataKeys[row.RowIndex]["ObtainedMark_ofStudent"].ToString()).ToString("0.00");
                        Text += ". Regards: " + Session["School_Name"];
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
                            PhoneNo = StudentsGridView.DataKeys[row.RowIndex]["SMSPhoneNo"].ToString();

                            if (StudentsGridView.DataKeys[row.RowIndex]["PassStatus_InSubject"].ToString() == "P")
                            {
                                Text = "Congratulation! " + StudentsGridView.DataKeys[row.RowIndex]["StudentsName"];
                                Text += "(ID " + StudentsGridView.DataKeys[row.RowIndex]["ID"] + ")";
                                Text += " You have successfully Passed " + ExamDropDownList.SelectedItem.Text + ". Your ";
                                Text += "Total Marks: " + Convert.ToDecimal(StudentsGridView.DataKeys[row.RowIndex]["ObtainedMark_ofStudent"].ToString()).ToString("0.00");
                                Text += ", Grade: " + StudentsGridView.DataKeys[row.RowIndex]["Student_Grade"];
                                Text += ", Point: " + Convert.ToDecimal(StudentsGridView.DataKeys[row.RowIndex]["Student_Point"].ToString()).ToString("0.00");
                                if (ClassPositionCheckBox.Checked)
                                {
                                    Text += ", Position In Class: " + row.Cells[8].Text;
                                }                                
                                if (SecPositionCheckBox.Checked)
                                {
                                    Text += ", Position In Section: " + row.Cells[9].Text;
                                }
                                Text += ". Regards: " + Session["School_Name"];
                            }
                            else
                            {
                                Text = "Alas!! " + StudentsGridView.DataKeys[row.RowIndex]["StudentsName"];
                                Text += "(ID " + StudentsGridView.DataKeys[row.RowIndex]["ID"] + ")";
                                Text += " You have failed " + ExamDropDownList.SelectedItem.Text;
                                Text += ". Your Total Marks Is : " + Convert.ToDecimal(StudentsGridView.DataKeys[row.RowIndex]["ObtainedMark_ofStudent"].ToString()).ToString("0.00");
                                Text += ". Regards: " + Session["School_Name"].ToString();
                            }


                            Get_Validation IsValid = SMS.SMS_Validation(PhoneNo, Text);

                            if (IsValid.Validation)
                            {
                                Guid SMS_Send_ID = SMS.SMS_Send(PhoneNo, Text, "Position Result");
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

        //Export To Word
        protected void ExportWordButton_Click(object sender, EventArgs e)
        {
            Export_ClassLabel.Text = CGSSLabel.Text;
            Response.Clear();
            Response.ContentEncoding = Encoding.Unicode;
            Response.BinaryWrite(Encoding.Unicode.GetPreamble());

            Response.AddHeader("content-disposition", "attachment;filename=Exam_Position.doc");
            Response.Charset = "";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.ContentType = "application/doc";
            StringWriter stringWrite = new StringWriter();
            HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);

            // Read Style file (css) here and add to response 
            FileInfo fi = new FileInfo(Server.MapPath("~/Exam/CSS/ExamPosition.css"));
            StringBuilder sb = new StringBuilder();
            StreamReader sr = fi.OpenText();

            while (sr.Peek() >= 0)
            {
                sb.Append(sr.ReadLine());
            }

            sr.Close();
            StudentsGridView.Columns[0].Visible = false;
            ExportPanel.RenderControl(htmlWrite);
            Response.Write("<html><head><style type='text/css'>" + sb.ToString() + "</style></head><body>" + stringWrite.ToString() + "</body></html>");

            Response.Write(stringWrite.ToString());
            Response.End();
        }
        public override void VerifyRenderingInServerForm(Control control)
        {
            /* Confirms that an HtmlForm control is rendered for the specified ASP.NET
               server control at run time. */
        }
    }
}