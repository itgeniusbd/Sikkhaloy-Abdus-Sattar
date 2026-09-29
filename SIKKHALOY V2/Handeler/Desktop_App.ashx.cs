using System.IO;
using System.Linq;
using System.Web;

namespace EDUCATION.COM.Handeler
{
    public class Desktop_App : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            var latestInstaller = GetLatestDesktopAppInstaller(context);
            if (latestInstaller == null)
            {
                context.Response.StatusCode = 404;
                context.Response.ContentType = "text/plain";
                context.Response.Write("Desktop app installer is not available yet.");
                return;
            }

            string fileName = latestInstaller.Name.Replace("\"", string.Empty);
            context.Response.Clear();
            context.Response.Buffer = true;
            context.Response.ContentType = "application/octet-stream";
            context.Response.AddHeader("Content-Disposition", "attachment; filename=\"" + fileName + "\"");
            context.Response.AddHeader("Content-Length", latestInstaller.Length.ToString());
            context.Response.TransmitFile(latestInstaller.FullName);
            context.Response.Flush();
        }

        public static FileInfo GetLatestDesktopAppInstaller(HttpContext context)
        {
            string folderPath = context.Server.MapPath("~/Home/App_For_Download/");
            if (!Directory.Exists(folderPath))
                return null;

            return new DirectoryInfo(folderPath)
                .GetFiles("*.exe", SearchOption.TopDirectoryOnly)
                .OrderByDescending(file => file.LastWriteTimeUtc)
                .FirstOrDefault();
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
