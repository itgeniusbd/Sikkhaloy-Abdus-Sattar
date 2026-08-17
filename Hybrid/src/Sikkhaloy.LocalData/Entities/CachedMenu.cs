namespace Sikkhaloy.LocalData.Entities;

public sealed class CachedMenu
{
    public string UserName { get; set; } = "";
    public string PayloadJson { get; set; } = "";
    public DateTime PulledUtc { get; set; }
}
