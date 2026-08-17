using Sikkhaloy.Shared.Sync;

namespace Sikkhaloy.Shared.Students;

public sealed class SchoolClassDto
{
    public Guid LocalId { get; set; }
    public int ClassID { get; set; }
    public string Name { get; set; } = "";
    public int SortOrder { get; set; }
    public SyncStatus SyncStatus { get; set; }
}
