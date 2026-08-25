namespace Sikkhaloy.Web.Services;

public sealed class LocalDbPath
{
    public string Value { get; }

    public LocalDbPath(IWebHostEnvironment env)
    {
        // IIS app-pool identity often has no writable LocalAppData unless
        // "Load User Profile" is true. Keep session SQLite under the site.
        var underIis = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("APP_POOL_ID"));
        var dir = underIis
            ? Path.Combine(env.ContentRootPath, "App_Data", "sessions")
            : Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SIKKHALOY",
                "Hybrid",
                "web");

        Directory.CreateDirectory(dir);
        Value = Path.Combine(dir, $"{Guid.NewGuid():N}.db");
    }
}
