using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Services;
using System.Web.UI.WebControls;
using System.Web.UI;

namespace EDUCATION.COM.Committee
{
    public partial class MemberAdd : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {


        }

        protected void AddMemberButton_Click(object sender, EventArgs e)
        {
            MemberSQL.Insert();
            MemberGridView.DataBind();
            
            // Clear form after successful insertion
            MemberNameTextBox.Text = "";
            ReferenceByTextBox.Text = "";
            PhoneTextBox.Text = "";
            EmailTextBox.Text = "";
            AddressTextBox.Text = "";
            TypeDropDownList.SelectedIndex = 0;
            
            ScriptManager.RegisterStartupScript(this, GetType(), "showalert", "alert('Member added successfully!');", true);
        }

        protected void UpdateMember_Command(object sender, CommandEventArgs e)
        {
            LinkButton btn = (LinkButton)sender;
            GridViewRow row = (GridViewRow)btn.NamingContainer;
            
            // Get controls from the edit row
            TextBox memberNameTB = (TextBox)row.FindControl("MemberNameTB");
            TextBox referenceByTB = (TextBox)row.FindControl("ReferenceByTB");
            TextBox smsNumberTB = (TextBox)row.FindControl("SmsNumberTB");
            TextBox emailTB = (TextBox)row.FindControl("EmailTB");
            TextBox addressTB = (TextBox)row.FindControl("AddressTB");
            DropDownList editTypeDropDownList = (DropDownList)row.FindControl("EditTypeDropDownList");
            DropDownList statusDropDownList = (DropDownList)row.FindControl("StatusDropDownList");
            FileUpload editPhotoFileUpload = (FileUpload)row.FindControl("EditPhotoFileUpload");
            
            // Get the member ID
            int memberId = Convert.ToInt32(MemberGridView.DataKeys[row.RowIndex].Value);
            
            // Check for duplicate phone number (excluding current member)
            string phone = smsNumberTB.Text.Trim();
            if (IsDuplicatePhoneForUpdate(phone, memberId))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showalert", 
                    "alert('এই মোবাইল নাম্বার দিয়ে ইতিমধ্যেই অন্য একজন ডোনার/মেম্বার যুক্ত আছে।');", true);
                return;
            }
            
            // Update the member with or without photo
            UpdateMemberData(memberId, memberNameTB.Text, referenceByTB.Text, smsNumberTB.Text, 
                            emailTB.Text, addressTB.Text, Convert.ToInt32(editTypeDropDownList.SelectedValue), 
                            statusDropDownList != null ? statusDropDownList.SelectedValue : "Active",
                            editPhotoFileUpload);
            
            // Exit edit mode and rebind
            MemberGridView.EditIndex = -1;
            MemberGridView.DataBind();
        }

        private bool IsDuplicatePhoneForUpdate(string phone, int currentMemberId)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
            {
                string query = @"SELECT COUNT(*) FROM CommitteeMember 
                               WHERE SchoolID = @SchoolID 
                               AND LTRIM(RTRIM(SmsNumber)) = @Phone
                               AND CommitteeMemberId != @CommitteeMemberId";
                
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                cmd.Parameters.AddWithValue("@Phone", phone);
                cmd.Parameters.AddWithValue("@CommitteeMemberId", currentMemberId);
                
                con.Open();
                int count = (int)cmd.ExecuteScalar();
                con.Close();
                
                return count > 0;
            }
        }

        private void UpdateMemberData(int memberId, string memberName, string referenceBy, string smsNumber, 
                                    string email, string address, int memberTypeId, string status, FileUpload photoUpload)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString))
            {
                con.Open();
                string updateQuery;
                SqlCommand cmd;
                
                if (photoUpload.HasFile)
                {
                    // Update with new photo
                    updateQuery = @"UPDATE CommitteeMember 
                                  SET CommitteeMemberTypeId = @CommitteeMemberTypeId, 
                                      MemberName = @MemberName, 
                                      ReferenceBy = @ReferenceBy, 
                                      SmsNumber = @SmsNumber, 
                                      Email = @Email,
                                      Address = @Address, 
                                      Status = @Status,
                                      Photo = @Photo 
                                  WHERE CommitteeMemberId = @CommitteeMemberId";
                    
                    cmd = new SqlCommand(updateQuery, con);
                    cmd.Parameters.AddWithValue("@Photo", photoUpload.FileBytes);
                }
                else
                {
                    // Update without changing photo
                    updateQuery = @"UPDATE CommitteeMember 
                                  SET CommitteeMemberTypeId = @CommitteeMemberTypeId, 
                                      MemberName = @MemberName, 
                                      ReferenceBy = @ReferenceBy, 
                                      SmsNumber = @SmsNumber, 
                                      Email = @Email,
                                      Address = @Address,
                                      Status = @Status
                                  WHERE CommitteeMemberId = @CommitteeMemberId";
                    
                    cmd = new SqlCommand(updateQuery, con);
                }
                
                // Add common parameters
                cmd.Parameters.AddWithValue("@CommitteeMemberTypeId", memberTypeId);
                cmd.Parameters.AddWithValue("@MemberName", memberName);
                cmd.Parameters.AddWithValue("@ReferenceBy", referenceBy);
                cmd.Parameters.AddWithValue("@SmsNumber", smsNumber.Trim());
                cmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email.Trim());
                cmd.Parameters.AddWithValue("@Address", address);
                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@CommitteeMemberId", memberId);
                
                cmd.ExecuteNonQuery();
            }
        }

        protected void MemberGridView_RowEditing(object sender, GridViewEditEventArgs e)
        {
            MemberGridView.EditIndex = e.NewEditIndex;
            MemberGridView.DataBind();
        }

        protected void MemberGridView_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            MemberGridView.EditIndex = -1;
            MemberGridView.DataBind();
        }

        protected void MemberGridView_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            // This will be handled by our custom UpdateMember_Command method
            // We can leave this empty or add additional logic if needed
        }
    }
}
