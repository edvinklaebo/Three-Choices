using System;
using UnityEngine;

/// <summary>
///     Fireball ability that triggers at turn start.
///     Deals damage that can crit, then applies burn based on final damage.
///     Burn cannot crit and does not stack.
/// </summary>
[Serializable]
public class Fireball : IAbility, ICombatListener, IActionCreator
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

    public void RegisterHandlers(CombatContext context)
    {
    }

    public void UnregisterHandlers(CombatContext context)
    {
    }

    public void CreateActions(CombatContext context, Unit source, Unit target, int hpBefore, int hpAfter)
    {
        if (hpAfter < hpBefore)
        {
            var damage = hpBefore - hpAfter;
            var maxHP = target.Stats.MaxHP;
            context.AddAction(new FireballAction(source, target, damage, hpBefore, hpAfter, maxHP));
        }
    }

    /// <summary>
    ///     This method is called from the ability triggering system, not during normal attack.
    ///     For Fireball, this is triggered at turn start.
    ///     Returns the damage dealt; the caller applies it via <see cref="CombatContext.ApplyDamage"/>.
    /// </summary>
    public int OnCast(Unit self, Unit target)
    {
        if (target == null || target.isDead)
            return 0;

        var ctx = new DamageContext(self, target, _baseDamage);
        DamagePipeline.Process(ctx);

        var finalDamage = ctx.FinalValue;

        var burnDamage = Mathf.CeilToInt(finalDamage * _burnDamagePercent);
        target.ApplyStatus(new Burn(_burnDuration, burnDamage));

        Log.Info("Fireball burn applied", new
        {
            target = target.Name,
            burnDamage,
            burnDuration = _burnDuration
        });

        return finalDamage;
    }
}