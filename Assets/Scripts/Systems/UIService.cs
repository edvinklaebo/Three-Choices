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

        // Animate health bar to current health value (fallback for non-combat or old callers)
        AnimateHealthBar(target);
    }

    /// <summary>
    /// Show damage with explicit HP values for proper health bar animation.
    /// This overload is used by DamageAction to animate from old HP to new HP.
    /// </summary>
    public void ShowDamage(Unit target, int amount, int hpBefore, int hpAfter, int maxHP, DamageType damageType = DamageType.Physical)
    {
        Log.Info("Showing damage UI with HP values", new 
        { 
            target = target.Name, 
            damage = amount, 
            type = damageType,
            hpBefore,
            hpAfter,
            maxHP
        });

        var worldPosition = GetUnitWorldPosition(target);
        if (worldPosition.HasValue && FloatingTextPool.Instance != null)
        {
            FloatingTextPool.Instance.Spawn(amount, damageType, worldPosition.Value);
        }

        // Animate health bar with explicit HP values
        AnimateHealthBarToValue(target, hpBefore, hpAfter, maxHP);
        
        // Update HP text with explicit values
        UpdateHealthText(target, hpAfter, maxHP);
    }

    public void ShowHealing(Unit target, int amount)
    {
        Log.Info("Showing healing UI", new { target = target.Name, healing = amount });

        var worldPosition = GetUnitWorldPosition(target);
        if (worldPosition.HasValue && FloatingTextPool.Instance != null)
        {
            FloatingTextPool.Instance.Spawn(amount, DamageType.Heal, worldPosition.Value);
        }

        // Animate health bar to current health value
        AnimateHealthBar(target);
    }

    public void ShowStatusEffect(Unit target, string effectName)
    {
        Log.Info("Showing status effect UI", new { target = target.Name, effect = effectName });
        // Status effects are displayed via StatusEffectPanel
        // This method is kept for compatibility with existing ICombatAction implementations
    }

    /// <summary>
    /// Animates the health bar for a unit to its current health value.
    /// This ensures health bar animations are driven by presentation events, not raw state changes.
    /// </summary>
    public void AnimateHealthBar(Unit target)
    {
        if (target == null)
            return;

        var healthBar = GetHealthBar(target);
        if (healthBar != null)
        {
            healthBar.AnimateToCurrentHealth();
        }
    }

    /// <summary>
    /// Animates the health bar for a unit from a specific old value to a specific new value.
    /// This allows proper animation even when the unit's state has already been modified.
    /// </summary>
    public void AnimateHealthBarToValue(Unit target, int hpBefore, int hpAfter, int maxHP)
    {
        if (target == null || maxHP <= 0)
            return;

        var healthBar = GetHealthBar(target);
        if (healthBar != null)
        {
            float fromNormalized = Mathf.Clamp01((float)hpBefore / maxHP);
            float toNormalized = Mathf.Clamp01((float)hpAfter / maxHP);
            healthBar.AnimateToHealth(fromNormalized, toNormalized);
        }
    }

    /// <summary>
    /// Updates the HP text for a unit with explicit values.
    /// This ensures HP text is synchronized with presentation events, not raw state changes.
    /// </summary>
    public void UpdateHealthText(Unit target, int currentHP, int maxHP)
    {
        if (target == null)
            return;

        var hudPanel = GetHUDPanel(target);
        if (hudPanel != null)
        {
            hudPanel.UpdateHealthText(currentHP, maxHP);
        }
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

    /// <summary>
    /// Get the HealthBarUI for a given Unit.
    /// </summary>
    private HealthBarUI GetHealthBar(Unit target)
    {
        if (_combatView == null || target == null)
            return null;

        var combatHUD = _combatView.GetComponentInChildren<CombatHUD>();
        if (combatHUD == null)
            return null;

        return combatHUD.GetHealthBar(target);
    }

    /// <summary>
    /// Get the UnitHUDPanel for a given Unit.
    /// </summary>
    private UnitHUDPanel GetHUDPanel(Unit target)
    {
        if (_combatView == null || target == null)
            return null;

        var combatHUD = _combatView.GetComponentInChildren<CombatHUD>();
        if (combatHUD == null)
            return null;

        return combatHUD.GetHUDPanel(target);
    }
}
