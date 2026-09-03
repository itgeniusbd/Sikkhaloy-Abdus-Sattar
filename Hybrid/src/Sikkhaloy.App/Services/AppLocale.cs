namespace Sikkhaloy.App.Services;

public sealed class AppLocale
{
    private readonly string _path;

    public AppLocale()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SIKKHALOY",
            "Hybrid");
        Directory.CreateDirectory(folder);
        _path = Path.Combine(folder, "locale.txt");
        Code = Load();
    }

    public string Code { get; private set; }
    public bool IsBengali => Code == "bn";
    public event Action? Changed;

    public string T(string key, params object[] args) => AppStrings.Get(Code, key, args);

    public bool IsNetNotice(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;
        if (text.Contains("Sync API", StringComparison.OrdinalIgnoreCase)
            || text.Contains("Sikkhaloy.SyncApi", StringComparison.OrdinalIgnoreCase)
            || text.Contains("ইন্টারনেট সংযোগ ছাড়া", StringComparison.Ordinal)
            || text.Contains("without an internet", StringComparison.OrdinalIgnoreCase)
            || text.Contains("internet connection is needed", StringComparison.OrdinalIgnoreCase)
            || text.Contains("সিঙ্কের জন্য ইন্টারনেট", StringComparison.Ordinal))
            return true;
        if (text.Equals("sync.apiDown", StringComparison.OrdinalIgnoreCase)
            || text.Equals("sync.needOnline", StringComparison.OrdinalIgnoreCase)
            || text == T("sync.apiDown")
            || text == T("sync.needOnline")
            || text.Contains("127.0.0.1:5135", StringComparison.Ordinal))
            return true;
        return text == T("net.pageNeed")
            || text == T("net.title")
            || text == T("subj.needOnline")
            || text == T("login.needOnline")
            || text == T("profile.needOnline")
            || text == T("report.needOnline")
            || text == T("access.needOnline")
            || text == T("sub.needOnline")
            || text == T("subact.needOnline")
            || text == T("readm.offline");
    }

    public string SyncError(string? error)
    {
        if (string.IsNullOrWhiteSpace(error))
            return T("sync.failed");
        if (IsNetNotice(error)
            || error.Equals("sync.apiDown", StringComparison.OrdinalIgnoreCase)
            || error.Equals("sync.needOnline", StringComparison.OrdinalIgnoreCase))
            return T("sync.needOnline");
        if (error.StartsWith("login.", StringComparison.OrdinalIgnoreCase)
            || error.StartsWith("sync.", StringComparison.OrdinalIgnoreCase)
            || error.StartsWith("dash.", StringComparison.OrdinalIgnoreCase))
            return T(error);
        return error;
    }

    public string Menu(string text)
    {
        var title = System.Text.RegularExpressions.Regex.Replace((text ?? "").Trim(), @"\s+", " ")
            .Replace('\u2013', '-')
            .Replace('\u2014', '-');
        if (title.Length == 0)
            return title;
        return AppStrings.GetOr(Code, "menu." + title, title);
    }

    public void Set(string language)
    {
        var next = string.Equals(language, "bn", StringComparison.OrdinalIgnoreCase) ? "bn" : "en";
        if (next == Code)
            return;

        Code = next;
        File.WriteAllText(_path, Code);
        Changed?.Invoke();
    }

    private string Load()
    {
        try
        {
            if (File.Exists(_path))
            {
                var saved = File.ReadAllText(_path).Trim();
                if (string.Equals(saved, "bn", StringComparison.OrdinalIgnoreCase))
                    return "bn";
            }
        }
        catch
        {
        }

        return "en";
    }
}
