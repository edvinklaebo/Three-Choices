using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DoubleStrike : IPassive, ICombatHandlerProvider
{
    [SerializeField] private float triggerChance;
    [SerializeField] private float damageMultiplier;
    
    private List<DoubleStrikeData> _pendingStrikes = new();
    private CombatContext _context;
    private bool _isProcessingStrikes; // Prevent recursive double strikes

    public int Priority => 210; // Late priority - after damage is dealt, after lifesteal

    public DoubleStrike(Unit owner, float triggerChance, float damageMultiplier)
    {
        this.triggerChance = triggerChance;
        this.damageMultiplier = damageMultiplier;
    }

    public void OnAttach(Unit owner)
    {
        owner.OnHit += TryTrigger;
    }

    public void OnDetach(Unit owner)
    {
        owner.OnHit -= TryTrigger;
    }

    public void RegisterHandlers(CombatContext context)
    {
        _context = context;
        context.On<AfterAttackEvent>(OnAfterAttack);
    }

    public void UnregisterHandlers(CombatContext context)
    {
        _context = null;
        context.Off<AfterAttackEvent>(OnAfterAttack);
    }

    private void OnDamageDealt(Unit self, Unit target, int damage)
    {
        if (_suspended)
            return;

        // Roll for double strike trigger
        var roll = UnityEngine.Random.value;
        
        if (roll < triggerChance)
        {
            Log.Info("Double Strike triggered", new
            {
                attacker = _owner.Name,
                target = target.Name,
                originalDamage = damage,
                damageMultiplier,
                roll,
                triggerChance
            });

            // Queue the second strike
            _pendingStrikes ??= new List<DoubleStrikeData>();
            _pendingStrikes.Add(new DoubleStrikeData(target, damageMultiplier));
        }
    }

    private void OnAfterAttack(AfterAttackEvent evt)
    {
        // Only process if we're the attacker
        if (evt.Source != _owner)
            return;

        _pendingStrikes.Add(new DoubleStrikeData(target, damageMultiplier));
    }

    /// <summary>
    ///     Get and clear all pending double strikes that need to be executed.
    /// </summary>
    public List<DoubleStrikeData> ConsumePendingStrikes()
    {
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
