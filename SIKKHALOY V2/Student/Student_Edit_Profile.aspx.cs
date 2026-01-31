using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Student
{
    public partial class Student_Edit_Profile : Page
    {
        protected FileUpload GuardianPhotoUploadControl;
        
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["RegistrationID"] == null)
            {
                Response.Redirect("~/Default.aspx");
                return;
            }

            if (Session["StudentID"] == null)
            {
                string constr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(constr))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("SELECT StudentID FROM Student WHERE StudentRegistrationID = @RegistrationID", con);
                    cmd.Parameters.AddWithValue("@RegistrationID", Session["RegistrationID"]);
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        Session["StudentID"] = result;
                    }
                }
            }
            
            StudentInfoFV.DataBound += StudentInfoFV_DataBound;
        }

        protected void StudentInfoFV_DataBound(object sender, EventArgs e)
        {
            if (StudentInfoFV.CurrentMode == FormViewMode.ReadOnly)
            {
                DropDownList bloodGroupDDL = (DropDownList)StudentInfoFV.FindControl("BloodGroupDDL");
                DropDownList religionDDL = (DropDownList)StudentInfoFV.FindControl("ReligionDDL");

                if (bloodGroupDDL != null && StudentInfoFV.DataItem != null)
                {
                    DataRowView drv = (DataRowView)StudentInfoFV.DataItem;
                    string bloodGroup = drv["BloodGroup"]?.ToString() ?? "";
                    
                    if (!string.IsNullOrEmpty(bloodGroup) && bloodGroupDDL.Items.FindByValue(bloodGroup) != null)
                    {
                        bloodGroupDDL.SelectedValue = bloodGroup;
                    }
                    else
                    {
                        bloodGroupDDL.SelectedIndex = 0;
                    }
                }

                if (religionDDL != null && StudentInfoFV.DataItem != null)
                {
                    DataRowView drv = (DataRowView)StudentInfoFV.DataItem;
                    string religion = drv["Religion"]?.ToString() ?? "";
                    
                    if (!string.IsNullOrEmpty(religion) && religionDDL.Items.FindByValue(religion) != null)
                    {
                        religionDDL.SelectedValue = religion;
                    }
                    else
                    {
                        religionDDL.SelectedIndex = 0;
                    }
                }
            }
        }

        protected void UpdateButton_Click(object sender, EventArgs e)
        {
            try
            {
                TextBox studentEmailAddressTB = (TextBox)StudentInfoFV.FindControl("StudentEmailAddressTB");
                TextBox dateofBirthTB = (TextBox)StudentInfoFV.FindControl("DateofBirthTB");
                TextBox legal_IdentityTB = (TextBox)StudentInfoFV.FindControl("Legal_IdentityTB");
                DropDownList bloodGroupDDL = (DropDownList)StudentInfoFV.FindControl("BloodGroupDDL");
                DropDownList religionDDL = (DropDownList)StudentInfoFV.FindControl("ReligionDDL");
                TextBox studentsLocalAddressTB = (TextBox)StudentInfoFV.FindControl("StudentsLocalAddressTB");
                TextBox studentPermanentAddressTB = (TextBox)StudentInfoFV.FindControl("StudentPermanentAddressTB");
                TextBox mothersNameTB = (TextBox)StudentInfoFV.FindControl("MothersNameTB");
                TextBox motherOccupationTB = (TextBox)StudentInfoFV.FindControl("MotherOccupationTB");
                TextBox motherPhoneNumberTB = (TextBox)StudentInfoFV.FindControl("MotherPhoneNumberTB");
                TextBox fathersNameTB = (TextBox)StudentInfoFV.FindControl("FathersNameTB");
                TextBox fatherOccupationTB = (TextBox)StudentInfoFV.FindControl("FatherOccupationTB");
                TextBox fatherPhoneNumberTB = (TextBox)StudentInfoFV.FindControl("FatherPhoneNumberTB");
                TextBox guardianNameTB = (TextBox)StudentInfoFV.FindControl("GuardianNameTB");
                TextBox guardianRelationshipTB = (TextBox)StudentInfoFV.FindControl("GuardianRelationshipTB");
                TextBox guardianPhoneNumberTB = (TextBox)StudentInfoFV.FindControl("GuardianPhoneNumberTB");
                Label messageLabel = (Label)StudentInfoFV.FindControl("MessageLabel");

                int studentID = 0;
                if (Session["StudentID"] != null)
                {
                    studentID = Convert.ToInt32(Session["StudentID"]);
                }
                else
                {
                    string constr = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                    using (SqlConnection con = new SqlConnection(constr))
                    {
                        con.Open();
                        SqlCommand cmd = new SqlCommand("SELECT StudentID FROM Student WHERE StudentRegistrationID = @RegistrationID", con);
                        cmd.Parameters.AddWithValue("@RegistrationID", Session["RegistrationID"]);
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            studentID = Convert.ToInt32(result);
                            Session["StudentID"] = studentID;
                        }
                    }
                }

                if (studentID == 0)
                {
                    ShowMessage("Error: Student ID not found", "danger", messageLabel);
                    return;
                }

                bool photoUpdated = false;
                bool guardianPhotoUpdated = false;

                string constr2 = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(constr2))
                {
                    con.Open();

                    string query = @"UPDATE Student 
                                   SET StudentEmailAddress = @StudentEmailAddress,
                                       DateofBirth = @DateofBirth,
                                       Legal_Identity = @Legal_Identity,
                                       BloodGroup = @BloodGroup,
                                       Religion = @Religion,
                                       StudentsLocalAddress = @StudentsLocalAddress,
                                       StudentPermanentAddress = @StudentPermanentAddress,
                                       MothersName = @MothersName,
                                       MotherOccupation = @MotherOccupation,
                                       MotherPhoneNumber = @MotherPhoneNumber,
                                       FathersName = @FathersName,
                                       FatherOccupation = @FatherOccupation,
                                       FatherPhoneNumber = @FatherPhoneNumber,
                                       GuardianName = @GuardianName,
                                       GuardianRelationshipwithStudent = @GuardianRelationshipwithStudent,
                                       GuardianPhoneNumber = @GuardianPhoneNumber
                                   WHERE StudentID = @StudentID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@StudentEmailAddress", 
                            string.IsNullOrWhiteSpace(studentEmailAddressTB?.Text) ? (object)DBNull.Value : studentEmailAddressTB.Text.Trim());
                        cmd.Parameters.AddWithValue("@DateofBirth", 
                            string.IsNullOrWhiteSpace(dateofBirthTB?.Text) ? (object)DBNull.Value : DateTime.Parse(dateofBirthTB.Text));
                        cmd.Parameters.AddWithValue("@Legal_Identity", legal_IdentityTB?.Text.Trim() ?? "");
                        cmd.Parameters.AddWithValue("@BloodGroup", bloodGroupDDL?.SelectedValue ?? "");
                        cmd.Parameters.AddWithValue("@Religion", religionDDL?.SelectedValue ?? "");
                        cmd.Parameters.AddWithValue("@StudentsLocalAddress", studentsLocalAddressTB?.Text.Trim() ?? "");
                        cmd.Parameters.AddWithValue("@StudentPermanentAddress", studentPermanentAddressTB?.Text.Trim() ?? "");
                        cmd.Parameters.AddWithValue("@MothersName", mothersNameTB?.Text.Trim() ?? "");
                        cmd.Parameters.AddWithValue("@MotherOccupation", motherOccupationTB?.Text.Trim() ?? "");
                        cmd.Parameters.AddWithValue("@MotherPhoneNumber", motherPhoneNumberTB?.Text.Trim() ?? "");
                        cmd.Parameters.AddWithValue("@FathersName", fathersNameTB?.Text.Trim() ?? "");
                        cmd.Parameters.AddWithValue("@FatherOccupation", fatherOccupationTB?.Text.Trim() ?? "");
                        cmd.Parameters.AddWithValue("@FatherPhoneNumber", fatherPhoneNumberTB?.Text.Trim() ?? "");
                        cmd.Parameters.AddWithValue("@GuardianName", guardianNameTB?.Text.Trim() ?? "");
                        cmd.Parameters.AddWithValue("@GuardianRelationshipwithStudent", guardianRelationshipTB?.Text.Trim() ?? "");
                        cmd.Parameters.AddWithValue("@GuardianPhoneNumber", guardianPhoneNumberTB?.Text.Trim() ?? "");
                        cmd.Parameters.AddWithValue("@StudentID", studentID);

                        cmd.ExecuteNonQuery();
                    }

                    if (PhotoUploadControl.HasFile)
                    {
                        string fileExtension = Path.GetExtension(PhotoUploadControl.FileName).ToLower();
                        string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
                        
                        if (Array.IndexOf(allowedExtensions, fileExtension) == -1)
                        {
                            ShowMessage("Invalid student photo file type. Please upload JPG, PNG, or GIF image only.", "warning", messageLabel);
                            return;
                        }

                        if (PhotoUploadControl.FileBytes.Length > 2097152)
                        {
                            ShowMessage("Student photo file size too large. Maximum allowed size is 2MB.", "warning", messageLabel);
                            return;
                        }

                        string getImageIdQuery = "SELECT StudentImageID FROM Student WHERE StudentID = @StudentID";
                        int studentImageID = 0;

                        using (SqlCommand cmd = new SqlCommand(getImageIdQuery, con))
                        {
                            cmd.Parameters.AddWithValue("@StudentID", studentID);
                            object result = cmd.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                            {
                                studentImageID = Convert.ToInt32(result);
                            }
                        }

                        if (studentImageID > 0)
                        {
                            string updateImageQuery = "UPDATE Student_Image SET Image = @ImageData WHERE StudentImageID = @StudentImageID";
                            using (SqlCommand cmd = new SqlCommand(updateImageQuery, con))
                            {
                                cmd.Parameters.AddWithValue("@ImageData", PhotoUploadControl.FileBytes);
                                cmd.Parameters.AddWithValue("@StudentImageID", studentImageID);
                                cmd.ExecuteNonQuery();
                                photoUpdated = true;
                            }
                        }
                        else
                        {
                            string insertImageQuery = @"INSERT INTO Student_Image (Image) VALUES (@ImageData);
                                                      SELECT SCOPE_IDENTITY();";
                            using (SqlCommand cmd = new SqlCommand(insertImageQuery, con))
                            {
                                cmd.Parameters.AddWithValue("@ImageData", PhotoUploadControl.FileBytes);
                                studentImageID = Convert.ToInt32(cmd.ExecuteScalar());
                                photoUpdated = true;
                            }

                            string updateStudentQuery = "UPDATE Student SET StudentImageID = @StudentImageID WHERE StudentID = @StudentID";
                            using (SqlCommand cmd = new SqlCommand(updateStudentQuery, con))
                            {
                                cmd.Parameters.AddWithValue("@StudentImageID", studentImageID);
                                cmd.Parameters.AddWithValue("@StudentID", studentID);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    if (GuardianPhotoUploadControl.HasFile)
                    {
                        string fileExtension = Path.GetExtension(GuardianPhotoUploadControl.FileName).ToLower();
                        string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
                        
                        if (Array.IndexOf(allowedExtensions, fileExtension) == -1)
                        {
                            ShowMessage("Invalid guardian photo file type. Please upload JPG, PNG, or GIF image only.", "warning", messageLabel);
                            return;
                        }

                        if (GuardianPhotoUploadControl.FileBytes.Length > 2097152)
                        {
                            ShowMessage("Guardian photo file size too large. Maximum allowed size is 2MB.", "warning", messageLabel);
                            return;
                        }

                        string getImageIdQuery = "SELECT StudentImageID FROM Student WHERE StudentID = @StudentID";
                        int studentImageID = 0;

                        using (SqlCommand cmd = new SqlCommand(getImageIdQuery, con))
                        {
                            cmd.Parameters.AddWithValue("@StudentID", studentID);
                            object result = cmd.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                            {
                                studentImageID = Convert.ToInt32(result);
                            }
                        }

                        if (studentImageID > 0)
                        {
                            string updateImageQuery = "UPDATE Student_Image SET Guardian_Photo = @ImageData WHERE StudentImageID = @StudentImageID";
                            using (SqlCommand cmd = new SqlCommand(updateImageQuery, con))
                            {
                                cmd.Parameters.AddWithValue("@ImageData", GuardianPhotoUploadControl.FileBytes);
                                cmd.Parameters.AddWithValue("@StudentImageID", studentImageID);
                                cmd.ExecuteNonQuery();
                                guardianPhotoUpdated = true;
                            }
                        }
                        else
                        {
                            string insertImageQuery = @"INSERT INTO Student_Image (Guardian_Photo) VALUES (@ImageData);
                                                      SELECT SCOPE_IDENTITY();";
                            using (SqlCommand cmd = new SqlCommand(insertImageQuery, con))
                            {
                                cmd.Parameters.AddWithValue("@ImageData", GuardianPhotoUploadControl.FileBytes);
                                studentImageID = Convert.ToInt32(cmd.ExecuteScalar());
                                guardianPhotoUpdated = true;
                            }

                            string updateStudentQuery = "UPDATE Student SET StudentImageID = @StudentImageID WHERE StudentID = @StudentID";
                            using (SqlCommand cmd = new SqlCommand(updateStudentQuery, con))
                            {
                                cmd.Parameters.AddWithValue("@StudentImageID", studentImageID);
                                cmd.Parameters.AddWithValue("@StudentID", studentID);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }

                StudentInfoFV.DataBind();

                string successMsg = "Profile updated successfully!";
                if (photoUpdated && guardianPhotoUpdated)
                {
                    successMsg = "✅ Profile, Student Photo and Guardian Photo updated successfully!";
                }
                else if (photoUpdated)
                {
                    successMsg = "✅ Profile and Student Photo updated successfully!";
                }
                else if (guardianPhotoUpdated)
                {
                    successMsg = "✅ Profile and Guardian Photo updated successfully!";
                }
                
                ShowMessage(successMsg, "success", messageLabel);
            }
            catch (Exception ex)
            {
                Label messageLabel = (Label)StudentInfoFV.FindControl("MessageLabel");
                ShowMessage("Error updating profile: " + ex.Message, "danger", messageLabel);
            }
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
