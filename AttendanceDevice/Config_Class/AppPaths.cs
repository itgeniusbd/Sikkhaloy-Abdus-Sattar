using System;
using System.IO;
using System.Reflection;

namespace AttendanceDevice.Config_Class
{
    internal static class AppPaths
    {
        public static string AppDirectory
        {
            get
            {
                var location = Assembly.GetExecutingAssembly().Location;
                if (!string.IsNullOrEmpty(location))
                    return Path.GetDirectoryName(location) ?? AppDomain.CurrentDomain.BaseDirectory;

                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                if (!string.IsNullOrEmpty(codeBase))
                {
                    var uri = new Uri(codeBase);
                    return Path.GetDirectoryName(Uri.UnescapeDataString(uri.LocalPath))
                           ?? AppDomain.CurrentDomain.BaseDirectory;
                }

                return AppDomain.CurrentDomain.BaseDirectory;
            }
        }

        /// <summary>Writable per-user folder (safe under Program Files install).</summary>
        public static string LocalDataDirectory
        {
            get
            {
                var path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "SIKKHALOY",
                    "AttendanceDevice");
                Directory.CreateDirectory(path);
                return path;
            }
        }

        public static string DatabasePath => Path.Combine(LocalDataDirectory, "SikkhaloyAppDB.db");

        public static string WebView2UserDataFolder
        {
            get
            {
                var path = Path.Combine(LocalDataDirectory, "WebView2");
                Directory.CreateDirectory(path);
                return path;
            }
        }

        public static string LogsDirectory
        {
            get
            {
                var path = Path.Combine(LocalDataDirectory, "Logs");
                Directory.CreateDirectory(path);
                return path;
            }
        }

        public static string LegacyDatabasePath => Path.Combine(AppDirectory, "SikkhaloyAppDB.db");

        /// <summary>All per-user folders removed by the installer on uninstall.</summary>
        public static string[] UninstallDataPaths =>
            new[]
            {
                LocalDataDirectory,
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SikkhaloyAttendance"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SIKKHALOY"),
            };
    }
}
