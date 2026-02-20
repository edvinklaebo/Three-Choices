using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DoubleStrike : IPassive, ICombatListener
{
    [SerializeField] private float triggerChance;
    [SerializeField] private float damageMultiplier;
    private Unit _owner;
    private List<DoubleStrikeData> _pendingStrikes = new();
    private CombatContext _context;
    private bool _isProcessingStrikes = false; // Prevent recursive double strikes

    public int Priority => 210; // Late priority - after damage is dealt, after lifesteal

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

    private void OnDamageDealt(Unit target, int damage)
    {
        // Don't trigger double strike during second hit processing to avoid infinite recursion
        if (_isProcessingStrikes)
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

        // Prevent recursive processing
        if (_isProcessingStrikes)
            return;

        // Execute pending strikes
        _isProcessingStrikes = true;
        try
        {
            var strikes = ConsumePendingStrikes();
            foreach (var strikeData in strikes)
            {
                if (strikeData.Target.isDead)
                    continue;

                // Calculate second hit base damage (armor + multiplier), then let ResolveAttack handle the rest
                var armorMultiplier = 100f / (100f + strikeData.Target.Stats.Armor);
                var secondBaseDamage = Mathf.CeilToInt(_owner.Stats.AttackPower * armorMultiplier * strikeData.DamageMultiplier);

                Log.Info("Double Strike second hit queued", new
                {
                    attacker = _owner.Name,
                    target = strikeData.Target.Name,
                    secondBaseDamage,
                    strikeData.DamageMultiplier
                });

                // Resolve the second hit through all combat phases — no direct HP mutation
                _context.ResolveAttack(_owner, strikeData.Target, secondBaseDamage);
            }
        }
        finally
        {
            _isProcessingStrikes = false;
        }
    }

    /// <summary>
    ///     Get and clear all pending double strikes that need to be executed.
    /// </summary>
    public List<DoubleStrikeData> ConsumePendingStrikes()
    {
        if(_pendingStrikes == null)
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
