using Microsoft.EntityFrameworkCore;
using Sikkhaloy.LocalData;

namespace Sikkhaloy.Web.Services;

public sealed class ScopedSqliteFactory : IDbContextFactory<LocalDbContext>
{
    private readonly string _path;

    public ScopedSqliteFactory(LocalDbPath path)
    {
        _path = path.Value;
        var folder = Path.GetDirectoryName(_path);
        if (!string.IsNullOrWhiteSpace(folder))
            Directory.CreateDirectory(folder);
    }

    public LocalDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<LocalDbContext>()
            .UseSqlite($"Data Source={_path}")
            .Options;
        return new LocalDbContext(options);
    }
}
