using AttendanceDevice.Config_Class;
using AttendanceDevice.Model;
using Microsoft.Win32;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AttendanceDevice.Settings.Pages
{
    /// <summary>
    /// Interaction logic for UserInfoPage.xaml
    /// </summary>
    public partial class UserInfoPage : Page
    {
        public UserInfoPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (LocalData.Current_Error.Type == Error_Type.UserInfoPage)
            {
                var message = LocalData.Current_Error.Message;


                ErrorSnackBar.Message.Content = message;
                ErrorSnackBar.IsActive = true;
            }

            var users = LocalData.Instance.UserViews;

            if (users.Count <= 0) return;

            UserList.ItemsSource = users;
            TotalRecord.Text = "Total Users: " + users.Count;

        }

        private void Upload_CSV_Click(object sender, RoutedEventArgs e)
        {
            var op = new OpenFileDialog { Title = "Select a .csv file", Filter = "Supported|*.csv;" };

            if (op.ShowDialog() != true) return;

            FileNameTB.Text = op.FileName;
            if (!Directory.Exists(FileNameTB.Text)) return;

            using (var db = new ModelContext())
            {
                //For deleting all previous data
                db.Users.Clear();

                using (var sr = new StreamReader(op.FileName))
                {
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        if (line == null) continue;

                        var value = line.Split(',');

                        db.Users.Add(new User
                        {
                            DeviceID = Convert.ToInt32(value[0]),
                            ScheduleID = Convert.ToInt32(value[1]),
                            ID = value[2],
                            RFID = value[3],
                            Name = value[4],
                            Designation = value[5],
                            Is_Student = Convert.ToBoolean(value[6])
                        });
                    }
                }

                db.SaveChanges();
                LocalData.Instance.Users = db.Users.ToList();
            }


            UserList.ItemsSource = LocalData.Instance.UserViews;
            TotalRecord.Text = "Total Users: " + LocalData.Instance.Users.Count;
        }

        private void UserList_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit) return;

            var upUser = e.Row.DataContext as UserView;

            var localUser = LocalData.Instance.Users.FirstOrDefault(u => upUser != null && u.DeviceID == upUser.DeviceID);
            if (localUser == null) return;

            if (upUser != null)
            {
                localUser.Name = upUser.Name;
                localUser.RFID = upUser.RFID;
                localUser.Designation = upUser.Designation;
            }

            using (var db = new ModelContext())
            {
                db.Entry(localUser).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        private void UserList_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            //User user = UserList.SelectedItem as User;

            // if (e.Command == DataGrid.DeleteCommand)
            //{
            //if (!(MessageBox.Show("want to delete?", "Confirm!", MessageBoxButton.YesNo) == MessageBoxResult.Yes))
            //{
            //    e.Handled = true;
            //}
            //else
            //{
            //    MessageBox.Show("data deleted");
            //}
            // }
        }

        private void Ellipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var op = new OpenFileDialog
            {
                Title = "Select a logo",
                Filter = "Supported|*.jpg;*.jpeg;*.png| JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg| Portable Network Graphic (*.png)|*.png"
            };

            if (op.ShowDialog() == true)
            {
                //LogoSource.Text = op.FileName;
                //UserImage.ImageSource = new BitmapImage(new Uri(op.FileName));
            }
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            LoadingPB.IsIndeterminate = true;
            DownloadButton.IsEnabled = false;

            var netCheck = await ApiUrl.IsNoNetConnection();
            if (netCheck)
            {
                MessageBox.Show("No Internet", "No Internet connection!");

                LoadingPB.IsIndeterminate = false;
                DownloadButton.IsEnabled = true;
                return;
            }

            try
            {
                var client = new RestClient(ApiUrl.EndPoint);
                var request = new RestRequest("api/Users/{id}", Method.GET);

                using (var db = new ModelContext())
                {
                    var ins = LocalData.Instance.institution;

                    request.AddHeader("Authorization", "Bearer " + ins.Token);
                    request.AddUrlSegment("id", ins.SchoolID);

                    // Execute the request
                    var response = await client.ExecuteTaskAsync<List<User>>(request);

                    if (response.StatusCode == HttpStatusCode.OK && response.Data != null)
                    {
                        //For deleting all previous data
                        db.Users.Clear();

                        if (!response.Data.Any())
                        {
                            MessageBox.Show("No User Found or User not Assign In Schedule");
                            return;
                        }


                        foreach (var item in response.Data)
                        {
                            db.Users.Add(item);
                        }

                        await db.SaveChangesAsync();
                        LocalData.Instance.Users = response.Data;
                    }
                }


                UserList.ItemsSource = LocalData.Instance.UserViews;
                TotalRecord.Text = "Total Users: " + LocalData.Instance.Users.Count;


                LoadingPB.IsIndeterminate = false;
                DownloadButton.IsEnabled = true;

                //Empty Error
                LocalData.Current_Error = new Setting_Error();
                ErrorSnackBar.IsActive = false;
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
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new ModelContext())
            {
                if (!db.Users.Any()) return;

                db.Users.Clear();
                db.SaveChanges();
                LocalData.Instance.UserViews.Clear();
                LocalData.Instance.Users = db.Users.ToList();
                UserList.ItemsSource = LocalData.Instance.Users;
                TotalRecord.Text = "";
            }
        }
    }
}
