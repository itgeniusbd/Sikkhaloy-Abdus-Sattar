namespace AttendanceDevice.Model
{
    public class Error
    {
        public Error(string title, string message)
        {
            this.ErrorTittle = title;
            this.ErrorMessage = message;
        }
        public string ErrorTittle { get; set; }
        public string ErrorMessage { get; set; }
    }
}
