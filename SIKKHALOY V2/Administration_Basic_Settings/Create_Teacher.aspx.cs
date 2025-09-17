using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.ADMINISTRATION_BASIC_SETTING
{
    public partial class Create_Edit_Delete_Teacher : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void TeacherInfoButton_Click(object sender, EventArgs e)
        {
            TeacherCreateUserWizard.ActiveStepIndex = 1;
        }

        protected void TeacherJobsButton_Click(object sender, EventArgs e)
        {
            bool isAmount = false;
            if (Abs_DeductedRadioButtonList.SelectedIndex == 0)
            {
                if (Abs_DeductedAmount_TextBox.Text.Trim() != "")
                {
                    isAmount = true;
                }
                else
                {
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Input Deduction Amount')", true);
                }
            }
            else
            {
                isAmount = true;
            }

            if (isAmount)
            {
                TeacherCreateUserWizard.ActiveStepIndex = 2;
            }
        }

        protected void TeacherSQL_Inserted(object sender, SqlDataSourceStatusEventArgs e)
        {
            ViewState["TeacherID"] = e.Command.Parameters["@TeacherID"].Value;
        }

        protected void TeacherCreateUserWizard_CreatedUser(object sender, EventArgs e)
        {
            Roles.AddUserToRole(TeacherCreateUserWizard.UserName, "Teacher");

            con.Open();
            SqlCommand Insert_Reg_cmd = new SqlCommand("INSERT INTO [Registration] ([SchoolID], [UserName], [Validation], [Category], [CreateDate]) VALUES (@SchoolID,@UserName, 'Valid' ,'Teacher', getdate())", con);
            Insert_Reg_cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
            Insert_Reg_cmd.Parameters.AddWithValue("@UserName", TeacherCreateUserWizard.UserName);
            Insert_Reg_cmd.ExecuteNonQuery();

            SqlCommand RegistrationID_Cmd = new SqlCommand("Select IDENT_CURRENT('Registration')", con);
            string RegistrationID = RegistrationID_Cmd.ExecuteScalar().ToString();
            con.Close();

            LITSQL.InsertParameters["RegistrationID"].DefaultValue = RegistrationID;
            LITSQL.InsertParameters["UserName"].DefaultValue = TeacherCreateUserWizard.UserName;
            LITSQL.InsertParameters["Password"].DefaultValue = TeacherCreateUserWizard.Password;
            LITSQL.InsertParameters["PasswordAnswer"].DefaultValue = TeacherCreateUserWizard.Answer;
            LITSQL.Insert();

            Edu_YearSQL.InsertParameters["RegistrationID"].DefaultValue = RegistrationID;
            Edu_YearSQL.Insert();

            TeacherSQL.InsertParameters["TeacherRegistrationID"].DefaultValue = RegistrationID;
            TeacherSQL.InsertParameters["Email"].DefaultValue = TeacherCreateUserWizard.Email;
            TeacherSQL.Insert();

            FileUpload ImageFileUpload = (FileUpload)TeacherCreateUserWizard.CreateUserStep.ContentTemplateContainer.FindControl("ImageFileUpload");

            if (ImageFileUpload.HasFile)
            {
                Image(ImageFileUpload);
            }

            //Teacher job info
            TeacherJobSQL.InsertParameters["TeacherID"].DefaultValue = ViewState["TeacherID"].ToString();
            TeacherJobSQL.Insert();

            Device_DataUpdateSQL.Insert();

            TeacherCreateUserWizard.ActiveStepIndex = 3;
        }

        protected void ContinueButton_Click(object sender, EventArgs e)
        {
            Session["T_RegistrationID"] = null;
            Response.Redirect("Create_Teacher.aspx");
        }

        protected void Image(FileUpload ImageFileUpload)
        {
            if (ImageFileUpload.PostedFile != null && ImageFileUpload.PostedFile.FileName != "")
            {
                string strExtension = System.IO.Path.GetExtension(ImageFileUpload.FileName);
                if ((strExtension.ToUpper() == ".JPG") | (strExtension.ToUpper() == ".JPEG") | (strExtension.ToUpper() == ".PNG"))
                {
                    // Resize Image Before Uploading to DataBase
                    System.Drawing.Image imageToBeResized = System.Drawing.Image.FromStream(ImageFileUpload.PostedFile.InputStream);
                    int imageHeight = imageToBeResized.Height;
                    int imageWidth = imageToBeResized.Width;

                    int maxHeight = 250;
                    int maxWidth = 200;

                    imageHeight = (imageHeight * maxWidth) / imageWidth;
                    imageWidth = maxWidth;

                    if (imageHeight > maxHeight)
                    {
                        imageWidth = (imageWidth * maxHeight) / imageHeight;
                        imageHeight = maxHeight;
                    }

                    Bitmap bitmap = new Bitmap(imageToBeResized, imageWidth, imageHeight);
                    MemoryStream stream = new MemoryStream();
                    bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    stream.Position = 0;
                    byte[] image = new byte[stream.Length + 1];
                    stream.Read(image, 0, image.Length);


                    // Create SQL Command
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = "UPDATE Teacher SET Image = @Image Where TeacherID = @TeacherID and SchoolID = @SchoolID";

                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                    cmd.Parameters.AddWithValue("@TeacherID", ViewState["TeacherID"].ToString());
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = con;

                    SqlParameter UploadedImage = new SqlParameter("@Image", SqlDbType.Image, image.Length);

                    UploadedImage.Value = image;
                    cmd.Parameters.Add(UploadedImage);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();

                }
            }
        }

    }
}