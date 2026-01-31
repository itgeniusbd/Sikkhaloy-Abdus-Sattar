using Education;
using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Committee
{
    public partial class Donor_Present_Due : System.Web.UI.Page
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

        protected void DonationCategoryDropDownList_DataBound(object sender, EventArgs e)
        {
            DonationCategoryDropDownList.Items.Insert(0, new ListItem("[ ALL CATEGORIES ]", "%"));
        }
        
        protected void DueRadioButtonList_SelectedIndexChanged(object sender, EventArgs e)
        {
            DueMultiView.ActiveViewIndex = DueRadioButtonList.SelectedIndex;
        }
        
        protected void TypeSendButton_Click(object sender, EventArgs e)
        {
            ErrorLabel.Text = "";
            DataTable dt = new DataTable();
            bool smsSend = false;
            int SentMsgCont = 0;
            int FailedMsgCont = 0;

            SMS_Class SMS = new SMS_Class(Session["SchoolID"].ToString());

            int TotalSMS = 0;
            string PhoneNo = "";
            string Msg = "";
            int SMSBalance = SMS.SMSBalance;

            // Try to get Donor Due notification template
            string donorDueTemplate = GetSMSTemplate("Donor", "DonorDue");

            foreach (GridViewRow row in TotalDonorDueGridView.Rows)
            {
                CheckBox SMSCheckBox = row.FindControl("SingleCheckBox") as CheckBox;

                if (SMSCheckBox.Checked)
                {
                    string MemberName = TotalDonorDueGridView.DataKeys[row.RowIndex]["MemberName"].ToString();
                    double dueAmount = Convert.ToDouble(TotalDonorDueGridView.DataKeys[row.RowIndex]["Due"]);

                    PhoneNo = TotalDonorDueGridView.DataKeys[row.DataItemIndex]["SmsNumber"].ToString();

                    // Build due details
                    string dueDetails = "";
                    dt = GetDonorDueDetails(TotalDonorDueGridView.DataKeys[row.RowIndex]["CommitteeMemberId"].ToString());
                    foreach (DataRow dr in dt.Rows)
                    {
                        dueDetails += dr["DonationCategory"].ToString() + ": " + dr["Description"].ToString() + " - " + dr["Due"].ToString() + " Tk, ";
                    }

                    if (!string.IsNullOrEmpty(donorDueTemplate))
                    {
                        // Use template
                        Msg = BuildDonorDueNotificationMessage(donorDueTemplate, MemberName, dueAmount, dueDetails);
                    }
                    else
                    {
                        // Default message
                        Msg = "সম্মানিত দাতা, " + MemberName + ". আস্সালামু আলাইকুম, আপনার বকেয়া ডোনেশন: ";
                        Msg += dueAmount.ToString() + " টাকা. ";
                        Msg += "ধন্যবাদ, " + Session["School_Name"].ToString();
                    }

                    // Log the message for debugging
                    System.Diagnostics.Debug.WriteLine("=== SMS DEBUG ===");
                    System.Diagnostics.Debug.WriteLine("Original Message: " + Msg);
                    System.Diagnostics.Debug.WriteLine("Message Length: " + Msg.Length);
                    System.Diagnostics.Debug.WriteLine("UTF-8 Bytes: " + BitConverter.ToString(Encoding.UTF8.GetBytes(Msg)));
                    System.Diagnostics.Debug.WriteLine("================");

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
                    foreach (GridViewRow row in TotalDonorDueGridView.Rows)
                    {
                        CheckBox SingleCheckBox = (CheckBox)row.FindControl("SingleCheckBox");
                        if (SingleCheckBox.Checked)
                        {
                            PhoneNo = TotalDonorDueGridView.DataKeys[row.RowIndex]["SmsNumber"].ToString();

                            string MemberName = TotalDonorDueGridView.DataKeys[row.RowIndex]["MemberName"].ToString();
                            double dueAmount = Convert.ToDouble(TotalDonorDueGridView.DataKeys[row.RowIndex]["Due"]);

                            // Build due details
                            string dueDetails = "";
                            dt = GetDonorDueDetails(TotalDonorDueGridView.DataKeys[row.RowIndex]["CommitteeMemberId"].ToString());
                            foreach (DataRow dr in dt.Rows)
                            {
                                dueDetails += dr["DonationCategory"].ToString() + ": " + dr["Description"].ToString() + " - " + dr["Due"].ToString() + " Tk, ";
                            }

                            if (!string.IsNullOrEmpty(donorDueTemplate))
                            {
                                // Use template
                                Msg = BuildDonorDueNotificationMessage(donorDueTemplate, MemberName, dueAmount, dueDetails);
                            }
                            else
                            {
                                // Default message
                                Msg = "সম্মানিত দাতা , " + MemberName + ".আস্সালামু আলাইকুম, আপনার বকেয়া ডোনেশন: ";
                                Msg += dueAmount.ToString() + " টাকা. ";
                                Msg += "ধন্যবাদ, " + Session["School_Name"].ToString();
                            }

                            // Log the message for debugging
                            System.Diagnostics.Debug.WriteLine("=== SMS DEBUG ===");
                            System.Diagnostics.Debug.WriteLine("Original Message: " + Msg);
                            System.Diagnostics.Debug.WriteLine("Message Length: " + Msg.Length);
                            System.Diagnostics.Debug.WriteLine("UTF-8 Bytes: " + BitConverter.ToString(Encoding.UTF8.GetBytes(Msg)));
                            System.Diagnostics.Debug.WriteLine("================");

                            Get_Validation IsValid = SMS.SMS_Validation(PhoneNo, Msg);

                            if (IsValid.Validation)
                            {
                                Guid SMS_Send_ID = SMS.SMS_Send(PhoneNo, Msg, "Donor Due SMS");

                                if (SMS_Send_ID != Guid.Empty)
                                {
                                    string committeeMemberId = TotalDonorDueGridView.DataKeys[row.RowIndex]["CommitteeMemberId"].ToString();
                                    
                                    SMS_OtherInfoSQL.InsertParameters["SMS_Send_ID"].DefaultValue = SMS_Send_ID.ToString();
                                    SMS_OtherInfoSQL.InsertParameters["SchoolID"].DefaultValue = Session["SchoolID"].ToString();
                                    SMS_OtherInfoSQL.InsertParameters["EducationYearID"].DefaultValue = Session["Edu_Year"].ToString();
                                    SMS_OtherInfoSQL.InsertParameters["StudentID"].DefaultValue = null;
                                    SMS_OtherInfoSQL.InsertParameters["TeacherID"].DefaultValue = null;
                                    SMS_OtherInfoSQL.InsertParameters["CommitteeMemberId"].DefaultValue = committeeMemberId;
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
                TotalDonorDueGridView.DataBind();
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Successfully Sent " + SentMsgCont.ToString() + " SMS. & Failed " + FailedMsgCont.ToString() + ".')", true);
            }
        }

        double SumFooter = 0;
        protected void ViewAllDueButton_Click(object sender, EventArgs e)
        {
            DataTable dt = new DataTable();
            ArrayList values = new ArrayList();

            foreach (GridViewRow row in TotalDonorDueGridView.Rows)
            {
                CheckBox SingleCheckBox = (CheckBox)row.FindControl("SingleCheckBox");
                if (SingleCheckBox.Checked)
                {
                    values.Add(1);
                }
            }

            DonorDataList.DataSource = values;
            DonorDataList.DataBind();

            int a = 0;
            foreach (GridViewRow row in TotalDonorDueGridView.Rows)
            {
                CheckBox SingleCheckBox = (CheckBox)row.FindControl("SingleCheckBox");
                if (SingleCheckBox.Checked)
                {
                    string MemberName = TotalDonorDueGridView.DataKeys[row.RowIndex]["MemberName"].ToString();
                    string Phone = TotalDonorDueGridView.DataKeys[row.RowIndex]["SmsNumber"].ToString();
                    string CommitteeMemberId = TotalDonorDueGridView.DataKeys[row.RowIndex]["CommitteeMemberId"].ToString();

                    dt = GetDonorDueDetails(CommitteeMemberId);
                    DataListItem Iteam = DonorDataList.Items[a];

                    Label NameLabel = (Label)Iteam.FindControl("NameLabel");
                    GridView AllDueGV = (GridView)Iteam.FindControl("AllDueGridView");

                    SumFooter = Convert.ToDouble(TotalDonorDueGridView.DataKeys[row.RowIndex]["Due"]);
                    NameLabel.Text = MemberName + ", Phone: " + Phone;

                    AllDueGV.DataSource = dt;

                    AllDueGV.DataBind();
                    a++;
                }
            }

            ScriptManager.RegisterStartupScript(this, this.GetType(), "Pop", "openModal();", true);
        }
        
        protected void NameSendButton_Click(object sender, EventArgs e)
        {
            ErrorLabel.Text = "";
            DataTable dt = new DataTable();

            SMS_Class SMS = new SMS_Class(Session["SchoolID"].ToString());

            int TotalSMS = 0;
            string PhoneNo = "";
            string Msg = "";
            int SMSBalance = SMS.SMSBalance;

            if (Name_DueDetailsGridView.Rows.Count > 0)
            {
                string MemberName = DonorInfoFormView.DataKey["MemberName"].ToString();
                PhoneNo = DonorInfoFormView.DataKey["SmsNumber"].ToString();
                double totalDue = Convert.ToDouble(DonorInfoFormView.DataKey["TotalDue"]);

                // Get CommitteeMemberId from database
                string committeeMemberId = "";
                try
                {
                    using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
                    {
                        string query = "SELECT CommitteeMemberId FROM CommitteeMember WHERE SchoolID = @SchoolID AND SmsNumber = @SmsNumber";
                        SqlCommand cmd = new SqlCommand(query, con);
                        cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                        cmd.Parameters.AddWithValue("@SmsNumber", PhoneNo);
                        
                        con.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            committeeMemberId = result.ToString();
                        }
                        con.Close();
                    }
                }
                catch { }

                // Build due details from GridView
                string dueDetails = "";
                foreach (GridViewRow row in Name_DueDetailsGridView.Rows)
                {
                    dueDetails += row.Cells[0].Text + ": " + row.Cells[1].Text + " - " + row.Cells[4].Text + " Tk, ";
                }

                // Try to get Donor Due notification template
                string donorDueTemplate = GetSMSTemplate("Donor", "DonorDue");

                if (!string.IsNullOrEmpty(donorDueTemplate))
                {
                    // Use template
                    Msg = BuildDonorDueNotificationMessage(donorDueTemplate, MemberName, totalDue, dueDetails);
                }
                else
                {
                    // Default message
                    Msg = "সম্মানিত দাতা, " + MemberName + ". আস্সালামু আলাইকুম, আপনার বকেয়া ডোনেশন(গুলো): ";
                    foreach (GridViewRow row in Name_DueDetailsGridView.Rows)
                    {
                        Msg += row.Cells[0].Text + " - " + row.Cells[4].Text + " Tk, ";
                    }
                    Msg += "মোট বকেয়া " + totalDue.ToString() + " টাকা. ";
                    Msg += "ধন্যবাদ, " + Session["School_Name"].ToString();
                }

                TotalSMS = SMS.SMS_Conut(Msg);

                if (SMSBalance >= TotalSMS)
                {
                    if (SMS.SMS_GetBalance() >= TotalSMS)
                    {
                        Get_Validation IsValid = SMS.SMS_Validation(PhoneNo, Msg);

                        if (IsValid.Validation)
                        {
                            Guid SMS_Send_ID = SMS.SMS_Send(PhoneNo, Msg, "Donor Due SMS");

                            if (SMS_Send_ID != Guid.Empty)
                            {
                                SMS_OtherInfoSQL.InsertParameters["SMS_Send_ID"].DefaultValue = SMS_Send_ID.ToString();
                                SMS_OtherInfoSQL.InsertParameters["SchoolID"].DefaultValue = Session["SchoolID"].ToString();
                                SMS_OtherInfoSQL.InsertParameters["EducationYearID"].DefaultValue = Session["Edu_Year"].ToString();
                                SMS_OtherInfoSQL.InsertParameters["StudentID"].DefaultValue = null;
                                SMS_OtherInfoSQL.InsertParameters["TeacherID"].DefaultValue = null;
                                SMS_OtherInfoSQL.InsertParameters["CommitteeMemberId"].DefaultValue = string.IsNullOrEmpty(committeeMemberId) ? null : committeeMemberId;

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

            Response.AddHeader("content-disposition", "attachment;filename=Donor_Due.doc");
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
        
        protected void TotalDonorDueGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            GridView_Printer(TotalDonorDueGridView);
        }
        
        private void GridView_Printer(GridView gridView)
        {
            if (gridView.Rows.Count > 0)
            {
                gridView.UseAccessibleHeader = true;
                gridView.HeaderRow.TableSection = TableRowSection.TableHeader;
            }
        }

        /// <summary>
        /// Get Donor Due Details by CommitteeMemberId
        /// </summary>
        private DataTable GetDonorDueDetails(string committeeMemberId)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
                {
                    string query = @"SELECT CommitteeDonationCategory.DonationCategory, CommitteeDonation.Description, 
                                   CommitteeDonation.Amount, CommitteeDonation.PaidAmount, CommitteeDonation.Due
                                   FROM CommitteeDonation 
                                   INNER JOIN CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId 
                                   WHERE (CommitteeDonation.SchoolID = @SchoolID) AND (CommitteeDonation.Due > 0) 
                                   AND (CommitteeDonation.PromiseDate < GETDATE() OR CommitteeDonation.PromiseDate IS NULL)
                                   AND (CommitteeDonation.CommitteeMemberId = @CommitteeMemberId)
                                   ORDER BY CommitteeDonation.PromiseDate";
                    
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@SchoolID", SchoolID);
                    cmd.Parameters.AddWithValue("@CommitteeMemberId", committeeMemberId);
                    
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                // Log error if needed
            }
            return dt;
        }

        /// <summary>
        /// Get SMS Template from database by category and type
        /// </summary>
        private string GetSMSTemplate(string category, string templateType)
        {
            try
            {
                using (SqlConnection tempCon = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
                {
                    tempCon.Open();

                    // First check if SMS_Template table exists
                    SqlCommand checkTableCmd = new SqlCommand(@"
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
                    SqlCommand checkColumnCmd = new SqlCommand(@"
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

                    SqlCommand cmd = new SqlCommand(selectQuery, tempCon);
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
        /// Build Donor Due notification SMS from template
        /// </summary>
        private string BuildDonorDueNotificationMessage(string template, string donorName, double totalDue, string dueDetails)
        {
            string message = template;

            // Replace placeholders
            message = message.Replace("{DonorName}", donorName);
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
