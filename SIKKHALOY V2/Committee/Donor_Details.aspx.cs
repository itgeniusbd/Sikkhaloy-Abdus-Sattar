using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Committee
{
    public partial class Donor_Details : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["RegistrationID"] == null)
            {
                Response.Redirect("~/Default.aspx");
                return;
            }
        }

        protected void UpdateButton_Click(object sender, EventArgs e)
        {
            try
            {
                TextBox memberNameTB = (TextBox)DonorInfoFV.FindControl("MemberNameTB");
                TextBox smsNumberTB = (TextBox)DonorInfoFV.FindControl("SmsNumberTB");
                TextBox emailTB = (TextBox)DonorInfoFV.FindControl("EmailTB");
                TextBox addressTB = (TextBox)DonorInfoFV.FindControl("AddressTB");
                Label messageLabel = (Label)DonorInfoFV.FindControl("MessageLabel");

                if (memberNameTB == null || smsNumberTB == null || addressTB == null)
                {
                    ShowMessage("Error: Required fields not found", "danger", messageLabel);
                    return;
                }

                int committeeMemberId = GetCommitteeMemberId();
                if (committeeMemberId == 0)
                {
                    ShowMessage("Error: Donor ID not found", "danger", messageLabel);
                    return;
                }

                bool photoUpdated = false;
                string constr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

                using (SqlConnection con = new SqlConnection(constr))
                {
                    con.Open();

                    string query = @"UPDATE CM SET 
                                   MemberName = @MemberName, 
                                   SmsNumber = @SmsNumber, 
                                   Email = @Email, 
                                   Address = @Address 
                                   FROM CommitteeMember CM
                                   INNER JOIN Registration R ON R.SchoolID = CM.SchoolID AND R.CommitteeMemberId = CM.CommitteeMemberId
                                   WHERE R.RegistrationID = @RegistrationID AND R.SchoolID = @SchoolID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@MemberName", memberNameTB.Text.Trim());
                        cmd.Parameters.AddWithValue("@SmsNumber", smsNumberTB.Text.Trim());
                        cmd.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(emailTB.Text) ? (object)DBNull.Value : emailTB.Text.Trim());
                        cmd.Parameters.AddWithValue("@Address", addressTB.Text.Trim());
                        cmd.Parameters.AddWithValue("@RegistrationID", Session["RegistrationID"]);
                        cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                        cmd.ExecuteNonQuery();
                    }

                    if (PhotoUploadControl.HasFile)
                    {
                        string fileExtension = Path.GetExtension(PhotoUploadControl.FileName).ToLower();
                        string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
                        
                        if (Array.IndexOf(allowedExtensions, fileExtension) == -1)
                        {
                            ShowMessage("Invalid photo file type. Please upload JPG, PNG, or GIF image only.", "warning", messageLabel);
                            return;
                        }

                        if (PhotoUploadControl.FileBytes.Length > 2097152)
                        {
                            ShowMessage("Photo file size too large. Maximum allowed size is 2MB.", "warning", messageLabel);
                            return;
                        }

                        string updatePhotoQuery = "UPDATE CommitteeMember SET Photo = @Photo WHERE CommitteeMemberId = @CommitteeMemberId AND SchoolID = @SchoolID";
                        using (SqlCommand cmd = new SqlCommand(updatePhotoQuery, con))
                        {
                            cmd.Parameters.AddWithValue("@Photo", PhotoUploadControl.FileBytes);
                            cmd.Parameters.AddWithValue("@CommitteeMemberId", committeeMemberId);
                            cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                            cmd.ExecuteNonQuery();
                            photoUpdated = true;
                        }
                    }
                }

                DonorInfoFV.DataBind();

                string successMsg = "? Profile updated successfully!";
                if (photoUpdated)
                {
                    successMsg = "? Profile and Photo updated successfully!";
                }
                
                ShowMessage(successMsg, "success", messageLabel);
            }
            catch (Exception ex)
            {
                Label messageLabel = (Label)DonorInfoFV.FindControl("MessageLabel");
                ShowMessage("Error updating profile: " + ex.Message, "danger", messageLabel);
            }
        }

        private int GetCommitteeMemberId()
        {
            int committeeMemberId = 0;
            string constr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = @"SELECT CM.CommitteeMemberId 
                               FROM CommitteeMember CM
                               INNER JOIN Registration R ON R.SchoolID = CM.SchoolID AND R.CommitteeMemberId = CM.CommitteeMemberId
                               WHERE R.RegistrationID = @RegistrationID AND R.SchoolID = @SchoolID";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@RegistrationID", Session["RegistrationID"]);
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"]);
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        committeeMemberId = Convert.ToInt32(result);
                    }
                }
            }

            return committeeMemberId;
        }

        private void ShowMessage(string message, string type, Label messageLabel)
        {
            if (messageLabel != null)
            {
                messageLabel.Text = message;
                messageLabel.CssClass = $"alert alert-{type} d-block";
                messageLabel.Attributes["role"] = "alert";
                
                string script = @"
                    window.scrollTo({top: 0, behavior: 'smooth'});
                    setTimeout(function() {
                        $('.alert').fadeOut('slow', function() {
                            $(this).removeClass('d-block').addClass('d-none');
                        });
                    }, 5000);
                ";
                ScriptManager.RegisterStartupScript(this, GetType(), "HideMessage", script, true);
            }
        }
    }
}
