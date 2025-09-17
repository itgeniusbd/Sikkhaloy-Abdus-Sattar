using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Administration_Basic_Settings
{
    public partial class Institution_Info : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void InstitutionInfoDetailsView_ItemUpdated(object sender, DetailsViewUpdatedEventArgs e)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
            FileUpload LogoFileUpload = (FileUpload)InstitutionInfoDetailsView.FindControl("LogoFileUpload");

            if (LogoFileUpload.PostedFile != null && LogoFileUpload.PostedFile.FileName != "")
            {
                string strExtension = System.IO.Path.GetExtension(LogoFileUpload.FileName);
                if ((strExtension.ToUpper() == ".JPG") || (strExtension.ToUpper() == ".JPEG") || (strExtension.ToUpper() == ".PNG"))
                {
                    // Resize Image Before Uploading to DataBase
                    System.Drawing.Image imageToBeResized = System.Drawing.Image.FromStream(LogoFileUpload.PostedFile.InputStream);
                    int imageHeight = imageToBeResized.Height;
                    int imageWidth = imageToBeResized.Width;

                    int maxHeight = 200;
                    int maxWidth = 200;

                    imageHeight = (imageHeight * maxWidth) / imageWidth;
                    imageWidth = maxWidth;

                    if (imageHeight > maxHeight)
                    {
                        imageWidth = (imageWidth * maxHeight) / imageHeight;
                        imageHeight = maxHeight;
                    }

                    Bitmap bitmap = new Bitmap(imageToBeResized, imageWidth, imageHeight);
                    System.IO.MemoryStream stream = new MemoryStream();
                    bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    stream.Position = 0;
                    byte[] image = new byte[stream.Length + 1];
                    stream.Read(image, 0, image.Length);


                    // Create SQL Command
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = "UPDATE SchoolInfo SET SchoolLogo = @SchoolLogo WHERE (SchoolID = @SchoolID)";
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = con;

                    SqlParameter UploadedImage = new SqlParameter("@SchoolLogo", SqlDbType.Image, image.Length);

                    UploadedImage.Value = image;
                    cmd.Parameters.Add(UploadedImage);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();

                }
            }

            Response.Redirect(Request.Url.AbsoluteUri);
        }
        protected void rbSendSMS_SelectedIndexChanged(object sender, EventArgs e)   //OnSelectedIndexChanged="rbSendSMS_SelectedIndexChanged"
        {
            SmsSettingSQL.Update();
        }
    }
}