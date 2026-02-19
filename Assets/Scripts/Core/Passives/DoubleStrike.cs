using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class DoubleStrike : IPassive, ICombatListener
{
    [SerializeField] private float triggerChance;
    [SerializeField] private float damageMultiplier;
    private Unit _owner;
    private List<DoubleStrikeData> _pendingStrikes = new();
    private CombatContext _context;

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

        // Execute pending strikes
        var strikes = ConsumePendingStrikes();
        foreach (var strikeData in strikes)
        {
            if (strikeData.Target.isDead)
                continue;

            // Calculate second hit damage with multiplier
            var armorMultiplier = 100f / (100f + strikeData.Target.Stats.Armor);
            var secondBaseDamage = Mathf.CeilToInt(_owner.Stats.AttackPower * armorMultiplier * strikeData.DamageMultiplier);

            var secondCtx = new DamageContext(_owner, strikeData.Target, secondBaseDamage);
            DamagePipeline.Process(secondCtx);

            // Capture HP before second hit
            var secondHpBefore = strikeData.Target.Stats.CurrentHP;
            var secondMaxHP = strikeData.Target.Stats.MaxHP;

            // Apply second hit damage
            strikeData.Target.ApplyDamage(_owner, secondCtx.FinalValue);

            // Capture HP after second hit
            var secondHpAfter = strikeData.Target.Stats.CurrentHP;

            Log.Info("Double Strike second hit applied", new
            {
                attacker = _owner.Name,
                target = strikeData.Target.Name,
                damage = secondCtx.FinalValue,
                hpBefore = secondHpBefore,
                hpAfter = secondHpAfter
            });

            // Add damage action for second hit
            _context.AddAction(new DamageAction(_owner, strikeData.Target, secondCtx.FinalValue, secondHpBefore, secondHpAfter, secondMaxHP));

            // Raise OnHit event for the second hit - this allows other passives to respond
            _context.Raise(new OnHitEvent(_owner, strikeData.Target, secondCtx.FinalValue));

            // Raise AfterAttack event for the second hit - this collects any pending actions (e.g., lifesteal heals)
            _context.Raise(new AfterAttackEvent(_owner, strikeData.Target));

            // Add death action if target died from second hit
            if (strikeData.Target.isDead && !_context.Actions.OfType<DeathAction>().Any(a => a.Target == strikeData.Target))
                _context.AddAction(new DeathAction(strikeData.Target));
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
