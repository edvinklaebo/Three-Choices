using System;
using UnityEngine;

[Serializable]
public class Poison : IStatusEffect
{
    public Poison(int stacks, int duration, int baseDamage)
    {
        Stacks = stacks;
        Duration = duration;
        BaseDamage = baseDamage;
    }

    public string Id => "Poison";

    [field: SerializeField] public int Stacks { get; set; }

    [field: SerializeField] public int Duration { get; set; }

    [field: SerializeField] public int BaseDamage { get; set; }

    public void OnApply(Unit target)
    {
        Log.Info("Poison applied", new
        {
            target = target.Name,
            stacks = Stacks,
            duration = Duration
        });
    }

    public void OnTurnStart(Unit target)
    {
        Log.Info("Poison ticking", new
        {
            target = target.Name,
            damage = Stacks,
            duration = Duration,
            hpBefore = target.Stats.CurrentHP
        });

        target.ApplyDirectDamage(Stacks);
        Duration--;

        Log.Info("Poison damage applied", new
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
        Log.Info("Poison expired", new
        {
            target = target.Name
        });
    }

    public void AddStacks(IStatusEffect effect)
    {
        Stacks += effect.Stacks;
    }
}