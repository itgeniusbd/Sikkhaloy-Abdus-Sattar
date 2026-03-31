using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Student
{
    public partial class Notice : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void DownloadFile(object sender, EventArgs e)
        {
            string filePath = (sender as LinkButton).CommandArgument;
            if (string.IsNullOrEmpty(filePath)) return;

            // Cloudinary URL — direct redirect
            if (filePath.StartsWith("http://") || filePath.StartsWith("https://"))
            {
                Response.Redirect(filePath, false);
                return;
            }

            // Legacy local file
            string path = Server.MapPath(filePath);
            if (!File.Exists(path)) return;

            string ext = Path.GetExtension(path).ToLower();
            string contentType = "application/octet-stream";
            if (ext == ".pdf") contentType = "application/pdf";
            else if (ext == ".doc" || ext == ".docx") contentType = "application/msword";
            else if (ext == ".xls" || ext == ".xlsx") contentType = "application/vnd.ms-excel";

            Response.Clear();
            Response.ContentType = contentType;
            Response.AppendHeader("Content-Disposition", "attachment; filename=\"" + Path.GetFileName(path) + "\"");
            Response.TransmitFile(path);
            Response.End();
        }
    }
}