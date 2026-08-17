namespace Sikkhaloy.Shared.Calendar;

public sealed class HolidayDto
{
    public int HolidayID { get; set; }
    public string HolidayName { get; set; } = "";
    public DateTime HolidayDate { get; set; }
    public bool IsWeekly { get; set; }
}

public sealed class SaveHolidayRequest
{
    public string HolidayName { get; set; } = "";
    public DateTime HolidayDate { get; set; }
}

public sealed class WeeklyHolidayRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<string> Days { get; set; } = [];
}

public sealed class RangeHolidayRequest
{
    public string HolidayName { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public sealed class HolidayResult
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public int Added { get; set; }
}
