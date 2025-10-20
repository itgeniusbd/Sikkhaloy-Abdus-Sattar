using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Administration_Basic_Settings
{
    public partial class Institution_Info : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Page load logic if needed
        }
        
        protected void InstitutionInfoDetailsView_ItemUpdated(object sender, DetailsViewUpdatedEventArgs e)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
            
            // Handle School Logo Upload
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

            // Handle School Name Logo Upload (New Feature)
            FileUpload SchoolNameLogoFileUpload = (FileUpload)InstitutionInfoDetailsView.FindControl("SchoolNameLogoFileUpload");

            if (SchoolNameLogoFileUpload.PostedFile != null && SchoolNameLogoFileUpload.PostedFile.FileName != "")
            {
                string strExtension = System.IO.Path.GetExtension(SchoolNameLogoFileUpload.FileName);
                if ((strExtension.ToUpper() == ".JPG") || (strExtension.ToUpper() == ".JPEG") || (strExtension.ToUpper() == ".PNG"))
                {
                    // Get original image
                    System.Drawing.Image imageToBeResized = System.Drawing.Image.FromStream(SchoolNameLogoFileUpload.PostedFile.InputStream);
                    int imageHeight = imageToBeResized.Height;
                    int imageWidth = imageToBeResized.Width;

                    // Set maximum dimensions for name logo - wider format
                    int maxHeight = 200;
                    int maxWidth = 1500;

                    int newWidth = imageWidth;
                    int newHeight = imageHeight;

                    // Only resize if image is larger than max dimensions
                    if (imageWidth > maxWidth || imageHeight > maxHeight)
                    {
                        // Calculate proportional dimensions
                        double widthRatio = (double)maxWidth / imageWidth;
                        double heightRatio = (double)maxHeight / imageHeight;
                        double ratio = Math.Min(widthRatio, heightRatio);

                        newWidth = (int)(imageWidth * ratio);
                        newHeight = (int)(imageHeight * ratio);
                    }

                    // Create high-quality bitmap
                    Bitmap bitmap = new Bitmap(newWidth, newHeight);
                    using (Graphics graphics = Graphics.FromImage(bitmap))
                    {
                        // Set high-quality rendering
                        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                        graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                        // Draw the image
                        graphics.DrawImage(imageToBeResized, 0, 0, newWidth, newHeight);
                    }

                    System.IO.MemoryStream stream = new MemoryStream();
                    
                    // Save as PNG for better quality (especially for text/logos)
                    if (strExtension.ToUpper() == ".PNG")
                    {
                        bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    else
                    {
                        // For JPEG, use high quality encoder
                        System.Drawing.Imaging.ImageCodecInfo jpegCodec = GetEncoderInfo("image/jpeg");
                        System.Drawing.Imaging.EncoderParameters encoderParams = new System.Drawing.Imaging.EncoderParameters(1);
                        encoderParams.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 95L);
                        bitmap.Save(stream, jpegCodec, encoderParams);
                    }

                    stream.Position = 0;
                    byte[] image = new byte[stream.Length];
                    stream.Read(image, 0, image.Length);

                    // Dispose objects
                    bitmap.Dispose();
                    imageToBeResized.Dispose();

                    // Check if column exists, if not create it
                    if (con.State == ConnectionState.Closed)
                        con.Open();

                    SqlCommand checkCmd = new SqlCommand(
                        @"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SchoolInfo]') AND name = 'SchoolNameLogo')
                        BEGIN
                            ALTER TABLE SchoolInfo ADD SchoolNameLogo VARBINARY(MAX)
                        END", con);
                    checkCmd.ExecuteNonQuery();

                    // Create SQL Command to update School Name Logo
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = "UPDATE SchoolInfo SET SchoolNameLogo = @SchoolNameLogo WHERE (SchoolID = @SchoolID)";
                    cmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = con;

                    SqlParameter UploadedImage = new SqlParameter("@SchoolNameLogo", SqlDbType.Image, image.Length);
                    UploadedImage.Value = image;
                    cmd.Parameters.Add(UploadedImage);

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

        // Delete School Name Logo Button Click Event
        protected void DeleteSchoolNameLogoButton_Click(object sender, EventArgs e)
        {
            try
            {
                int schoolId = Convert.ToInt32(Session["SchoolID"]);
                string connectionString = ConfigurationManager.ConnectionStrings["EducationConnectionString"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string updateQuery = "UPDATE SchoolInfo SET SchoolNameLogo = NULL WHERE SchoolID = @SchoolID";
                    
                    using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        
                        if (rowsAffected > 0)
                        {
                            // Refresh the page to show updated logo status
                            Response.Redirect(Request.RawUrl);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error silently or show generic message
                // Note: DebugLabel might not be available in production
                Response.Write("<script>alert('Error deleting school name logo. Please try again.');</script>");
            }
        }

        // Helper method to get JPEG encoder
        private System.Drawing.Imaging.ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            System.Drawing.Imaging.ImageCodecInfo[] codecs = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders();
            foreach (System.Drawing.Imaging.ImageCodecInfo codec in codecs)
            {
                if (codec.MimeType == mimeType)
                {
                    return codec;
                }
            }
            return null;
        }

    }
}