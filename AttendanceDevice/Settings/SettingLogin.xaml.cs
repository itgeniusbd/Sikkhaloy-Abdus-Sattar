using AttendanceDevice.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AttendanceDevice.Settings
{
    /// <summary>
    /// Interaction logic for SettingLogin.xaml
    /// </summary>
    public partial class SettingLogin : Window
    {
        public SettingLogin()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (SettingPasswordBox.Password.Trim() == "") return;

            using (var db = new ModelContext())
            {
                var institution = db.Institutions.FirstOrDefault();

                if (institution == null) return;

                if (institution.SettingKey != SettingPasswordBox.Password)
                {
                    Error.Text = "Setting key is incorrect!";
                    SettingPasswordBox.Password = "";
                    return;
                }

                var settings = new Setting();
                settings.Show();

                Close();
            }
        }
    }
}
