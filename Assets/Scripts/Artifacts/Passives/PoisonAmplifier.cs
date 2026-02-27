using System;
using UnityEngine;

/// <summary>
/// Poison Tipped Darts effect.
/// When the owner hits an enemy that has Poison, adds bonus stacks equal to the current stack count,
/// effectively doubling any poison stacks applied on the same hit.
/// Subscribes after PoisonUpgrade so stacks are already present when this fires.
/// </summary>
[Serializable]
public class PoisonAmplifier : IPassive
{
    [SerializeField] private int _bonusStacks;
    [SerializeField] private int _bonusDuration;
    [SerializeField] private int _bonusBaseDamage;

    public PoisonAmplifier(int bonusStacks = 2, int bonusDuration = 3, int bonusBaseDamage = 2)
    {
        _bonusStacks = bonusStacks;
        _bonusDuration = bonusDuration;
        _bonusBaseDamage = bonusBaseDamage;
    }

    public void OnAttach(Unit owner)
    {
        owner.OnHit += OnHit;
    }

    public void OnDetach(Unit owner)
    {
        owner.OnHit -= OnHit;
    }

    private void OnHit(Unit self, Unit target, int damage)
    {
        if (target == null || target.IsDead)
            return;

        for (var i = 0; i < target.StatusEffects.Count; i++)
        {
            if (target.StatusEffects[i].Id != "Poison")
                continue;

            target.ApplyStatus(new Poison(_bonusStacks, _bonusDuration, _bonusBaseDamage));

            Log.Info("[PoisonAmplifier] Bonus poison stacks added", new
            {
                target = target.Name,
                bonusStacks = _bonusStacks
            });

            break;
        }
    }
}
