namespace Sikkhaloy.LocalData.Entities;

public sealed class CachedApiSnapshot
{
    public string CacheKey { get; set; } = "";
    public string PayloadJson { get; set; } = "[]";
    public DateTime PulledUtc { get; set; }
}
