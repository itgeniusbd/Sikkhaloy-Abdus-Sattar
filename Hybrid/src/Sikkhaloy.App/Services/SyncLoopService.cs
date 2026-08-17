using Sikkhaloy.LocalData.Sync;

namespace Sikkhaloy.App.Services;

public sealed class SyncLoopService : IDisposable
{
    private readonly AppSession _session;
    private readonly SyncEngine _engine;
    private readonly CancellationTokenSource _cts = new();
    private int _started;

    public SyncLoopService(AppSession session, SyncEngine engine)
    {
        _session = session;
        _engine = engine;
        _session.Changed += OnSessionChanged;
    }

    public void Start()
    {
        if (Interlocked.Exchange(ref _started, 1) == 1)
            return;
        _ = RunAsync(_cts.Token);
    }

    private void OnSessionChanged()
    {
        if (_session.Current is null || string.IsNullOrWhiteSpace(_session.AccessToken))
            return;
        _ = SyncNowAsync(_cts.Token);
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            await SyncNowAsync(cancellationToken);
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
            while (await timer.WaitForNextTickAsync(cancellationToken))
                await SyncNowAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task SyncNowAsync(CancellationToken cancellationToken)
    {
        var current = _session.Current;
        if (current is null || string.IsNullOrWhiteSpace(_session.AccessToken))
            return;

        try
        {
            await _engine.RunOnceAsync(current, _session.AccessToken, force: true, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
        }
    }

    public void Dispose()
    {
        _session.Changed -= OnSessionChanged;
        _cts.Cancel();
        _cts.Dispose();
    }
}
