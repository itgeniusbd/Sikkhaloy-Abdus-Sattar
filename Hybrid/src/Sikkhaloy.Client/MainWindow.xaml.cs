using System.Windows;
using Sikkhaloy.Client.Updates;

namespace Sikkhaloy.Client;

public partial class MainWindow : Window
{
    private HybridUpdateService? _updater;
    private HybridUpdateInfo? _offered;
    private string? _dismissedVersion;
    private bool _busy;

    public MainWindow()
    {
        InitializeComponent();
    }

    /// <summary>Shows the update banner when the auto-updater finds a newer version.</summary>
    public void AttachUpdater(HybridUpdateService updater)
    {
        _updater = updater;
        updater.UpdateAvailable += info => Dispatcher.InvokeAsync(() => ShowUpdate(info));
    }

    private void ShowUpdate(HybridUpdateInfo info)
    {
        if (_busy || string.Equals(_dismissedVersion, info.Version, StringComparison.OrdinalIgnoreCase))
            return;

        _offered = info;
        UpdateText.Text = $"SIKKHALOY Hybrid-এর নতুন ভার্সন {info.Version} এসেছে (এই কম্পিউটারে আছে {HybridUpdateService.FormatVersion(HybridUpdateService.InstalledVersion)}, সাইজ {info.FileSize / (1024d * 1024d):0.0} MB)।";
        UpdateProgress.Visibility = Visibility.Collapsed;
        UpdateNowButton.IsEnabled = true;
        UpdateLaterButton.IsEnabled = true;
        UpdateBanner.Visibility = Visibility.Visible;
    }

    private void UpdateLaterButton_Click(object sender, RoutedEventArgs e)
    {
        _dismissedVersion = _offered?.Version;
        UpdateBanner.Visibility = Visibility.Collapsed;
    }

    private async void UpdateNowButton_Click(object sender, RoutedEventArgs e)
    {
        if (_updater is null || _offered is null || _busy)
            return;

        var info = _offered;
        _busy = true;
        UpdateNowButton.IsEnabled = false;
        UpdateLaterButton.IsEnabled = false;
        IDisposable? syncPause = null;
        var handedOver = false;
        try
        {
            UpdateText.Text = $"নতুন ভার্সন {info.Version} ডাউনলোড হচ্ছে...";
            UpdateProgress.IsIndeterminate = false;
            UpdateProgress.Value = 0;
            UpdateProgress.Visibility = Visibility.Visible;
            var progress = new Progress<double>(p => UpdateProgress.Value = p);
            var setup = await _updater.DownloadAndVerifyAsync(info, progress);

            // Never update in the middle of a sync: wait for it, then keep sync paused until the app exits.
            UpdateText.Text = "সিঙ্ক চলছে, শেষ হওয়ার অপেক্ষা করা হচ্ছে...";
            UpdateProgress.IsIndeterminate = true;
            syncPause = await _updater.PauseSyncAsync(TimeSpan.FromMinutes(10));
            UpdateProgress.IsIndeterminate = false;
            if (syncPause is null)
            {
                UpdateText.Text = "সিঙ্ক এখনো চলছে। কিছুক্ষণ পরে আবার \"এখনই আপডেট করুন\" চাপুন।";
                return;
            }

            var pending = _updater.PendingSyncCount;
            var message = $"অ্যাপটি এখন বন্ধ হবে, নতুন ভার্সন {info.Version} ইনস্টল হবে এবং নিজে থেকে আবার চালু হবে।\n"
                          + "Windows অনুমতি (UAC) চাইলে \"Yes\" দিন।\n\n"
                          + "আপনার সব ডাটা এই কম্পিউটারে নিরাপদ থাকবে।"
                          + (pending > 0 ? $"\nসিঙ্ক বাকি আছে {pending}টি; আপডেটের পরে নিজে থেকে সিঙ্ক হবে।" : "");
            if (MessageBox.Show(this, message, "SIKKHALOY Hybrid আপডেট", MessageBoxButton.OKCancel, MessageBoxImage.Information) != MessageBoxResult.OK)
            {
                UpdateText.Text = $"নতুন ভার্সন {info.Version} ডাউনলোড করা আছে। প্রস্তুত হলে \"এখনই আপডেট করুন\" চাপুন।";
                return;
            }

            _updater.LaunchInstaller(setup);
            handedOver = true; // sync stays paused until the app has exited
            UpdateText.Text = "আপডেট ইনস্টল হচ্ছে... অ্যাপটি বন্ধ হয়ে নতুন ভার্সনে আবার চালু হবে।";
            Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            HybridUpdateService.Log("update failed: " + ex.Message);
            UpdateText.Text = "আপডেট করা যায়নি: " + ex.Message + " পরে আবার চেষ্টা করুন।";
        }
        finally
        {
            if (!handedOver)
            {
                syncPause?.Dispose();
                _busy = false;
                UpdateNowButton.IsEnabled = true;
                UpdateLaterButton.IsEnabled = true;
                UpdateProgress.IsIndeterminate = false;
                UpdateProgress.Visibility = Visibility.Collapsed;
            }
        }
    }
}
