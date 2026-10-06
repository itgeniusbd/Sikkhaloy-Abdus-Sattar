namespace Sikkhaloy.SyncApi.Services;

/// <summary>
/// Student field values in the exact format V2 pages expect. V2 Edit_Student_information binds BloodGroup to a
/// dropdown with only these items; anything else (NULL, "", "B(+)" ...) used to crash that page.
/// </summary>
internal static class V2StudentFields
{
    private static readonly string[] BloodGroups = ["Unknown", "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-"];

    /// <summary>Maps any input to one of V2's blood group values; empty/unrecognised -> "Unknown" (V2's none value, V2 admission default).</summary>
    public static string BloodGroup(string? value)
    {
        var text = (value ?? "").Trim().ToUpperInvariant()
            .Replace("POSITIVE", "+").Replace("NEGATIVE", "-").Replace("+VE", "+").Replace("-VE", "-");
        var compact = new string(text
            .Where(ch => ch is 'A' or 'B' or 'O' or '0' or '+' or '-')
            .Select(ch => ch == '0' ? 'O' : ch)
            .ToArray());
        return BloodGroups.FirstOrDefault(g => g == compact) ?? "Unknown";
    }
}
