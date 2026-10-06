using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Sikkhaloy.LocalData.Sync;

namespace Sikkhaloy.Client.Updates;

/// <summary>A newer installer offered by the server (Handeler/Desktop_App_Update.ashx on sikkhaloy.com).</summary>
public sealed record HybridUpdateInfo(string Version, Version ParsedVersion, string FileName, long FileSize, string Sha256, Uri DownloadUrl, string Notes);

/// <summary>
/// Desktop auto-update, modelled on the attendance PC app (AttendanceDevice\Config_Class\AppAutoUpdate.cs):
/// ask the V2 site for the latest installer, compare with this app's version, and when the user agrees download it,
/// verify size + SHA-256, wait until no sync is running, then hand over to a small helper that runs the setup
/// silently (in-place upgrade, same AppId) and starts the app again. Every failure while checking is silent.
/// Local data (%LOCALAPPDATA%\SIKKHALOY\Hybrid\sikkhaloy.db, incl. pending sync items) is never touched by the setup.
/// </summary>
public sealed class HybridUpdateService : IDisposable
{
    public const string DefaultInfoUrl = "https://sikkhaloy.com/Handeler/Desktop_App_Update.ashx";
    public const string UrlOverrideVariable = "SIKKHALOY_HYBRID_UPDATE_URL";

    private static readonly TimeSpan FirstCheckDelay = TimeSpan.FromSeconds(20);
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(6);

    private readonly string _infoUrl;
    private readonly SyncEngine _sync;
    private readonly HttpClient _http = new() { Timeout = TimeSpan.FromMinutes(30) };
    private readonly CancellationTokenSource _cts = new();
    private int _started;

    public HybridUpdateService(string? configuredInfoUrl, SyncEngine sync)
    {
        _sync = sync;
        _infoUrl = FirstNonEmpty(Environment.GetEnvironmentVariable(UrlOverrideVariable), configuredInfoUrl, DefaultInfoUrl);
    }

    /// <summary>Raised (on a background thread) when the server has a newer version.</summary>
    public event Action<HybridUpdateInfo>? UpdateAvailable;

    public static Version InstalledVersion => Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(0, 0, 0, 0);

    public int PendingSyncCount => _sync.PendingCount;

    public static string UpdateFolder => Path.Combine(Path.GetTempPath(), "SikkhaloyHybridUpdate");

    private static string LogPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SIKKHALOY", "Hybrid", "update.log");

    /// <summary>Checks shortly after startup and then every 6 hours.</summary>
    public void Start()
    {
#if DEBUG
        // Dev builds are 1.0.0; only check when a test URL is set explicitly.
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(UrlOverrideVariable)))
            return;
