using AttendanceDevice.Config_Class;
using System;
using System.Globalization;

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
                if (ScheduleTimeHelper.TryParse(value, out var time))
                    this._Entry_Time = DateTime.Today.Add(time).ToString("hh:mm tt", CultureInfo.CurrentCulture);
                else if (DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out var dt))
                    this._Entry_Time = dt.ToString("hh:mm tt", CultureInfo.CurrentCulture);
                else
                    this._Entry_Time = value;
            }
        }
        public string Entry_Date { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public bool Is_Student { get; set; }
        public string Backup_Reason { get; set; }
        public DateTime dtEntry_Date
        {
            get
            {
                return AttendanceDateHelper.TryParse(this.Entry_Date, out var date) ? date : DateTime.MinValue;
            }
        }
    }
}
