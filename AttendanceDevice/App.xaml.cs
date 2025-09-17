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

        protected override void OnStartup(StartupEventArgs e)
        {
            _mutex = new Mutex(true, "sikkhaloy_attendance", out bool createdNew);

            if (!createdNew)
            {
                //app is already running! Exiting the application
                Current.Shutdown();
            }

            base.OnStartup(e);
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("An unhandled exception just occurred: " + e.Exception.Message, "App Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            e.Handled = true;
        }
    }
}
