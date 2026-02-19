using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Lifesteal : IPassive, ICombatListener
{
    [SerializeField] private float percent;
    private Unit _owner;
    private List<HealData> _pendingHeals = new();
    private CombatContext _context;

    public int Priority => 200; // Late priority - after damage is dealt

    public Lifesteal(Unit owner, float percent)
    {
        this.percent = percent;
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
        var heal = Mathf.CeilToInt(damage * percent);

        // Track HP before healing
        var hpBefore = _owner.Stats.CurrentHP;
        var maxHP = _owner.Stats.MaxHP;

        // Apply healing to state
        _owner.Heal(heal);

        // Track HP after healing
        var hpAfter = _owner.Stats.CurrentHP;

        // Store heal data for action queue
        _pendingHeals ??= new List<HealData>();
        _pendingHeals.Add(new HealData(heal, hpBefore, hpAfter, maxHP));
    }

    private void OnAfterAttack(AfterAttackEvent evt)
    {
        // Only process if we're the attacker
        if (evt.Source != _owner)
            return;

        // Add heal actions to context
        var heals = ConsumePendingHeals();
        foreach (var healData in heals)
        {
            _context.AddAction(new HealAction(
                _owner,
                healData.Amount,
                healData.HPBefore,
                healData.HPAfter,
                healData.MaxHP
            ));
        }
    }

    /// <summary>
    ///     Get and clear all pending heals that need to be shown as actions.
    /// </summary>
    public List<HealData> ConsumePendingHeals()
    {
        var heals = new List<HealData>(_pendingHeals);
        _pendingHeals.Clear();
        return heals;
    }

    public readonly struct HealData
    {
        public readonly int Amount;
        public readonly int HPBefore;
        public readonly int HPAfter;
        public readonly int MaxHP;

        public HealData(int amount, int hpBefore, int hpAfter, int maxHP)
        {
            Amount = amount;
            HPBefore = hpBefore;
            HPAfter = hpAfter;
            MaxHP = maxHP;
        }
    }
}