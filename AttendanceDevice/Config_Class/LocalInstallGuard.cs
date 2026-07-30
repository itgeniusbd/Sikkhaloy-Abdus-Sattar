using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Deployment.Application;
using System.IO;
using System.Linq;

namespace AttendanceDevice.Config_Class
{
    /// <summary>
    /// Detects uninstall/reinstall and removes orphaned login data left in LocalAppData.
    /// ClickOnce uninstall does not run Inno Setup cleanup scripts.
    /// </summary>
    internal static class LocalInstallGuard
    {
        private const string RegistrySubKey = @"Software\SIKKHALOY\AttendanceDevice";
        private const string RegistryInstallIdValue = "InstallId";
        private const string MarkerFileName = "install.marker";

        private sealed class InstallMarker
        {
            public string InstallId { get; set; }
            public string Source { get; set; }
            public string AppVersion { get; set; }
            public string AppDirectory { get; set; }
        }

        public static void EnsureInstallState()
        {
            try
            {
                Directory.CreateDirectory(AppPaths.LocalDataDirectory);
                var markerPath = Path.Combine(AppPaths.LocalDataDirectory, MarkerFileName);
                var marker = ReadMarker(markerPath);
                var registryInstallId = ReadRegistryInstallId();

                if (!string.IsNullOrWhiteSpace(registryInstallId))
                {
                    if (marker != null &&
                        !string.IsNullOrWhiteSpace(marker.InstallId) &&
                        !string.Equals(marker.InstallId, registryInstallId, StringComparison.OrdinalIgnoreCase))
                    {
                        WipeLocalUserData();
                        marker = null;
                    }

                    WriteMarker(markerPath, registryInstallId, "InnoSetup");
                    return;
                }

                if (marker != null &&
                    string.Equals(marker.Source, "InnoSetup", StringComparison.OrdinalIgnoreCase))
                {
                    WipeLocalUserData();
                    marker = null;
                }

                if (TryGetClickOnceInfo(out var clickOnceVersion, out var clickOnceAppDir))
                {
                    HandleClickOnceState(markerPath, marker, clickOnceVersion, clickOnceAppDir);
                    return;
                }

                if (marker == null)
                    WriteMarker(markerPath, null, "Portable");
            }
            catch
            {
                // Never block startup because guard failed.
            }
        }

        private static void HandleClickOnceState(
            string markerPath,
            InstallMarker marker,
            Version clickOnceVersion,
            string clickOnceAppDir)
        {
            var versionText = clickOnceVersion?.ToString() ?? "0.0.0.0";

            if (marker == null)
            {
                WriteMarker(markerPath, null, "ClickOnce", versionText, clickOnceAppDir);
                return;
            }

            if (!string.Equals(marker.Source, "ClickOnce", StringComparison.OrdinalIgnoreCase))
            {
                WipeLocalUserData();
                WriteMarker(markerPath, null, "ClickOnce", versionText, clickOnceAppDir);
                return;
            }

            var savedVersion = ParseVersion(marker.AppVersion);
            var currentVersion = clickOnceVersion ?? new Version(0, 0, 0, 0);
            var savedAppDir = NormalizePath(marker.AppDirectory);
            var currentAppDir = NormalizePath(clickOnceAppDir);

            if (savedVersion != null &&
                savedVersion == currentVersion &&
                !string.Equals(savedAppDir, currentAppDir, StringComparison.OrdinalIgnoreCase) &&
                !Directory.Exists(savedAppDir))
            {
                WipeLocalUserData();
            }

            if (savedVersion == null || currentVersion > savedVersion ||
                !string.Equals(savedAppDir, currentAppDir, StringComparison.OrdinalIgnoreCase))
            {
                WriteMarker(markerPath, null, "ClickOnce", versionText, clickOnceAppDir);
            }
        }

        private static bool TryGetClickOnceInfo(out Version version, out string appDirectory)
        {
            version = null;
            appDirectory = AppPaths.AppDirectory;

            try
            {
                if (!ApplicationDeployment.IsNetworkDeployed)
                    return false;

                var deployment = ApplicationDeployment.CurrentDeployment;
                version = deployment?.CurrentVersion;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string ReadRegistryInstallId()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RegistrySubKey, false))
                {
                    return key?.GetValue(RegistryInstallIdValue) as string;
                }
            }
            catch
            {
                return null;
            }
        }

        private static InstallMarker ReadMarker(string markerPath)
        {
            try
            {
                if (!File.Exists(markerPath))
                    return null;

                return JsonConvert.DeserializeObject<InstallMarker>(File.ReadAllText(markerPath));
            }
            catch
            {
                return null;
            }
        }

        private static void WriteMarker(
            string markerPath,
            string installId,
            string source,
            string appVersion = null,
            string appDirectory = null)
        {
            var marker = new InstallMarker
            {
                InstallId = installId,
                Source = source,
                AppVersion = appVersion ?? GetExecutingVersion(),
                AppDirectory = appDirectory ?? AppPaths.AppDirectory
            };

            File.WriteAllText(markerPath, JsonConvert.SerializeObject(marker));
        }

        private static string GetExecutingVersion()
        {
            try
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0.0";
            }
            catch
            {
                return "0.0.0.0";
            }
        }

        private static Version ParseVersion(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            Version parsed;
            return Version.TryParse(value, out parsed) ? parsed : null;
        }

        private static string NormalizePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            try
            {
                return Path.GetFullPath(path.Trim())
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }
            catch
            {
                return path.Trim();
            }
        }

        public static void WipeLocalUserData()
        {
            foreach (var target in AppPaths.UninstallDataPaths.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct())
            {
                TryDeletePath(target);
            }
        }

        private static void TryDeletePath(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);

                if (Directory.Exists(path))
                    Directory.Delete(path, true);
            }
            catch
            {
                // Best effort; uninstall cleanup script handles locked files.
            }
        }
    }
}
