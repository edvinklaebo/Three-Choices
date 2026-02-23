using System;
using UnityEngine;

/// <summary>
///     Fireball ability that triggers at turn start.
///     Deals damage that can crit, then applies burn scaled to the final damage dealt.
///     Burn cannot crit and does not stack.
/// </summary>
[Serializable]
public class Fireball : IAbility
{
    [SerializeField] private int _baseDamage;
    [SerializeField] private int _burnDuration;
    [SerializeField] private float _burnDamagePercent;

    public int Priority => 50;

    public Fireball(int baseDamage = 10, int burnDuration = 3, float burnDamagePercent = 0.5f)
    {
        _baseDamage = baseDamage;
        _burnDuration = burnDuration;
        _burnDamagePercent = burnDamagePercent;
    }

    public void OnCast(Unit self, Unit target, CombatContext context)
    {
        if (target == null || target.IsDead)
            return;

        context.DealDamage(self, target, _baseDamage,
            finalDamage => new Burn(_burnDuration, Mathf.CeilToInt(finalDamage * _burnDamagePercent)));
    }
}