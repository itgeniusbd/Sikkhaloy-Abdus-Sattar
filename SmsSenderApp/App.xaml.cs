using Serilog;
using System;
using System.Drawing;
using System.Threading;
using System.Windows;
using System.IO;
using Forms = System.Windows.Forms;

namespace SmsSenderApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly Forms.NotifyIcon _notifyIcon;
        private static readonly Mutex mutex = new Mutex(true, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);

        public App()
        {
            _notifyIcon = new Forms.NotifyIcon();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize Logger
            try
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.Console()
                    .WriteTo.File("Log/log.txt",
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true)
                    .CreateLogger();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize logger: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // prevent duplication running
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                Current.Shutdown();
                return;
            }

            try
            {
                // Load icon from resources
                var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Sikkhaloy.ico");
                if (File.Exists(iconPath))
                {
                    _notifyIcon.Icon = new Icon(iconPath);
                }
                else
                {
                    // Use default application icon if custom icon not found
                    _notifyIcon.Icon = Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location);
                }

                _notifyIcon.Text = "Sikkhaloy SMS Sender";
                _notifyIcon.DoubleClick += NotifyIcon_Click;

                _notifyIcon.ContextMenuStrip = new Forms.ContextMenuStrip();
                _notifyIcon.ContextMenuStrip.Items.Add("Exit", null, OnExitClicked);
                _notifyIcon.Visible = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to setup notify icon");
            }

            try
            {
                // Add a shortcut to the application in the Startup folder
                var startUpFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                var appShortcutPath = Path.Combine(startUpFolder, "SikkhaloySmsSender.lnk");
                var appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var appDirectory = AppDomain.CurrentDomain.BaseDirectory;

                if (!File.Exists(appShortcutPath))
                {
                    // Create a shortcut to the application
                    try
                    {
                        var shell = new IWshRuntimeLibrary.WshShell();
                        var shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(appShortcutPath);
                        shortcut.TargetPath = appPath;
                        shortcut.WorkingDirectory = appDirectory;
                        shortcut.Description = "Sikkhaloy SMS Sender - Auto Start";

                        // Set icon if available
                        var iconPath = Path.Combine(appDirectory, "Resources", "Sikkhaloy.ico");
                        if (File.Exists(iconPath))
                        {
                            shortcut.IconLocation = iconPath;
                        }
                        else
                        {
                            shortcut.IconLocation = appPath + ",0";
                        }

                        shortcut.Save();
                        Log.Information("Startup shortcut created successfully");
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Failed to create startup shortcut");
                    }
                }

                // Also create desktop shortcut (optional, one time)
                var desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var desktopShortcutPath = Path.Combine(desktopFolder, "Sikkhaloy SMS Sender.lnk");

                if (!File.Exists(desktopShortcutPath))
                {
                    try
                    {
                        var shell = new IWshRuntimeLibrary.WshShell();
                        var shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(desktopShortcutPath);
                        shortcut.TargetPath = appPath;
                        shortcut.WorkingDirectory = appDirectory;
                        shortcut.Description = "Sikkhaloy SMS Sender";

                        // Set icon if available
                        var iconPath = Path.Combine(appDirectory, "Resources", "Sikkhaloy.ico");
                        if (File.Exists(iconPath))
                        {
                            shortcut.IconLocation = iconPath;
                        }
                        else
                        {
                            shortcut.IconLocation = appPath + ",0";
                        }

                        shortcut.Save();
                        Log.Information("Desktop shortcut created successfully");
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Failed to create desktop shortcut");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to setup startup configuration");
            }

            Log.Information("Application started");

            try
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Failed to start main window");
                MessageBox.Show($"Failed to start application: {ex.Message}\n\nPlease check the log file for details.",
                    "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
            }
        }

        private void OnExitClicked(object sender, EventArgs e)
        {
            Current.Shutdown();
        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            try
            {
                // Show or hide toggle
                if (Current.MainWindow != null)
                {
                    Current.MainWindow.Visibility = Current.MainWindow.Visibility == Visibility.Hidden ? Visibility.Visible : Visibility.Hidden;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to toggle window visibility");
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                // prevent duplication running
                mutex.ReleaseMutex();

                base.OnExit(e);
                GlobalClass.Instance.SenderUpdate();
                Log.Information("Application Closed");

                Log.CloseAndFlush();
                _notifyIcon.Dispose();
            }
            catch (Exception ex)
            {
                // Log might already be closed
                try
                {
                    Log.Error(ex, "Error during application shutdown");
                }
                catch { }
            }
        }
    }
}
