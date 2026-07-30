using AttendanceDevice.Config_Class;
using System;

namespace AttendanceDevice.ViewModel
{
    class ErrorData_View
    {
        public int id { get; set; }
        public string ErrorType { get; set; }
        public string ErrorDescription { get; set; }
        public string ErrorDate { get; set; }
        public DateTime dtErrorDate
        {
            get
            {
                return AttendanceDateHelper.TryParse(this.ErrorDate, out var date) ? date : DateTime.MinValue;
            }
        }
    }
}
