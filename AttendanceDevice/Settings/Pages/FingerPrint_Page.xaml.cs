using AttendanceDevice.Config_Class;
using AttendanceDevice.Model;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AttendanceDevice.Settings.Pages
{
    public partial class FingerPrint_Page : Page
    {
        public FingerPrint_Page()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UserFP.ItemsSource = LocalData.Instance.Get_AllUserFP();
        }

        private void SetBusy(bool busy, string loadingText = "Loading...")
        {
            DownloadButton.IsEnabled = !busy;
            Upload_Button.IsEnabled = !busy;
            LoadingMessage.Text = loadingText;
            LoadingDH.IsOpen = busy;
        }

        private void SetStatus(string message, bool success)
        {
            StatusText.Text = message;
            StatusText.Foreground = success
                ? new SolidColorBrush(Color.FromRgb(10, 122, 0))
                : new SolidColorBrush(Color.FromRgb(198, 40, 40));
        }

        private async void Download_Button_Click(object sender, RoutedEventArgs e)
        {
            SetStatus("", true);
            SetBusy(true, "Downloading fingerprints...");

            var NetCheck = await ApiUrl.IsNoNetConnection();
            if (NetCheck)
            {
                SetBusy(false);
                SetStatus("No internet connection.", false);
                MessageBox.Show("No Internet connection.", "Download FingerPrint", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var client = new RestClient(ApiUrl.EndPoint);
                var request = new RestRequest("api/Users/{id}/FingerPrint", Method.GET);

                using (var db = new ModelContext())
                {
                    var Ins = LocalData.Instance.institution;

                    request.AddHeader("Authorization", "Bearer " + Ins.Token);
                    request.AddUrlSegment("id", Ins.SchoolID);

                    IRestResponse response = await client.ExecuteTaskAsync(request);

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        var detail = string.IsNullOrWhiteSpace(response.Content)
                            ? response.StatusDescription
                            : response.Content;
                        SetBusy(false);
                        SetStatus("Download failed.", false);
                        MessageBox.Show(
                            "Fingerprint download failed.\n" + detail,
                            "Download FingerPrint",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }

                    var userObj = Newtonsoft.Json.JsonConvert.DeserializeObject<List<User_FingerPrint>>(response.Content)
                                  ?? new List<User_FingerPrint>();

                    db.user_FingerPrints.Clear();

                    foreach (var item in userObj)
                        db.user_FingerPrints.Add(item);

                    await db.SaveChangesAsync();

                    SetBusy(false);
                    SetStatus("Fingerprint download successful.", true);
                    MessageBox.Show(
                        "Fingerprint download successful.",
                        "Download FingerPrint",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                SetBusy(false);
                SetStatus("Download failed.", false);
                MessageBox.Show(ex.Message, "Download FingerPrint", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (LoadingDH.IsOpen)
                    SetBusy(false);
            }

            UserFP.ItemsSource = LocalData.Instance.Get_AllUserFP();
        }

        private async void Upload_Button_Click(object sender, RoutedEventArgs e)
        {
            SetStatus("", true);
            SetBusy(true, "Uploading fingerprints...");

            var internet = await ApiUrl.IsNoNetConnection();
            if (internet)
            {
                SetBusy(false);
                SetStatus("No internet connection.", false);
                MessageBox.Show("No Internet connection.", "Upload FingerPrint", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var ins = LocalData.Instance.institution;
                var FpLog = LocalData.Instance.FingerPrintData();
                if (FpLog == null || FpLog.Count == 0)
                {
                    SetBusy(false);
                    SetStatus("No fingerprint on this PC to upload.", false);
                    MessageBox.Show(
                        "No fingerprint found on this PC.\nDownload from server or enroll on the device first.",
                        "Upload FingerPrint",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }

                var client = new RestClient(ApiUrl.EndPoint);
                var request = new RestRequest("api/Users/{id}/FingerPrintPost", Method.POST);
                request.AddUrlSegment("id", ins.SchoolID);
                request.AddHeader("Authorization", "Bearer " + ins.Token);
                request.AddJsonBody(FpLog);

                var response = await client.ExecutePostTaskAsync(request);

                SetBusy(false);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    SetStatus("Fingerprint upload successful.", true);
                    MessageBox.Show(
                        "Fingerprint upload successful.",
                        "Upload FingerPrint",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    var detail = string.IsNullOrWhiteSpace(response.Content)
                        ? response.StatusDescription
                        : response.Content;
                    SetStatus("Upload failed.", false);
                    MessageBox.Show(
                        "Fingerprint upload failed.\n" + detail,
                        "Upload FingerPrint",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                SetBusy(false);
                SetStatus("Upload failed.", false);
                MessageBox.Show(ex.Message, "Upload FingerPrint", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (LoadingDH.IsOpen)
                    SetBusy(false);
            }
        }
    }
}
