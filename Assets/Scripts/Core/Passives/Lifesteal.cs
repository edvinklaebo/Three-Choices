using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Lifesteal : Passive
{
    [NonSerialized] private readonly List<HealData> _pendingHeals = new();
    [SerializeField] private float percent;

    public Lifesteal(Unit owner, float percent)
    {
        Owner = owner;
        this.percent = percent;
        OnAttach();
    }

    protected override void OnAttach()
    {
        Owner.OnHit += OnDamageDealt;
    }

    protected override void OnDetach()
    {
        Owner.OnHit -= OnDamageDealt;
    }

    private void OnDamageDealt(Unit target, int damage)
    {
        var heal = Mathf.CeilToInt(damage * percent);

        // Track HP before healing
        var hpBefore = Owner.Stats.CurrentHP;
        var maxHP = Owner.Stats.MaxHP;

        // Apply healing to state
        Owner.Heal(heal);

        // Track HP after healing
        var hpAfter = Owner.Stats.CurrentHP;

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