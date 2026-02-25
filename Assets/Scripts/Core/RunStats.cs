using System.Collections.Generic;

/// <summary>
/// Accumulated statistics for a single run. Populated by <see cref="RunStatsTracker"/>.
/// Read-only outside of the tracker.
/// </summary>
public class RunStats
{
    public int TotalDamageDealt { get; set; }
    public int TotalDamageTaken { get; set; }
    public int TotalHealingDone { get; set; }
    public int EnemiesKilled { get; set; }
    public int FightsCompleted { get; set; }

    public IEnumerable<StatViewData> ToViewData()
    {
        yield return new StatViewData("Damage Dealt", TotalDamageDealt);
        yield return new StatViewData("Damage Taken", TotalDamageTaken);
        yield return new StatViewData("Healing Done", TotalHealingDone);
        yield return new StatViewData("Enemies Killed", EnemiesKilled);
        yield return new StatViewData("Fights Completed", FightsCompleted);
    }
}
