using System;

/// <summary>
/// Burn status effect that deals damage over time.
/// Unlike stacking effects, Burn refreshes duration and stores the highest damage value.
/// </summary>
[Serializable]
public class Burn : IStatusEffect
{
    private readonly int _baseDuration;

    public Burn(int damage, int duration)
    {
        Stacks = damage;
        Duration = duration;
        _baseDuration = duration;
    }

    public string Id => "Burn";
    public int Stacks { get; private set; }
    public int Duration { get; private set; }

    public void OnApply(Unit target)
    {
        Log.Info("Burn applied", new
        {
            target = target.Name,
            damage = Stacks,
            duration = Duration
        });
    }

    public void OnTurnStart(Unit target)
    {
        Log.Info("Burn ticking", new
        {
            target = target.Name,
            damage = Stacks,
            duration = Duration,
            hpBefore = target.Stats.CurrentHP
        });

        target.ApplyDirectDamage(Stacks);
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
        if (amount > Stacks)
        {
            Log.Info("Burn refreshed with higher damage", new
            {
                oldDamage = Stacks,
                newDamage = amount
            });
            Stacks = amount;
        }
        else
        {
            Log.Info("Burn refreshed but kept higher damage", new
            {
                currentDamage = Stacks,
                attemptedDamage = amount
            });
        }
        
        // Reset duration to base duration when burn is refreshed
        Duration = _baseDuration;
    }
}
