using System;
using UnityEngine;

/// <summary>
///     Arcane Missiles ability that fires multiple missiles each turn.
///     Each missile is processed independently through the damage pipeline.
/// </summary>
[Serializable]
public class ArcaneMissiles : IAbility
{
    [SerializeField] private int _baseDamage = 5;
    [SerializeField] private int _missileCount = 3;

    public int Priority => 40;

    public ArcaneMissiles(int baseDamage = 5, int missileCount = 3)
    {
        _baseDamage = baseDamage;
        _missileCount = missileCount;
    }

    public void OnCast(Unit self, Unit target, CombatContext context)
    {
        if (target == null || target.IsDead)
            return;

        for (var i = 0; i < _missileCount; i++)
        {
            if (target.IsDead)
                break;

            context.DealDamage(self, target, _baseDamage);
        }
    }
}
