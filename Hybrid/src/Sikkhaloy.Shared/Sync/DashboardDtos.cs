namespace Sikkhaloy.Shared.Sync;

public sealed class DashboardNamedCountDto
{
    public string Name { get; set; } = "";
    public int Count { get; set; }
}

public sealed class DashboardClassRowDto
{
    public int ClassID { get; set; }
    public string ClassName { get; set; } = "";
    public int NewCount { get; set; }
    public int OldCount { get; set; }
    public int Total => NewCount + OldCount;
}

public sealed class DashboardOverviewDto
{
    public int SmsRemaining { get; set; }
    public int SmsSent { get; set; }
    public List<DashboardNamedCountDto> SmsByYear { get; set; } = [];
    public int EmployeeCount { get; set; }
    public List<DashboardNamedCountDto> Employees { get; set; } = [];
    public List<DashboardNamedCountDto> AttendanceToday { get; set; } = [];
    public decimal Paid { get; set; }
    public decimal PresentDue { get; set; }
    public decimal Unpaid { get; set; }
    public List<DashboardNamedCountDto> Sessions { get; set; } = [];
}
