/// <summary>
/// Pure C# service that accumulates run statistics by subscribing to <see cref="Unit"/> events.
/// Decoupled from gameplay logic — for debugging and score-screen display only.
/// </summary>
public class RunStatsTracker
{
    public RunStats Stats { get; private set; } = new RunStats();

    public void Reset()
    {
        Stats = new RunStats();
    }

    public void RegisterPlayer(Unit player)
    {
        if (player == null)
        {
            Log.Warning("[RunStatsTracker] Cannot register null player");
            return;
        }

        player.OnHit += OnPlayerHit;
        player.Damaged += OnPlayerDamaged;
        player.Healed += OnPlayerHealed;
    }

    public void UnregisterPlayer(Unit player)
    {
        if (player == null)
            return;

        player.OnHit -= OnPlayerHit;
        player.Damaged -= OnPlayerDamaged;
        player.Healed -= OnPlayerHealed;
    }

    public void RegisterEnemy(Unit enemy)
    {
        if (enemy == null)
        {
            Log.Warning("[RunStatsTracker] Cannot register null enemy");
            return;
        }
    }

    public void UnregisterEnemy(Unit enemy)
    {
    }

    public void IncrementFightsCompleted()
    {
        Stats.FightsCompleted++;
    }

    private void OnPlayerHit(Unit attacker, Unit target, int damage)
    {
        Stats.TotalDamageDealt += damage;
    }

    private void OnPlayerDamaged(Unit self, Unit attacker, int damage)
    {
        Stats.TotalDamageTaken += damage;
    }

    private void OnPlayerHealed(Unit unit, int amount)
    {
        Stats.TotalHealingDone += amount;
    }
}
