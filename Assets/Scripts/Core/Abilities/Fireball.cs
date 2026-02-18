using System;
using UnityEngine;

/// <summary>
/// Fireball ability that triggers at turn start.
/// Deals damage that can crit, then applies burn based on final damage.
/// Burn cannot crit and does not stack.
/// </summary>
[Serializable]
public class Fireball : IAbility
{
    [NonSerialized] private readonly Unit _owner;
    [SerializeField] private int _baseDamage;
    [SerializeField] private int _burnDuration;
    [SerializeField] private float _burnDamagePercent;

    public Fireball(Unit owner, int baseDamage = 10, int burnDuration = 3, float burnDamagePercent = 0.5f)
    {
        _owner = owner;
        _baseDamage = baseDamage;
        _burnDuration = burnDuration;
        _burnDamagePercent = burnDamagePercent;
    }

    /// <summary>
    /// This method is called from the ability triggering system, not during normal attack.
    /// For Fireball, this is triggered at turn start.
    /// </summary>
    public void OnAttack(Unit self, Unit target)
    {
        if (target == null || target.isDead)
            return;

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

        // Apply damage to target
        target.ApplyDamage(self, finalDamage);

        Log.Info("Fireball damage applied", new
        {
            target = target.Name,
            damage = finalDamage,
            targetHP = target.Stats.CurrentHP
        });

        // Apply burn based on final damage (but burn damage cannot crit)
        if (!target.isDead)
        {
            var burnDamage = Mathf.CeilToInt(finalDamage * _burnDamagePercent);
            target.ApplyStatus(new Burn(burnDamage, _burnDuration));

            Log.Info("Fireball burn applied", new
            {
                target = target.Name,
                burnDamage,
                burnDuration = _burnDuration
            });
        }
    }

    private static float GetDamageMultiplier(int armor)
    {
        return 100f / (100f + armor);
    }
}
