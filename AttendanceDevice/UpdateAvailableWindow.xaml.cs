using System.Windows;
using System.Windows.Input;

namespace AttendanceDevice
{
    public partial class UpdateAvailableWindow : Window
    {
        public bool Accepted { get; private set; }

        public UpdateAvailableWindow(string installedVersion, string latestVersion, string sizeText)
        {
            InitializeComponent();
            InstalledText.Text = "বর্তমান: " + installedVersion;
            LatestText.Text = "নতুন: " + latestVersion;
            SizeText.Text = string.IsNullOrWhiteSpace(sizeText) ? string.Empty : sizeText;
            SizeText.Visibility = string.IsNullOrWhiteSpace(sizeText)
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private void UpdateNowButton_Click(object sender, RoutedEventArgs e)
        {
            Accepted = true;
            DialogResult = true;
            Close();
        }

        private void LaterButton_Click(object sender, RoutedEventArgs e)
        {
            Accepted = false;
            DialogResult = false;
            Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}
