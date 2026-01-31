using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using Education;

namespace EDUCATION.COM.Committee
{
    public partial class Create_Donor_Username_password : System.Web.UI.Page
    {
        private string _lastRegId;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindGrids();
            }
            else
            {
                // Update debug info on postback too
                UpdateDebugInfo();
            }
        }

        protected void DonorTypeDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindGrids();
        }

        protected void FindButton_Click(object sender, EventArgs e)
        {
            BindGrids();
        }

        private void BindGrids()
        {
            try
            {
                string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                using (var conn = new System.Data.SqlClient.SqlConnection(connStr))
                {
                    conn.Open();
                    
                    // Bind CreateUserGridView (donors without registration)
                    string typeId = DonorTypeDropDownList.SelectedValue;
                    string search = FindDonorTextBox.Text.Trim();
                    
                    string query1 = @"
                        SELECT CM.CommitteeMemberId, CM.MemberName, 
                               LTRIM(RTRIM(ISNULL(CM.SmsNumber, ''))) as SmsNumber, 
                               ISNULL(CM.Address, '') as Address, 
                               CMT.CommitteeMemberType
                        FROM CommitteeMember CM 
                        INNER JOIN CommitteeMemberType CMT ON CM.CommitteeMemberTypeId = CMT.CommitteeMemberTypeId 
                        WHERE CM.SchoolID = @SchoolID 
                        AND CM.CommitteeMemberId IS NOT NULL
                        AND (ISNULL(@CommitteeMemberTypeId, '') = '' OR CM.CommitteeMemberTypeId = @CommitteeMemberTypeId)
                        AND (ISNULL(@Search, '') = '' OR CM.MemberName LIKE '%' + @Search + '%' OR CM.SmsNumber LIKE '%' + @Search + '%')
                        AND NOT EXISTS (
                            SELECT 1 FROM Registration R 
                            WHERE R.SchoolID = CM.SchoolID 
                            AND R.CommitteeMemberId = CM.CommitteeMemberId
                        )
                        ORDER BY CM.InsertDate DESC";
                    
                    var cmd1 = new System.Data.SqlClient.SqlCommand(query1, conn);
                    cmd1.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                    cmd1.Parameters.AddWithValue("@CommitteeMemberTypeId", string.IsNullOrEmpty(typeId) ? "" : typeId);
                    cmd1.Parameters.AddWithValue("@Search", string.IsNullOrEmpty(search) ? "" : search);
                    
                    var adapter1 = new System.Data.SqlClient.SqlDataAdapter(cmd1);
                    var dt1 = new System.Data.DataTable();
                    adapter1.Fill(dt1);
                    
                    CreateUserGridView.DataSource = dt1;
                    CreateUserGridView.DataBind();
                    
                    // Bind AlreadyCreatedGridView (donors with registration)
                    string query2 = @"
                        SELECT CM.CommitteeMemberId, CM.MemberName, 
                               ISNULL(CM.SmsNumber, '') as SmsNumber, 
                               R.UserName, ISNULL(A.Password, '') as Password, 
                               R.CreateDate 
                        FROM CommitteeMember CM 
                        INNER JOIN Registration R ON R.SchoolID = CM.SchoolID 
                            AND R.CommitteeMemberId = CM.CommitteeMemberId
                        LEFT JOIN AST A ON R.RegistrationID = A.RegistrationID
                        WHERE CM.SchoolID = @SchoolID 
                        AND CM.CommitteeMemberId IS NOT NULL
                        AND (ISNULL(@CommitteeMemberTypeId, '') = '' OR CM.CommitteeMemberTypeId = @CommitteeMemberTypeId)
                        AND (ISNULL(@Search, '') = '' OR CM.MemberName LIKE '%' + @Search + '%' OR CM.SmsNumber LIKE '%' + @Search + '%')
                        ORDER BY R.CreateDate DESC";
                    
                    var cmd2 = new System.Data.SqlClient.SqlCommand(query2, conn);
                    cmd2.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                    cmd2.Parameters.AddWithValue("@CommitteeMemberTypeId", string.IsNullOrEmpty(typeId) ? "" : typeId);
                    cmd2.Parameters.AddWithValue("@Search", string.IsNullOrEmpty(search) ? "" : search);
                    
                    var adapter2 = new System.Data.SqlClient.SqlDataAdapter(cmd2);
                    var dt2 = new System.Data.DataTable();
                    adapter2.Fill(dt2);
                    
                    AlreadyCreatedGridView.DataSource = dt2;
                    AlreadyCreatedGridView.DataBind();
                }
                
                UpdateDebugInfo();
            }
            catch (Exception ex)
            {
                ErrorLabel.Text = "Error loading data: " + ex.Message;
            }
        }

        private void UpdateDebugInfo()
        {
            int alreadyCreated = AlreadyCreatedGridView.Rows.Count;
            int readyToCreate = CreateUserGridView.Rows.Count;
            
            DebugLabel.Text = $"Total: {alreadyCreated + readyToCreate} | Ready to Create: {readyToCreate} | Already Created: {alreadyCreated}";
        }

        protected void AllCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox allCb = (CheckBox)sender;
            foreach (GridViewRow row in CreateUserGridView.Rows)
            {
                CheckBox cb = (CheckBox)row.FindControl("SingleCheckBox");
                cb.Checked = allCb.Checked;
            }
            
            UpdateDebugInfo();
        }

        protected void AllSMSCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox allCb = (CheckBox)sender;
            foreach (GridViewRow row in AlreadyCreatedGridView.Rows)
            {
                CheckBox cb = (CheckBox)row.FindControl("SingleSMSCheckBox");
                if (cb != null) cb.Checked = allCb.Checked;
            }
            
            UpdateDebugInfo();
        }

        protected void CreateUserButton_Click(object sender, EventArgs e)
        {
            bool isCreated = false;
            ErrorLabel.Text = "";
            
            foreach (GridViewRow row in CreateUserGridView.Rows)
            {
                CheckBox cb = (CheckBox)row.FindControl("SingleCheckBox");
                if (cb.Checked)
                {
                    string memberId = CreateUserGridView.DataKeys[row.RowIndex]["CommitteeMemberId"].ToString();
                    string donorName = CreateUserGridView.DataKeys[row.RowIndex]["MemberName"].ToString();
                    string smsNumber = CreateUserGridView.DataKeys[row.RowIndex]["SmsNumber"].ToString().Trim();

                    if (string.IsNullOrEmpty(smsNumber)) continue;

                    // Generate unique username: SchoolID + Random 6-digit number
                    string username = GenerateUniqueUsername();
                    
                    // Generate random 6-digit password
                    Random rnd = new Random();
                    string password = rnd.Next(100000, 999999).ToString();

                    try
                    {
                        // Create user in ASP.NET Membership
                        if (Membership.GetUser(username) == null)
                        {
                            Membership.CreateUser(username, password);
                            if (!Roles.RoleExists("Donor")) Roles.CreateRole("Donor");
                            Roles.AddUserToRole(username, "Donor");
                        }

                        // Insert into Registration table
                        RegistrationSQL.InsertParameters["UserName"].DefaultValue = username;
                        RegistrationSQL.InsertParameters["CommitteeMemberId"].DefaultValue = memberId;
                        RegistrationSQL.Insert();

                        if (!string.IsNullOrEmpty(_lastRegId))
                        {
                            // Insert into AST (password storage)
                            ASTSQL.InsertParameters["RegistrationID"].DefaultValue = _lastRegId;
                            ASTSQL.InsertParameters["UserName"].DefaultValue = username;
                            ASTSQL.InsertParameters["Password"].DefaultValue = password;
                            ASTSQL.InsertParameters["SmsNumber"].DefaultValue = smsNumber;
                            ASTSQL.Insert();

                            // Insert into EducationYearUser
                            EduYearUserSQL.InsertParameters["RegistrationID"].DefaultValue = _lastRegId;
                            EduYearUserSQL.Insert();

                            isCreated = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorLabel.Text += $"Error for {donorName}: {ex.Message}<br/>";
                    }
                }
            }

            if (isCreated)
            {
                BindGrids();
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alert", "alert('Selected Donor accounts created successfully!');", true);
            }
            else if (ErrorLabel.Text == "")
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alert", "alert('Please select at least one donor to create account.');", true);
            }
        }

        private string GenerateUniqueUsername()
        {
            string schoolId = Session["SchoolID"]?.ToString() ?? "0";
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            string username = "";
            int attempts = 0;
            
            do
            {
                // Generate: SchoolID + 6 random digits
                int randomNumber = rnd.Next(100000, 999999);
                username = schoolId + randomNumber.ToString();
                attempts++;
                
                // Safety check to prevent infinite loop
                if (attempts > 50)
                {
                    // If too many attempts, use timestamp
                    username = schoolId + DateTime.Now.ToString("HHmmssfff");
                    break;
                }
                
            } while (Membership.GetUser(username) != null);
            
            return username;
        }

        protected void RegistrationSQL_Inserted(object sender, SqlDataSourceStatusEventArgs e)
        {
            if (e.Command.Parameters["@RegistrationID"].Value != null)
            {
                _lastRegId = e.Command.Parameters["@RegistrationID"].Value.ToString();
            }
        }

        protected void SendSMSButton_Click(object sender, EventArgs e)
        {
            SMS_Class sms = new SMS_Class(Session["SchoolID"].ToString());
            string schoolName = Session["School_Name"]?.ToString() ?? "";
            bool isSmsSent = false;

            foreach (GridViewRow row in AlreadyCreatedGridView.Rows)
            {
                CheckBox cb = (CheckBox)row.FindControl("SingleSMSCheckBox");
                if (cb.Checked)
                {
                    string donorName = AlreadyCreatedGridView.DataKeys[row.RowIndex]["MemberName"].ToString();
                    string phone = AlreadyCreatedGridView.DataKeys[row.RowIndex]["SmsNumber"].ToString();
                    string user = AlreadyCreatedGridView.DataKeys[row.RowIndex]["UserName"].ToString();
                    string pass = AlreadyCreatedGridView.DataKeys[row.RowIndex]["Password"].ToString();

                    string msg = $" সম্মানিত দাতা {donorName},আপনার লগিন ইউজার আইডি: {user},ও পাসওয়ার্ড: {pass}. ভবিষ্যতের জন্য সংরক্ষণ করুন, ধন্যবাদ:, {schoolName}";

                    if (sms.SMSBalance > 0)
                    {
                        var result = sms.SMS_Send(phone, msg, "Donor Login Info");
                        if (result != Guid.Empty)
                        {
                            isSmsSent = true;
                        }
                    }
                }
            }

            if (isSmsSent)
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alert", "alert('SMS login details sent successfully!')", true);
            }
        }
    }
}
