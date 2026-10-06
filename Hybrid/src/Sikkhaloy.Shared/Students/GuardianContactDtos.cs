namespace Sikkhaloy.Shared.Students;

/// <summary>One active student of the current session with the guardian SMS number (Student.SMSPhoneNo).</summary>
public sealed class GuardianContactDto
{
    public int StudentID { get; set; }
    public string ID { get; set; } = "";
    public string? RollNo { get; set; }
    public string Name { get; set; } = "";
    public string? FatherName { get; set; }
    public string? ClassName { get; set; }
    public string? SectionName { get; set; }
    public string? Phone { get; set; }
}

public sealed class GuardianContactPageDto
{
    /// <summary>All matching students (not only this page).</summary>
    public int Total { get; set; }
    public List<GuardianContactDto> Rows { get; set; } = [];
}

/// <summary>Bangladesh mobile numbers for tel: / wa.me links.</summary>
public static class BdPhone
{
    /// <summary>
    /// Returns the number as 8801XXXXXXXXX (13 digits) or null when it is not a valid BD mobile.
    /// Accepts spaces, dashes, dots, brackets, Bangla digits and the prefixes +880 / 880 / 00880 / 0
    /// (or none: 1XXXXXXXXX). If the field holds several numbers (",", ";", "/"), the first one is used.
    /// </summary>
    public static string? Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;
        var first = raw.Split([',', ';', '/', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault() ?? "";
        var digits = new System.Text.StringBuilder(first.Length);
        foreach (var ch in first)
        {
            if (ch is >= '0' and <= '9')
                digits.Append(ch);
            else if (ch is >= '\u09E6' and <= '\u09EF')
                digits.Append((char)('0' + (ch - '\u09E6')));
        }
        var d = digits.ToString();
        if (d.StartsWith("00880", StringComparison.Ordinal))
            d = d[2..];
        if (d.Length == 11 && d.StartsWith("01", StringComparison.Ordinal))
            d = "88" + d;
        else if (d.Length == 10 && d[0] == '1')
            d = "880" + d;
        if (d.Length != 13 || !d.StartsWith("8801", StringComparison.Ordinal) || d[4] < '3')
            return null;
        return d;
    }

    /// <summary>01XXXXXXXXX for display, or null.</summary>
    public static string? Local(string? raw)
    {
        var n = Normalize(raw);
        return n is null ? null : n[2..];
    }

    public static string? TelHref(string? raw)
    {
        var n = Normalize(raw);
        return n is null ? null : "tel:+" + n;
    }

    public static string? WhatsAppHref(string? raw)
    {
        var n = Normalize(raw);
        return n is null ? null : "https://wa.me/" + n;
    }
}
