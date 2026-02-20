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
    private CombatContext _context;

    public int Priority => 50; // Early priority - abilities trigger before normal attacks

    public Fireball(int baseDamage = 10, int burnDuration = 3, float burnDamagePercent = 0.5f)
    {
        _baseDamage = baseDamage;
        _burnDuration = burnDuration;
        _burnDamagePercent = burnDamagePercent;
    }

    public void RegisterHandlers(CombatContext context)
    {
        _context = context;
    }

    public void UnregisterHandlers(CombatContext context)
    {
        _context = null;
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
    public int OnAttack(Unit self, Unit target)
    {
        if (target == null || target.isDead)
            return 0;

        Log.Info("Fireball ability triggered", new
        {
            source = self.Name,
            target = target.Name,
            baseDamage = _baseDamage
        });

        // Calculate damage through damage pipeline (can crit)
        var armorMultiplier = GetDamageMultiplier(target.Stats.Armor);
        var adjustedBaseDamage = Mathf.CeilToInt(_baseDamage * armorMultiplier);

        var ctx = new DamageContext(self, target, adjustedBaseDamage);
        DamagePipeline.Process(ctx);

        var finalDamage = ctx.FinalValue;

        Log.Info("Fireball damage calculated", new
        {
            source = self.Name,
            target = target.Name,
            baseDamage = _baseDamage,
            adjustedBaseDamage,
            finalDamage,
            isCritical = ctx.IsCritical
        });

        // Apply burn based on final damage (burn damage cannot crit).
        // Burn is queued before the caller applies HP damage intentionally: the burn effect starts
        // this turn regardless of whether the fireball kills the target. A burn on a dead target
        // never ticks since the fight ends immediately after the kill.
        var burnDamage = Mathf.CeilToInt(finalDamage * _burnDamagePercent);
        target.ApplyStatus(new Burn(_burnDuration, burnDamage));

        Log.Info("Fireball burn applied", new
        {
            target = target.Name,
            burnDamage,
            burnDuration = _burnDuration
        });

        // Return damage — caller (CombatEngine) applies it via _context.ApplyDamage
        return finalDamage;
    }

    private static float GetDamageMultiplier(int armor)
    {
        return 100f / (100f + armor);
    }
}