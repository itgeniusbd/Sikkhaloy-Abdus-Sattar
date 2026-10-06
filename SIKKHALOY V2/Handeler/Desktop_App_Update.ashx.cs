using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using System.Web.Script.Serialization;

namespace EDUCATION.COM.Handeler
{
    /// <summary>
    /// Public update endpoint for the SIKKHALOY Hybrid desktop app (same pattern as Attendances/AttendanceDevice_Update.ashx).
    /// ?mode=info (default)            -> JSON { available, version, fileName, fileSize, sha256, notes, downloadUrl }
    /// ?mode=download or ?download=1   -> streams that installer
    /// Files live in ~/Home/App_For_Download/ (the same folder as the "Download Desktop App" button).
    /// With SIKKHALOY_Hybrid_Update.json (written by Hybrid\Installer\Build-Installer.ps1) its version/SHA-256 are used, but only
    /// when the exe it names exists with exactly that size, so a half-uploaded exe is never offered (until then: available=false).
    /// Without the json, the newest SIKKHALOY_Hybrid_Setup_x.y.z.exe (by the version in its name) is offered and its SHA-256 is
    /// computed on the server (cached); files changed in the last 2 minutes are skipped as they may still be uploading.
    /// </summary>
    public class Desktop_App_Update : IHttpHandler
    {
        public const string FolderVirtualPath = "~/Home/App_For_Download/";
        public const string ManifestFileName = "SIKKHALOY_Hybrid_Update.json";

        private static readonly Regex SetupRegex = new Regex(
            @"^SIKKHALOY_Hybrid_Setup_(\d+(?:\.\d+){1,3})\.exe$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex Sha256Regex = new Regex("^[0-9a-fA-F]{64}$", RegexOptions.Compiled);

        private sealed class Release
        {
            public FileInfo File;
            public string Version;
            public string Sha256;
            public string Notes;
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);

            Release latest;
            try
            {
                latest = FindLatest(context);
            }
            catch (Exception)
            {
                latest = null;
            }

            if (latest == null)
            {
                context.Response.StatusCode = 404;
                context.Response.ContentType = "application/json; charset=utf-8";
                context.Response.Write("{\"available\":false,\"message\":\"Installer not found.\"}");
                return;
            }

            var mode = (context.Request["mode"] ?? string.Empty).Trim().ToLowerInvariant();
            if (mode == "download" || string.Equals(context.Request["download"], "1", StringComparison.OrdinalIgnoreCase))
            {
                TransmitInstaller(context, latest.File);
                return;
            }

            var payload = new
            {
                available = true,
                version = latest.Version,
                fileName = latest.File.Name,
                fileSize = latest.File.Length,
                sha256 = latest.Sha256,
                notes = latest.Notes ?? string.Empty,
                lastWriteUtc = latest.File.LastWriteTimeUtc.ToString("o"),
                downloadUrl = BuildAbsoluteUrl(context, "~/Handeler/Desktop_App_Update.ashx?download=1&v=" + HttpUtility.UrlEncode(latest.Version))
            };

            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.Write(new JavaScriptSerializer().Serialize(payload));
        }

        private static Release FindLatest(HttpContext context)
        {
            var folder = context.Server.MapPath(FolderVirtualPath);
            if (!Directory.Exists(folder))
                return null;

            var manifestPath = Path.Combine(folder, ManifestFileName);
            if (File.Exists(manifestPath))
                return ReadManifest(folder, manifestPath); // null = manifest and exe don't match yet -> offer nothing

            var candidate = new DirectoryInfo(folder)
                .GetFiles("SIKKHALOY_Hybrid_Setup_*.exe", SearchOption.TopDirectoryOnly)
                .Where(f => f.LastWriteTimeUtc < DateTime.UtcNow.AddMinutes(-2))
                .Select(f => new { File = f, Version = ParseVersion(f.Name) })
                .Where(x => x.Version != null)
                .OrderByDescending(x => x.Version)
                .ThenByDescending(x => x.File.LastWriteTimeUtc)
                .FirstOrDefault();
            if (candidate == null)
                return null;

            return new Release
            {
                File = candidate.File,
                Version = FormatVersion(candidate.Version),
                Sha256 = ComputeSha256Cached(candidate.File)
            };
        }

        private static Release ReadManifest(string folder, string manifestPath)
        {
            var data = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(File.ReadAllText(manifestPath));
            if (data == null)
                return null;

            var fileName = Path.GetFileName(Text(data, "fileName"));
            var sha256 = Text(data, "sha256");
            var version = Text(data, "version");
            long size;
            if (string.IsNullOrEmpty(fileName) || !SetupRegex.IsMatch(fileName) || !Sha256Regex.IsMatch(sha256)
                || !long.TryParse(Text(data, "fileSize"), out size))
                return null;

            var fileVersion = ParseVersion(fileName);
            Version manifestVersion;
            if (fileVersion == null || !TryParseVersion(version, out manifestVersion) || manifestVersion != fileVersion)
                return null;

            var file = new FileInfo(Path.Combine(folder, fileName));
            if (!file.Exists || file.Length != size)
                return null;

            return new Release
            {
                File = file,
                Version = FormatVersion(fileVersion),
                Sha256 = sha256.ToLowerInvariant(),
                Notes = Text(data, "notes")
            };
        }

        private static string Text(Dictionary<string, object> data, string key)
        {
            object value;
            return data.TryGetValue(key, out value) && value != null ? Convert.ToString(value).Trim() : string.Empty;
        }

        private static string ComputeSha256Cached(FileInfo file)
        {
            var key = "hybrid-setup-sha256|" + file.FullName + "|" + file.Length + "|" + file.LastWriteTimeUtc.Ticks;
            var cached = HttpRuntime.Cache[key] as string;
            if (cached != null)
                return cached;

            string hash;
            using (var sha = SHA256.Create())
            using (var stream = file.OpenRead())
                hash = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty).ToLowerInvariant();

            HttpRuntime.Cache.Insert(key, hash, null, Cache.NoAbsoluteExpiration, TimeSpan.FromHours(12));
            return hash;
        }

        private static Version ParseVersion(string fileName)
        {
            var match = SetupRegex.Match(fileName ?? string.Empty);
            Version version;
            return match.Success && TryParseVersion(match.Groups[1].Value, out version) ? version : null;
        }

        private static bool TryParseVersion(string raw, out Version version)
        {
            version = null;
            var parts = (raw ?? string.Empty).Trim().TrimStart('v', 'V').Split('.');
            if (parts.Length < 2 || parts.Length > 4)
                return false;
            var numbers = new int[4];
            for (var i = 0; i < parts.Length; i++)
                if (!int.TryParse(parts[i], out numbers[i]) || numbers[i] < 0)
                    return false;
            version = new Version(numbers[0], numbers[1], numbers[2], numbers[3]);
            return true;
        }

        private static string FormatVersion(Version version)
        {
            return version.Revision > 0 ? version.ToString(4) : version.ToString(3);
        }

        private static void TransmitInstaller(HttpContext context, FileInfo file)
        {
            var fileName = file.Name.Replace("\"", string.Empty);
            context.Response.Clear();
            context.Response.Buffer = false;
            context.Response.ContentType = "application/octet-stream";
            context.Response.AddHeader("Content-Disposition", "attachment; filename=\"" + fileName + "\"");
            context.Response.AddHeader("Content-Length", file.Length.ToString());
            context.Response.TransmitFile(file.FullName);
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
