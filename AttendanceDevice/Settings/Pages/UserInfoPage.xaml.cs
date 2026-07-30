using AttendanceDevice.APIClass;
using AttendanceDevice.Config_Class;
using AttendanceDevice.Model;
using AttendanceDevice.ViewModel;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
                var ins = LocalData.Instance.institution;

                // Schedules + device assignments first — user list is filtered to assigned devices only.
                var scheduleResult = await ScheduleAssignmentSync.EnsureScheduleBundleAsync(
                    client, ins.SchoolID, ins.Token);
                LocalData.Instance.RefreshUserSchedulesFromDb();

                var assignedDeviceIds = LocalData.Instance.User_Schedules
                    .Where(a => a.DeviceID > 0)
                    .Select(a => a.DeviceID)
                    .Distinct()
                    .ToHashSet();

                var knownScheduleIds = LocalData.Instance.Schedules_Get()
                    .Select(s => s.ScheduleID)
                    .Distinct()
                    .ToHashSet();

                var request = new RestRequest("api/Users/{id}", Method.GET);

                using (var db = new ModelContext())
                {
                    ApiRequestHelper.AddAuthorizedJsonHeaders(request, ins.Token);
                    request.AddUrlSegment("id", ins.SchoolID);

                    var response = await client.ExecuteTaskAsync(request);

                    if (response.StatusCode == HttpStatusCode.OK && !string.IsNullOrWhiteSpace(ApiResponseHelper.ReadContent(response)))
                    {
                        var apiUsers = JsonConvert.DeserializeObject<List<UserApiDto>>(
                            ApiResponseHelper.ReadContent(response),
                            new JsonSerializerSettings
                            {
                                ContractResolver = new CamelCasePropertyNamesContractResolver()
                            }) ?? new List<UserApiDto>();

                        //For deleting all previous data
                        db.Users.Clear();

                        if (!apiUsers.Any())
                        {
                            MessageBox.Show("No User Found or User not Assign In Schedule");
                            return;
                        }

                        var uniqueUsers = apiUsers
                            .Where(u => u.DeviceID > 0)
                            .Where(u => IsScheduleAssignedUser(u, assignedDeviceIds, knownScheduleIds))
                            .GroupBy(u => u.DeviceID)
                            .Select(g => MapApiUser(g.First()))
                            .ToList();

                        if (!uniqueUsers.Any())
                        {
                            MessageBox.Show(
                                "No schedule-assigned users returned from server.\n\n" +
                                "Assign students/employees to schedules on sikkhaloy.com, then download again.",
                                "User download");
                            return;
                        }

                        foreach (var item in uniqueUsers)
                            db.Users.Add(item);

                        await db.SaveChangesAsync();
                        LocalData.Instance.Users = uniqueUsers;
                        await ScheduleAssignmentSync.SyncAssignmentsFromServerAsync(client, ins.SchoolID, ins.Token);
                        LocalData.Instance.EnsureUserScheduleAssignmentsFromUsers();
                        LocalData.Instance.PruneUserSchedulesToLocalUsers();
                        LocalData.Instance.Users = db.Users.ToList();
                        uniqueUsers = LocalData.Instance.Users;

                        LocalData.Current_Error.Type = Error_Type.NoError;
                        LocalData.Current_Error.Message = string.Empty;
                        ErrorSnackBar.IsActive = false;

                        UserList.ItemsSource = LocalData.Instance.UserViews;
                        var studentCount = uniqueUsers.Count(u => u.Is_Student);
                        var employeeCount = uniqueUsers.Count - studentCount;
                        TotalRecord.Text = $"Total Users: {uniqueUsers.Count} (Student: {studentCount}, Employee: {employeeCount})";

                        var skippedCount = apiUsers.Count(u => u.DeviceID > 0) - uniqueUsers.Count;
                        if (skippedCount > 0)
                        {
                            MessageBox.Show(
                                $"{skippedCount} user(s) from server were skipped.\n\n" +
                                "Check on sikkhaloy.com:\n" +
                                "1) Device ID is set\n" +
                                "2) Assigned to an attendance schedule\n" +
                                "3) Student/employee status is Active",
                                "Some users skipped",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }
                        else if (studentCount == 0)
                        {
                            MessageBox.Show(
                                "Employee download OK, but 0 students returned.\n\n" +
                                "Check on server:\n" +
                                "1) Student.DeviceID is set\n" +
                                "2) Schedule_AssignStudent has rows\n" +
                                "3) Attendance_API is published (latest Users API)",
                                "Student download missing");
                        }
                        else if (!scheduleResult.Success && !LocalData.Instance.Schedules_Get().Any())
                        {
                            MessageBox.Show(
                                "Users downloaded, but schedule data could not be saved on this PC.\n\n" +
                                "Open Settings → Schedule → Download from Server.",
                                "Schedule download failed",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                        }
                        else if (scheduleResult.UserScheduleMismatch)
                        {
                            ErrorSnackBar.Message.Content =
                                "Some users still have schedule mismatch. Download again or check server assignments.";
                            ErrorSnackBar.IsActive = true;
                        }
                    }
                    else if (response.StatusCode != HttpStatusCode.OK)
                    {
                        MessageBox.Show($"User download failed ({(int)response.StatusCode}): {response.Content}");
                    }
                }


                UserList.ItemsSource = LocalData.Instance.UserViews;
                if (string.IsNullOrWhiteSpace(TotalRecord.Text))
                    TotalRecord.Text = "Total Users: " + LocalData.Instance.Users.Count;


                LoadingPB.IsIndeterminate = false;
                DownloadButton.IsEnabled = true;

                //Empty Error only when download completed without mismatch warning
                if (!ErrorSnackBar.IsActive)
                {
                    LocalData.Current_Error = new Setting_Error();
                    ErrorSnackBar.IsActive = false;
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
        }

        private static bool IsScheduleAssignedUser(
            UserApiDto user,
            HashSet<int> assignedDeviceIds,
            HashSet<int> knownScheduleIds)
        {
            if (user == null || user.DeviceID <= 0)
                return false;

            if (assignedDeviceIds != null && assignedDeviceIds.Contains(user.DeviceID))
                return true;

            var scheduleId = user.ScheduleID ?? 0;
            return scheduleId > 0 &&
                   knownScheduleIds != null &&
                   knownScheduleIds.Contains(scheduleId);
        }

        private async void DownloadPhotosButton_Click(object sender, RoutedEventArgs e)
        {
            LoadingPB.IsIndeterminate = true;
            DownloadPhotosButton.IsEnabled = false;

            var netCheck = await ApiUrl.IsNoNetConnection();
            if (netCheck)
            {
                MessageBox.Show("No Internet", "No Internet connection!");
                LoadingPB.IsIndeterminate = false;
                DownloadPhotosButton.IsEnabled = true;
                return;
            }

            try
            {
                var ins = LocalData.Instance.institution;
                var client = new RestClient(ApiUrl.EndPoint);
                var result = await UserPhotoSync.DownloadToFolderAsync(client, ins);

                if (!string.IsNullOrWhiteSpace(result.Error))
                {
                    MessageBox.Show(result.Error, "Photo download");
                    return;
                }

                foreach (var user in LocalData.Instance.UserViews)
                    user.ImgLink = UserPhotoHelper.ResolvePhotoUri(ins.Image_Link, user.ID);

                UserList.ItemsSource = null;
                UserList.ItemsSource = LocalData.Instance.UserViews;
                MessageBox.Show($"{result.Summary}\n\nFolder:\n{ins.Image_Link}", "Photo download");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Photo download");
            }
            finally
            {
                LoadingPB.IsIndeterminate = false;
                DownloadPhotosButton.IsEnabled = true;
            }
        }

        private static User MapApiUser(UserApiDto apiUser)
        {
            return new User
            {
                DeviceID = apiUser.DeviceID,
                ID = apiUser.ID,
                RFID = apiUser.RFID,
                Name = apiUser.Name,
                Designation = apiUser.Designation,
                Is_Student = apiUser.IsStudent == true
                    || string.Equals(apiUser.Designation, "Student", StringComparison.OrdinalIgnoreCase),
                ScheduleID = apiUser.ScheduleID ?? 0
            };
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
