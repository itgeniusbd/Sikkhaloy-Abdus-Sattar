using Sikkhaloy.Shared.Sync;

namespace Sikkhaloy.LocalData.Entities;

public sealed class LocalSchoolClass
{
    public Guid LocalId { get; set; }
    public int ClassID { get; set; }
    public int SchoolID { get; set; }
    public string Name { get; set; } = "";
    public int SortOrder { get; set; }
    public SyncStatus SyncStatus { get; set; }
}
