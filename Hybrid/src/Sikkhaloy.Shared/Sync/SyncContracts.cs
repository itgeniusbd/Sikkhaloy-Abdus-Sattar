namespace Sikkhaloy.Shared.Sync;

public static class EntityTypes
{
    public const string Student = "Student";
    public const string Class = "Class";
    public const string ClassGroup = "ClassGroup";
    public const string ClassSection = "ClassSection";
    public const string ClassShift = "ClassShift";
    public const string ClassJoin = "ClassJoin";
    public const string ApiCall = "ApiCall";
    public const string PendingSms = "PendingSms";

    public static readonly string[] ClassStructure =
    [
        Class, ClassGroup, ClassSection, ClassShift, ClassJoin
    ];

    public static bool IsClassStructure(string entityType) =>
        ClassStructure.Any(x => string.Equals(x, entityType, StringComparison.OrdinalIgnoreCase));
}

public enum SyncStatus
{
    Synced = 0,
    PendingCreate = 1,
    PendingUpdate = 2,
    PendingDelete = 3,
    Conflict = 4
}

public enum SyncOperation
{
    Create = 1,
    Update = 2,
    Delete = 3
}

public sealed class SyncChangeDto
{
    public Guid LocalId { get; set; }
    public string EntityType { get; set; } = EntityTypes.Student;
    public SyncOperation Operation { get; set; }
    public int? ServerId { get; set; }
    public DateTime UpdatedUtc { get; set; }
    public string PayloadJson { get; set; } = "{}";
}

public sealed class PushRequest
{
    public string DeviceId { get; set; } = "";
    public IList<SyncChangeDto> Changes { get; set; } = new List<SyncChangeDto>();
}

public sealed class PushItemResult
{
    public Guid LocalId { get; set; }
    public bool Succeeded { get; set; }
    public int? ServerId { get; set; }
    public string? Error { get; set; }
    public bool IsConflict { get; set; }
}

public sealed class PushResponse
{
    public IList<PushItemResult> Results { get; set; } = new List<PushItemResult>();
}

public sealed class PullResponse
{
    public long Watermark { get; set; }
    public bool HasMore { get; set; }
    public IList<SyncChangeDto> Changes { get; set; } = new List<SyncChangeDto>();
}
