<%@ WebHandler Language="C#" Class="AttendanceDevice_UpdateHandler" %>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;

/// <summary>
/// Self-contained handler (no CodeBehind/DLL required). C# 5 compatible.
/// ?mode=info → JSON   |   ?download=1 → installer exe
/// </summary>
public class AttendanceDevice_UpdateHandler : IHttpHandler
{
    private static readonly Regex SetupVersionRegex = new Regex(
        @"SIKKHALOY_AttendanceDevice_Setup_(\d+(?:\.\d+){1,3})",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public void ProcessRequest(HttpContext context)
    {
        FileInfo latest = GetLatestInstaller(context);
        if (latest == null)
        {
            context.Response.StatusCode = 404;
            context.Response.ContentType = "application/json";
            context.Response.Write("{\"available\":false,\"message\":\"Installer not found.\"}");
            return;
        }

        string mode = (context.Request["mode"] ?? string.Empty).Trim().ToLowerInvariant();
        bool forceDownload = string.Equals(context.Request["download"], "1", StringComparison.OrdinalIgnoreCase)
                             || mode == "download";

        if (forceDownload)
        {
            string fileName = latest.Name.Replace("\"", string.Empty);
            context.Response.Clear();
            context.Response.Buffer = true;
            context.Response.ContentType = "application/octet-stream";
            context.Response.AddHeader("Content-Disposition", "attachment; filename=\"" + fileName + "\"");
            context.Response.AddHeader("Content-Length", latest.Length.ToString());
            context.Response.TransmitFile(latest.FullName);
            context.Response.Flush();
            return;
        }

        Version version;
        if (!TryParseVersionFromFileName(latest.Name, out version))
            version = new Version(0, 0, 0, 0);

        string relative = VirtualPathUtility.ToAbsolute("~/Attendances/AttendanceDevice_Update.ashx?download=1");
        string downloadUrl = context.Request.Url.GetLeftPart(UriPartial.Authority) + relative;

        var payload = new Dictionary<string, object>();
        payload["available"] = true;
        payload["version"] = FormatVersion(version);
        payload["fileName"] = latest.Name;
        payload["fileSize"] = latest.Length;
        payload["lastWriteUtc"] = latest.LastWriteTimeUtc.ToString("o");
        payload["downloadUrl"] = downloadUrl;

        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
        context.Response.Write(new JavaScriptSerializer().Serialize(payload));
    }

    private static FileInfo GetLatestInstaller(HttpContext context)
    {
        string folderPath = context.Server.MapPath("~/Attendances/App_For_Download/");
        if (!Directory.Exists(folderPath))
            return null;

        FileInfo[] files = new DirectoryInfo(folderPath).GetFiles("*.exe", SearchOption.TopDirectoryOnly);
        FileInfo best = null;
        Version bestVersion = null;

        for (int i = 0; i < files.Length; i++)
        {
            FileInfo file = files[i];
            Version parsed;
            bool hasVersion = TryParseVersionFromFileName(file.Name, out parsed);

            if (best == null)
            {
                best = file;
                bestVersion = hasVersion ? parsed : null;
                continue;
            }

            if (hasVersion && bestVersion == null)
            {
                best = file;
                bestVersion = parsed;
                continue;
            }

            if (hasVersion && bestVersion != null && parsed > bestVersion)
            {
                best = file;
                bestVersion = parsed;
                continue;
            }

            if (hasVersion == (bestVersion != null) && file.LastWriteTimeUtc > best.LastWriteTimeUtc)
            {
                if (!hasVersion || parsed >= bestVersion)
                {
                    best = file;
                    bestVersion = hasVersion ? parsed : bestVersion;
                }
            }
        }

        return best;
    }

    private static bool TryParseVersionFromFileName(string fileName, out Version version)
    {
        version = null;
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        Match match = SetupVersionRegex.Match(fileName);
        if (!match.Success)
            return false;

        string raw = match.Groups[1].Value;
        string[] parts = raw.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2 || parts.Length > 4)
            return false;

        int major, minor, build = 0, revision = 0;
        if (!int.TryParse(parts[0], out major) || !int.TryParse(parts[1], out minor))
            return false;
        if (parts.Length > 2 && !int.TryParse(parts[2], out build))
            return false;
        if (parts.Length > 3 && !int.TryParse(parts[3], out revision))
            return false;

        version = new Version(major, minor, build, revision);
        return true;
    }

    private static string FormatVersion(Version version)
    {
        if (version == null)
            return "0.0.0";
        return version.Major + "." + version.Minor + "." + Math.Max(version.Build, 0);
    }

    public bool IsReusable
    {
        get { return false; }
    }
}
