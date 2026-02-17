using System.Collections.Generic;
using UnityEngine;

public class Lifesteal : Passive
{
    private readonly float percent;
    private readonly List<HealData> _pendingHeals = new();

    public Lifesteal(Unit owner, float percent)
    {
        Owner = owner;
        this.percent = percent;

        owner.Damaged += OnDamageDealt;
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
        _pendingHeals.Add(new HealData
        {
            Amount = heal,
            HPBefore = hpBefore,
            HPAfter = hpAfter,
            MaxHP = maxHP
        });
    }

    /// <summary>
    /// Get and clear all pending heals that need to be shown as actions.
    /// </summary>
    public List<HealData> ConsumePendingHeals()
    {
        var heals = new List<HealData>(_pendingHeals);
        _pendingHeals.Clear();
        return heals;
    }

    public struct HealData
    {
        public int Amount;
        public int HPBefore;
        public int HPAfter;
        public int MaxHP;
    }
}