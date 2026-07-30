using System;
using System.Globalization;

class TestTimeSpanFormat
{
    static void Main()
    {
        var t = TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(15));
        try
        {
            Console.WriteLine("HH=" + t.ToString(@"HH\:mm\:ss", CultureInfo.InvariantCulture));
        }
        catch (Exception ex)
        {
            Console.WriteLine("HH fail: " + ex.GetType().Name + " " + ex.Message);
        }

        try
        {
            Console.WriteLine("hh=" + t.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture));
        }
        catch (Exception ex)
        {
            Console.WriteLine("hh fail: " + ex.GetType().Name + " " + ex.Message);
        }
    }
}
