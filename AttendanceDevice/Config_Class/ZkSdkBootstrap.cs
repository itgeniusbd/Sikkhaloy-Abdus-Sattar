using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace AttendanceDevice.Config_Class
{
    internal static class ZkSdkBootstrap
    {
        public static void EnsureInstalled()
        {
            if (IsSdkInstalled())
                return;

            string helperPath = Path.Combine(AppPaths.AppDirectory, "ZKdllRegistrationApp.exe");
            string sdkSourcePath = Path.Combine(AppPaths.AppDirectory, "libs", "Zktec 32bit");

            if (!File.Exists(helperPath) || !Directory.Exists(sdkSourcePath))
            {
                MessageBox.Show(
                    "ZKTeco device SDK is not installed on this PC, and the setup files are missing from this install.\n\n" +
                    "Please reinstall SIKKHALOY Attendance Device using the latest setup.exe.",
                    "Device SDK Required",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show(
                "This PC needs a one-time ZKTeco SDK install for attendance devices.\n\n" +
                "Click OK, then allow Administrator permission in the next Windows prompt.\n" +
                "You only need to do this once on each school PC.",
                "Install Device SDK",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            if (!TryRegisterSdk(helperPath, sdkSourcePath))
            {
                MessageBox.Show(
                    "Device SDK install did not complete.\n\n" +
                    "Right-click this file and choose Run as administrator:\n" +
                    helperPath + "\n\n" +
                    "When prompted, use this folder as the SDK source:\n" +
                    sdkSourcePath,
                    "Device SDK",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        public static bool IsSdkInstalled()
        {
            string windowsDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            string targetDir = Path.Combine(windowsDir, Environment.Is64BitOperatingSystem ? "SysWOW64" : "System32");
            return File.Exists(Path.Combine(targetDir, "zkemkeeper.dll"));
        }

        private static bool TryRegisterSdk(string helperPath, string sdkSourcePath)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = helperPath,
                    Arguments = "\"" + sdkSourcePath + "\"",
                    Verb = "runas",
                    UseShellExecute = true,
                    WorkingDirectory = AppPaths.AppDirectory
                };

                using (var process = Process.Start(startInfo))
                {
                    process?.WaitForExit();
                }
            }
            catch
            {
                // User cancelled UAC prompt.
                return false;
            }

            return IsSdkInstalled();
        }
    }
}
