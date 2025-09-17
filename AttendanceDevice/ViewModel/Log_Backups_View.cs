using System;

namespace AttendanceDevice.ViewModel
{
    class Log_Backups_View
    {
        string _Entry_Time;
        public int DeviceID { get; set; }
        public string Entry_Time
        {
            get
            {
                return this._Entry_Time;
            }
            set
            {
                this._Entry_Time = Convert.ToDateTime(value).ToString("hh:mm tt");
            }
        }
        public string Entry_Date { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public bool Is_Student { get; set; }
        public string Backup_Reason { get; set; }
        public DateTime dtEntry_Date { get { return Convert.ToDateTime(this.Entry_Date); } }
    }
}
