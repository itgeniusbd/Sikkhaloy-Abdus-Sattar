namespace Sikkhaloy.Web.Services;

public sealed class LocalDbPath
{
    public string Value { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SIKKHALOY",
        "Hybrid",
        "web",
        $"{Guid.NewGuid():N}.db");
}
