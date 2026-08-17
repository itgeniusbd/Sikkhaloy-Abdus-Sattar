namespace Sikkhaloy.App.Services;

public sealed class FeeMonth
{
    public string Name { get; init; } = "";
    public DateTime Start { get; init; }
    public DateTime End { get; init; }
}

public static class FeeMonthHelper
{
    public static IReadOnlyList<FeeMonth> FromSession(DateTime start, DateTime end)
    {
        var list = new List<FeeMonth>();
        var cursor = new DateTime(start.Year, start.Month, 1);
        var last = new DateTime(end.Year, end.Month, 1);
        while (cursor <= last)
        {
            list.Add(new FeeMonth
            {
                Name = cursor.ToString("MMMM yyyy"),
                Start = cursor,
                End = cursor.AddDays(9)
            });
            cursor = cursor.AddMonths(1);
        }
        return list;
    }

    public static IReadOnlyList<FeeMonth> Pick(IReadOnlyList<FeeMonth> months, int count)
    {
        if (months.Count == 0 || count <= 0)
            return [];
        if (count == months.Count)
            return months.ToList();
        if (count < months.Count)
        {
            var interval = months.Count / count;
            var remainder = months.Count % count;
            var picked = new List<FeeMonth>(count);
            var index = 0;
            for (var i = 0; i < count; i++)
            {
                picked.Add(months[Math.Min(index, months.Count - 1)]);
                index += interval + (i < remainder ? 1 : 0);
            }
            return picked;
        }

        return Enumerable.Range(0, count).Select(i => months[i % months.Count]).ToList();
    }

    public static IReadOnlyList<FeeMonth> TakeNext(IReadOnlyList<FeeMonth> months, int count, IEnumerable<string> skipNames)
    {
        if (months.Count == 0 || count <= 0)
            return [];
        var skip = new HashSet<string>(
            skipNames.Where(x => !string.IsNullOrWhiteSpace(x)),
            StringComparer.OrdinalIgnoreCase);
        return months.Where(m => !skip.Contains(m.Name)).Take(count).ToList();
    }
}
