using UnityEngine;

/// <summary>
/// Service for UI presentation during combat.
/// Handles damage numbers, healing effects, and status indicators.
/// </summary>
public class UIService
{
    private CombatView _combatView;

    public void SetCombatView(CombatView combatView)
    {
        _combatView = combatView;
    }

    public void ShowDamage(Unit target, int amount, DamageType damageType = DamageType.Physical)
    {
        Log.Info("Showing damage UI", new { target = target.Name, damage = amount, type = damageType });

        var worldPosition = GetUnitWorldPosition(target);
        if (worldPosition.HasValue && FloatingTextPool.Instance != null)
        {
            FloatingTextPool.Instance.Spawn(amount, damageType, worldPosition.Value);
        }
    }

    public void ShowHealing(Unit target, int amount)
    {
        Log.Info("Showing healing UI", new { target = target.Name, healing = amount });

        var worldPosition = GetUnitWorldPosition(target);
        if (worldPosition.HasValue && FloatingTextPool.Instance != null)
        {
            FloatingTextPool.Instance.Spawn(amount, DamageType.Heal, worldPosition.Value);
        }
    }

    public void ShowStatusEffect(Unit target, string effectName)
    {
        Log.Info("Showing status effect UI", new { target = target.Name, effect = effectName });
        // Status effects are displayed via StatusEffectPanel
        // This method is kept for compatibility with existing ICombatAction implementations
    }

    /// <summary>
    /// Get the world position of a unit for UI spawn location.
    /// </summary>
    private Vector3? GetUnitWorldPosition(Unit target)
    {
        if (_combatView == null)
            return null;

        var unitView = GetUnitView(target);
        if (unitView != null)
        {
            // Spawn above unit's center
            return unitView.transform.position + Vector3.up * 0.5f;
        }

        return null;
    }

    /// <summary>
    /// Get the UnitView for a given Unit.
    /// </summary>
    private UnitView GetUnitView(Unit target)
    {
        if (_combatView == null)
            return null;

        return _combatView.GetUnitView(target);
    }
}
