using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AttendanceDevice.Config_Class
{
    /// <summary>
    /// Compares the installed AttendanceDevice version with the installer on sikkhaloy.com
    /// (same file as Attendance Settings → Download Latest Version).
    /// </summary>
    internal static class AppAutoUpdate
    {
        private static readonly string InfoUrl =
            ApiUrl.WebUrl.TrimEnd('/') + "/Attendances/AttendanceDevice_Update.ashx";

        public static Version GetInstalledVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 0);
        }

        /// <summary>
        /// Returns true when the app is shutting down to run the installer.
        /// </summary>
        public static async Task<bool> CheckAndPromptAsync(Window owner = null)
        {
            try
            {
                if (await ApiUrl.IsNoNetConnection())
                    return false;

                var info = await FetchLatestInfoAsync().ConfigureAwait(true);
                if (info == null || !info.Available || string.IsNullOrWhiteSpace(info.DownloadUrl))
                {
                    StartupLogger.LogStage("auto-update: no info from server (ashx missing or empty)");
                    return false;
                }

                if (!TryParseVersion(info.Version, out var remoteVersion))
                {
                    StartupLogger.LogStage("auto-update: bad remote version " + info.Version);
                    return false;
                }

                var localVersion = GetInstalledVersion();
                StartupLogger.LogStage(
                    $"auto-update: local={FormatVersion(localVersion)} remote={FormatVersion(remoteVersion)} file={info.FileName}");

                if (Compare(remoteVersion, localVersion) <= 0)
                    return false;

                var localText = FormatVersion(localVersion);
                var remoteText = FormatVersion(remoteVersion);
                var sizeMb = info.FileSize > 0
                    ? $"সাইজ: {(info.FileSize / (1024d * 1024d)):0.0} MB"
                    : string.Empty;

                var accepted = false;
                if (Application.Current?.Dispatcher != null &&
                    !Application.Current.Dispatcher.CheckAccess())
                {
                    accepted = Application.Current.Dispatcher.Invoke(() =>
                        ShowUpdateDialog(owner, localText, remoteText, sizeMb));
                }
                else
                {
                    accepted = ShowUpdateDialog(owner, localText, remoteText, sizeMb);
                }

                if (!accepted)
                    return false;

                var ok = await DownloadAndLaunchInstallerAsync(info, owner).ConfigureAwait(true);
                if (!ok)
                    return false;

                Application.Current?.Shutdown();
                return true;
            }
            catch (Exception ex)
            {
                StartupLogger.LogStage("auto-update: " + ex.Message);
                return false;
            }
        }

        private static bool ShowUpdateDialog(Window owner, string localText, string remoteText, string sizeText)
        {
            var dialog = new UpdateAvailableWindow(localText, remoteText, sizeText);
            if (owner != null && owner.IsVisible)
                dialog.Owner = owner;
            var result = dialog.ShowDialog();
            return result == true && dialog.Accepted;
        }

        private static async Task<UpdateInfo> FetchLatestInfoAsync()
        {
            return await Task.Run(() =>
            {
                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.CacheControl] = "no-cache";
                    var json = client.DownloadString(InfoUrl + "?mode=info");
                    if (string.IsNullOrWhiteSpace(json))
                        return null;

                    var obj = JObject.Parse(json);
                    return new UpdateInfo
                    {
                        Available = obj.Value<bool?>("available") ?? false,
                        Version = obj.Value<string>("version"),
                        FileName = obj.Value<string>("fileName"),
                        FileSize = obj.Value<long?>("fileSize") ?? 0,
                        DownloadUrl = obj.Value<string>("downloadUrl")
                    };
                }
            }).ConfigureAwait(false);
        }

        private static async Task<bool> DownloadAndLaunchInstallerAsync(UpdateInfo info, Window owner)
        {
            var fileName = string.IsNullOrWhiteSpace(info.FileName)
                ? $"SIKKHALOY_AttendanceDevice_Setup_{FormatVersion(GetInstalledVersion())}.exe"
                : Path.GetFileName(info.FileName);

            var targetPath = Path.Combine(Path.GetTempPath(), fileName);

            try
            {
                    Mouse.OverrideCursor = Cursors.Wait;
                if (owner != null)
                    owner.IsEnabled = false;

                await Task.Run(() =>
                {
                    if (File.Exists(targetPath))
                    {
                        try { File.Delete(targetPath); }
                        catch { /* overwrite below */ }
                    }

                    using (var client = new WebClient())
                    {
                        client.Headers[HttpRequestHeader.CacheControl] = "no-cache";
                        client.DownloadFile(info.DownloadUrl, targetPath);
                    }
                }).ConfigureAwait(true);

                if (!File.Exists(targetPath) || new FileInfo(targetPath).Length < 1024)
                {
                    MessageBox.Show(
                        owner,
                        "Download failed or the installer file is incomplete. Try again from the website Download Latest Version button.",
                        "Update Failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return false;
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = targetPath,
                    // Silent upgrade: close running app, no wizard cancel confusion.
                    Arguments = "/SILENT /CLOSEAPPLICATIONS /NORESTART /SUPPRESSMSGBOXES",
                    UseShellExecute = true,
                    Verb = "runas"
                });

                // Release single-instance lock so the installer can replace files.
                App.ReleaseSingleInstanceMutex();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    owner,
                    "Could not download or start the installer.\n\n" + ex.Message,
                    "Update Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
            finally
            {
                Mouse.OverrideCursor = null;
                if (owner != null)
                    owner.IsEnabled = true;
            }
        }

        internal static bool TryParseVersion(string raw, out Version version)
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

            version = new Version(major, minor, build, revision);
            return true;
        }

        private static int Compare(Version remote, Version local)
        {
            var a = new Version(remote.Major, remote.Minor, Math.Max(remote.Build, 0), Math.Max(remote.Revision, 0));
            var b = new Version(local.Major, local.Minor, Math.Max(local.Build, 0), Math.Max(local.Revision, 0));
            return a.CompareTo(b);
        }

        private static string FormatVersion(Version version)
        {
            if (version == null)
                return "0.0.0";
            if (version.Revision > 0)
                return $"{version.Major}.{version.Minor}.{Math.Max(version.Build, 0)}.{version.Revision}";
            return $"{version.Major}.{version.Minor}.{Math.Max(version.Build, 0)}";
        }

        private sealed class UpdateInfo
        {
            public bool Available { get; set; }
            public string Version { get; set; }
            public string FileName { get; set; }
            public long FileSize { get; set; }
            public string DownloadUrl { get; set; }
        }
    }
}
