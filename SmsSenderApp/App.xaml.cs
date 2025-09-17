
using Serilog;
using System;
using System.Drawing;
using System.Threading;
using System.Windows;
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

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File("Log/log.txt",
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true)
                .CreateLogger();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // prevent duplication running
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
               Current.Shutdown();
                return;
            }

            _notifyIcon.Icon = new Icon("Resources/Sikkhaloy.ico");
            _notifyIcon.Text = "Sikkhaloy SMS Sender";
            _notifyIcon.DoubleClick += NotifyIcon_Click;

            _notifyIcon.ContextMenuStrip = new Forms.ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add("Exit", Image.FromFile("Resources/Sikkhaloy.ico"), OnExitClicked);
            _notifyIcon.Visible = true;


            // Add a shortcut to the application in the Startup folder
            var startUpFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            var appShortcutPath = System.IO.Path.Combine(startUpFolder, "StartupApplication.lnk");

            if (!System.IO.File.Exists(appShortcutPath))
            {
                // Create a shortcut to the application
                var shell = new IWshRuntimeLibrary.WshShell();
                var shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(appShortcutPath);
                shortcut.TargetPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                shortcut.Save();
            }

            Log.Information("Application started");

            var mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private void OnExitClicked(object sender, EventArgs e)
        {
            Current.Shutdown();
        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            // Show or hide toggle
            Current.MainWindow.Visibility = Current.MainWindow.Visibility == Visibility.Hidden ? Visibility.Visible : Visibility.Hidden;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // prevent duplication running
               mutex.ReleaseMutex();

            base.OnExit(e);
            GlobalClass.Instance.SenderUpdate();
            Log.Information("Application Closed");
           
            Log.CloseAndFlush();
            _notifyIcon.Dispose();
        }        
    }
}
