using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Service for UI presentation during combat.
///     Handles damage numbers, healing effects, and status indicators.
/// </summary>
public class UIService
{
    private CombatView _combatView;
    private readonly Dictionary<Unit, UnitUIBinding> _bindings = new();

    public void SetCombatView(CombatView combatView)
    {
        _combatView = combatView;
        _bindings.Clear();
    }

    /// <summary>
    ///     Build a deterministic mapping of units to their UI components.
    ///     Call this once after <see cref="CombatView.Initialize" /> so all lookups
    ///     use direct references instead of repeated component searches.
    /// </summary>
    public void BuildBindings(Unit player, Unit enemy)
    {
        _bindings.Clear();

        if (_combatView == null || player == null || enemy == null)
        {
            Log.Warning("UIService.BuildBindings: called with null CombatView or unit — bindings not built");
            return;
        }

        var combatHUD = _combatView.CombatHUD;

        _bindings[player] = new UnitUIBinding
        {
            UnitView = _combatView.PlayerView,
            HealthBar = combatHUD?.GetHealthBar(player),
            HUDPanel = combatHUD?.GetHUDPanel(player)
        };

        _bindings[enemy] = new UnitUIBinding
        {
            UnitView = _combatView.EnemyView,
            HealthBar = combatHUD?.GetHealthBar(enemy),
            HUDPanel = combatHUD?.GetHUDPanel(enemy)
        };
    }

    public void ShowDamage(Unit target, int amount, DamageType damageType = DamageType.Physical)
    {
        Log.Info("Showing damage UI", new { target = target.Name, damage = amount, type = damageType });

        var worldPosition = GetUnitWorldPosition(target);
        if (worldPosition.HasValue && FloatingTextPool.Instance != null)
            FloatingTextPool.Instance.Spawn(amount, damageType, worldPosition.Value);

        // Animate health bar to current health value (fallback for non-combat or old callers)
        AnimateHealthBar(target);
    }

    /// <summary>
    ///     Show damage with explicit HP values for proper health bar animation.
    ///     This overload is used by DamageAction to animate from old HP to new HP.
    /// </summary>
    public void ShowDamage(Unit target, int amount, int hpBefore, int hpAfter, int maxHP,
        DamageType damageType = DamageType.Physical)
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
            FloatingTextPool.Instance.Spawn(amount, damageType, worldPosition.Value);

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
            FloatingTextPool.Instance.Spawn(amount, DamageType.Heal, worldPosition.Value);

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
    ///     Animates the health bar for a unit to its current health value.
    ///     This ensures health bar animations are driven by presentation events, not raw state changes.
    /// </summary>
    public void AnimateHealthBar(Unit target)
    {
        if (target == null)
            return;

        var healthBar = GetHealthBar(target);
        if (healthBar != null) healthBar.AnimateToCurrentHealth();
    }

    /// <summary>
    ///     Animates the health bar for a unit from a specific old value to a specific new value.
    ///     This allows proper animation even when the unit's state has already been modified.
    /// </summary>
    public void AnimateHealthBarToValue(Unit target, int hpBefore, int hpAfter, int maxHP)
    {
        if (target == null || maxHP <= 0)
            return;

        var healthBar = GetHealthBar(target);
        if (healthBar != null)
        {
            var fromNormalized = Mathf.Clamp01((float)hpBefore / maxHP);
            var toNormalized = Mathf.Clamp01((float)hpAfter / maxHP);
            healthBar.AnimateToHealth(fromNormalized, toNormalized);
        }
    }

    /// <summary>
    ///     Updates the HP text for a unit with explicit values.
    ///     This ensures HP text is synchronized with presentation events, not raw state changes.
    /// </summary>
    public void UpdateHealthText(Unit target, int currentHP, int maxHP)
    {
        if (target == null)
            return;

        var hudPanel = GetHUDPanel(target);
        if (hudPanel != null) hudPanel.UpdateHealthText(currentHP, maxHP);
    }

    /// <summary>
    ///     Get the world position of a unit for UI spawn location.
    /// </summary>
    private Vector3? GetUnitWorldPosition(Unit target)
    {
        var unitView = GetUnitView(target);
        if (unitView != null)
            // Spawn above unit's center
            return unitView.transform.position + Vector3.up * 0.5f;

        return null;
    }

    private UnitView GetUnitView(Unit target)
    {
        return TryGetBinding(target, out var b) ? b.UnitView : null;
    }

    private HealthBarUI GetHealthBar(Unit target)
    {
        return TryGetBinding(target, out var b) ? b.HealthBar : null;
    }

    private UnitHUDPanel GetHUDPanel(Unit target)
    {
        return TryGetBinding(target, out var b) ? b.HUDPanel : null;
    }

    private bool TryGetBinding(Unit target, out UnitUIBinding binding)
    {
        binding = null;
        return target != null && _bindings.TryGetValue(target, out binding);
    }
}