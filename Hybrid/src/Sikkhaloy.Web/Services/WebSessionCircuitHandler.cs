using Microsoft.AspNetCore.Components.Server.Circuits;
using Sikkhaloy.App.Services;

namespace Sikkhaloy.Web.Services;

public sealed class WebSessionCircuitHandler : CircuitHandler
{
    private readonly SyncLoopService _syncLoop;
    private readonly LocalDbPath _dbPath;

    public WebSessionCircuitHandler(SyncLoopService syncLoop, LocalDbPath dbPath)
    {
        _syncLoop = syncLoop;
        _dbPath = dbPath;
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _syncLoop.Dispose();
        TryDelete(_dbPath.Value);
        TryDelete(_dbPath.Value + "-wal");
        TryDelete(_dbPath.Value + "-shm");
        return Task.CompletedTask;
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
        }
    }
}
