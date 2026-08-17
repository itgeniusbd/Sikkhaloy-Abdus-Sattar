namespace Sikkhaloy.App.Services;

public sealed class AppHostInfo
{
    public bool IsWeb { get; init; }

    public static AppHostInfo Desktop { get; } = new() { IsWeb = false };
    public static AppHostInfo Web { get; } = new() { IsWeb = true };
}
