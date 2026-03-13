using System.Collections.Generic;

namespace UI.Stats
{
    public static class StatsExtensions
    {
        public static IEnumerable<StatViewData> ToViewData(this Core.Stats stats)
        {
            foreach (var (name, value) in stats.Enumerate())
                yield return new StatViewData(name, value);
        }
    }
}