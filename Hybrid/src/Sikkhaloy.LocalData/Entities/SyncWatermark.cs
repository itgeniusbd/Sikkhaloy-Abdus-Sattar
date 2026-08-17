namespace Sikkhaloy.LocalData.Entities;

public sealed class SyncWatermark
{
    public int SchoolID { get; set; }
    public long LastChangeId { get; set; }
    public DateTime PulledUtc { get; set; }
}
