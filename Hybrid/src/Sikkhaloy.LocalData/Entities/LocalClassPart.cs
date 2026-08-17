using Sikkhaloy.Shared.Sync;

namespace Sikkhaloy.LocalData.Entities;

public sealed class LocalClassGroup
{
    public Guid LocalId { get; set; }
    public int SubjectGroupID { get; set; }
    public int SchoolID { get; set; }
    public int ClassID { get; set; }
    public string Name { get; set; } = "";
    public SyncStatus SyncStatus { get; set; }
}

public sealed class LocalClassSection
{
    public Guid LocalId { get; set; }
    public int SectionID { get; set; }
    public int SchoolID { get; set; }
    public int ClassID { get; set; }
    public string Name { get; set; } = "";
    public SyncStatus SyncStatus { get; set; }
}

public sealed class LocalClassShift
{
    public Guid LocalId { get; set; }
    public int ShiftID { get; set; }
    public int SchoolID { get; set; }
    public int ClassID { get; set; }
    public string Name { get; set; } = "";
    public SyncStatus SyncStatus { get; set; }
}

public sealed class LocalClassJoin
{
    public Guid LocalId { get; set; }
    public int JoinID { get; set; }
    public int SchoolID { get; set; }
    public int ClassID { get; set; }
    public int SubjectGroupID { get; set; }
    public int SectionID { get; set; }
    public int ShiftID { get; set; }
    public string GroupName { get; set; } = "";
    public string SectionName { get; set; } = "";
    public string ShiftName { get; set; } = "";
    public SyncStatus SyncStatus { get; set; }
}
