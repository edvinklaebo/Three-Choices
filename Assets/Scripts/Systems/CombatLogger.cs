using System;

public class CombatLogger
{
    public static CombatLogger Instance { get; } = new CombatLogger();

    private CombatLogger() { }

    /// <summary>
    ///     Raised whenever the logger produces a new formatted message.
    ///     Subscribe to display log entries in a UI scroll view.
    /// </summary>
    public event Action<string> LogAdded;

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

    private void Emit(string message)
    {
        Log.Info(message);
        LogAdded?.Invoke(message);
    }

    private void OnDamaged(Unit self, Unit attacker, int damage)
    {
        Emit($"[Damaged] {self.Name} took {damage} damage from {attacker?.Name ?? "unknown"}");
    }

    private void OnHit(Unit self, Unit target, int damage)
    {
        Emit($"[Hit] {self.Name} hit {target.Name} for {damage} damage");
    }

    private void OnHealthChanged(Unit unit, int currentHP, int maxHP)
    {
        Emit($"[Health] {unit.Name}: {currentHP}/{maxHP} HP");
    }

    private void OnDied(Unit unit)
    {
        Emit($"[Death] {unit.Name} has died");
    }

    private void OnStatusEffectApplied(Unit unit, IStatusEffect effect, bool stacked)
    {
        var message = stacked
            ? $"[Status] {unit.Name} stacked {effect.Id} (new total: {effect.Stacks})"
            : $"[Status] {unit.Name} gained {effect.Id} ({effect.Stacks} stacks)";
        Emit(message);
    }

    public void Invoke(string name)
    {
        LogAdded?.Invoke(name);
    }
}
