using System.Collections.Generic;

public static class StatsExtensions
{
    public static IEnumerable<StatViewData> ToViewData(this Stats stats)
    {
        foreach (var (name, value) in stats.Enumerate())
            yield return new StatViewData(name, value);
    }
}