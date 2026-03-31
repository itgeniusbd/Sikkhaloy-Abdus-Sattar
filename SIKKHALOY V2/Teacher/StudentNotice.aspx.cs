using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Teacher
{
    public partial class StudentNotice : System.Web.UI.Page
    {
        private const string CloudinaryCloudName = "djp4lgeal";
        private const string CloudinaryApiKey = "853443919881232";
        private const string CloudinaryApiSecret = "XfjK4FCLZhXzi_RREJnXKgv83to";

        protected void Page_Load(object sender, EventArgs e) { }

        private string UploadToCloudinary(Stream fileStream, string fileName)
        {
            string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            string ext = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(ext)) ext = ".pdf";
            string safeBaseName = "notice_" + timestamp;
            string safePublicId = "sikkhaloy/notices/" + safeBaseName;

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
                    byte[] b = Encoding.UTF8.GetBytes("--" + boundary + "\r\nContent-Disposition: form-data; name=\"" + name + "\"\r\n\r\n" + value + "\r\n");
                    reqStream.Write(b, 0, b.Length);
                };
                writeField("api_key",   CloudinaryApiKey);
                writeField("timestamp", timestamp);
                writeField("signature", signature);
                writeField("public_id", safePublicId);

                string safeFileName = safeBaseName + ext;
                byte[] hdr = Encoding.UTF8.GetBytes("--" + boundary + "\r\nContent-Disposition: form-data; name=\"file\"; filename=\"" + safeFileName + "\"\r\nContent-Type: application/octet-stream\r\n\r\n");
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

            string key = "\"secure_url\":\"";
            int start = responseJson.IndexOf(key);
            if (start < 0) throw new Exception("No secure_url: " + responseJson);
            start += key.Length;
            int end = responseJson.IndexOf("\"", start);
            // Return clean URL without any transformation flags
            return responseJson.Substring(start, end - start).Replace("\\/", "/");
        }

        protected void NoticeButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NoticeTitleTextBox.Text))
            {
                Response.Write("<script>alert('Notice Title is Required!');</script>");
                return;
            }

            bool isSelected = false;
            foreach (ListItem item in ClassCheckBoxList.Items)
                if (item.Selected) { isSelected = true; break; }

            if (!isSelected)
            {
                Response.Write("<script>alert('Please Select Class!');</script>");
                return;
            }

            string noticeFilePath = "";

            if (FileUploadPDF.HasFile)
            {
                int filesize = FileUploadPDF.PostedFile.ContentLength / 1024;
                if (filesize > 500)
                {
                    Response.Write("<script>alert('File size must be below 500 KB!');</script>");
                    return;
                }

                try
                {
                    string cloudUrl = UploadToCloudinary(
                        FileUploadPDF.PostedFile.InputStream,
                        Path.GetFileName(FileUploadPDF.FileName));
                    noticeFilePath = cloudUrl;
                }
                catch (Exception ex)
                {
                    string safeMsg = ex.Message.Replace("'", "\\'").Replace("\r", "").Replace("\n", " ");
                    Response.Write($"<script>alert('File upload failed: {safeMsg}');</script>");
                    return;
                }
            }

            StudentNoticeSQL.InsertParameters["Notice_file"].DefaultValue = noticeFilePath;
            StudentNoticeSQL.Insert();

            foreach (ListItem item in ClassCheckBoxList.Items)
            {
                if (item.Selected)
                {
                    StudentNoticeClassSQL.InsertParameters["StudentNoticeId"].DefaultValue = ViewState["StudentNoticeId"].ToString();
                    StudentNoticeClassSQL.InsertParameters["ClassId"].DefaultValue = item.Value;
                    StudentNoticeClassSQL.Insert();
                }
            }

            Response.Write("<script>alert('Notice Submitted Successfully');</script>");
        }

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

        private string ExtractPublicIdFromUrl(string url)
        {
            string marker = "/raw/upload/";
            int idx = url.IndexOf(marker);
            if (idx < 0) return null;
            string afterMarker = url.Substring(idx + marker.Length);
            int slashIdx = afterMarker.IndexOf('/');
            if (slashIdx >= 0 && afterMarker.StartsWith("v") &&
                afterMarker.Substring(1, slashIdx - 1).All(char.IsDigit))
            {
                afterMarker = afterMarker.Substring(slashIdx + 1);
            }
            return afterMarker;
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
            if (IsValidate() != true)
            {
                foreach (GridViewRow Row in NoticeGridView.Rows)
                {
                    CheckBox cb = Row.FindControl("SingleCheckBox") as CheckBox;
                    if (cb != null && cb.Checked)
                        DeleteNoticeById(NoticeGridView.DataKeys[Row.DataItemIndex]["StudentNoticeId"].ToString());
                }
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alert",
                    "alert('Notice Deleted Successfully!'); window.location='StudentNotice.aspx';", true);
                NoticeGridView.DataBind();
            }
        }

        private void DeleteNoticeById(string noticeId)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString()))
            {
                con.Open();
                SqlTransaction t = con.BeginTransaction();
                try
                {
                    SqlCommand cmd = new SqlCommand("DELETE FROM StudentNoticeClass WHERE StudentNoticeId=@id", con, t);
                    cmd.Parameters.AddWithValue("@id", noticeId);
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "DELETE FROM StudentNotice WHERE StudentNoticeId=@id";
                    cmd.ExecuteNonQuery();
                    t.Commit();
                }
                catch { t.Rollback(); }
            }
        }

        private bool IsValidate()
        {
            foreach (GridViewRow Row in NoticeGridView.Rows)
            {
                CheckBox cb = Row.FindControl("SingleCheckBox") as CheckBox;
                if (cb != null && cb.Checked) return false;
            }
            Response.Write("<script>alert('Please check at least one');</script>");
            return true;
        }
    }
}