namespace Sikkhaloy.Shared.Students;

public sealed class EducationYearDto
{
    public int EducationYearID { get; set; }
    public string Name { get; set; } = "";
    public int SortOrder { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCurrent { get; set; }
}

public sealed class SaveEducationYearRequest
{
    public string Name { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public sealed class EducationYearResult
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public int EducationYearID { get; set; }
}
