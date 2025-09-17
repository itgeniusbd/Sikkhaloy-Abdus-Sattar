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
            AddressTextBox.Text = "";
            TypeDropDownList.SelectedIndex = 0;
            
            // Show success message (you can add a label for this)
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
            TextBox addressTB = (TextBox)row.FindControl("AddressTB");
            DropDownList editTypeDropDownList = (DropDownList)row.FindControl("EditTypeDropDownList");
            FileUpload editPhotoFileUpload = (FileUpload)row.FindControl("EditPhotoFileUpload");
            
            // Get the member ID
            int memberId = Convert.ToInt32(MemberGridView.DataKeys[row.RowIndex].Value);
            
            // Update the member with or without photo
            UpdateMemberData(memberId, memberNameTB.Text, referenceByTB.Text, smsNumberTB.Text, 
                            addressTB.Text, Convert.ToInt32(editTypeDropDownList.SelectedValue), editPhotoFileUpload);
            
            // Exit edit mode and rebind
            MemberGridView.EditIndex = -1;
            MemberGridView.DataBind();
        }

        private void UpdateMemberData(int memberId, string memberName, string referenceBy, string smsNumber, 
                                    string address, int memberTypeId, FileUpload photoUpload)
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
                                      Address = @Address, 
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
                                      Address = @Address 
                                  WHERE CommitteeMemberId = @CommitteeMemberId";
                    
                    cmd = new SqlCommand(updateQuery, con);
                }
                
                // Add common parameters
                cmd.Parameters.AddWithValue("@CommitteeMemberTypeId", memberTypeId);
                cmd.Parameters.AddWithValue("@MemberName", memberName);
                cmd.Parameters.AddWithValue("@ReferenceBy", referenceBy);
                cmd.Parameters.AddWithValue("@SmsNumber", smsNumber);
                cmd.Parameters.AddWithValue("@Address", address);
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
