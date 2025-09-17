namespace AttendanceDevice.Config_Class
{
    public static class ExtensionMethods
    {
        public static int ToInt(this string s, int @default)
        {
            return int.TryParse(s, out var number) ? number : @default;
        }
    }
}