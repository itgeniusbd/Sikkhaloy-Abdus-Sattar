namespace Sikkhaloy.LocalData.Entities;

public sealed class YearWatermark
{
    public int SchoolID { get; set; }
    public int EducationYearID { get; set; }
    public long LastChangeId { get; set; }
    public DateTime PulledUtc { get; set; }
}
