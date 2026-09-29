using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace EDUCATION.COM.Attendances
{
    /// <summary>
    /// Finds the newest AttendanceDevice installer under Attendances/App_For_Download.
    /// Prefers version in the file name (SIKKHALOY_AttendanceDevice_Setup_x.y.z.exe).
    /// </summary>
    public static class AttendanceDeviceInstallerHelper
    {
        private static readonly Regex SetupVersionRegex = new Regex(
            @"SIKKHALOY_AttendanceDevice_Setup_(\d+(?:\.\d+){1,3})",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string FolderVirtualPath => "~/Attendances/App_For_Download/";

        public static FileInfo GetLatestInstaller(HttpContext context)
        {
            if (context == null)
                return null;

            var folderPath = context.Server.MapPath(FolderVirtualPath);
            if (!Directory.Exists(folderPath))
                return null;

            return new DirectoryInfo(folderPath)
                .GetFiles("*.exe", SearchOption.TopDirectoryOnly)
                .Select(file =>
                {
                    Version parsed;
                    var hasVersion = TryParseVersionFromFileName(file.Name, out parsed);
                    return new
                    {
                        File = file,
                        Version = hasVersion ? parsed : null
                    };
                })
                .OrderByDescending(x => x.Version != null)
                .ThenByDescending(x => x.Version)
                .ThenByDescending(x => x.File.LastWriteTimeUtc)
                .Select(x => x.File)
                .FirstOrDefault();
        }

        public static bool TryParseVersionFromFileName(string fileName, out Version version)
        {
            version = null;
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            var match = SetupVersionRegex.Match(fileName);
            if (!match.Success)
                return false;

            return TryParseLooseVersion(match.Groups[1].Value, out version);
        }

        public static bool TryParseLooseVersion(string raw, out Version version)
        {
            version = null;
            if (string.IsNullOrWhiteSpace(raw))
                return false;

            var parts = raw.Trim().TrimStart('v', 'V').Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2 || parts.Length > 4)
                return false;

            int major, minor, build = 0, revision = 0;
            if (!int.TryParse(parts[0], out major) || !int.TryParse(parts[1], out minor))
                return false;
            if (parts.Length > 2 && !int.TryParse(parts[2], out build))
                return false;
            if (parts.Length > 3 && !int.TryParse(parts[3], out revision))
                return false;

            version = parts.Length == 2
                ? new Version(major, minor)
                : parts.Length == 3
                    ? new Version(major, minor, build)
                    : new Version(major, minor, build, revision);
            return true;
        }

        public static string FormatVersion(Version version)
        {
            if (version == null)
                return null;

            if (version.Revision > 0)
                return version.ToString(4);
            if (version.Build >= 0)
                return version.ToString(3);
            return version.ToString(2);
        }
    }
}
