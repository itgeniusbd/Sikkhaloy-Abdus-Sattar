using AttendanceDevice.Model;
using System.Linq;

namespace AttendanceDevice.ViewModel
{
    public class Setting_Dashboard_View
    {
        public Setting_Dashboard_View()
        {
            using (var db = new ModelContext())
            {
                Total_users = db.Users.Count();
                Total_Stuent = db.Users.Where(u => u.Is_Student).Count();
                Total_Employee = db.Users.Where(u => !u.Is_Student).Count();
                Total_Device = db.Devices.Count();
                Backup_Datas = db.attendanceLog_Backups.Select(b => new Log_Backups_View
                {
                    DeviceID = b.DeviceID,
                    Entry_Time = b.Entry_Time,
                    Entry_Date = b.Entry_Date,
                    Backup_Reason = b.Backup_Reason
                }).Distinct().Count();
                Pending_Attn_Records = db.attendance_Records.Where(a => !a.Is_Sent || !a.Is_Updated).Count();
                No_of_Schedules = db.attendance_Schedule_Days.Count();
                Total_Leave_Users = db.user_Leave_Records.Count();
                Ins = db.Institutions.FirstOrDefault();
            }
        }
        public int Total_users { get; set; }
        public int Total_Stuent { get; set; }
        public int Total_Employee { get; set; }
        public int Total_Device { get; set; }
        public int Backup_Datas { get; set; }
        public int Pending_Attn_Records { get; set; }
        public int No_of_Schedules { get; set; }
        public int Total_Leave_Users { get; set; }
        public Institution Ins { get; set; }
    }
}
