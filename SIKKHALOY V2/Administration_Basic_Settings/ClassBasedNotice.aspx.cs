using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Administration_Basic_Settings
{
    public partial class ClassBasedNotice : System.Web.UI.Page
    {
        // Cloudinary credentials
        private const string CloudinaryCloudName = "djp4lgeal";
        private const string CloudinaryApiKey = "853443919881232";
        private const string CloudinaryApiSecret = "XfjK4FCLZhXzi_RREJnXKgv83to";

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Uploads a file stream to Cloudinary and returns the secure URL.
        /// Uses raw resource_type so any file (PDF, DOCX, etc.) is accepted.
        /// </summary>
        private string UploadToCloudinary(Stream fileStream, string fileName)
        {
            string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

            string ext = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(ext)) ext = ".pdf";
            string safeBaseName = "notice_" + timestamp;
            string safePublicId = "sikkhaloy/notices/" + safeBaseName;

            // Signature: alphabetical params only (public_id, timestamp)
            string sigString = "public_id=" + safePublicId + "&timestamp=" + timestamp + CloudinaryApiSecret;
            string signature;
            using (var sha1 = SHA1.Create())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(sigString));
                signature = BitConverter.ToString(hash).Replace("-", "").ToLower();
            }

            string uploadUrl = "https://api.cloudinary.com/v1_1/" + CloudinaryCloudName + "/raw/upload";
            string boundary = "----FormBoundary" + Guid.NewGuid().ToString("N");

            var request = (HttpWebRequest)WebRequest.Create(uploadUrl);
            request.Method = "POST";
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.Timeout = 60000;

            using (var reqStream = request.GetRequestStream())
            {
                Action<string, string> writeField = (name, value) =>
                {
                    byte[] b = Encoding.UTF8.GetBytes(
                        "--" + boundary + "\r\nContent-Disposition: form-data; name=\"" + name + "\"\r\n\r\n" + value + "\r\n");
                    reqStream.Write(b, 0, b.Length);
                };

                writeField("api_key",   CloudinaryApiKey);
                writeField("timestamp", timestamp);
                writeField("signature", signature);
                writeField("public_id", safePublicId);

                string safeFileName = safeBaseName + ext;
                byte[] hdr = Encoding.UTF8.GetBytes(
                    "--" + boundary + "\r\nContent-Disposition: form-data; name=\"file\"; filename=\"" + safeFileName + "\"\r\nContent-Type: application/octet-stream\r\n\r\n");
                reqStream.Write(hdr, 0, hdr.Length);
                fileStream.CopyTo(reqStream);
                byte[] footer = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");
                reqStream.Write(footer, 0, footer.Length);
            }

            string responseJson;
            try
            {
                using (var resp = (HttpWebResponse)request.GetResponse())
                using (var reader = new StreamReader(resp.GetResponseStream()))
                    responseJson = reader.ReadToEnd();
            }
            catch (WebException wex) when (wex.Response != null)
            {
                using (var r = new StreamReader(wex.Response.GetResponseStream()))
                    throw new Exception("Cloudinary: " + r.ReadToEnd());
            }

            // Extract secure_url
            string key = "\"secure_url\":\"";
            int start = responseJson.IndexOf(key);
            if (start < 0) throw new Exception("No secure_url: " + responseJson);
            start += key.Length;
            int end = responseJson.IndexOf("\"", start);
            // Return clean URL — download is handled server-side via proxy
            return responseJson.Substring(start, end - start).Replace("\\/", "/");
        }

        protected void NoticeButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NoticeTitleTextBox.Text))
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alert", "alert('Notice Title is Required!');", true);
                return;
            }

            // Check if at least one class is selected
            bool isSelected = false;
            foreach (ListItem item in ClassCheckBoxList.Items)
            {
                if (item.Selected) { isSelected = true; break; }
            }

            if (!isSelected)
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alert", "alert('Please Select at least one Class!');", true);
                return;
            }

            string noticeFilePath = "";

            // Handle file upload via Cloudinary
            if (FileUploadPDF.HasFile)
            {
                int filesize = FileUploadPDF.PostedFile.ContentLength / 1024;
                if (filesize > 500)
                {
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alert", "alert('File size must be below 500 KB!');", true);
                    return;
                }

                try
                {
                    string originalFileName = Path.GetFileName(FileUploadPDF.FileName);
                    string cloudUrl = UploadToCloudinary(FileUploadPDF.PostedFile.InputStream, originalFileName);
                    noticeFilePath = cloudUrl; // store full Cloudinary URL
                }
                catch (Exception ex)
                {
                    string safeMsg = ex.Message.Replace("'", "\\'").Replace("\r", "").Replace("\n", " ");
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alert",
                        "alert('File upload failed: " + safeMsg + "');", true);
                    return;
                }
            }

            StudentNoticeSQL.InsertParameters["Notice_file"].DefaultValue = noticeFilePath;
            StudentNoticeSQL.Insert();

            if (ViewState["StudentNoticeId"] != null)
            {
                foreach (ListItem item in ClassCheckBoxList.Items)
                {
                    if (item.Selected)
                    {
                        StudentNoticeClassSQL.InsertParameters["StudentNoticeId"].DefaultValue = ViewState["StudentNoticeId"].ToString();
                        StudentNoticeClassSQL.InsertParameters["ClassId"].DefaultValue = item.Value;
                        StudentNoticeClassSQL.Insert();
                    }
                }
                ViewState["StudentNoticeId"] = null;
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alert",
                    "alert('Notice Submitted Successfully!'); window.location='ClassBasedNotice.aspx';", true);
            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alert",
                    "alert('Error: Could not save notice. Please try again.');", true);
            }
        }

        // Download file — uses Cloudinary Admin API to fetch bytes and stream to browser
        protected void DownloadFile(object sender, EventArgs e)
        {
            string filePath = (sender as LinkButton).CommandArgument;
            if (string.IsNullOrEmpty(filePath)) return;

            if (filePath.StartsWith("http://") || filePath.StartsWith("https://"))
            {
                Response.Redirect(filePath, false);
                return;
            }

            // Legacy local file
            string physicalPath = Server.MapPath(filePath);
            if (!File.Exists(physicalPath)) return;
            string lext = Path.GetExtension(physicalPath).ToLower();
            Response.Clear();
            Response.ContentType = "application/octet-stream";
            Response.AppendHeader("Content-Disposition", "attachment; filename=\"notice" + lext + "\"");
            Response.TransmitFile(physicalPath);
            Response.End();
        }

        private string GetFileTypeByFileExtension(string fileExtension)
        {
            switch (fileExtension.ToLower())
            {
                case ".pdf": return "pdf File";
                case ".doc": case ".docx": return "Microsoft Word Document";
                case ".xls": case ".xlsx": return "Microsoft Excel Document";
                case ".txt": return "Text File";
                case ".png": case ".jpg": return "Windows Image file";
                default: return "Unknown file type";
            }
        }

        protected void StudentNoticeSQL_Inserted(object sender, SqlDataSourceStatusEventArgs e)
        {
            ViewState["StudentNoticeId"] = e.Command.Parameters["@StudentNoticeId"].Value;
        }

        protected void DeleteButton_Click(object sender, EventArgs e)
        {
            CheckBox SingleCheckBox = new CheckBox();

            if (IsValidate() != true)
            {
                foreach (GridViewRow Row in NoticeGridView.Rows)
                {
                    SingleCheckBox = Row.FindControl("SingleCheckBox") as CheckBox;
                    if (SingleCheckBox.Checked)
                    {
                        string noticeId = NoticeGridView.DataKeys[Row.DataItemIndex]["StudentNoticeId"].ToString();
                        DeleteNoticeById(noticeId);
                    }
                }
                foreach (GridViewRow Row in HomeWorkGridView.Rows)
                {
                    SingleCheckBox = Row.FindControl("SingleCheckBox") as CheckBox;
                    if (SingleCheckBox.Checked)
                    {
                        string noticeId = HomeWorkGridView.DataKeys[Row.DataItemIndex]["StudentNoticeId"].ToString();
                        DeleteNoticeById(noticeId);
                    }
                }
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alert",
                    "alert('Notice Deleted Successfully!'); window.location='ClassBasedNotice.aspx';", true);
                NoticeGridView.DataBind();
            }
        }

        private void DeleteNoticeById(string noticeId)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString()))
            {
                con.Open();
                SqlTransaction transection = con.BeginTransaction();
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = con;
                    cmd.Transaction = transection;
                    cmd.CommandText = "DELETE FROM StudentNoticeClass WHERE StudentNoticeId=@id";
                    cmd.Parameters.AddWithValue("@id", noticeId);
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "DELETE FROM StudentNotice WHERE StudentNoticeId=@id";
                    cmd.ExecuteNonQuery();
                    transection.Commit();
                }
                catch
                {
                    transection.Rollback();
                }
            }
        }

        private bool IsValidate()
        {
            bool check1 = false;
            foreach (GridViewRow Row in NoticeGridView.Rows)
            {
                CheckBox cb = Row.FindControl("SingleCheckBox") as CheckBox;
                if (cb != null && cb.Checked) { check1 = true; break; }
            }
            if (!check1)
            {
                foreach (GridViewRow Row in HomeWorkGridView.Rows)
                {
                    CheckBox cb = Row.FindControl("SingleCheckBox") as CheckBox;
                    if (cb != null && cb.Checked) { check1 = true; break; }
                }
            }
            if (!check1)
            {
                Response.Write("<script>alert('Please check at least one');</script>");
                return true;
            }
            return false;
        }
    }
}


