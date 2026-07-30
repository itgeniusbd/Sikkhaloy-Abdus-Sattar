using AttendanceDevice.Config_Class;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace AttendanceDevice
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex _mutex = null;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);

        protected override void OnStartup(StartupEventArgs e)
        {
            EnsureNativeDependencies();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            _mutex = new Mutex(true, "sikkhaloy_attendance", out bool createdNew);

            if (!createdNew)
            {
                MessageBox.Show(
                    "SIKKHALOY Attendance is already running.\n\nClose the other window or end AttendanceDevice in Task Manager, then try again.",
                    "Already Running",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                Current.Shutdown();
                return;
            }

            try
            {
                if (!IsDotNet48OrLater())
                {
                    MessageBox.Show(
                        "Microsoft .NET Framework 4.8 is required.\n\n" +
                        "Install it from:\nhttps://dotnet.microsoft.com/download/dotnet-framework/net48\n\n" +
                        "Then restart this app.",
                        "Missing .NET Framework",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    Current.Shutdown();
                    return;
                }

                if (!EnsureSqliteNative())
                {
                    MessageBox.Show(
                        "SQLite native library (x86\\SQLite.Interop.dll) is missing from the install folder.\n\n" +
                        "Please reinstall SIKKHALOY Attendance Device using the latest setup.exe.",
                        "Install Incomplete",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    Current.Shutdown();
                    return;
                }

                LocalInstallGuard.EnsureInstallState();
                SqliteDatabaseBootstrap.EnsureDatabase();
                SqliteMultiScheduleMigration.EnsureApplied();
                ZkSdkBootstrap.EnsureInstalled();
            }
            catch (Exception ex)
            {
                LogStartupFailure(ex);
                MessageBox.Show(GetExceptionMessage(ex), "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
                return;
            }

            base.OnStartup(e);
        }

        private static void EnsureNativeDependencies()
        {
            try
            {
                var baseDir = AppPaths.AppDirectory;
                var x86Dir = Path.Combine(baseDir, "x86");
                if (Directory.Exists(x86Dir))
                    SetDllDirectory(x86Dir);
            }
            catch
            {
                // ignored
            }
        }

        private static bool EnsureSqliteNative()
        {
            var x86Dll = Path.Combine(AppPaths.AppDirectory, "x86", "SQLite.Interop.dll");
            var rootDll = Path.Combine(AppPaths.AppDirectory, "SQLite.Interop.dll");
            return File.Exists(x86Dll) || File.Exists(rootDll);
        }

        private static bool IsDotNet48OrLater()
        {
            try
            {
                var release = (int)Microsoft.Win32.Registry.GetValue(
                    @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full",
                    "Release",
                    0);
                if (release == 0)
                {
                    release = (int)Microsoft.Win32.Registry.GetValue(
                        @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\NET Framework Setup\NDP\v4\Full",
                        "Release",
                        0);
                }

                return release >= 528040;
            }
            catch
            {
                return true;
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                LogStartupFailure(ex);
                MessageBox.Show(GetExceptionMessage(ex), "App Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogStartupFailure(e.Exception);
            MessageBox.Show(GetExceptionMessage(e.Exception), "App Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            e.Handled = true;
        }

        private static void LogStartupFailure(Exception ex)
        {
            try
            {
                var logPath = Path.Combine(AppPaths.LogsDirectory, "startup-error.log");
                File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex}{Environment.NewLine}");
            }
            catch
            {
                // ignored
            }
        }

        private static string GetExceptionMessage(Exception ex)
        {
            while (ex.InnerException != null)
                ex = ex.InnerException;

            return ex.Message;
        }
    }
}
