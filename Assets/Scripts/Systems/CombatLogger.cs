public class CombatLogger
{
    public static CombatLogger Instance { get; } = new CombatLogger();

    private CombatLogger() { }

    public void RegisterUnit(Unit unit)
    {
        if (unit == null)
        {
            Log.Warning("[CombatLogger] Cannot register null unit");
            return;
        }

        unit.Damaged += OnDamaged;
        unit.OnHit += OnHit;
        unit.HealthChanged += OnHealthChanged;
        unit.Died += OnDied;
        unit.StatusEffectApplied += OnStatusEffectApplied;
    }

    public void UnregisterUnit(Unit unit)
    {
        if (unit == null)
            return;

        unit.Damaged -= OnDamaged;
        unit.OnHit -= OnHit;
        unit.HealthChanged -= OnHealthChanged;
        unit.Died -= OnDied;
        unit.StatusEffectApplied -= OnStatusEffectApplied;
    }

    private void OnDamaged(Unit attacker, int damage)
    {
        Log.Info($"[Damaged] {attacker.Name} dealt {damage} damage");
    }

    private void OnHit(Unit target, int damage)
    {
        Log.Info($"[Hit] Hit landed on {target.Name} for {damage} damage");
    }

    private void OnHealthChanged(Unit unit, int currentHP, int maxHP)
    {
        Log.Info($"[Health] {unit.Name}: {currentHP}/{maxHP} HP");
    }

    private void OnDied(Unit unit)
    {
        Log.Info($"[Death] {unit.Name} has died");
    }

    private void OnStatusEffectApplied(Unit unit, IStatusEffect effect, bool stacked)
    {
        if (stacked)
            Log.Info($"[Status] {unit.Name} stacked {effect.Id} (new total: {effect.Stacks})");
        else
            Log.Info($"[Status] {unit.Name} gained {effect.Id} ({effect.Stacks} stacks)");
    }
}
