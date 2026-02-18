using System;
using UnityEngine;

/// <summary>
/// Burn status effect that deals damage over time.
/// Unlike stacking effects, Burn refreshes duration and stores the highest damage value.
/// </summary>
[Serializable]
public class Burn : IStatusEffect
{
    [SerializeField] private int _baseDuration;

    [SerializeField] private int _baseDamage;


    public Burn(int damage, int duration)
    {
        this._baseDamage = damage;
        this._baseDuration = duration;
        Duration = duration;
    }

    public string Id => "Burn";

    // Burn does not have traditional stacks - it uses damage value instead. Stacks is always 1 for simplicity.
    public int Stacks => 1;

    [field: SerializeField]
    public int Duration { get; private set; }

    public void OnApply(Unit target)
    {
        Log.Info("Burn applied", new
        {
            target = target.Name,
            damage = this._baseDamage,
            duration = Duration
        });
    }

    public void OnTurnStart(Unit target)
    {
        Log.Info("Burn ticking", new
        {
            target = target.Name,
            damage = this._baseDamage,
            duration = Duration,
            hpBefore = target.Stats.CurrentHP
        });

        target.ApplyDirectDamage(this._baseDamage);
        Duration--;

        Log.Info("Burn damage applied", new
        {
            target = target.Name,
            hpAfter = target.Stats.CurrentHP,
            remainingDuration = Duration
        });
    }

    public void OnTurnEnd(Unit target)
    {
        // No behavior on turn end
    }

    public void OnExpire(Unit target)
    {
        Log.Info("Burn expired", new
        {
            target = target.Name
        });
    }

    public void AddStacks(int amount)
    {
        // Burn does not stack - instead, refresh duration and keep highest damage
        // This is called when a new burn is applied
        if (amount > this._baseDamage)
        {
            Log.Info("Burn refreshed with higher damage", new
            {
                oldDamage = this._baseDamage,
                newDamage = amount
            });
            this._baseDamage = amount;
        }
        else
        {
            Log.Info("Burn refreshed but kept higher damage", new
            {
                currentDamage = this._baseDamage,
                attemptedDamage = amount
            });
        }
        
        // Reset duration to base duration when burn is refreshed
        Duration = _baseDuration;
    }
}
