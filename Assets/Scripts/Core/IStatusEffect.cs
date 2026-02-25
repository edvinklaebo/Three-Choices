public interface IStatusEffect
{
    string Id { get; }
    int Stacks { get; }
    int Duration { get; }
    int BaseDamage { get; }

    void OnApply(Unit target);
    /// <summary>
    /// Called at the start of the owning unit's turn.
    /// Returns the amount of damage to deal; the caller is responsible for applying it.
    /// </summary>
    int OnTurnStart(Unit target);

    /// <summary>
    /// Called at the end of the owning unit's turn.
    /// Returns the amount of damage to deal; the caller is responsible for applying it.
    /// </summary>
    int OnTurnEnd(Unit target);
    void OnExpire(Unit target);
    void AddStacks(IStatusEffect effect);
}