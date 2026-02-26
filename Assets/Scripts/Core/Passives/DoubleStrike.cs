using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DoubleStrike : IPassive, ICombatHandlerProvider
{
    [SerializeField] private float triggerChance;
    [SerializeField] private float damageMultiplier;
    private Unit _owner;
    private List<DoubleStrikeData> _pendingStrikes = new();
    private bool _suspended;

    public DoubleStrike(float triggerChance, float damageMultiplier)
    {
        this.triggerChance = triggerChance;
        this.damageMultiplier = damageMultiplier;
    }

    public void OnAttach(Unit owner)
    {
        _owner = owner;
        owner.OnHit += TryTrigger;
    }

    public void OnDetach(Unit owner)
    {
        owner.OnHit -= TryTrigger;
        _owner = null;
    }

    public ICombatListener CreateCombatHandler(Unit owner) => new ExtraAttackHandler(owner, this);

    /// <summary>Suspends strike queuing during second-hit processing (called by <see cref="ExtraAttackHandler"/>).</summary>
    internal void Suspend() => _suspended = true;

    /// <summary>Resumes strike queuing after second-hit processing (called by <see cref="ExtraAttackHandler"/>).</summary>
    internal void Resume() => _suspended = false;

    private void TryTrigger(Unit self, Unit target, int damage)
    {
        if (_suspended)
            return;

        if (UnityEngine.Random.value >= triggerChance)
            return;

        _pendingStrikes.Add(new DoubleStrikeData(target, damageMultiplier));
    }

    /// <summary>
    ///     Get and clear all pending double strikes that need to be executed.
    /// </summary>
    public List<DoubleStrikeData> ConsumePendingStrikes()
    {
        if (_pendingStrikes == null)
            return new List<DoubleStrikeData>();
        var strikes = new List<DoubleStrikeData>(_pendingStrikes);
        _pendingStrikes.Clear();
        return strikes;
    }

    public readonly struct DoubleStrikeData
    {
        public readonly Unit Target;
        public readonly float DamageMultiplier;

        public DoubleStrikeData(Unit target, float damageMultiplier)
        {
            Target = target;
            DamageMultiplier = damageMultiplier;
        }
    }
}
