using System;
using UnityEngine;

/// <summary>
///     Arcane Missiles ability that triggers at turn start.
///     Fires multiple missiles, each processed through the damage pipeline independently.
///     Each missile can crit individually.
/// </summary>
[Serializable]
public class ArcaneMissiles : IAbility, ICombatListener, IActionCreator
{
    [SerializeField] private int _baseDamage;
    [SerializeField] private int _missileCount;

    public int Priority => 40; // Early priority - before normal attacks
    private CombatContext _context;
    
    public ArcaneMissiles(int baseDamage = 5, int missileCount = 3)
    {
        _baseDamage = baseDamage;
        _missileCount = missileCount;
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
        if (hpAfter >= hpBefore)
            return;

        var damage = hpBefore - hpAfter;
        var maxHP = target.Stats.MaxHP;
        context.AddAction(new ArcaneMissilesAction(source, target, damage, hpBefore, hpAfter, maxHP));
    }

    /// <summary>
    ///     Fires <see cref="_missileCount"/> missiles, each processed independently through the damage pipeline.
    ///     Each missile can crit on its own. Returns the total damage dealt.
    /// </summary>
    public int OnCast(Unit self, Unit target)
    {
        if (target == null || target.IsDead)
            return 0;

        var totalDamage = 0;
        for (var i = 0; i < _missileCount; i++)
        {
            var ctx = new DamageContext(self, target, _baseDamage); 
            DamagePipeline.Process(ctx); 
            
            var finalDamage = ctx.FinalValue; 
            totalDamage += finalDamage;
            
            var hpBefore = target.Stats.CurrentHP - totalDamage;
            var hpAfter = target.Stats.CurrentHP - totalDamage - finalDamage;
            var maxHP = target.Stats.MaxHP;
            if (totalDamage > maxHP)
                break;
            
            // Animate 3 missiles but add the damage in one total
            _context.AddAction(new ArcaneMissilesAction(self, target, _baseDamage, hpBefore, hpAfter, maxHP));
        }

        return totalDamage; 
    }
}
