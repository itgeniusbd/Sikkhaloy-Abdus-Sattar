using AttendanceDevice.Model;
using System.Windows;
using System.Windows.Input;

namespace AttendanceDevice
{
    /// <summary>
    /// Interaction logic for Error_Window.xaml
    /// </summary>
    public partial class Error_Window
    {
        private readonly Error _error;
        public Error_Window(Error error)
        {
            this._error = error;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ErTitle.Text = _error.ErrorTittle;
            ErMessage.Text = _error.ErrorMessage;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
