using AttendanceDevice.Config_Class;
using AttendanceDevice.Model;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace AttendanceDevice.Settings.Pages
{
    /// <summary>
    /// Interaction logic for FingerPrint_Page.xaml
    /// </summary>
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

        private async void Download_Button_Click(object sender, RoutedEventArgs e)
        {
            LoadingPB.IsIndeterminate = true;
            DownloadButton.IsEnabled = false;

            var NetCheck = await ApiUrl.IsNoNetConnection();
            if (NetCheck)
            {
                MessageBox.Show("No Internet", "No Internet connection!");

                LoadingPB.IsIndeterminate = false;
                DownloadButton.IsEnabled = true;
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

                    // Execute the request
                    IRestResponse response = await client.ExecuteTaskAsync(request);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        List<User_FingerPrint> userObj = Newtonsoft.Json.JsonConvert.DeserializeObject<List<User_FingerPrint>>(response.Content);

                        //For deleting all previous data
                        db.user_FingerPrints.Clear();

                        foreach (var item in userObj)
                        {
                            db.user_FingerPrints.Add(item);
                        }

                        await db.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                LoadingPB.IsIndeterminate = false;
                DownloadButton.IsEnabled = true;
            }

            UserFP.ItemsSource = LocalData.Instance.Get_AllUserFP();
        }

        private async void Upload_Button_Click(object sender, RoutedEventArgs e)
        {
            var internet = await ApiUrl.IsNoNetConnection();
            if (internet) return;


            var ins = LocalData.Instance.institution;
            var FpLog = LocalData.Instance.FingerPrintData();
            var client = new RestClient(ApiUrl.EndPoint);

            if (FpLog.Count > 0)
            {
                var request = new RestRequest("api/Users/{id}/FingerPrintPost", Method.POST);

                request.AddUrlSegment("id", ins.SchoolID);
                request.AddHeader("Authorization", "Bearer " + ins.Token);
                request.AddJsonBody(FpLog);

                var response = await client.ExecutePostTaskAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {

                }
            }

        }
    }
}
