using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DoubleStrike : IPassive
{
    [SerializeField] private float triggerChance;
    [SerializeField] private float damageMultiplier;
    private Unit _owner;
    private List<DoubleStrikeData> _pendingStrikes = new();

    public DoubleStrike(Unit owner, float triggerChance, float damageMultiplier)
    {
        this.triggerChance = triggerChance;
        this.damageMultiplier = damageMultiplier;
        OnAttach(owner);
    }

    public void OnAttach(Unit owner)
    {
        _owner = owner;
        owner.OnHit += OnDamageDealt;
    }

    public void OnDetach(Unit owner)
    {
        _owner = null;
        owner.OnHit -= OnDamageDealt;
    }

    private void OnDamageDealt(Unit target, int damage)
    {
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
