using Education;
using EDUCATION.COM.PaymentDataSetTableAdapters;
using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.ACCOUNTS.AccountsAnalysis
{
    public partial class Present_Due : System.Web.UI.Page
    {
        int SchoolID;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["SchoolID"] != null)
            {
                SchoolID = Convert.ToInt32(Session["SchoolID"].ToString());
                DueMultiView.ActiveViewIndex = DueRadioButtonList.SelectedIndex;
            }
        }

        protected void SectionDropDownList_DataBound(object sender, EventArgs e)
        {
            SectionDropDownList.Items.Insert(0, new ListItem("[ ALL SECTION ]", "%"));
        }
        protected void DueRadioButtonList_SelectedIndexChanged(object sender, EventArgs e)
        {
            DueMultiView.ActiveViewIndex = DueRadioButtonList.SelectedIndex;
        }
        protected void ClassSendButton_Click(object sender, EventArgs e)
        {
            ErrorLabel.Text = "";
            DueDetailsTableAdapter due = new DueDetailsTableAdapter();
            DataTable dt = new DataTable();
            bool smsSend = false;
            int SentMsgCont = 0;
            int FailedMsgCont = 0;

            SMS_Class SMS = new SMS_Class(Session["SchoolID"].ToString());

            int TotalSMS = 0;
            string PhoneNo = "";
            string Msg = "";
            int SMSBalance = SMS.SMSBalance;

            // Try to get Due notification template
            string dueTemplate = GetSMSTemplate("Due", "Due"); // Updated to use "Due" category

            foreach (GridViewRow row in TotalDueGridView.Rows)
            {
                CheckBox SMSCheckBox = row.FindControl("SingleCheckBox") as CheckBox;

                if (SMSCheckBox.Checked)
                {
                    string ID = TotalDueGridView.DataKeys[row.RowIndex]["ID"].ToString();
                    string SName = TotalDueGridView.DataKeys[row.RowIndex]["StudentsName"].ToString();
                    double dueAmount = Convert.ToDouble(TotalDueGridView.DataKeys[row.RowIndex]["Due"]);

                    PhoneNo = TotalDueGridView.DataKeys[row.DataItemIndex]["SMSPhoneNo"].ToString();

                    // Build due details WITH BREAKDOWN
                    string dueDetails = "";
                    dt = due.GetData(ID, SchoolID, RoleDropDownList.SelectedValue);
                    foreach (DataRow dr in dt.Rows)
                    {
                        dueDetails += dr["Role"].ToString() + ": " + dr["PayFor"].ToString() + " - " + dr["Due"].ToString() + " Tk, ";
                    }

                    if (!string.IsNullOrEmpty(dueTemplate))
                    {
                        // Use template
                        Msg = BuildDueNotificationMessage(dueTemplate, SName, ID, dueAmount, dueDetails);
                    }
                    else
                    {
                        // Default message
                        Msg = "Dear, " + SName + ", ID:" + ID + ". You've Due Payment: ";
                        Msg += dueAmount.ToString() + " Tk. ";
                        Msg += "Regards, " + Session["School_Name"].ToString();
                    }

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
                    foreach (GridViewRow row in TotalDueGridView.Rows)
                    {
                        CheckBox SingleCheckBox = (CheckBox)row.FindControl("SingleCheckBox");
                        if (SingleCheckBox.Checked)
                        {
                            PhoneNo = TotalDueGridView.DataKeys[row.RowIndex]["SMSPhoneNo"].ToString();

                            string ID = TotalDueGridView.DataKeys[row.RowIndex]["ID"].ToString();
                            string SName = TotalDueGridView.DataKeys[row.RowIndex]["StudentsName"].ToString();
                            double dueAmount = Convert.ToDouble(TotalDueGridView.DataKeys[row.RowIndex]["Due"]);

                            // Build due details WITH BREAKDOWN
                            string dueDetails = "";
                            dt = due.GetData(ID, SchoolID, RoleDropDownList.SelectedValue);
                            foreach (DataRow dr in dt.Rows)
                            {
                                dueDetails += dr["Role"].ToString() + ": " + dr["PayFor"].ToString() + " - " + dr["Due"].ToString() + " Tk, ";
                            }

                            if (!string.IsNullOrEmpty(dueTemplate))
                            {
                                // Use template
                                Msg = BuildDueNotificationMessage(dueTemplate, SName, ID, dueAmount, dueDetails);
                            }
                            else
                            {
                                // Default message
                                Msg = "Dear, " + SName + ", ID: " + ID + ". You've Due Payment: ";
                                Msg += dueAmount.ToString() + " Tk. ";
                                Msg += "Regards, " + Session["School_Name"].ToString();
                            }

                            Get_Validation IsValid = SMS.SMS_Validation(PhoneNo, Msg);

                            if (IsValid.Validation)
                            {
                                Guid SMS_Send_ID = SMS.SMS_Send(PhoneNo, Msg, "Due SMS");

                                if (SMS_Send_ID != Guid.Empty)
                                {
                                    SMS_OtherInfoSQL.InsertParameters["SMS_Send_ID"].DefaultValue = SMS_Send_ID.ToString();
                                    SMS_OtherInfoSQL.InsertParameters["SchoolID"].DefaultValue = Session["SchoolID"].ToString();
                                    SMS_OtherInfoSQL.InsertParameters["EducationYearID"].DefaultValue = Session["Edu_Year"].ToString();
                                    SMS_OtherInfoSQL.InsertParameters["StudentID"].DefaultValue = TotalDueGridView.DataKeys[row.DataItemIndex]["StudentID"].ToString();
                                    SMS_OtherInfoSQL.InsertParameters["TeacherID"].DefaultValue = "";
                                    SMS_OtherInfoSQL.Insert();

                                    smsSend = true;
                                    SentMsgCont++;
                                }
                                else
                                {
                                    FailedMsgCont++;
                                    row.BackColor = System.Drawing.Color.Red;
                                }
                            }
                            else
                            {
                                row.BackColor = System.Drawing.Color.Red;
                                FailedMsgCont++;
                            }
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

            if (smsSend)
            {
                TotalDueGridView.DataBind();
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Successfully Sent " + SentMsgCont.ToString() + " SMS. & Failed " + FailedMsgCont.ToString() + ".')", true);
            }
        }

        double SumFooter = 0;
        protected void ViewAllDueButton_Click(object sender, EventArgs e)
        {
            DueDetailsTableAdapter due = new DueDetailsTableAdapter();
            DataTable dt = new DataTable();
            ArrayList values = new ArrayList();

            foreach (GridViewRow row in TotalDueGridView.Rows)
            {
                CheckBox SingleCheckBox = (CheckBox)row.FindControl("SingleCheckBox");
                if (SingleCheckBox.Checked)
                {
                    values.Add(1);
                }
            }

            RoleDataList.DataSource = values;
            RoleDataList.DataBind();

            int a = 0;
            foreach (GridViewRow row in TotalDueGridView.Rows)
            {
                CheckBox SingleCheckBox = (CheckBox)row.FindControl("SingleCheckBox");
                if (SingleCheckBox.Checked)
                {
                    string ID = TotalDueGridView.DataKeys[row.RowIndex]["ID"].ToString();
                    string RollNo = TotalDueGridView.DataKeys[row.RowIndex]["RollNo"].ToString();
                    string Class = ClassDropDownList.SelectedItem.Text;
                    string SName = TotalDueGridView.DataKeys[row.RowIndex]["StudentsName"].ToString();
                    string SMSPhoneNo = TotalDueGridView.DataKeys[row.RowIndex]["SMSPhoneNo"].ToString();

                    dt = due.GetData(ID, SchoolID, RoleDropDownList.SelectedValue);
                    DataListItem Iteam = RoleDataList.Items[a];

                    Label NameLabel = (Label)Iteam.FindControl("NameLabel");
                    GridView AllDueGV = (GridView)Iteam.FindControl("AllDueGridView");

                    SumFooter = Convert.ToDouble(TotalDueGridView.DataKeys[row.RowIndex]["Due"]);
                    NameLabel.Text = SName + ", Class: " + Class + ", Roll: " + RollNo + ", ID: " + ID + ", Mob: " + SMSPhoneNo;

                    AllDueGV.DataSource = dt;

                    AllDueGV.DataBind();
                    a++;
                }
            }

            ScriptManager.RegisterStartupScript(this, this.GetType(), "Pop", "openModal();", true);
        }
        protected void IDSendButton_Click(object sender, EventArgs e)
        {
            ErrorLabel.Text = "";
            DueDetailsTableAdapter due = new DueDetailsTableAdapter();
            DataTable dt = new DataTable();

            SMS_Class SMS = new SMS_Class(Session["SchoolID"].ToString());

            int TotalSMS = 0;
            string PhoneNo = "";
            string Msg = "";
            int SMSBalance = SMS.SMSBalance;

            if (ID_DueDetailsGridView.Rows.Count > 0)
            {
                string ID = IDTextBox.Text;
                PhoneNo = StudentInfoFormView.DataKey["SMSPhoneNo"].ToString();
                string SName = StudentInfoFormView.DataKey["StudentsName"].ToString();
                double totalDue = Convert.ToDouble(StudentInfoFormView.DataKey["Due"]);

                // Build due details from GridView
                string dueDetails = "";
                dt = due.GetData(ID, SchoolID, RoleDropDownList.SelectedValue);
                foreach (DataRow dr in dt.Rows)
                {
                    dueDetails += dr["Role"].ToString() + " for " + dr["PayFor"].ToString() + " due " + dr["Due"].ToString() + " Tk, ";
                }

                // Try to get Due notification template
                string dueTemplate = GetSMSTemplate("Due", "Due"); // Updated to use "Due" category

                if (!string.IsNullOrEmpty(dueTemplate))
                {
                    // Use template
                    Msg = BuildDueNotificationMessage(dueTemplate, SName, ID, totalDue, dueDetails);
                }
                else
                {
                    // Default message
                    Msg = "Dear, " + SName + ". You've Due Payment(s): ";
                    foreach (DataRow dr in dt.Rows)
                    {
                        Msg += dr["Role"].ToString() + " for " + dr["PayFor"].ToString() + " due " + dr["Due"].ToString() + " Tk.";
                    }
                    Msg += "Total Due " + totalDue.ToString() + " Tk";
                    Msg += " Regards, " + Session["School_Name"].ToString();
                }

                TotalSMS = SMS.SMS_Conut(Msg);

                if (SMSBalance >= TotalSMS)
                {
                    if (SMS.SMS_GetBalance() >= TotalSMS)
                    {
                        Get_Validation IsValid = SMS.SMS_Validation(PhoneNo, Msg);

                        if (IsValid.Validation)
                        {
                            Guid SMS_Send_ID = SMS.SMS_Send(PhoneNo, Msg, "Due SMS");

                            if (SMS_Send_ID != Guid.Empty)
                            {
                                SMS_OtherInfoSQL.InsertParameters["SMS_Send_ID"].DefaultValue = SMS_Send_ID.ToString();
                                SMS_OtherInfoSQL.InsertParameters["SchoolID"].DefaultValue = Session["SchoolID"].ToString();
                                SMS_OtherInfoSQL.InsertParameters["EducationYearID"].DefaultValue = Session["Edu_Year"].ToString();
                                SMS_OtherInfoSQL.InsertParameters["StudentID"].DefaultValue = "";
                                SMS_OtherInfoSQL.InsertParameters["TeacherID"].DefaultValue = "";

                                SMS_OtherInfoSQL.Insert();

                                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('SMS Sent Successfully.')", true);
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
        protected void AllDueGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Footer)
            {
                Label InSumLabel = e.Row.FindControl("InSumLabel") as Label;
                InSumLabel.Text = SumFooter.ToString() + " Tk";
            }
        }
        protected void ExportWordButton_Click(object sender, EventArgs e)
        {
            Response.Clear();
            Response.ContentEncoding = Encoding.Unicode;
            Response.BinaryWrite(Encoding.Unicode.GetPreamble());

            Response.AddHeader("content-disposition", "attachment;filename=Current_Due.doc");
            Response.Charset = "";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.ContentType = "application/doc";

            StringWriter stringWrite = new StringWriter();
            HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);

            ExportPanel.RenderControl(htmlWrite);
            Response.Write(stringWrite.ToString());

            Response.End();
        }
        public override void VerifyRenderingInServerForm(Control control)
        {
            //'Export to word' required to avoid the run time error Control 
        }
        protected void TotalDueGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            GridView_Printer(TotalDueGridView);
        }
        private void GridView_Printer(GridView gridView)
        {
            if (gridView.Rows.Count > 0)
            {
                gridView.UseAccessibleHeader = true;
                gridView.HeaderRow.TableSection = TableRowSection.TableHeader;
            }
        }

        protected void RoleDropDownList_DataBound(object sender, EventArgs e)
        {
            RoleDropDownList.Items.Insert(0, new ListItem("[ All ROLE ]", "%"));
        }

        /// <summary>
        /// Get SMS Template from database by category and type
        /// </summary>
        private string GetSMSTemplate(string category, string templateType)
        {
            try
            {
                using (System.Data.SqlClient.SqlConnection tempCon = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
                {
                    tempCon.Open();

                    // First check if SMS_Template table exists
                    System.Data.SqlClient.SqlCommand checkTableCmd = new System.Data.SqlClient.SqlCommand(@"
       IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
      WHERE TABLE_NAME = 'SMS_Template')
      SELECT 1
ELSE
              SELECT 0", tempCon);

                    int tableExists = (int)checkTableCmd.ExecuteScalar();

                    if (tableExists == 0)
                    {
                        return string.Empty;
                    }

                    // Check if TemplateCategory column exists
                    System.Data.SqlClient.SqlCommand checkColumnCmd = new System.Data.SqlClient.SqlCommand(@"
     IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
   WHERE TABLE_NAME = 'SMS_Template' AND COLUMN_NAME = 'TemplateCategory')
           SELECT 1
  ELSE
              SELECT 0", tempCon);

                    int columnExists = (int)checkColumnCmd.ExecuteScalar();

                    string selectQuery;
                    if (columnExists == 1)
                    {
                        selectQuery = @"SELECT TOP 1 MessageTemplate 
  FROM SMS_Template 
        WHERE SchoolID = @SchoolID 
AND TemplateCategory = @TemplateCategory
        AND TemplateType = @TemplateType 
  AND IsActive = 1 
      ORDER BY CreatedDate DESC";
                    }
                    else
                    {
                        selectQuery = @"SELECT TOP 1 MessageTemplate 
    FROM SMS_Template 
          WHERE SchoolID = @SchoolID 
       AND TemplateType = @TemplateType 
        AND IsActive = 1 
       ORDER BY CreatedDate DESC";
                    }

                    System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(selectQuery, tempCon);
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                    if (columnExists == 1)
                    {
                        cmd.Parameters.AddWithValue("@TemplateCategory", category);
                    }
                    cmd.Parameters.AddWithValue("@TemplateType", templateType);

                    object result = cmd.ExecuteScalar();
                    return result != null ? result.ToString() : string.Empty;
                }
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Build Due notification SMS from template
        /// </summary>
        private string BuildDueNotificationMessage(string template, string studentName, string studentId, double totalDue, string dueDetails)
        {
            string message = template;

            // Replace placeholders
            message = message.Replace("{StudentName}", studentName);
            message = message.Replace("{ID}", studentId);
            message = message.Replace("{TotalDue}", totalDue.ToString("0.00"));
            
            // Clean up due details
            if (!string.IsNullOrEmpty(dueDetails))
            {
                dueDetails = dueDetails.TrimStart(',', ' ').TrimEnd(',', ' ');
  message = message.Replace("{DueDetails}", dueDetails);
            }
  else
    {
   message = message.Replace(", {DueDetails}", "")
        .Replace("{DueDetails}", "");
    }

            message = message.Replace("{SchoolName}", Session["School_Name"].ToString());

            return message;
        }
    }
}