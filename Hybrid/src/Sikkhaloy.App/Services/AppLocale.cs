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
