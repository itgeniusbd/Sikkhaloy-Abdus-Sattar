using Sikkhaloy.Shared.Students;
using Sikkhaloy.Shared.Sync;

namespace Sikkhaloy.LocalData.Sync;

public sealed class DashboardStats
{
    public int TotalStudents { get; set; }
    public int MaleCount { get; set; }
    public int FemaleCount { get; set; }
    public int NewCount { get; set; }
    public int OldCount { get; set; }
    public int ClassCount { get; set; }
    public int PendingSync { get; set; }
    public IReadOnlyList<DashboardClassRowDto> Classes { get; set; } = [];
    public IReadOnlyList<DashboardNamedCountDto> BloodGroups { get; set; } = [];
    public IReadOnlyList<StudentDto> BirthdaysToday { get; set; } = [];
    public IReadOnlyList<StudentDto> BirthdaysUpcoming { get; set; } = [];
    public IReadOnlyList<StudentDto> Recent { get; set; } = [];
}
