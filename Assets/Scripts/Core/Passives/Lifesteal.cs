using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Lifesteal : IPassive
{
    [SerializeField] private float percent;
    private Unit _owner;
    private readonly List<HealData> _pendingHeals = new();

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
        _pendingHeals.Add(new HealData(heal, hpBefore, hpAfter, maxHP));
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