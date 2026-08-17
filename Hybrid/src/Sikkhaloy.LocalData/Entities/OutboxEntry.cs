using Sikkhaloy.Shared.Sync;

namespace Sikkhaloy.LocalData.Entities;

public sealed class OutboxEntry
{
    public long OutboxId { get; set; }
    public Guid LocalId { get; set; }
    public string EntityType { get; set; } = EntityTypes.Student;
    public SyncOperation Operation { get; set; }
    public string PayloadJson { get; set; } = "{}";
    public DateTime CreatedUtc { get; set; }
    public int AttemptCount { get; set; }
    public string? LastError { get; set; }
}
