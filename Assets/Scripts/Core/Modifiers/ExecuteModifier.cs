using UnityEngine;

/// <summary>
/// Increases damage against targets below a health threshold.
/// Executes as a late-stage multiplier (priority 205).
/// </summary>
public class ExecuteModifier : IDamageModifier
{
    public int Priority => 205; // Late multiplier

    private readonly Unit _owner;
    private readonly float _healthThreshold;
    private readonly float _damageBonus;

    /// <param name="owner">The unit that owns this modifier</param>
    /// <param name="healthThreshold">Target HP% threshold (0.0-1.0)</param>
    /// <param name="damageBonus">Damage multiplier when threshold is met (e.g., 1.5 = +50% damage)</param>
    public ExecuteModifier(Unit owner, float healthThreshold, float damageBonus)
    {
        _owner = owner;
        _healthThreshold = Mathf.Clamp01(healthThreshold);
        _damageBonus = damageBonus;
    }

    public void Modify(DamageContext ctx)
    {
        if (ctx.Source != _owner) return;
        if (ctx.Target == null) return;

        var targetHpPercent = (float)ctx.Target.Stats.CurrentHP / ctx.Target.Stats.MaxHP;
        if (targetHpPercent <= _healthThreshold)
        {
            ctx.FinalValue = Mathf.CeilToInt(ctx.FinalValue * _damageBonus);

            Log.Info("Execute bonus triggered", new
            {
                source = _owner.Name,
                target = ctx.Target.Name,
                targetHpPercent,
                threshold = _healthThreshold,
                bonus = _damageBonus,
                damage = ctx.FinalValue
            });
        }
    }
}
