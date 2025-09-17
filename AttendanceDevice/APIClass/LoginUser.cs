using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceDevice.APIClass
{
    public class LoginUser
    {
        public LoginUser(string Username, string Password)
        {
            this.username = Username;
            this.password = Password;
            this.grant_type = "password";
        }
        public string username { get; private set; }
        public string password { get; private set; }
        public string grant_type { get; private set; }
    }
}
