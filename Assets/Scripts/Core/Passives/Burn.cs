using System;
using UnityEngine;

/// <summary>
///     Burn status effect that deals damage over time.
///     Unlike stacking effects, Burn refreshes duration and stores the highest damage value.
/// </summary>
[Serializable]
public class Burn : IStatusEffect
{
    [SerializeField] private int _baseDuration;


    public Burn(int duration, int baseDamage)
    {
        _baseDuration = duration;
        Duration = duration;
        BaseDamage = baseDamage;
    }

    public string Id => "Burn";

    // Burn does not have traditional stacks - it uses damage value instead. Stacks is always 1 for simplicity.
    public int Stacks => 1;

    [field: SerializeField] public int Duration { get; private set; }

    [field: SerializeField] public int BaseDamage { get; set; }

    public void OnApply(Unit target)
    {
        Log.Info("Burn applied", new
        {
            target = target.Name,
            damage = BaseDamage,
            duration = Duration
        });
    }

    public int OnTurnStart(Unit target)
    {
        Log.Info("Burn ticking", new
        {
            target = target.Name,
            damage = BaseDamage,
            duration = Duration,
            hpBefore = target.Stats.CurrentHP
        });

        var damage = BaseDamage;
        Duration--;

        Log.Info("Burn damage calculated", new
        {
            target = target.Name,
            damage,
            remainingDuration = Duration
        });

        return damage;
    }

    public int OnTurnEnd(Unit target)
    {
        // No behavior on turn end
        return 0;
    }

    public void OnExpire(Unit target)
    {
        Log.Info("Burn expired", new
        {
            target = target.Name
        });
    }

    public void AddStacks(IStatusEffect effect)
    {
        // Burn does not stack - instead, refresh duration and keep highest damage
        // This is called when a new burn is applied
        if (effect.BaseDamage > BaseDamage)
        {
            Log.Info("Burn refreshed with higher damage", new
            {
                oldDamage = BaseDamage,
                newDamage = effect.BaseDamage
            });
            BaseDamage = effect.BaseDamage;
        }
        else
        {
            Log.Info("Burn refreshed but kept higher damage", new
            {
                currentDamage = BaseDamage,
                attemptedDamage = effect.BaseDamage
            });
        }

        // Reset duration to base duration when burn is refreshed
        Duration = _baseDuration;
    }
}