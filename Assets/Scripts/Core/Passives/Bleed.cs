using System;
using UnityEngine;

[Serializable]
public class Bleed : Passive, IStatusEffect
{
    [SerializeField] private int passiveDuration;

    [SerializeField] private int passiveStacks;

    // Constructor for status effect usage
    public Bleed(int stacks, int duration)
    {
        Stacks = stacks;
        Duration = duration;
    }

    // Constructor for passive usage
    public Bleed(Unit owner, int stacks = 2, int duration = 3)
    {
        Owner = owner;
        passiveStacks = stacks;
        passiveDuration = duration;
        OnAttach();
    }

    protected override void OnAttach()
    {
        Owner.OnHit += ApplyBleed;
    }

    protected override void OnDetach()
    {
        Owner.OnHit -= ApplyBleed;
    }

    public string Id => "Bleed";

    [field: SerializeField]
    public int Stacks { get; set; }

    [field: SerializeField]
    public int Duration { get; set; }

    public void OnApply(Unit target)
    {
        Log.Info("Bleed applied", new
        {
            target = target.Name,
            stacks = Stacks,
            duration = Duration
        });
    }

    public void OnTurnStart(Unit target)
    {
        Log.Info("Bleed ticking", new
        {
            target = target.Name,
            damage = Stacks,
            duration = Duration,
            hpBefore = target.Stats.CurrentHP
        });

        target.ApplyDirectDamage(Stacks);
        Duration--;

        Log.Info("Bleed damage applied", new
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
        Log.Info("Bleed expired", new
        {
            target = target.Name
        });
    }

    public void AddStacks(int amount)
    {
        Stacks += amount;
    }

    // Passive behavior - applies bleed when owner hits something
    private void ApplyBleed(Unit target, int _)
    {
        if (target == null)
            return;

        Log.Info("Bleed passive triggered", new
        {
            attacker = Owner.Name,
            target = target.Name,
            bleedStacks = passiveStacks,
            bleedDuration = passiveDuration
        });

        target.ApplyStatus(new Bleed(passiveStacks, passiveDuration));
    }
}
