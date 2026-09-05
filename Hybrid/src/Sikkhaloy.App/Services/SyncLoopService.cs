using Sikkhaloy.LocalData.Sync;

namespace Sikkhaloy.App.Services;

public sealed class SyncLoopService : IDisposable
{
    private readonly AppSession _session;
    private readonly SyncEngine _engine;
    private readonly CancellationTokenSource _cts = new();
    private int _started;
    private int _inFlight;
    private volatile bool _blocking;

    public SyncLoopService(AppSession session, SyncEngine engine)
    {
        _session = session;
        _engine = engine;
        _session.Changed += OnSessionChanged;
    }

    public bool ShowLoginSync => _blocking;
    public event Action? Changed;

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
        var bootstrap = _session.AwaitInitialSync && !_session.IsStudent;
        if (bootstrap)
        {
            _session.MarkSyncOverlaySeen();
            _session.FinishInitialSync();
            Changed?.Invoke();
        }
        _ = SyncNowAsync(_cts.Token, bootstrap);
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (!_session.AwaitInitialSync)
                await SyncNowAsync(cancellationToken, force: false);
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
            while (await timer.WaitForNextTickAsync(cancellationToken))
                await SyncNowAsync(cancellationToken, force: false);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task SyncNowAsync(CancellationToken cancellationToken, bool force = false)
    {
        var current = _session.Current;
        if (current is null || string.IsNullOrWhiteSpace(_session.AccessToken))
            return;

        Interlocked.Increment(ref _inFlight);
        try
        {
            await _engine.RunOnceAsync(current, _session.AccessToken, force, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
        }
        finally
        {
            if (Interlocked.Decrement(ref _inFlight) == 0 && _blocking)
            {
                _blocking = false;
                Changed?.Invoke();
            }
        }
    }

    public void Dispose()
    {
        _session.Changed -= OnSessionChanged;
        _cts.Cancel();
        _cts.Dispose();
    }
}
