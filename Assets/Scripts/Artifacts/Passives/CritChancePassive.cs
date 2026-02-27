using System;
using UnityEngine;

/// <summary>
/// Lucky Horseshoe effect.
/// Adds a flat critical hit chance to the owner's attacks.
/// Implements IDamageModifier so DamagePipeline picks it up automatically from Unit.Passives.
/// Crits deal 2× damage.
/// </summary>
[Serializable]
public class CritChancePassive : IPassive, IDamageModifier
{
    private const float CritMultiplier = 2f;

    [SerializeField] private float _critChance;
    [NonSerialized] private Unit _owner;

    public CritChancePassive(float critChance)
    {
        _critChance = Mathf.Clamp01(critChance);
    }

    public int Priority => 210; // Late multiplier, runs after standard modifiers

    public void OnAttach(Unit owner)
    {
        _owner = owner;
    }

    public void OnDetach(Unit owner)
    {
        _owner = null;
    }

    public void Modify(DamageContext ctx)
    {
        if (ctx.Source != _owner) return;
        if (ctx.IsCritical) return;

        if (Random.value < _critChance)
        {
            ctx.IsCritical = true;
            ctx.FinalValue = Mathf.CeilToInt(ctx.FinalValue * CritMultiplier);

            Log.Info("[CritChancePassive] Critical hit!", new
            {
                source = _owner.Name,
                critChance = _critChance,
                damage = ctx.FinalValue
            });
        }
    }
}
