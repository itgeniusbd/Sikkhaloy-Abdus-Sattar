namespace Sikkhaloy.LocalData.Entities;

public sealed class LocalEducationYear
{
    public int EducationYearID { get; set; }
    public int SchoolID { get; set; }
    public string Name { get; set; } = "";
    public int SortOrder { get; set; }
}
