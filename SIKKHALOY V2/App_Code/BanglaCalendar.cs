using System;

/// <summary>
/// Bengali Calendar Helper Class
/// Converts Gregorian dates to Bengali calendar dates
/// </summary>
public class BanglaCalendar
{
    private static readonly string[] BanglaMonths = {
  "?????", "???????", "?????", "??????", "?????", "??????",
        "???????", "?????????", "???", "???", "???????", "?????"
    };

    private static readonly string[] BanglaDigits = {
        "?", "?", "?", "?", "?", "?", "?", "?", "?", "?"
    };

    private static readonly string[] BanglaDayNames = {
        "??????", "??????", "??????", "????????", "??????", "???????????", "????????"
    };

  // Month lengths in Bengali calendar
  private static readonly int[] MonthDays = { 31, 31, 31, 31, 31, 31, 30, 30, 30, 30, 30, 30 };

    /// <summary>
    /// Convert English number to Bangla digits
    /// </summary>
    public static string ConvertToBanglaDigits(int number)
    {
 string result = "";
 foreach (char digit in number.ToString())
   {
     result += BanglaDigits[int.Parse(digit.ToString())];
        }
        return result;
    }

    /// <summary>
  /// Get Bangla date string from Gregorian date
    /// </summary>
    public static string GetBanglaDate(DateTime gregorianDate)
    {
        try
   {
       int day, month, year;
     ConvertGregorianToBengali(gregorianDate, out day, out month, out year);

   string banglaDay = ConvertToBanglaDigits(day);
      string banglaMonth = BanglaMonths[month - 1];

         return string.Format("{0} {1}", banglaDay, banglaMonth);
        }
        catch
        {
  return "";
    }
    }

    /// <summary>
    /// Get full Bangla date with year
    /// </summary>
    public static string GetFullBanglaDate(DateTime gregorianDate)
    {
        try
        {
            int day, month, year;
 ConvertGregorianToBengali(gregorianDate, out day, out month, out year);

 string banglaDay = ConvertToBanglaDigits(day);
   string banglaMonth = BanglaMonths[month - 1];
            string banglaYear = ConvertToBanglaDigits(year);

         return string.Format("{0} {1} {2}", banglaDay, banglaMonth, banglaYear);
        }
        catch
        {
       return "";
        }
    }

    /// <summary>
    /// Get Bangla day name
    /// </summary>
    public static string GetBanglaDayName(DateTime date)
    {
      return BanglaDayNames[(int)date.DayOfWeek];
    }

    /// <summary>
    /// Convert Gregorian date to Bengali calendar
    /// Bengali year starts from 14/15 April (Pohela Boishakh)
    /// </summary>
private static void ConvertGregorianToBengali(DateTime gregorianDate, out int day, out int month, out int year)
    {
        // Bengali calendar epoch: 593 years behind Gregorian
   int gYear = gregorianDate.Year;
        int gMonth = gregorianDate.Month;
  int gDay = gregorianDate.Day;

        // Determine if leap year in Gregorian calendar
        bool isLeapYear = DateTime.IsLeapYear(gYear);

        // Bengali New Year starts on April 14 (or April 15 in leap years)
        int pohela = isLeapYear ? 15 : 14;

        if (gMonth < 4 || (gMonth == 4 && gDay < pohela))
{
         // Before Bengali New Year - previous Bengali year
            year = gYear - 594;

            // Calculate month and day
            DateTime bengaliNewYear = new DateTime(gYear - 1, 4, pohela);
  TimeSpan diff = gregorianDate - bengaliNewYear;
  int totalDays = diff.Days + 1;

  CalculateMonthAndDay(totalDays, out month, out day);
     }
        else
        {
            // After or on Bengali New Year - current Bengali year
year = gYear - 593;

      DateTime bengaliNewYear = new DateTime(gYear, 4, pohela);
            TimeSpan diff = gregorianDate - bengaliNewYear;
       int totalDays = diff.Days + 1;

   CalculateMonthAndDay(totalDays, out month, out day);
     }
    }

    /// <summary>
    /// Calculate Bengali month and day from total days
    /// </summary>
    private static void CalculateMonthAndDay(int totalDays, out int month, out int day)
 {
        month = 1;
        day = totalDays;

        for (int i = 0; i < 12; i++)
        {
        if (day <= MonthDays[i])
            {
            month = i + 1;
  break;
            }
          day -= MonthDays[i];
        }

        // Handle edge case
        if (day > MonthDays[month - 1])
        {
        day = 1;
            month++;
          if (month > 12)
   {
         month = 1;
 }
        }
    }

    /// <summary>
    /// Get Bengali month name
    /// </summary>
    public static string GetBanglaMonthName(int monthIndex)
    {
        if (monthIndex >= 1 && monthIndex <= 12)
        {
          return BanglaMonths[monthIndex - 1];
      }
        return "";
    }

    /// <summary>
    /// Get current Bengali year
    /// </summary>
    public static int GetCurrentBengaliYear()
    {
     DateTime today = DateTime.Today;
      int year, month, day;
        ConvertGregorianToBengali(today, out day, out month, out year);
     return year;
    }
}
