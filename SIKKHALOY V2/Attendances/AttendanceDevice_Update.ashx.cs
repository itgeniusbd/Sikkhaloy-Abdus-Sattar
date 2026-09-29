using System;
using System.IO;
using System.Web;
using System.Web.Script.Serialization;

namespace EDUCATION.COM.Attendances
{
    /// <summary>
    /// Public update endpoint for the AttendanceDevice desktop app.
    /// ?mode=info (default) → JSON with version + download URL
    /// ?mode=download or ?download=1 → streams the latest installer
    /// </summary>
    public class AttendanceDevice_Update : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            var latest = AttendanceDeviceInstallerHelper.GetLatestInstaller(context);
            if (latest == null)
            {
                context.Response.StatusCode = 404;
                context.Response.ContentType = "application/json";
                context.Response.Write("{\"available\":false,\"message\":\"Installer not found.\"}");
                return;
            }

            var mode = (context.Request["mode"] ?? string.Empty).Trim().ToLowerInvariant();
            var forceDownload = string.Equals(context.Request["download"], "1", StringComparison.OrdinalIgnoreCase)
                                || mode == "download";

            if (forceDownload)
            {
                TransmitInstaller(context, latest);
                return;
            }

            Version version;
            if (!AttendanceDeviceInstallerHelper.TryParseVersionFromFileName(latest.Name, out version))
                version = new Version(0, 0, 0, 0);

            var downloadUrl = BuildAbsoluteUrl(context, "~/Attendances/AttendanceDevice_Update.ashx?download=1");
            var payload = new
            {
                available = true,
                version = AttendanceDeviceInstallerHelper.FormatVersion(version),
                fileName = latest.Name,
                fileSize = latest.Length,
                lastWriteUtc = latest.LastWriteTimeUtc.ToString("o"),
                downloadUrl
            };

            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            context.Response.Write(new JavaScriptSerializer().Serialize(payload));
        }

        private static void TransmitInstaller(HttpContext context, FileInfo latest)
        {
            var fileName = latest.Name.Replace("\"", string.Empty);
            context.Response.Clear();
            context.Response.Buffer = true;
            context.Response.ContentType = "application/octet-stream";
            context.Response.AddHeader("Content-Disposition", "attachment; filename=\"" + fileName + "\"");
            context.Response.AddHeader("Content-Length", latest.Length.ToString());
            context.Response.TransmitFile(latest.FullName);
            context.Response.Flush();
        }

        private static string BuildAbsoluteUrl(HttpContext context, string virtualPath)
        {
            var relative = VirtualPathUtility.ToAbsolute(virtualPath);
            var baseUrl = context.Request.Url.GetLeftPart(UriPartial.Authority);
            return baseUrl + relative;
        }

        public bool IsReusable => false;
    }
}
