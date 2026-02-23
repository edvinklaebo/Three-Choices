using System;
using UnityEngine;

/// <summary>
///     Fireball ability that triggers at turn start.
///     Deals damage that can crit, then applies burn based on final damage dealt.
///     Burn cannot crit and does not stack.
/// </summary>
[Serializable]
public class Fireball : IAbility
{
    [SerializeField] private int _baseDamage;
    [SerializeField] private int _burnDuration;
    [SerializeField] private float _burnDamagePercent;

    public int Priority => 50; // Early priority - abilities trigger before normal attacks

    public Fireball(int baseDamage = 10, int burnDuration = 3, float burnDamagePercent = 0.5f)
    {
        _baseDamage = baseDamage;
        _burnDuration = burnDuration;
        _burnDamagePercent = burnDamagePercent;
    }

    /// <summary>
    ///     Deals fireball damage and applies a burn status effect scaled to the damage dealt.
    ///     A <see cref="FireballAction"/> is added for visual presentation.
    /// </summary>
    public void OnCast(Unit self, Unit target, CombatContext context)
    {
        if (target == null || target.IsDead)
            return;

        var hpBefore = target.Stats.CurrentHP;

        context.DealDamage(self, target, _baseDamage);

        var hpAfter = target.Stats.CurrentHP;
        var finalDamage = hpBefore - hpAfter;

        if (finalDamage > 0)
        {
            var burnDamage = Mathf.CeilToInt(finalDamage * _burnDamagePercent);
            target.ApplyStatus(new Burn(_burnDuration, burnDamage));

            context.AddAction(new FireballAction(self, target, finalDamage, hpBefore, hpAfter, target.Stats.MaxHP));

            Log.Info("Fireball burn applied", new
            {
                target = target.Name,
                burnDamage,
                burnDuration = _burnDuration
            });
        }
    }
}