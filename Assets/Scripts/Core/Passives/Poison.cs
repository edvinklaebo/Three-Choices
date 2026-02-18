using System;
using UnityEngine;

[Serializable]
public class Poison : IPassive, IStatusEffect
{
    [SerializeField] private int passiveDuration;

    [SerializeField] private int passiveStacks;
    
    [SerializeField] private int passiveBaseDamage;

    // Constructor for status effect usage
    public Poison(int stacks, int duration, int baseDamage)
    {
        Stacks = stacks;
        Duration = duration;
        BaseDamage = baseDamage;
    }

    // Constructor for passive usage
    public Poison(Unit owner, int stacks = 2, int duration = 3, int baseDamage = 2)
    {
        passiveStacks = stacks;
        passiveDuration = duration;
        passiveBaseDamage = baseDamage;
        OnAttach(owner);
    }

    public void OnAttach(Unit target)
    {
        target.OnHit += ApplyPoison;
    }

    public void OnDetach(Unit target)
    {
        target.OnHit -= ApplyPoison;
    }

    public string Id => "Poison";

    [field: SerializeField] public int Stacks { get; set; }

    [field: SerializeField] public int Duration { get; set; }

    [field: SerializeField] public int BaseDamage { get; set; }

    // IStatusEffect implementation
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

    // Passive behavior - applies poison when owner hits something
    private void ApplyPoison(Unit target, int _)
    {
        if (target == null)
            return;

        Log.Info("Poison passive triggered", new
        {
            target = target.Name,
            poisonStacks = passiveStacks,
            poisonDuration = passiveDuration,
            poisonBaseDamage = passiveBaseDamage
        });

        target.ApplyStatus(new Poison(passiveStacks, passiveDuration, passiveBaseDamage));
    }
}