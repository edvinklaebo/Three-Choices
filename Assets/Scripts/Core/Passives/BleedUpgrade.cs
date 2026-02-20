using System;
using UnityEngine;

/// <summary>
///     Passive upgrade that applies Bleed status effect when owner hits a target.
/// </summary>
[Serializable]
public class BleedUpgrade : IPassive
{
    [SerializeField] private int stacks;
    [SerializeField] private int duration;
    [SerializeField] private int baseDamage;

    public BleedUpgrade(Unit owner, int stacks = 2, int duration = 3, int baseDamage = 2)
    {
        this.stacks = stacks;
        this.duration = duration;
        this.baseDamage = baseDamage;
        OnAttach(owner);
    }

    public void OnAttach(Unit owner)
    {
        owner.OnHit += ApplyBleed;
    }

    public void OnDetach(Unit owner)
    {
        owner.OnHit -= ApplyBleed;
    }

    private void ApplyBleed(Unit self, Unit target, int _)
    {
        if (target == null)
            return;

        Log.Info("Bleed passive triggered", new
        {
            target = target.Name,
            bleedStacks = stacks,
            bleedDuration = duration,
            bleedBaseDamage = baseDamage
        });

        target.ApplyStatus(new Bleed(stacks, duration, baseDamage));
    }
}