#endif
        if (Interlocked.Exchange(ref _started, 1) == 1)
            return;
        _ = LoopAsync(_cts.Token);
    }

    private async Task LoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(FirstCheckDelay, cancellationToken);
            while (!cancellationToken.IsCancellationRequested)
            {
                var info = await CheckAsync(cancellationToken);
                if (info is not null)
                {
                    try { UpdateAvailable?.Invoke(info); } catch { }
                }
                await Task.Delay(CheckInterval, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    /// <summary>Returns the offered update when it is newer than this app; null otherwise (never throws).</summary>
    public async Task<HybridUpdateInfo?> CheckAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(20));
            var url = new Uri(_infoUrl + (_infoUrl.Contains('?') ? "&" : "?") + "mode=info&current="
                              + Uri.EscapeDataString(FormatVersion(InstalledVersion)));
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true };
            using var response = await _http.SendAsync(request, timeout.Token);
            if (!response.IsSuccessStatusCode)
            {
                Log($"check: HTTP {(int)response.StatusCode} (no update offered)");
                return null;
            }

            var info = Parse(await response.Content.ReadAsStringAsync(timeout.Token), url);
            if (info is null)
            {
                Log("check: no valid update info");
                return null;
            }

            var newer = Compare(info.ParsedVersion, InstalledVersion) > 0;
            Log($"check: installed {FormatVersion(InstalledVersion)}, server {info.Version} -> {(newer ? "update available" : "up to date")}");
            return newer ? info : null;
        }
        catch (Exception ex)
        {
            Log("check: " + ex.GetType().Name + ": " + ex.Message);
            return null;
        }
    }

    internal static HybridUpdateInfo? Parse(string json, Uri infoUrl)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
            return null;
        if (!root.TryGetProperty("available", out var available) || available.ValueKind != JsonValueKind.True)
            return null;

        var version = Str(root, "version");
        var fileName = Path.GetFileName(Str(root, "fileName"));
        var sha256 = Str(root, "sha256");
        var download = Str(root, "downloadUrl");
        long size = 0;
        if (root.TryGetProperty("fileSize", out var sizeEl) && sizeEl.ValueKind == JsonValueKind.Number)
            sizeEl.TryGetInt64(out size);

        if (!TryParseVersion(version, out var parsed) || string.IsNullOrWhiteSpace(fileName)
            || !fileName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) || size <= 0
            || sha256.Length != 64 || !sha256.All(Uri.IsHexDigit)
            || !Uri.TryCreate(infoUrl, download, out var downloadUrl)
            || (downloadUrl.Scheme != Uri.UriSchemeHttps && downloadUrl.Scheme != Uri.UriSchemeHttp))
            return null;

        return new HybridUpdateInfo(FormatVersion(parsed), parsed, fileName, size, sha256.ToLowerInvariant(), downloadUrl, Str(root, "notes"));
    }

    /// <summary>Downloads the setup to %TEMP%\SikkhaloyHybridUpdate and verifies size + SHA-256. Throws on failure.</summary>
    public async Task<string> DownloadAndVerifyAsync(HybridUpdateInfo info, IProgress<double>? progress, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(UpdateFolder);
        var target = Path.Combine(UpdateFolder, info.FileName);
        var part = target + ".part";

        if (File.Exists(target) && await VerifyAsync(target, info, cancellationToken))
        {
            progress?.Report(100);
            Log($"download: reusing verified {target}");
            return target;
        }
        TryDelete(target);
        TryDelete(part);

        Log($"download: {info.DownloadUrl} -> {target}");
        using (var response = await _http.GetAsync(info.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
        {
            response.EnsureSuccessStatusCode();
            await using var source = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var destination = new FileStream(part, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
            var buffer = new byte[81920];
            long total = 0;
            int read;
            var lastReported = -1;
            while ((read = await source.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                total += read;
                if (total > info.FileSize)
                    throw new InvalidDataException("The downloaded file is larger than expected.");
                var percent = (int)(total * 100 / info.FileSize);
                if (percent != lastReported)
                {
                    lastReported = percent;
                    progress?.Report(percent);
                }
            }
        }

        if (!await VerifyAsync(part, info, cancellationToken))
        {
            TryDelete(part);
            Log("download: verification FAILED (size or SHA-256 mismatch)");
            throw new InvalidDataException("ডাউনলোড করা ফাইল যাচাইয়ে মেলেনি (SHA-256)।");
        }

        File.Move(part, target, overwrite: true);
        Log($"download: verified {info.FileSize} bytes, SHA-256 {info.Sha256}");
        return target;
    }

    internal static async Task<bool> VerifyAsync(string path, HybridUpdateInfo info, CancellationToken cancellationToken)
    {
        var file = new FileInfo(path);
        if (!file.Exists || file.Length != info.FileSize)
            return false;
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
        var hash = await SHA256.HashDataAsync(stream, cancellationToken);
        return string.Equals(Convert.ToHexString(hash), info.Sha256, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Waits for a running sync to finish and then keeps sync paused (null when it is still busy after the timeout).
    /// Pending local changes stay in the local database and are pushed after the app starts again.
    /// </summary>
    public Task<IDisposable?> PauseSyncAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
        => _sync.PauseForUpdateAsync(timeout, cancellationToken);

    /// <summary>
    /// Starts the helper (hidden PowerShell, runs as this user): it waits for this app to exit, runs the setup silently
    /// with elevation (Windows asks UAC), then starts the app again as this user - the new version, or the old one if the
    /// setup was cancelled or failed. The caller must shut the app down right after this returns.
    /// </summary>
    public void LaunchInstaller(string setupPath)
    {
        Directory.CreateDirectory(UpdateFolder);
        var script = Path.Combine(UpdateFolder, "apply-update.ps1");
        File.WriteAllText(script, HelperScript, new UTF8Encoding(true));
        var appExe = Environment.ProcessPath ?? Path.Combine(AppContext.BaseDirectory, "SikkhaloyHybrid.exe");
        var log = Path.Combine(UpdateFolder, "apply-update.log");
        var setupLog = Path.Combine(UpdateFolder, "setup.log");
        var powershell = Path.Combine(Environment.SystemDirectory, "WindowsPowerShell", "v1.0", "powershell.exe");

        var psi = new ProcessStartInfo
        {
            FileName = powershell,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = UpdateFolder
        };
        foreach (var arg in new[]
                 {
                     "-NoProfile", "-ExecutionPolicy", "Bypass", "-WindowStyle", "Hidden", "-File", script,
                     "-Setup", setupPath, "-AppExe", appExe, "-WaitPid", Environment.ProcessId.ToString(),
                     "-Log", log, "-SetupLog", setupLog
                 })
            psi.ArgumentList.Add(arg);

        Process.Start(psi);
        Log($"install: helper started for {setupPath} (app {appExe}, pid {Environment.ProcessId})");
    }

    // Kept ASCII-only. Setup switches as in the attendance app: /SILENT shows Inno's progress window without questions.
    internal const string HelperScript = """
param([string]$Setup, [string]$AppExe, [int]$WaitPid, [string]$Log, [string]$SetupLog, [switch]$NoElevate)
$ErrorActionPreference = 'Continue'
function Write-Log([string]$m) { try { Add-Content -LiteralPath $Log -Value ((Get-Date -Format 'yyyy-MM-dd HH:mm:ss') + '  ' + $m) } catch { } }
Write-Log "apply-update: setup=$Setup app=$AppExe pid=$WaitPid"
try { Wait-Process -Id $WaitPid -Timeout 120 -ErrorAction Stop; Write-Log 'app exited' } catch { Write-Log ('wait: ' + $_.Exception.Message) }
$setupArgs = '/SILENT /SUPPRESSMSGBOXES /NORESTART /CLOSEAPPLICATIONS /SP- /LOG="' + $SetupLog + '"'
try {
    if ($NoElevate) { $p = Start-Process -FilePath $Setup -ArgumentList $setupArgs -PassThru -Wait }
    else { $p = Start-Process -FilePath $Setup -ArgumentList $setupArgs -Verb RunAs -PassThru -Wait }
    Write-Log ('setup exit code ' + $p.ExitCode)
} catch { Write-Log ('setup not run: ' + $_.Exception.Message) }
Start-Sleep -Seconds 1
try { Start-Process -FilePath $AppExe; Write-Log "relaunched $AppExe" } catch { Write-Log ('relaunch failed: ' + $_.Exception.Message) }
""";

    internal static bool TryParseVersion(string? raw, out Version version)
    {
        version = new Version(0, 0, 0, 0);
        var parts = (raw ?? "").Trim().TrimStart('v', 'V').Split('.');
        if (parts.Length < 2 || parts.Length > 4)
            return false;
        var n = new int[4];
        for (var i = 0; i < parts.Length; i++)
            if (!int.TryParse(parts[i], out n[i]) || n[i] < 0)
                return false;
        version = new Version(n[0], n[1], n[2], n[3]);
        return true;
    }

    internal static int Compare(Version a, Version b)
        => new Version(a.Major, a.Minor, Math.Max(a.Build, 0), Math.Max(a.Revision, 0))
            .CompareTo(new Version(b.Major, b.Minor, Math.Max(b.Build, 0), Math.Max(b.Revision, 0)));

    public static string FormatVersion(Version v)
        => v.Revision > 0 ? $"{v.Major}.{v.Minor}.{Math.Max(v.Build, 0)}.{v.Revision}" : $"{v.Major}.{v.Minor}.{Math.Max(v.Build, 0)}";

    private static string Str(JsonElement root, string name)
        => root.TryGetProperty(name, out var el) && el.ValueKind == JsonValueKind.String ? (el.GetString() ?? "").Trim() : "";

    private static string FirstNonEmpty(params string?[] values)
        => values.First(v => !string.IsNullOrWhiteSpace(v))!.Trim();

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { }
    }

    internal static void Log(string message)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
            if (File.Exists(LogPath) && new FileInfo(LogPath).Length > 512 * 1024)
                File.Delete(LogPath);
            File.AppendAllText(LogPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}  {message}{Environment.NewLine}");
        }
        catch
        {
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _http.Dispose();
    }
}
